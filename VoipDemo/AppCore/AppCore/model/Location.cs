using System;

namespace AppCore
{
	public class Location : BaseEntity
	{
		public Country Country { get; set;}
		public double Latitude { get; set;}
		public double Longitude { get; set;} 
	}
}

