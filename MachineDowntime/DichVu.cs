using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Timers;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CSDL;
using Android.App;
using Android.Support.V4.App;
using static Android.OS.PowerManager;
using Java.Lang;
using Exception = System.Exception;

namespace MachineDowntime
{
    [Service(Label = "MCDT", Icon = "@drawable/application", Enabled = true)]
    class DichVu : IntentService
    {
        private static int MY_NOTIFICATION_ID = 12345;

        private static int MY_REQUEST_CODE = 100;

        private string _channelID = "1001";

        Connect kn;

        string user = "";
        protected override void OnHandleIntent(Intent intent)
        {
            string chuoi = "Data Source=192.168.1.245;Initial Catalog=Maintenance;Integrated Security=False;User ID=sa;Password=007;Connect Timeout=30;Encrypt=False;";
            kn = new Connect(chuoi);

            //Timer tm = new Timer();
            //tm.Interval = 5 * 1000;
            //tm.Enabled = true;
            //tm.Elapsed += delegate { Notification(); };
            //tm.Start();

            Notification();
        }

        private void Notification()
        {
            try
            {
                ISharedPreferences pre = GetSharedPreferences("MachineDowntime", FileCreationMode.Private);
                string ch2 = pre.GetString("user", "").ToString();
                if (ch2 != "") user = ch2;

                //PowerManager pm = (PowerManager)GetSystemService(PowerService);
                //bool isScreenOn = pm.IsInteractive; // check if screen is on
                //if (!isScreenOn)
                //{
                //    PowerManager.WakeLock wl = pm.NewWakeLock(WakeLockFlags.ScreenDim | WakeLockFlags.AcquireCausesWakeup, "myApp:notificationLock");
                //    wl.Acquire(3000); //set your time in milliseconds
                //}

                Temp.msg = "Service is running " + user;
                DataTable login = kn.Doc("select * from DownTimeUserList where ID = '" + user + "' and Levels > 0").Tables[0] ?? new DataTable();

                if (login.Rows.Count > 0)
                {
                    DataRow r = login.Rows[0];

                    DataTable dt = kn.Doc("SELECT McSerialNo,FacLine,OccurTime,datediff(minute,OccurTime,getdate()) TimeWait " +
                                        "FROM DowntimeReport where StartTime is null order by TimeWait").Tables[0] ?? new DataTable();

                    string msg = "";
                    int i = 0;
                    foreach (DataRow rr in dt.Rows)
                    {
                        i++;
                        msg += i + ". " + rr[0].ToString() +
                                        "\n     Chuyền : " + rr[1].ToString() +
                                        "\n     Máy hư lúc : " + rr[2].ToString() +
                                        "\n     Thời gian chờ : " + rr[3].ToString() +
                                        "\n\n";
                    }

                    if (i > 0)
                    {
                        Temp.msg = "Service is running " + i;
                        NotificationManager notificationService = (NotificationManager)GetSystemService(NotificationService);
                        Intent t = new Intent(this, typeof(NotificationActivity));
                        t.PutExtra("msg", msg);

                        PendingIntent pendingIntent = PendingIntent.GetActivity(this, MY_REQUEST_CODE, t, PendingIntentFlags.UpdateCurrent);
                        string title = "Có " + i + " máy hư cần sửa !!!";

                        if (CreateNotificationChannel())
                            Notification(Resources.GetString(Resource.String.app_name), title, "THONG BAO", 101, pendingIntent);
                    }
                }
            }
            catch (Exception ex) { Temp.msg = "Service error " + ex.ToString(); }
        }

        private bool CreateNotificationChannel()
        {
            bool t = true;
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                t = false;
            else
            {
                var channelName = Resources.GetString(Resource.String.app_name);
                var channelDescription = GetString(Resource.String.app_name);

                var channel = new NotificationChannel(_channelID, channelName, NotificationImportance.Default)
                {
                    Description = channelDescription
                };
                channel.EnableLights(true);
                channel.LightColor = Color.Blue;
                channel.SetShowBadge(true);
                channel.SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification),
                                new AudioAttributes.Builder().SetUsage(AudioUsageKind.Notification).Build());
                channel.LockscreenVisibility = NotificationVisibility.Public;

                var notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager.CreateNotificationChannel(channel);
            }

            return t;
        }

        private void Notification(string title, string msg, string ticker, int notifId, PendingIntent pendingIntent)
        {
            Notification notification = new Notification.Builder(this, _channelID)
                    .SetContentTitle(title)
                    .SetContentText(msg)
                    .SetSmallIcon(Resource.Drawable.application)
                    .SetDefaults(NotificationDefaults.Sound | NotificationDefaults.Vibrate)
                    .SetAutoCancel(true)
                    .SetContentIntent(pendingIntent)
                    .SetPriority((int)NotificationPriority.High)
                    .SetVibrate(new long[] { 1000, 1000, 1000 })
                    .SetOngoing(true)
                    .Build();
            notification.Defaults |= NotificationDefaults.Vibrate;
            notification.Defaults |= NotificationDefaults.Sound;

            PowerManager pm = (PowerManager)ApplicationContext.GetSystemService(PowerService);

            WakeLock wl = pm.NewWakeLock(WakeLockFlags.Partial, "TAG");

            wl.Acquire(15000);

            NotificationManagerCompat notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(notifId, notification);

            wl.Release();
        }

        //public void OnTaskRemoved(Intent rootIntent)
        //{
        //    Intent restartServiceIntent = new Intent(getApplicationContext(), this.getClass());
        //    restartServiceIntent.setPackage(getPackageName());

        //    PendingIntent restartServicePendingIntent = PendingIntent.getService(getApplicationContext(), 1, restartServiceIntent, PendingIntent.FLAG_ONE_SHOT);
        //    AlarmManager alarmService = (AlarmManager)getApplicationContext().getSystemService(Context.ALARM_SERVICE);
        //    alarmService.set(
        //    AlarmManager.ELAPSED_REALTIME,
        //    SystemClock.elapsedRealtime() + 1000,
        //    restartServicePendingIntent);

        //    super.onTaskRemoved(rootIntent);
        //}
    }
}