using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using CSDL;
using Java.Lang;
using MachineDowntime;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using static Android.OS.PowerManager;
using Application = Android.App.Application;

[assembly: Dependency(typeof(startServiceAndroid))]
namespace MachineDowntime
{
    public class startServiceAndroid : IStartService
    {
        public void StartForegroundServiceCompat()
        {
            var intent = new Intent(Application.Context, typeof(NewService));
            intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);

            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                Application.Context.StartForegroundService(intent);
            }
            else
            {
                Application.Context.StartService(intent);
            }

        }
    }
    [Service]
    class NewService : Service
    {
        private static int MY_REQUEST_CODE = 100;
        public static string CHANNEL_ID1 = "MCDT", CHANNEL_ID2 = "SCANLOAD";
        public static string CHANNEL_NAME1 = "MCDT Message", CHANNEL_NAME2 = "SCANLOAD Message";
        public static string chuoi = "Data Source=192.168.1.245;Initial Catalog=Maintenance;Integrated Security=False;User ID=prog4;Password=DeS;Connect Timeout=30;Encrypt=False;";
        public static string com = "Data Source=192.168.1.245;Initial Catalog=DtradeProduction;Integrated Security=False;User ID=prog4;Password=DeS;Connect Timeout=30;Encrypt=False;";
        //public static string chuoi = "Data Source=192.168.54.8;Initial Catalog=Maintenance;Integrated Security=False;User ID=sa;Password=Admin@168*;Connect Timeout=30;Encrypt=False;";
        //public static string com = "Data Source=192.168.54.8;Initial Catalog=DtradeProduction;Integrated Security=False;User ID=sa;Password=Admin@168*;Connect Timeout=30;Encrypt=False;";
        string text = "Welcome";
        private string lsmc = "", mc = "";
        Connect kn1, kn2;
        bool loop = true, loop1 = true;

        private DataTable NgonNgu = new DataTable();

        private int lg = 1;
        int c = 0;
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            kn1 = new Connect(chuoi);
            kn2 = new Connect(com);
            if (NgonNgu.Rows.Count > 0) NgonNgu.Rows.Clear();
            NgonNgu = kn1.Doc("select * from LanguageTable where ItemNO  like 'DV%'").Tables[0];

            if (text != "")
            {
                ChayNgam(1, text, text);

                text = "";
            }

            Device.StartTimer(TimeSpan.FromSeconds(20), () =>
            {
                ISharedPreferences pre = GetSharedPreferences("MachineDowntime", FileCreationMode.Private);

                if (c == 0) { BreakDown(pre); c = 1; }
                else if (c == 1) { ScanLoading(pre); c = 2; }
                else { ScanLoadingSPMK(pre); c = 0; }

                return true;
            });

            try
            {
                if (intent.Action != null)
                {
                    if (intent.Action.Equals("STOP"))
                    {
                        StopForeground(true);
                        StopSelf();

                        Toast.MakeText(Application.Context, TT("DV09"), ToastLength.Long).Show();
                    }
                    else if (intent.Action.Equals("PASS"))
                    {
                        loop = false;
                        lsmc += mc;

                        ChayNgam(1, "Waiting for new breakdown", "MCDT " + DateTime.Now.ToString());
                    }
                }
            }
            catch { }

            return StartCommandResult.Sticky;
        }
        private void ScanLoading(ISharedPreferences pre)
        {
            string line = pre.GetString("line", "").ToString();

            try
            {
                DataSet ds = kn2.Doc("exec GetLoadData 13,'" + DateTime.Now.ToString("yyyyMMdd") + "','" + line + "'");

                if (ds.Tables[3].Rows.Count > 0)
                {
                    Intent t = new Intent(this, typeof(ScanLoadActivity));
                    t.PutExtra("Load", 0);

                    PendingIntent pendingIntent = PendingIntent.GetActivity(this, MY_REQUEST_CODE, t, PendingIntentFlags.UpdateCurrent);

                    text = line + " : " + TT("DV10");// + " " + DateTime.Now.ToString("HHmmss");

                    string job = TT("DV10");
                    foreach (DataRow r in ds.Tables[3].Rows)
                    {
                        job += "\n  " + r[0].ToString() + "  : " + r[1].ToString();
                    }

                    if (text != "")
                    {
                        ChayNgam(2, text, job + "\n" + DateTime.Now.ToString(), pendingIntent);

                        text = "";loop1 = true;
                    }
                }
                else if(loop1)
                {
                    ChayNgam(2, text, TT("DT111") + "\n" + DateTime.Now.ToString());

                    loop1 = false;
                }
            }
            catch (System.Exception ex)
            {
                //ChayNgam(2, text, ex.ToString() + "\n" + DateTime.Now.ToString());

                //text = "";
            }
        }
        private void ScanLoadingSPMK(ISharedPreferences pre)
        {
            string line = pre.GetString("line", "").ToString();

            try
            {
                DataSet ds = kn2.Doc("exec GetLoadData 17,'" + DateTime.Now.ToString("yyyyMMdd") + "','" + line + "'");

                if (ds.Tables[2].Rows.Count > 0)
                {
                    Intent t = new Intent(this, typeof(ScanLoadActivity));
                    t.PutExtra("Load", 1);

                    PendingIntent pendingIntent = PendingIntent.GetActivity(this, MY_REQUEST_CODE, t, PendingIntentFlags.UpdateCurrent);

                    text = line + " : " + TT("DV11");// + " " + DateTime.Now.ToString("HHmmss");

                    string job = TT("DV11");
                    foreach (DataRow r in ds.Tables[2].Rows)
                    {
                        job += "\n  " + r[0].ToString() + "   " + r[1].ToString() + "  : " + r[2].ToString();
                    }

                    if (text != "")
                    {
                        ChayNgam(2, text, job + "\n" + DateTime.Now.ToString(), pendingIntent);

                        text = "";
                    }
                }
            }
            catch (System.Exception ex)
            {
                //ChayNgam(2, text, ex.ToString() + "\n" + DateTime.Now.ToString());

                //text = "";
            }
        }
        private void BreakDown(ISharedPreferences pre)
        {
            string user = "", name = "", fac = "", line = ""; mc = "";

            try
            {
                //Connect kn = new Connect(chuoi);
                //DataTable d = kn.Doc("select * from AAData").Tables[0];

                //dt = d.Rows[0][0].ToString();

                //kn.Ghi("truncate table AAData");

                string ch2 = pre.GetString("user", "").ToString();
                string ch3 = pre.GetString("line", "").ToString();

                int ch5 = pre.GetInt("lg", 1);
                if (ch5 != 0) lg = ch5;

                if (ch2 != "") user = ch2;

                if (ch3.Length > 4) { fac = ch3.Substring(0, 2); line = ch3.Substring(ch3.Length - 2, 2); }

                DataTable login = kn1.Doc("select * from DownTimeUserList where ID = '" + user + "' and Levels > 0").Tables[0] ?? new DataTable();

                if (login.Rows.Count > 0)
                {
                    DataRow r = login.Rows[0];
                    int lv = int.Parse(string.IsNullOrEmpty(r["Levels"].ToString()) ? "0" : r["Levels"].ToString());
                    string area = r["Area"].ToString();
                    name = r["Name"].ToString().Contains(" ") ? r["Name"].ToString().Split(" ").Last() : r["Name"].ToString();

                    if (area == "A")
                    {
                        for (int j = 1; j < 16; j++) area += j.ToString("00") + ",";
                        area += "33,PPA";
                    }
                    if (area == "B")
                    {
                        for (int j = 16; j < 32; j++) area += j.ToString("00") + ",";
                    }

                    DataTable dt = kn1.Doc("SELECT McSerialNo,FacLine,OccurTime,datediff(minute,OccurTime,getdate()) TimeWait " +
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
                        Intent t = new Intent(this, typeof(NotificationActivity));
                        t.PutExtra("msg", msg);

                        PendingIntent pendingIntent = PendingIntent.GetActivity(this, MY_REQUEST_CODE, t, PendingIntentFlags.UpdateCurrent);
                        text = TT("DV04") + " " + i + " " + TT("DV05");

                        loop = true;

                        if (text != "")
                        {
                            ChayNgam(1, text, text + "\n" + name + "\n" + DateTime.Now.ToString(), pendingIntent);

                            text = "";
                        }
                    }
                    else if (loop)
                    {
                        loop = false;
                        ChayNgam(1, TT("DV04") + " 0 " + TT("DV05"), TT("DV04") + " 0 " + TT("DV05") + "\n" + name + "\n" + DateTime.Now.ToString());
                    }
                }
                //else ChayNgam("Welcome Level 0 " + user);
            }
            catch (System.Exception ex)
            {

                //text = ex.ToString();

                //ChayNgam(1, text, line + " " + DateTime.Now.ToString());

                //text = "";
            }
        }
        private void ChayNgam(int i, string text, string bigtext, PendingIntent pendingIntent = null)
        {
            RegisterNotificationChannel();

            NotificationCompat.BigTextStyle textStyle = new NotificationCompat.BigTextStyle();

            if (i == 1)
            {
                int notifyId = (int)JavaSystem.CurrentTimeMillis();
                textStyle.BigText(bigtext);
                NotificationCompat.Builder mBuilder = new NotificationCompat.Builder(this, CHANNEL_ID1);
                mBuilder.SetSmallIcon(Resource.Drawable.application)
                        //.SetSubText(data + "  \n " + DateTime.Now.ToString())
                        .SetContentTitle("Breakdown Machine")
                        .SetContentText(text)
                        .SetStyle(textStyle)
                        .SetOngoing(true)
                        .AddAction(STOPSERVICE())
                        .AddAction(PASSSERVICE());

                if (pendingIntent != null) mBuilder.SetContentIntent(pendingIntent);

                if (Build.VERSION.SdkInt < BuildVersionCodes.N)
                {
                    mBuilder.SetContentTitle("Machine Downtime");
                }

                StartForeground(notifyId, mBuilder.Build());

                NotificationCompat.Action STOPSERVICE()
                {
                    var restartTimerIntent = new Intent(this, GetType());
                    restartTimerIntent.SetAction("STOP");
                    var restartTimerPendingIntent = PendingIntent.GetService(this, 0, restartTimerIntent, 0);

                    var builder = new NotificationCompat.Action.Builder(Resource.Drawable.application,
                                                      TT("DV07"),
                                                      restartTimerPendingIntent);

                    return builder.Build();
                }
                NotificationCompat.Action PASSSERVICE()
                {
                    var restartTimerIntent = new Intent(this, GetType());
                    restartTimerIntent.SetAction("PASS");
                    var restartTimerPendingIntent = PendingIntent.GetService(this, 0, restartTimerIntent, 0);

                    var builder = new NotificationCompat.Action.Builder(Resource.Drawable.application,
                                                      TT("DV08"),
                                                      restartTimerPendingIntent);

                    return builder.Build();
                }
            }
            else
            {
                int notifyId = (int)JavaSystem.CurrentTimeMillis();
                textStyle.BigText(bigtext);
                NotificationCompat.Builder mBuilder = new NotificationCompat.Builder(this, CHANNEL_ID2);
                mBuilder.SetSmallIcon(Resource.Drawable.application)
                        //.SetSubText(data + "  \n " + DateTime.Now.ToString())
                        .SetContentTitle("Scan Loading")
                        .SetContentText(text)
                        .SetStyle(textStyle)
                        .SetOngoing(true);

                if (pendingIntent != null) mBuilder.SetContentIntent(pendingIntent);

                if (Build.VERSION.SdkInt < BuildVersionCodes.N)
                {
                    mBuilder.SetContentTitle("Loading Data");
                }

                StartForeground(notifyId, mBuilder.Build());
            }
        }
        private void RegisterNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                NotificationManager mNotificationManager = (NotificationManager)GetSystemService(Context.NotificationService);

                NotificationChannel notificationChannel1 = mNotificationManager.GetNotificationChannel(CHANNEL_ID1);
                if (notificationChannel1 == null)
                {
                    NotificationChannel channel = new NotificationChannel(CHANNEL_ID1,
                            CHANNEL_NAME1, NotificationImportance.High);
                    channel.EnableLights(true);
                    channel.LightColor = Android.Graphics.Color.Blue;
                    channel.LockscreenVisibility = NotificationVisibility.Public;
                    channel.EnableVibration(true);
                    channel.SetVibrationPattern(new long[] { 1000, 1000, 1000, 1000, 1000 });

                    mNotificationManager.CreateNotificationChannel(channel);
                }

                NotificationChannel notificationChannel2 = mNotificationManager.GetNotificationChannel(CHANNEL_ID2);
                if (notificationChannel1 == null)
                {
                    NotificationChannel channel = new NotificationChannel(CHANNEL_ID2,
                            CHANNEL_NAME2, NotificationImportance.High);
                    channel.EnableLights(true);
                    channel.LightColor = Android.Graphics.Color.Blue;
                    channel.LockscreenVisibility = NotificationVisibility.Public;
                    channel.EnableVibration(true);
                    channel.SetVibrationPattern(new long[] { 1000, 1000, 1000, 1000, 1000 });

                    mNotificationManager.CreateNotificationChannel(channel);
                }
            }
        }
        private void CoTheChay()
        {
            Runnable runnable = new Runnable(() =>
            {
                KeyguardManager km = (KeyguardManager)GetSystemService(Context.KeyguardService);
                KeyguardManager.KeyguardLock kl = km.NewKeyguardLock("MyKeyguardLock");
                kl.DisableKeyguard();

                PowerManager pm = (PowerManager)GetSystemService(Context.PowerService);
                WakeLock wakeLock = pm.NewWakeLock(WakeLockFlags.Full | WakeLockFlags.AcquireCausesWakeup | WakeLockFlags.OnAfterRelease, "MyWakeLock");
                wakeLock.Acquire();

                Android.App.AlertDialog.Builder b = new AlertDialog.Builder(Application.Context);

                b.SetMessage(DateTime.Now.ToString());

                AlertDialog alert = b.Create();
                alert.Window.SetType(WindowManagerTypes.ApplicationPanel);
                alert.Window.AddFlags(WindowManagerFlags.KeepScreenOn | WindowManagerFlags.DismissKeyguard | WindowManagerFlags.ShowWhenLocked | WindowManagerFlags.TurnScreenOn);
                alert.Show();
            });
            new Handler(Looper.MainLooper).Post(runnable);
        }
        private string TT(string code)
        {
            return NgonNgu.Select().Where(x => x[0].ToString() == code).FirstOrDefault()[lg].ToString();
        }
    }
}