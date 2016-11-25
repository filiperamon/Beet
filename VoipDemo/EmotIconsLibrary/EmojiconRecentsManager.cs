using System;
using Android.Content;
using System.Collections.Generic;
using System.Text;

namespace com.vasundharareddy.emojicon
{
	public static class EmojiconRecentsManager
	{
		private const string PREFERENCE_NAME = "emojicon";
		private const string PREF_RECENTS = "recent_emojis";
		private const string PREF_PAGE = "recent_page";
		private const int MAX_SAVE = 40;
		private static Context m_Context;
		private static List<Emojicon> m_Emojicons;

		public static Context Context
		{ 
			set{
				m_Context = value.ApplicationContext;
				LoadRecents();
			}
		}
		private static ISharedPreferences Preferences
		{
			get
			{
				return m_Context.GetSharedPreferences (PREFERENCE_NAME, FileCreationMode.Private);
			}
		}
		public static int RecentPage{
			get{
				return Preferences.GetInt (PREF_PAGE, 0);
			}
			set{
				Preferences.Edit().PutInt(PREF_PAGE, value).Commit ();
			}
		}
		public static int Count
		{
			get{ return m_Emojicons.Count; }
		}
		public static List<Emojicon> Recents
		{
			get{return m_Emojicons; }
		}
		public static void Push(Emojicon emojicon)
		{
			if (m_Emojicons.Contains (emojicon))
				m_Emojicons.Remove (emojicon);
			m_Emojicons.Insert (0, emojicon);
			SaveRecents ();
		}
		public static void Add (Emojicon emojicon)
		{
			m_Emojicons.Add (emojicon);
			SaveRecents ();
		}
		public static void Add(int index,Emojicon emojicon)
		{
			m_Emojicons.Insert(index, emojicon);
		}
		public static void Remove(Emojicon emojicon)
		{
			m_Emojicons.Remove (emojicon);
			SaveRecents ();
		}
		static void LoadRecents ()
		{
			ISharedPreferences pref = Preferences;
			string str = pref.GetString (PREF_RECENTS, "");
			Console.WriteLine("Load:"+str);
			string[] emojicons = str.Split ('#');
			m_Emojicons = new List<Emojicon> ();
			foreach (var emojicon in emojicons) {
				try{
					//int codepoint =  Convert.ToInt32(emojicon);// Char.ConvertToUtf32(emojicon,0);
					if(emojicon != "")
					Add(Emojicon.FromChars(emojicon));
					//Add(Emojicon.FromCodePoint(codepoint));
				}
				catch
				{

				}
			}
		}

		static void SaveRecents ()
		{
			if (m_Emojicons.Count == MAX_SAVE)
				m_Emojicons.RemoveRange (0, 1);
			string str = "";
			foreach(var emoji in m_Emojicons)
			{
				//for(int i=0;i<emoji.Emoji.Length;i++)
				if(emoji.Emoji != "")
				str += emoji.Emoji + "#"; //Char.ConvertToUtf32 (emoji.Emoji, i) + "#";
			}
            Console.WriteLine("Save:"+str);
			Preferences.Edit ().PutString (PREF_RECENTS, str).Commit();
		}
	}
}

