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
using Android.App.Job;
using Android.Util;
using System.Threading;
using Android.Net;

namespace Legion.Client.utils
{
    [Service]
    public class ConnectionService : Service
    {
        static readonly string TAG = "X:" + typeof(ConnectionService).Name;
        static readonly int TimerWait = 5000; //5 segundos
        Timer _timer;
        ConnectivityManager connectivityManager;
        NetworkInfo activeConnection;

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
            
            _timer = new Timer(o => { getConnection(); },
                               null,
                               0,
                               TimerWait);
            return StartCommandResult.NotSticky;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            _timer.Dispose();
            _timer = null;

            Log.Debug(TAG, "SimpleService destroyed at {0}.", DateTime.UtcNow);
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
        
        public void getConnection()
        {
            Log.Debug(TAG, "Verifica conexão: {0}", DateTime.UtcNow);

            activeConnection = connectivityManager.ActiveNetworkInfo;
            bool isOnline = (activeConnection != null) && activeConnection.IsConnected;
            
            if(isOnline)
                Log.Debug(TAG, "Online em : {0}", DateTime.UtcNow);
            else
                Log.Debug(TAG, "Offline em : {0}", DateTime.UtcNow);
        }
    }
}