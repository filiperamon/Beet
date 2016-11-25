using System;
using System.Collections.Generic;
using System.Linq;

namespace Legion.Client
{
	public class UserService
	{
		private AppPreferences appPreferences { get; set; }

		public UserService()
		{
		}

		public UserService(AppPreferences appPref)
		{
			this.appPreferences = appPref;
		}

		public Dictionary<string, AppCore.Message> getRecentUsers(AppPreferences appPreferences)
		{
			Dictionary<string, AppCore.Message> recentConversationsUserName =
				LegionUtils.getMenssagesByUserName(appPreferences.getValueKey(AppPreferences.CONVERSA_BY_JID));
			if (recentConversationsUserName == null)
			{
				return new Dictionary<string, AppCore.Message>();
			}
			else {
				return recentConversationsUserName;
			}
		}

		public AppCore.User getValidUser(Matrix.Xmpp.Vcard.Vcard vc, string resourceName)
		{

			AppCore.User contac = new AppCore.User();
			contac.Jid = vc.Jid;

			if (vc.Nickname != null && vc.Nickname != "")
			{
				contac.username = vc.Nickname;
				contac.First = vc.Fullname;

				if (vc.GetTagXElement("Phone") != null)
				{
					contac.Phone = vc.GetTagXElement("Phone").ToString();
				}

				if (vc.Photo != null)
				{

					if (vc.Photo.Binval != null)
					{
						contac.Photo = LegionUtils.bitmapToBase64(vc.Photo.Binval);
						appPreferences.saveKey(contac.username + "_ICON", contac.Photo);
					}
				}

			}
			else
			{
				vc.Nickname = vc.Jid.ToString().Replace("@" + resourceName, "");
				contac.username = vc.Nickname;
				if (vc.Photo != null)
				{
					if (vc.Photo.Binval != null)
					{
						contac.Photo = LegionUtils.bitmapToBase64(vc.Photo.Binval);
					}
				}

			}

			return contac;
		}

		public AppCore.User turnGroupToUserToList(string groupId, string groupName) {
			AppCore.User user = new AppCore.User();
			user.isGroup = true;
			user.Jid = groupId;
			user.Nick = groupName;

			return user;
		}

		public List<AppCore.User> removeGroupNoName(List<AppCore.User> groups) {
			return (List<AppCore.User>)groups.Where(y => y.Nick != null || y.username != null);
		}

		public AppCore.User removeRepetUsers(AppCore.User user) {

			List<AppCore.User> users = new List<AppCore.User>();

			foreach (AppCore.User u in user.users) {
				if (!users.Any(o => o.Jid.Equals(u.Jid))) { 
					users.Add(u);
				}

			}

			if (users.Count > 0) {
				user.users = users;
			}

			return user;
		}

		public AppCore.User removeNullUsersJidUnsername(AppCore.User user) {
			return removeRepetUsers(user);
		}

		public List<AppCore.User> moveItemToFirst(List<AppCore.User> users, int index) { 
			var item = users[index];
			users.RemoveAt(index);
			users.Insert(0, item);
			return users;
		}
	}
}

