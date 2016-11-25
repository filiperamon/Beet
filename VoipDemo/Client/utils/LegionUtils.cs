using System;
using AppCore;
using Java.Util.Regex;
using System.Collections.Generic;
using System.Linq;
using Android.Graphics;
using Matrix1 = Android.Graphics.Matrix;
using Android.Provider;
using Legion.Client.utils;
using Android.App;
using Android.Content;
using System.Text.RegularExpressions;
using Java.Lang;
using Java.IO;
using System.IO;
using Java.Text;
using Xamarin.Contacts;

namespace Legion.Client
{
	public class LegionUtils
	{
		static string encodedString = "data:image/jpg;base64,";
		public static string APP_PATH_SD_CARD = "/LegionApp/";
		public static string APP_THUMBNAIL_PATH_SD_CARD = "thumbnails";
        public static Java.IO.File _file;
        public static Java.IO.File _dir;
        public static Bitmap bitmap;
        public static System.Collections.Generic.List<string> JIdsOnline { get; set; }


        public static string parceToGson<T>(T t)
		{
			string json = Newtonsoft.Json.JsonConvert.SerializeObject(t);
			return json;
		}

		public static T getGomJson<T>(string json)
		{
			T t = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
			return t;
		}

		public static List<User> getListJson(string json)
		{
			List<User> t;
			if (json == null || json.Equals(""))
			{
				return t = new List<User>();
			}
			else {
				t = Newtonsoft.Json.JsonConvert.DeserializeObject<List<User>>(json);
			}
			return t;
		}

		public static List<Message> getListMessageJson(string json)
		{
			List<Message> t;
			if (json == null)
			{
				return t = new List<Message>();
			}
			else {
				t = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Message>>(json);
			}
			return t;
		}

		public static List<string> getListStringJson(string json)
		{
			List<string> t;
			if (json == null)
			{
				return t = new List<string>();
			}
			else {
				t = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(json);
			}
			return t;
		}

		public static List<AppCore.Group> getListGroupJson(string json)
		{
			List<AppCore.Group> t;
			if (json == null)
			{
				return t = new List<AppCore.Group>();
			}
			else {
				t = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AppCore.Group>>(json);
			}
			return t;
		}

		public static Dictionary<string, AppCore.Message> getMenssagesByUserName(string json)
		{
			Dictionary<string, AppCore.Message> t;
			if (json == null)
			{
				return t = new Dictionary<string, AppCore.Message>();
			}
			else {
				t = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, AppCore.Message>>(json);
			}
			return t;
		}


		public static User getUserson(string json)
		{
			User t = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(json);
			return t;
		}

		public static AppCore.Group getGroupson(string json)
		{
			AppCore.Group t = Newtonsoft.Json.JsonConvert.DeserializeObject<AppCore.Group>(json);
			return t;
		}

		public static bool isValidEmail(string email)
		{
			string EMAIL_PATTERN = "^[_A-Za-z0-9-\\+]+(\\.[_A-Za-z0-9-]+)*@"
					+ "[A-Za-z0-9-]+(\\.[A-Za-z0-9]+)*(\\.[A-Za-z]{2,})$";

			Pattern pattern = Pattern.Compile(EMAIL_PATTERN);
			Matcher matcher = pattern.Matcher(email);
			return matcher.Matches();
		}

		public static void disposeBitmap(Bitmap bitmap)
		{
			bitmap.Recycle();
			bitmap = null;
		}

		public static string Truncate(string value, int maxLength)
		{
			if (string.IsNullOrEmpty(value)) return value;
			return value.Length <= maxLength ? value : value.Substring(0, maxLength);
		}

		public static string GenerateSessionId()
		{
			return new FM.Randomizer().Next(100000, 999999).ToString();
		}

        public static Bitmap getCircleDrawable(Intent data, Activity activity)
        {
            Android.Net.Uri imageUri = data.Data;
            Bitmap bitmap = MediaStore.Images.Media.GetBitmap(activity.ContentResolver, imageUri);
            bitmap = LegionUtils.getResizedBitmap(bitmap, 170, 170);            

            return bitmap;
        }


        public static Bitmap getResizedBitmap(Bitmap bm, int newWidth, int newHeight)
        {
            int width = bm.Width;
            int height = bm.Height;
            newHeight =  CalculateProportionalHeight(bm.Width, bm.Height, newWidth);

            float scaleWidth = ((float)newWidth) / width;
            float scaleHeight = ((float)newHeight) / height;
            var matrix = new Matrix1();
            matrix.PostScale(scaleWidth, scaleHeight);

            Bitmap resizedBitmap = Bitmap.CreateBitmap(
                bm, 0, 0, width, height, matrix, false);
            bm.Recycle();
            return resizedBitmap;
        }

        public static int CalculateProportionalHeight(int oldWidth, int oldHeight, int newWidth)
        {
            if (oldWidth <= 0 || oldHeight <= 0 || newWidth <= 0)
                // For safety.
                return oldHeight;
            double widthFactor = (double)newWidth / (double)oldWidth;
            int newHeight = (int)System.Math.Round(widthFactor * (double)oldHeight);
            if (newHeight < 1)
                newHeight = 1; // just in case.
            return newHeight;
        }

        public static string GetUserNameClear(string username) {
			if (username == null)
			{
				return "";
			}
			else { 
			    string[] names = username.Split('@');
				return names[0];
			}

        }

		public static string GetUserNameClearByBar(string username)
		{
			string[] names = username.Split('/');
			if (names.Length > 1)
			{
				return names[1];
			}
			else {
				return names[0];
			}

		}

		public static void saveFoundContacts(List<User> contacts, AppPreferences appPref, string KEY)
		{
			string json = LegionUtils.parceToGson(contacts);
			appPref.saveKey(KEY, json);
		}

		public static void saveFoundGroups(List<AppCore.Group> groups, AppPreferences appPref, string KEY)
		{
			string json = LegionUtils.parceToGson(groups);
			appPref.saveKey(KEY, json);
		}

		public static void saveFoundGroups(List<AppCore.User> groups, AppPreferences appPref, string KEY)
		{
			string json = LegionUtils.parceToGson(groups);
			appPref.saveKey(KEY, json);
		}

		public static void saveFoundJidsServer(List<string> contacts, AppPreferences appPref, string KEY)
		{
			string json = LegionUtils.parceToGson(contacts);
			appPref.saveKey(KEY, json);
		}

		public static Bitmap Base64ToBitmap(string base64String)
		{
			byte[] imageAsBytes = Android.Util.Base64.Decode(base64String, Android.Util.Base64Flags.Default);
			return BitmapFactory.DecodeByteArray(imageAsBytes, 0, imageAsBytes.Length);
		}


		public static bool IsBase64String(string value)
		{

			if (value == null || value.Length == 0 || value.Length % 4 != 0
			|| value.Contains(' ') || value.Contains('\t') || value.Contains('\r') || value.Contains('\n'))
				return false;
			var index = value.Length - 1;
			if (value[index] == '=')
				index--;
			if (value[index] == '=')
				index--;
			for (var i = 0; i <= index; i++)
				if (IsInvalid(value[i]))
					return false;
			return true;
		
		}

		// Make it private as there is the name makes no sense for an outside caller
		private static System.Boolean IsInvalid(char value)
		{
			var intValue = (Int32)value;
			if (intValue >= 48 && intValue <= 57)
				return false;
			if (intValue >= 65 && intValue <= 90)
				return false;
			if (intValue >= 97 && intValue <= 122)
				return false;
			return intValue != 43 && intValue != 47;
		}

		public static byte[] base64ToByte(string base64) {
			byte[] imageAsBytes = null;
			try
			{
				imageAsBytes = Android.Util.Base64.Decode(base64, Android.Util.Base64Flags.Default);
			}
			catch (System.Exception e) {
				imageAsBytes = null;
			}

			return imageAsBytes;
		}

		public static Bitmap bytesToUIImage(byte[] bytes)
		{

			if (bytes == null)
				return null;

			Bitmap bitmap;


			//var documentsFolder = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;

			//Create a folder for the images if not exists
			//System.IO.Directory.CreateDirectory(System.IO.Path.Combine(documentsFolder, "images"));

			//string imatge = System.IO.Path.Combine(documentsFolder, "Legion", new DateTime().Millisecond+"legion_image.png");


			//System.IO.File.WriteAllBytes(imatge, bytes.Concat(new byte[] { (byte)0xD9 }).ToArray());
			//BitmapFactory.Options op = new BitmapFactory.Options();
			//op.InSampleSize = 2;

			//try
			//{
			//	bitmap = BitmapFactory.DecodeFile(imatge);
			//}
			//catch (OutOfMemoryError e) {
			//	bitmap = BitmapFactory.DecodeFile(imatge, op);

			//}


			return null;

		}

		public static Bitmap BitmapByName(string filename) {

			CreateDirectoryForPictures();

			Bitmap bmp = null;
			Java.IO.File myPath = new Java.IO.File(_dir, filename);

			if (myPath.Exists()) {
				bmp = BitmapFactory.DecodeFile(myPath.AbsolutePath);
			}

			return bmp;
		}

		public static string SaveBitmapGalerry(Bitmap bitmap)
		{
			CreateDirectoryForPictures();

			ContextWrapper cw = new ContextWrapper(Application.Context);
			Java.IO.File directory = cw.GetDir("LegionApp", FileCreationMode.Private);

			DateTime dt = DateTime.Now;
			string filename = (dt.ToString("yyyyMMddHHmmss") + "_img.png");
			Java.IO.File myPath = new Java.IO.File(_dir, filename);

			var stream = new FileStream(myPath.AbsolutePath, FileMode.Create);
			bitmap.Compress(Bitmap.CompressFormat.Png, 50, stream);
			stream.Flush();
			stream.Close();

			bitmap.Recycle();
			bitmap = null;
			MediaStore.Images.Media.InsertImage(cw.ContentResolver, myPath.AbsolutePath, filename, filename);

			return filename;
		}

		private static void CreateDirectoryForPictures()
		{
			    _dir = new Java.IO.File(
				Android.OS.Environment.GetExternalStoragePublicDirectory(
					Android.OS.Environment.DirectoryPictures), "LegionApp");
			if (!_dir.Exists())
			{
				_dir.Mkdirs();
			}
		}

		public static bool ContainsSpecialChars(string value)
		{
			var list = new[] { "~", "`", "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "+", "=", "\"","_" };
			return list.Any(value.Contains);
		}

		//read contacts phone device by email and phone
		public static List<User> readContacts(AddressBook addressBook)
		{
			List<User> contacts = new List<User>();

			//if contatc heave phone validation
			//IEnumerable<Contact> iContacts = addressBook.AsEnumerable().Where(c => { return c.Phones.Count<Phone>() > 0 && c.Emails.Count<Email>() > 0; });

			//only email
			IEnumerable<Contact> iContacts = addressBook.AsEnumerable().Where(c => { return c.Emails.Count<Email>() > 0; });


			foreach (Contact contact in iContacts)
			{
				User cont = new User();
				cont.username = contact.DisplayName;

				// if contatc have phone on validation
				//foreach (Phone p in contact.Phones)
				//{
				//	cont.Phone = p.Number;
				//	break;
				//}

				foreach (Email e in contact.Emails)
				{
					cont.email = e.Address;
					break;
				}

				if (cont.email != null)
				{
					contacts.Add(cont);
				}
			}

			return contacts;

		}

		public static string bitmapToBase64(byte[] data)
		{
			Bitmap bitmap = BitmapFactory.DecodeByteArray(data, 0, data.Length);
			var ms = new MemoryStream();
			bitmap.Compress(Bitmap.CompressFormat.Png, 0, ms);
			var imageByteArray = ms.ToArray();
			string avatarBase64String = Convert.ToBase64String(imageByteArray);
			LegionUtils.disposeBitmap(bitmap);
			return avatarBase64String;
		}

		public static List<AppCore.User> getGroupsTempToList(string APP_PREF, AppPreferences appPref)
		{
			string jsonGroup = appPref.getValueKey(APP_PREF);
			List<AppCore.User> usersGroups = null;
			if (jsonGroup != null || jsonGroup != "")
			{
				usersGroups = LegionUtils.getListJson(jsonGroup);

			}
			else {
				usersGroups = new List<User>();
			}
			return usersGroups;
		}

	}
}

