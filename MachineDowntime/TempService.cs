using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Security;

namespace MachineDowntime
{
    [Service(Label = "MCDT", Icon = "@drawable/application", Enabled = true)]
    class TempService : Service
    {
        static readonly string TAG = typeof(TempService).FullName;
        static readonly int DELAY_BETWEEN_LOG_MESSAGES = 5000; // milliseconds
        static readonly int NOTIFICATION_ID = 10000;

        //DateTime.Now.get timestamper;
        bool isStarted;
        Handler handler;
        Action runnable;

        public override void OnCreate()
        {
            base.OnCreate();
            Temp.msg = "OnCreate: the service is initializing.";

            //timestamper = new UtcTimestamper();
            handler = new Handler();

            // This Action is only for demonstration purposes.
            runnable = new Action(() =>
            {
                //if (timestamper != null)
                {
                    // Log.Debug(TAG, timestamper.GetFormattedTimestamp());
                    handler.PostDelayed(runnable, DELAY_BETWEEN_LOG_MESSAGES);
                }
            });
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (isStarted)
            {
                Temp.msg = "OnStartCommand: This service has already been started.";
            }
            else
            {
                Temp.msg = "OnStartCommand: The service is starting.";
                DispatchNotificationThatServiceIsRunning();
                handler.PostDelayed(runnable, DELAY_BETWEEN_LOG_MESSAGES);
                isStarted = true;
            }

            // This tells Android not to restart the service if it is killed to reclaim resources.
            return StartCommandResult.Sticky;
        }


        public override IBinder OnBind(Intent intent)
        {
            // Return null because this is a pure started service. A hybrid service would return a binder that would
            // allow access to the GetFormattedStamp() method.
            return null;
        }


        public override void OnDestroy()
        {
            // We need to shut things down.
            //Log.Debug(TAG, GetFormattedTimestamp());
            Temp.msg = "OnDestroy: The started service is shutting down.";

            // Stop the handler.
            handler.RemoveCallbacks(runnable);

            // Remove the notification from the status bar.
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.Cancel(NOTIFICATION_ID);

            //timestamper = null;
            isStarted = false;
            base.OnDestroy();
        }

        /// <summary>
        /// This method will return a formatted timestamp to the client.
        /// </summary>
        /// <returns>A string that details what time the service started and how long it has been running.</returns>
        //string GetFormattedTimestamp()
        //{
        //    return timestamper?.GetFormattedTimestamp();
        //}

        void DispatchNotificationThatServiceIsRunning()
        {
            //Notification.Builder notificationBuilder = new Notification.Builder(this)
            //    .SetSmallIcon(Resource.Drawable.application)
            //    .SetContentTitle(Resources.GetString(Resource.String.app_name))
            //    .SetContentText("Service run " + DateTime.Now.ToString("HHmmss"));

            //var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            //notificationManager.Notify(NOTIFICATION_ID, notificationBuilder.Build());
            NotificationManager notificationService = (NotificationManager)GetSystemService(NotificationService);
            Intent t = new Intent(this, typeof(NotificationActivity));
            PendingIntent pendingIntent = PendingIntent.GetActivity(this, 100, t, PendingIntentFlags.UpdateCurrent);

            Notification notification = new Notification.Builder(this, "1001")
                    .SetContentTitle(Resources.GetString(Resource.String.app_name))
                    .SetSmallIcon(Resource.Drawable.application)
                    .SetContentText("Service run " + DateTime.Now.ToString("HHmmss"))
                    .SetDefaults(NotificationDefaults.Sound | NotificationDefaults.Vibrate)
                    .SetAutoCancel(true)
                    //.SetContentIntent(pendingIntent)
                    .SetPriority((int)NotificationPriority.High)
                    .SetVibrate(new long[] { 1000, 1000, 1000 })
                    .SetOngoing(true)
                    .Build();
            notification.Defaults |= NotificationDefaults.Vibrate;
            notification.Defaults |= NotificationDefaults.Sound;

            NotificationManagerCompat notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(NOTIFICATION_ID, notification);
        }
    }
}