using System;


namespace AppCore
{
	public class Utils
	{

		public const string _MUS_SERVER = "conference.messaging.beetmessenger.com";
		public const string _PUB_SUB_SERVER = "pubsub.messaging.beetmessenger.com";


		public enum DistanceUnit { Miles, Kilometers };  
		public static double ToRadian(double value)  
		{  
			return (Math.PI / 180) * value;  
		}  
		public static double HaversineDistance(LatLng coord1, LatLng coord2, DistanceUnit unit)  
		{  
			double R = (unit == DistanceUnit.Miles) ? 3960 : 6371;  

			var lat = ToRadian(coord2.Latitude - coord1.Latitude);  
			var lng = ToRadian(coord2.Longitude - coord1.Longitude);  

			var h1 = Math.Sin(lat / 2) * Math.Sin(lat / 2) +  
				Math.Cos(ToRadian(coord1.Latitude)) * Math.Cos(ToRadian(coord2.Latitude)) *  
				Math.Sin(lng / 2) * Math.Sin(lng / 2);  

			var h2 = 2 * Math.Asin(Math.Min(1, Math.Sqrt(h1)));  

			return R * h2;  
		}  
	}
}

