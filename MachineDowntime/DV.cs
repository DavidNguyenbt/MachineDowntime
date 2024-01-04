using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Timers;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using CSDL;
using static Android.OS.PowerManager;

namespace MachineDowntime
{
    [Service]
    class DV : Service
    {
        private static int MY_REQUEST_CODE = 100;

        private string _channelID = "1001", lsmc, mc;

        private DataTable NgonNgu = new DataTable();

        private int lg = 0;
        public override IBinder OnBind(Intent intent)
        {
            //throw new NotImplementedException();
            return null;
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            Timer tm = new Timer();
            tm.Interval = 10 * 1000;
            tm.Enabled = true;
            tm.Elapsed += delegate { Notification(); };
            tm.Start();

            if (intent.Action.Equals("STOP"))
            {
                StopForeground(true);
                StopSelf();

                Toast.MakeText(Application.Context, TT("DV09"), ToastLength.Long).Show();
            }
            else if (intent.Action.Equals("PASS"))
            {
                StopForeground(true);

                ISharedPreferences edpre = GetSharedPreferences("MachineDowntime", FileCreationMode.Private);
                ISharedPreferencesEditor editor = edpre.Edit();
                editor.PutString("machine", lsmc + mc);
                editor.Commit();
            }

            return base.OnStartCommand(intent, flags, startId);
        }

        private void Notification()
        {
            try
            {
                string chuoi = "Data Source=192.168.1.245;Initial Catalog=Maintenance;Integrated Security=False;User ID=sa;Password=007;Connect Timeout=30;Encrypt=False;";
                Connect kn = new Connect(chuoi);
                string user = "", fac = "", line = "";

                ISharedPreferences pre = GetSharedPreferences("MachineDowntime", FileCreationMode.Private);

                string ch2 = pre.GetString("user", "").ToString();
                string ch3 = pre.GetString("line", "").ToString();
                lsmc = pre.GetString("machine", "").ToString();
                int ch5 = pre.GetInt("lg", 0);
                if (ch5 != 0) lg = ch5;

                if (ch2 != "") user = ch2;

                if (ch3.Length > 4) { fac = ch3.Substring(0, 2); line = ch3.Substring(ch3.Length - 2, 2); }

                //PowerManager pm = (PowerManager)GetSystemService(PowerService);
                //bool isScreenOn = pm.IsInteractive; // check if screen is on
                //if (!isScreenOn)
                //{
                //    PowerManager.WakeLock wl = pm.NewWakeLock(WakeLockFlags.ScreenDim | WakeLockFlags.AcquireCausesWakeup, "myApp:notificationLock");
                //    wl.Acquire(3000); //set your time in milliseconds
                //}

                Temp.msg = "Service is running " + fac + "/" + user;
                DataTable login = kn.Doc("select * from DownTimeUserList where ID = '" + user + "' and Levels > 0").Tables[0] ?? new DataTable();
                NgonNgu = kn.Doc("select * from LanguageTable where ItemNO  like 'DV%'").Tables[0];

                if (login.Rows.Count > 0)
                {
                    DataRow r = login.Rows[0];
                    int lv = int.Parse(string.IsNullOrEmpty(r["Levels"].ToString()) ? "0" : r["Levels"].ToString());
                    string area = r["Area"].ToString();

                    if (area == "A")
                    {
                        for (int j = 1; j < 16; j++) area += j.ToString("00") + ",";
                        area += "33,PPA";
                    }
                    if (area == "B")
                    {
                        for (int j = 16; j < 32; j++) area += j.ToString("00") + ",";
                    }

                    DataTable dt = kn.Doc("SELECT McSerialNo,FacLine,OccurTime,datediff(minute,OccurTime,getdate()) TimeWait " +
                                        "FROM DowntimeReport where FacZone = 'A1A" + fac + "' and StartTime is null order by TimeWait desc").Tables[0] ?? new DataTable();

                    string msg = "";
                    int i = 0, run = 0;
                    foreach (DataRow rr in dt.Rows)
                    {
                        DateTime date = DateTime.Parse(rr[2].ToString());
                        string key = rr[0].ToString() + date.ToString("HHmmss");

                        if (!lsmc.Contains(key))
                        {
                            int time = int.Parse(string.IsNullOrEmpty(rr["TimeWait"].ToString()) ? "0" : rr["TimeWait"].ToString());

                            if (lv == 1 && time > 5)
                            {
                                string l = rr["FacLine"].ToString().Substring(rr["FacLine"].ToString().Length - 2, 2);

                                if (area.Contains(l)) { mc += key + "\n"; run++; }
                            }
                            else if (lv == 2 && time > 10)
                            {
                                mc += key + "\n"; run++;
                            }
                            else if (lv >= 10) { mc += key + "\n"; run++; }
                        }

                        i++;
                        msg += i + ". " + rr[0].ToString() +
                                        "\n     " + TT("DV01") + rr[1].ToString() +
                                        "\n     " + TT("DV02") + rr[2].ToString() +
                                        "\n     " + TT("DV03") + rr[3].ToString() +
                                        "\n\n";
                    }


                    if (run > 0)
                    {
                        Temp.msg = "Service is running " + i + " - " + DateTime.Now.ToString("HHmmss");
                        NotificationManager notificationService = (NotificationManager)GetSystemService(NotificationService);
                        Intent t = new Intent(this, typeof(NotificationActivity));
                        t.PutExtra("msg", msg);

                        PendingIntent pendingIntent = PendingIntent.GetActivity(this, MY_REQUEST_CODE, t, PendingIntentFlags.UpdateCurrent);
                        string title = TT("DV04") + " " + i + " " + TT("DV05");

                        if (CreateNotificationChannel())
                            Notification(Resources.GetString(Resource.String.app_name), title, TT("DV06"), 101, pendingIntent);
                    }
                    else StopForeground(true);
                }
                else StopForeground(true);
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
                    .AddAction(STOPSERVICE())
                    .AddAction(PASSSERVICE())
                    .Build();
            notification.Defaults |= NotificationDefaults.Vibrate;
            notification.Defaults |= NotificationDefaults.Sound;

            PowerManager pm = (PowerManager)ApplicationContext.GetSystemService(PowerService);

            WakeLock wl = pm.NewWakeLock(WakeLockFlags.Partial, "TAG");

            wl.Acquire(15000);

            //NotificationManagerCompat notificationManager = NotificationManagerCompat.From(this);
            //notificationManager.Notify(notifId, notification);
            StartForeground(notifId, notification);

            wl.Release();

            Notification.Action STOPSERVICE()
            {
                var restartTimerIntent = new Intent(this, GetType());
                restartTimerIntent.SetAction("STOP");
                var restartTimerPendingIntent = PendingIntent.GetService(this, 0, restartTimerIntent, 0);

                var builder = new Notification.Action.Builder(Resource.Drawable.application,
                                                  TT("DV07"),
                                                  restartTimerPendingIntent);

                return builder.Build();
            }
            Notification.Action PASSSERVICE()
            {
                var restartTimerIntent = new Intent(this, GetType());
                restartTimerIntent.SetAction("PASS");
                var restartTimerPendingIntent = PendingIntent.GetService(this, 0, restartTimerIntent, 0);

                var builder = new Notification.Action.Builder(Resource.Drawable.application,
                                                  TT("DV08"),
                                                  restartTimerPendingIntent);

                return builder.Build();
            }
        }
        private string TT(string code)
        {
            return NgonNgu.Select().Where(x => x[0].ToString() == code).FirstOrDefault()[lg].ToString();
        }
        //public override void OnTaskRemoved(Intent rootIntent)
        //{
        //    Intent restartServiceIntent = new Intent(Application.Context, Class);
        //    restartServiceIntent.SetPackage(Resources.GetString(Resource.String.app_name));

        //    PendingIntent restartServicePendingIntent = PendingIntent.GetService(Application.Context, 1, restartServiceIntent, PendingIntentFlags.OneShot);
        //    AlarmManager alarmService = (AlarmManager)(Application.Context.GetSystemService(AlarmService));
        //    alarmService.Set(AlarmType.ElapsedRealtime, 5 * 1000, restartServicePendingIntent);

        //    base.OnTaskRemoved(rootIntent);
        //}
    }
}