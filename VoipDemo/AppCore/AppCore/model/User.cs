using System;
using System.Collections.Generic;

namespace AppCore
{
	public class User : BaseEntity
	{

		public string Jid { get; set; }
		public string First { get; set; }
		public string Last { get; set; }
		public string Nick { get; set; }
		public string email { get; set; }
		public string username { get; set; }
		public string password { get; set; }
		public string android_id { get; set; }
		public string Phone { get; set; }
		public string Photo { get; set; }
		public string friend { get; set;}
		public bool isGroup { get; set;}
		public bool isOnline { get; set;}
        public bool isCheck { get; set; }

        public int NumChatConversation
		{
			get { return 20; }
		}

		private User[] userArray { get; set;}
		public List<User> users {get; set;}

		public User()
		{
		}
		public User(string Jid, string username) {
			this.Jid = Jid;
			this.username = username;
		}

        public User(string JidGroup)
        {
            this.Jid = Jid;
            this.isGroup = true;
        }
    }
}

