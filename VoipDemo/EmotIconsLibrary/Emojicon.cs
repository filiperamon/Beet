using System;
using System.Runtime.Serialization;

namespace com.vasundharareddy.emojicon
{
	public class Emojicon : Java.Lang.Object
	{
		private int icon;
		private char value;
		private string emoji;
		public Emojicon ()
		{
		}
		public static Emojicon FromResource(int icon, int value) 
		{
			Emojicon emoji = new Emojicon();
			emoji.icon = icon;
			emoji.value = (char) value;
			return emoji;
		}
		public static Emojicon FromCodePoint(int codePoint) {
			Emojicon emoji = new Emojicon();
			emoji.emoji = Char.ConvertFromUtf32(codePoint);
			return emoji;
		}

		public static Emojicon FromChar(char ch) {
			Emojicon emoji = new Emojicon();
			emoji.emoji = Convert.ToString(ch);
			return emoji;
		}

		public static Emojicon FromChars(string chars) {
			Emojicon emoji = new Emojicon();
			emoji.emoji = chars;
			return emoji;
		}
		public Emojicon(string emoji) {
			this.emoji = emoji;
		}

		public char Value
		{
			get{ return value; }
		}

		public int Icon
		{
			get
			{
				return icon;
			}
		}

		public string Emoji
		{
			get
			{
				return emoji;
			}
		}
        public override bool Equals(object obj)
        {
             if((obj.GetType() == typeof(Emojicon))&&(emoji.Equals(((Emojicon)obj).Emoji)))
             {
                 return true;
             }
             else
             {
                 return false;
             }
        }
        public override int GetHashCode()
        {
            return emoji.GetHashCode();
        }
        public override string ToString()
        {
            return emoji;
        }
    }
}

