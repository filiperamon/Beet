using System;
namespace Legion.Client
{
	public class WServiceUrl
	{
		public static string SUCCESS = "SUCCESS";
		public static string ERR_EMAIL_EXISTS = "ERR_EMAIL_EXISTS";
		public static string ERR_AUTH_FAILED = "ERR_AUTH_FAILED";

		public enum MessagegingServer
		{
			SUCCESS,
			ERR_AUTH_FAILED,
			ERR_USERNAME_EXISTS,
			ERR_EMAIL_EXISTS,
			ERR_USERNAME_INVALID,
			ERR_PASSWORD_INVALID,
			ERR_EMAIL_INVALID,
			ERR_UNKNOWN,
		}

	
		//public static string APP_PATH = "http://web.legiontech.org/";
		//public static string CREATE_USER = "http://web.legiontech.org/user/";
		//public static string USER_PATH = "user/";

        //url webservice
        public static string CREATE_USER = "http://web.beetmessenger.com/user/";
		public static string XMPP_URL_LEGION = "52.67.30.228";

        public static MessagegingServer getMessagegingServer(string message)
		{
			if (message.Equals(MessagegingServer.SUCCESS.ToString()))
			{
				return MessagegingServer.SUCCESS;
			}
			else if (message.Equals(MessagegingServer.ERR_AUTH_FAILED.ToString()))
			{
				return MessagegingServer.ERR_AUTH_FAILED;
			}
			else if (message.Equals(MessagegingServer.ERR_USERNAME_EXISTS.ToString()))
			{
				return MessagegingServer.ERR_USERNAME_EXISTS;
			}
			else if (message.Equals(MessagegingServer.ERR_EMAIL_EXISTS.ToString()))
			{
				return MessagegingServer.ERR_EMAIL_EXISTS;
			}
			else if (message.Equals(MessagegingServer.ERR_USERNAME_INVALID.ToString()))
			{
				return MessagegingServer.ERR_USERNAME_INVALID;
			}
			else if (message.Equals(MessagegingServer.ERR_PASSWORD_INVALID.ToString())) 
			{ 
				return MessagegingServer.ERR_PASSWORD_INVALID;
			}
			else if (message.Equals(MessagegingServer.ERR_EMAIL_INVALID.ToString()))
			{
				return MessagegingServer.ERR_EMAIL_INVALID;
			}
			else {

				return MessagegingServer.ERR_UNKNOWN;
			}
		}

		public static int getMessagingDesc(MessagegingServer messaging) {

			int id;

			switch (messaging){

				case MessagegingServer.SUCCESS:
					id = Resource.String.success;
					break;
				case MessagegingServer.ERR_AUTH_FAILED:
					id = Resource.String.authFailed;
					break;
				case MessagegingServer.ERR_USERNAME_EXISTS:
					id = Resource.String.userNameExist;
					break;
				case MessagegingServer.ERR_EMAIL_EXISTS:
					id = Resource.String.emailExist;
					break;
				case MessagegingServer.ERR_USERNAME_INVALID:
					id = Resource.String.userNameInvalid;
					break;
				case MessagegingServer.ERR_PASSWORD_INVALID:
					id = Resource.String.passwInvalid;
					break;
				case MessagegingServer.ERR_EMAIL_INVALID:
					id = Resource.String.emailErro;
					break;
				default:
					id = Resource.String.unknowError;
					break;
			}

			return id;
		}
	}
}

