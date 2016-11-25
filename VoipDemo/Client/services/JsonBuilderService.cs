using System;
using AppCore;

namespace Legion.Client
{
	public class JsonBuilderService
	{
		public string jsonCreateUser(User user) {
			string json = "{\"api_username\":\"" + AppPreferences.API_USERNAME + "\",\"api_password\":\"" + AppPreferences.API_PASSWORD + "\"," +
				"\"username\":\"" + user.username + "\",\"email\":\"" + user.email + "\",\"password\":\"" + user.password + "\"," +
			    "\"android_id\":\"1c3acb357a70c0ba\",\"avatar\":\""+user.Photo+"\",\"phone\":\""+user.Phone+ "\",\"firstname\":\""+user.First+ "\",\"lastname\":\"" + user.Last + "\"}";


			return json;
		}
	}
}

