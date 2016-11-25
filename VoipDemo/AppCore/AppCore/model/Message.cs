using System;

namespace AppCore
{
	public class Message : BaseEntity
	{
		public Contacts Contacs { get; set;}
		public string FriendId { get; set; }
		public string GroupId { get; set;}
		public string Msg { get; set;}
		public DateTime Msgdate { get; set;}
		public bool isMe { get; set;}
		public string imgName { get; set;}
		public bool isImage { get; set;}
		public bool isGroup { get; set;}

		public Message() { 
		}

		public Message(string FriendId, string Msg, DateTime Msgdate, bool isMe) {
			this.FriendId = FriendId;
			this.Msg = Msg;
			this.Msgdate = Msgdate;
			this.isMe = isMe;
		}

		public Message(string FriendId, string Msg, DateTime Msgdate, bool isMe, string imgName, bool isImage)
		{
			this.FriendId = FriendId;
			this.Msg = Msg;
			this.Msgdate = Msgdate;
			this.isMe = isMe;
			this.imgName = imgName;
			this.isImage = isImage;
		}

		public Message(string GroupId, string Msg, DateTime Msgdate, bool isMe, bool isGroup) { 
			this.GroupId = GroupId;
			this.Msg = Msg;
			this.Msgdate = Msgdate;
			this.isMe = isMe;
			this.isGroup = isGroup;
		}

		public Message(string GroupId, string Msg, DateTime Msgdate, bool isMe, bool isGroup, string imgName, bool isImage)
		{
			this.GroupId = GroupId;
			this.Msg = Msg;
			this.Msgdate = Msgdate;
			this.isMe = isMe;
			this.isGroup = isGroup;
			this.imgName = imgName;
			this.isImage = isImage;
		}
	}
}

