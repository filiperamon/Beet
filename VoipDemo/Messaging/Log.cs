using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Messaging
{
    public class Log
    {
        static string tag = "Legion";
        public static void Verbose(string message)
        {
            Android.Util.Log.Verbose(tag, message);
        }

        public static void Debug(string message)
        {
            Android.Util.Log.Debug(tag, message);
        }

        public static void Info(string message)
        {
            Android.Util.Log.Info(tag, message);
        }



        public static void Error(string message)
        {
            Android.Util.Log.Error(tag, message);
        }
    }
}