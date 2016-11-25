using System;
using System.Collections.Generic;

namespace AppCore
{
	public class Group : BaseEntity
	{
		public string Jid { get; set; }
		public string username { get; set; }
		public string Photo { get; set; }
		public List<User> users { get; set; }
	}
}

