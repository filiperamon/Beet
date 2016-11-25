using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Bcpg;
using System.IO;
using Org.BouncyCastle.Utilities.IO;

using Matrix.Xml;

namespace Messaging
{
    public class OpenPGP
    {
        private string jid;
        private PgpSecretKey secretKey;
        private PgpPrivateKey privateKey
        {
            get {
                return secretKey.ExtractPrivateKey(this.passPhrase.ToCharArray());
            }
        }
        private PgpPublicKey publicKey {
            get {
                return secretKey.PublicKey;
            }
        }
        private Xmpp xmpp;
        private ISharedPreferences contextPrefs;

        private string passPhrase = "";

        public OpenPGP(Xmpp xmpp, string jid)
        {
            this.xmpp = xmpp;
            this.jid = jid;

            this.contextPrefs = Android.App.Application.Context.GetSharedPreferences("Legion", FileCreationMode.Private);

            Log.Info("PGP initializing");
            var p = this.contextPrefs.GetString("SecretKey", "");
            if (p != "")
            {
                Log.Info("Imported secret key from private store.");
                ImportSecretKey(Streamify(p));
            }
            else
            {
                Log.Info("Generating secret key.");
                Generate();
                Log.Info("Saving secret key to private store.");
                var edit = this.contextPrefs.Edit();
                edit.PutString("SecretKey", Convert.ToBase64String(secretKey.GetEncoded()));
                edit.Commit();
                Log.Info("Publishing public key.");
                Publish();
            }
            Log.Info("PGP initialized");
            //Publish();
            /* 
             * var = priavteKeyInSecureStorage
             * if var then
             *      importPrivateKey
             * else
             *      generate(jid)
             *      saveInSecureStorage
             *      publish
             * end if
             */
        }

        internal void AddPublicKey(string jid, string publicKey)
        {
            var edit = this.contextPrefs.Edit();
            edit.PutString(jid, publicKey);
            edit.Commit();
        }

        internal string GetPublicKey(string jid)
        {
            return this.contextPrefs.GetString(jid, "");
        }

        internal void Publish()
        {
            // TODO: need this to roll up existing ones as well though
            XmppXElement pubkeys = new XmppXElement("urn:xmpp:openpgp:0", "pubkeys");
            XmppXElement pubkey = new XmppXElement("urn:xmpp:openpgp:0", "pubkey") { Value = Convert.ToBase64String(this.secretKey.PublicKey.GetEncoded()) };
            pubkeys.Add(pubkey);
            var item = new Matrix.Xmpp.PubSub.Item();
            item.Add(pubkeys);
            this.xmpp.pubsubManager.PublishItem("", "urn:xmpp:openpgp:0", item);            
        }

        internal void Generate()
        {
            char[] passPhrase = "".ToCharArray();
            IAsymmetricCipherKeyPairGenerator kpg = new RsaKeyPairGenerator();
            kpg.Init(new RsaKeyGenerationParameters(BigInteger.ValueOf(0x13), new SecureRandom(), 1024, 8));
            AsymmetricCipherKeyPair kp = kpg.GenerateKeyPair();

            this.secretKey = new PgpSecretKey(
                PgpSignature.DefaultCertification,
                PublicKeyAlgorithmTag.RsaGeneral,
                kp.Public,
                kp.Private,
                DateTime.Now,
                this.jid,
                SymmetricKeyAlgorithmTag.Cast5,
                this.passPhrase.ToCharArray(),
                null,
                null,
                new SecureRandom()
            );

            // this.publicKey = secretKey.PublicKey;
            // this.privateKey = secretKey.ExtractPrivateKey(this.passPhrase.ToCharArray());
        }

        private static PgpPublicKey ImportPublicKey(Stream publicIn)
        {
            var pubRings =
                new PgpPublicKeyRingBundle(PgpUtilities.GetDecoderStream(publicIn)).GetKeyRings().OfType<PgpPublicKeyRing>();
            var pubKeys = pubRings.SelectMany(x => x.GetPublicKeys().OfType<PgpPublicKey>());
            var pubKey = pubKeys.FirstOrDefault();
            return pubKey;
        }

        private static PgpSecretKey ImportSecretKey(Stream secretIn)
        {
            var secRings =
                new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(secretIn)).GetKeyRings().OfType<PgpSecretKeyRing>();
            var secKeys = secRings.SelectMany(x => x.GetSecretKeys().OfType<PgpSecretKey>());
            var secKey = secKeys.FirstOrDefault();
            return secKey;
        }

        public string Encrypt(string jid, string message)
        {
            // lookup public key
            PgpPublicKey publicKey = null;
            var pub = GetPublicKey(jid);            
            if (pub != "")
                publicKey = ImportPublicKey(Streamify(pub));

            if (publicKey == null)
                throw new FileNotFoundException("Cannot find a public key for " + jid);

            var encryptedBytes = this.Encrypt(System.Text.Encoding.ASCII.GetBytes(message), publicKey, true, false);
            return Convert.ToBase64String(encryptedBytes);
        }

        protected byte[] Encrypt(byte[] inputData, PgpPublicKey publicKey, bool withIntegrityCheck, bool armor)
        {
            byte[] processedData = Compress(inputData, PgpLiteralData.Console, CompressionAlgorithmTag.Uncompressed);

            MemoryStream bOut = new MemoryStream();
            Stream output = bOut;

            if (armor)
                output = new ArmoredOutputStream(output);

            PgpEncryptedDataGenerator encGen = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Cast5, withIntegrityCheck, new SecureRandom());
            encGen.AddMethod(publicKey);

            Stream encOut = encGen.Open(output, processedData.Length);

            encOut.Write(processedData, 0, processedData.Length);
            encOut.Close();

            if (armor)
                output.Close();

            return bOut.ToArray();
        }

        public string Decrypt(string inputData)
        {
            PgpPrivateKey privateKey = this.privateKey;

            var pgpData = System.Text.Encoding.ASCII.GetBytes(inputData);
            byte[] decryptedData = Decrypt(pgpData, privateKey);

            return System.Text.Encoding.ASCII.GetString(decryptedData);
        }
        /**
            * Decrypt the byte array passed into inputData and return it as
            * another byte array.
            *
            * @param inputData - the data to decrypt
            * @param keyIn - a stream from your private keyring file
            * @param passCode - the password
            * @return - decrypted data as byte array
            */
        public byte[] Decrypt(byte[] inputData, PgpPrivateKey sKey)
        {
            byte[] error = System.Text.Encoding.ASCII.GetBytes("ERROR");

            Stream inputStream = new MemoryStream(inputData);
            inputStream = PgpUtilities.GetDecoderStream(inputStream);
            MemoryStream decoded = new MemoryStream();

            try
            {
                PgpObjectFactory pgpF = new PgpObjectFactory(inputStream);
                PgpEncryptedDataList enc;
                PgpObject o = pgpF.NextPgpObject();

                //
                // the first object might be a PGP marker packet.
                //
                if (o is PgpEncryptedDataList)
                    enc = (PgpEncryptedDataList)o;
                else
                    enc = (PgpEncryptedDataList)pgpF.NextPgpObject();

                //
                // find the secret key
                //
                PgpPublicKeyEncryptedData pbe = null;
                foreach (PgpPublicKeyEncryptedData pked in enc.GetEncryptedDataObjects())
                {
                    if (sKey != null)
                    {
                        pbe = pked;
                        break;
                    }
                }
                if (sKey == null)
                    throw new ArgumentException("secret key for message not found.");

                Stream clear = pbe.GetDataStream(sKey);
                PgpObjectFactory plainFact = new PgpObjectFactory(clear);
                PgpObject message = plainFact.NextPgpObject();

                if (message is PgpCompressedData)
                {
                    PgpCompressedData cData = (PgpCompressedData)message;
                    PgpObjectFactory pgpFact = new PgpObjectFactory(cData.GetDataStream());
                    message = pgpFact.NextPgpObject();
                }
                if (message is PgpLiteralData)
                {
                    PgpLiteralData ld = (PgpLiteralData)message;
                    Stream unc = ld.GetInputStream();
                    Streams.PipeAll(unc, decoded);
                }
                else if (message is PgpOnePassSignatureList)
                    throw new PgpException("encrypted message contains a signed message - not literal data.");
                else
                    throw new PgpException("message is not a simple encrypted file - type unknown.");

                if (pbe.IsIntegrityProtected())
                {
                    if (!pbe.Verify())
                        Log.Info("Message failed integrity check.");
                    else
                        Log.Info("Message integrity check passed.");
                }
                else
                {
                    Log.Info("No message integrity check.");
                }

                return decoded.ToArray();
            }
            catch (Exception e)
            {
                if (e.Message.StartsWith("Checksum mismatch"))
                    Log.Info("Likely invalid passcode. Possible data corruption.");
                else if (e.Message.StartsWith("Object reference not"))
                    Log.Info("PGP data does not exist.");
                else if (e.Message.StartsWith("Premature end of stream"))
                    Log.Info("Partial PGP data found.");
                else
                    Log.Info(e.Message);
                Exception underlyingException = e.InnerException;
                if (underlyingException != null)
                    Log.Info(underlyingException.Message);

                return error;
            }
        }

        private static Stream Streamify(string theString, System.Text.Encoding encoding = null)
        {
            encoding = encoding ?? System.Text.Encoding.UTF8;
            var stream = new MemoryStream(encoding.GetBytes(theString));
            return stream;
        }

        private static byte[] Compress(byte[] clearData, string fileName, CompressionAlgorithmTag algorithm)
        {
            MemoryStream bOut = new MemoryStream();

            PgpCompressedDataGenerator comData = new PgpCompressedDataGenerator(algorithm);
            Stream cos = comData.Open(bOut); // open it with the final destination
            PgpLiteralDataGenerator lData = new PgpLiteralDataGenerator();

            // we want to Generate compressed data. This might be a user option later,
            // in which case we would pass in bOut.
            Stream pOut = lData.Open(
            cos,                    // the compressed output stream
            PgpLiteralData.Binary,
            fileName,               // "filename" to store
            clearData.Length,       // length of clear data
            DateTime.UtcNow         // current time
            );

            pOut.Write(clearData, 0, clearData.Length);
            pOut.Close();

            comData.Close();

            return bOut.ToArray();
        }
    }
}