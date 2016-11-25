using System;

namespace AppCore
{
	public class Contacts : BaseEntity
	{
		public string UserName { get; set;}
		public User Contact { get; set;}
		public DateTime FriendFrom { get; set;}

	}
}

