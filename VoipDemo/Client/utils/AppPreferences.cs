using System;
using Android.Content;
using Android.Preferences;


namespace Legion.Client
{
	public class AppPreferences
	{

		private ISharedPreferences mSharedPrefs;
		private ISharedPreferencesEditor mPrefsEditor;
		private Context mContext;

		private static String LEGION_PREFERENCES = "LEGION_PREFERENCES";
		public static String LOCALE_USER = "LOCALE";
		public static string FIRST_ACCESS = "FIRST_ACCESS";
		public static string FIRST_ACCESS_SESSION = "FIRST_ACCESS_SESSION";
		public static string USERNAME = "username";
		public static string PASSWORD = "password";
		public static string FIRST_NAME = "firstname";
		public static string LAST_NAME = "lastname";
		public static string PHONE = "phone";
		public static string ENABLE = "enable";
		public static string AVATAR = "avatar";
		public static string USER = "user";
		public static string CONTACTS = "CONTACTS";
		public static string CONTACTS_VCARDS = "VCARDS";
		public static string JIDSFROM_SERVER = "JIDSFROM_SERVER";
		public static string CONVERSATION_CONTATCS = "CONVERSATION_CONTATCS";
		public static string CONVERSA_BY_JID = "CONVERSA_BY_JID";
		public static string CONVERSA_RECENTS = "CONVERSA_RECENTS";
		public static string GROUPS_TEMP = "GROUPS_TEMP";
		public static string GROUPS = "GROUPS";
		public static string SUBJECTS = "SUBJECTS";
		public static string GROUP_JSON = "GROUP_JSON";

		//Api server
		public static string API_USERNAME = "n4yN8tl3GzSmTBUtyCzK";
		public static string API_PASSWORD = "Ah8zaaZSIQMnKFJVWulD";

		//Xmpp_OnSubscribeRequest
		public static string FRIEND_REQUEST = "FRIEND_REQUEST";

		public AppPreferences(Context context)
		{
			this.mContext = context;
			mSharedPrefs = context.ApplicationContext.GetSharedPreferences(LEGION_PREFERENCES, FileCreationMode.Private);
			mPrefsEditor = mSharedPrefs.Edit();

		}

		public void saveAccessKey(string key)
		{
			mPrefsEditor.PutString(LEGION_PREFERENCES, key);
			mPrefsEditor.Commit();
		}

		public string getAccessKey()
		{
			return mSharedPrefs.GetString(LEGION_PREFERENCES, "");
		}

		public void saveKey(string KEY, string value)
		{
			mPrefsEditor.PutString(KEY, value);
			mPrefsEditor.Commit();
		}

		public void saveKey(string KEY, bool value)
		{
			mPrefsEditor.PutBoolean(KEY, value);
			mPrefsEditor.Commit();
		}

        public void saveKeyMSGCount(string KEY, int value)
        {
            mPrefsEditor.PutInt(KEY+"_MSG", value);
            mPrefsEditor.Commit();
        }

        public int getValueKeyMSGCount(string KEY)
        {
            return mSharedPrefs.GetInt(KEY+ "_MSG", 0);
        }


        public string getValueKey(string KEY)
		{
			return mSharedPrefs.GetString(KEY, null);
		}

		public bool isFisrtAcess(string KEY)
		{
			return mSharedPrefs.GetBoolean(KEY, true);
		}

		public void clearPreferences()
		{
			mSharedPrefs.Edit();
			mPrefsEditor.Clear();
			mPrefsEditor.Commit();
		}
	}
}

