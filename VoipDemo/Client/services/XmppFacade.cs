using System;
using Messaging;
using AppCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using Android.Graphics;
using System.IO;

namespace Legion.Client
{
	class XmppFacade
	{
		
		public async void searchContactByEmail(string email, ActivityService ActivityService) {
			User user = new User();
			ActivityService.GetInstance.App.Xmpp.Search(email, delegate (object s, Matrix.Xmpp.Client.IqEventArgs eIq)
			{
				var iq = eIq.Iq;
				if (iq.Type == Matrix.Xmpp.IqType.Result)
				{
					var searchQuery = iq.Query as Matrix.Xmpp.Search.Search;
					foreach (var sItem in searchQuery.GetItems())
					{
						Log.Info("search result: " + sItem.Jid + "\n");
						user.Jid = sItem.Jid;
					}
				}
			});

		}

		[Obsolete("Method is deprecated, please use LegionUtils.saveFoundContacts instead.")]
        public void saveFoundContacts(List<User> contacts, AppPreferences appPref) {
            String json = LegionUtils.parceToGson(contacts);
			appPref.saveKey(AppPreferences.CONTACTS, json);
        }

		public void saveFoundVcards(List<User> contacts, AppPreferences appPref)
		{
			String json = LegionUtils.parceToGson(contacts);
			appPref.saveKey(AppPreferences.CONTACTS_VCARDS, json);
		}

		public Matrix.Xmpp.Vcard.Vcard saveVCard(User user, AppPreferences appPref) { 
			var vCard = new Matrix.Xmpp.Vcard.Vcard();
            vCard.SetElementValue("First", user.First);
            vCard.SetElementValue("Last", user.Last);
            vCard.Fullname = user.First + " " + user.Last;
			vCard.Nickname = user.username;            
			vCard.Photo = new Matrix.Xmpp.Vcard.Photo();
			vCard.Photo.Binval = ByteBufferFromImage(Base64ToBitmap(appPref.getValueKey(AppPreferences.AVATAR)));
			vCard.SetElementValue("Phone", user.Phone);
            return vCard;
		}

        public Matrix.Xmpp.Vcard.Vcard saveVCard(User user, AppPreferences appPref, string avatar)
        {
            var vCard = new Matrix.Xmpp.Vcard.Vcard();
            vCard.SetElementValue("First", user.First);
            vCard.SetElementValue("Last", user.Last);
            vCard.Fullname = user.First + " " + user.Last;
            vCard.Nickname = user.username;
            vCard.Photo = new Matrix.Xmpp.Vcard.Photo();
			if (avatar != null)
			{
				vCard.Photo.Binval = ByteBufferFromImage(Base64ToBitmap(avatar));
			}
            vCard.SetElementValue("Phone", user.Phone);
            return vCard;
        }

        public static Byte[] ByteBufferFromImage(Bitmap bitmap)
		{

			byte[] bitmapData;
			using (var stream = new MemoryStream())
			{
				bitmap.Compress(Bitmap.CompressFormat.Png, 0, stream);
				bitmapData = stream.ToArray();
			}
			return bitmapData;
		}

		public Bitmap Base64ToBitmap(String base64String)
		{
			if (base64String == null) { return null; }
			byte[] imageAsBytes = Android.Util.Base64.Decode(base64String, Android.Util.Base64Flags.Default);
			return BitmapFactory.DecodeByteArray(imageAsBytes, 0, imageAsBytes.Length);
		}

    }
}

