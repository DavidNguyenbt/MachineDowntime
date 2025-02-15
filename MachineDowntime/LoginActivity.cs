using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Android.Content;
using System;
using CSDL;
using Android.Views;
using System.Linq;
using Android.Content.PM;
using System.Data;
using UpdateManager;
using Android.Support.V4.Content;
using Android;
using System.Threading.Tasks;
using AndroidScreenStretching;
using Android.Text.Method;
using System.IO;
using static Android.Provider.Settings;
using Android.Support.V4.Hardware.Fingerprint;
using Android.Hardware.Fingerprints;
using Android.Content.Res;
using Orientation = Android.Widget.Orientation;
using System.Net.Http;
using Xamarin.Forms;
using Button = Android.Widget.Button;
using RelativeLayout = Android.Widget.RelativeLayout;
using CheckBox = Android.Widget.CheckBox;
using Application = Android.App.Application;
using Xamarin.Essentials;
using Android.Support.V4.App;
using System.Threading;
using System.Timers;

namespace MachineDowntime       //Theme = "@style/Theme.AppCompat.Light.NoActionBar"
{
    [Activity(Label = "MCDT & ScanLoad", Theme = "@style/Theme.AppCompat.Light.NoActionBar", Icon = "@drawable/application", LaunchMode = LaunchMode.SingleTop, MainLauncher = true, ScreenOrientation = ScreenOrientation.SensorPortrait, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)]
    public class LoginActivity : Activity
    {
        CheckBox chlg;
        EditText edten, edmk, edfac, edline;
        TextView txtserver, txttitle, txtversion;
        Button btexit, btlogin;
        RelativeLayout layout;
        string[] language = { "VN", "EN", "KHM", "THAI" };
        Connect kn, cn;
        bool tt = true;
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.login);

            Xamarin.Forms.Forms.Init(this, savedInstanceState);
            RequestInit.Init(this);

            Window.AddFlags(WindowManagerFlags.Fullscreen);
            Window.AddFlags(WindowManagerFlags.TranslucentNavigation);
            //Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(SystemUiFlags.HideNavigation | SystemUiFlags.Fullscreen | SystemUiFlags.LowProfile | SystemUiFlags.ImmersiveSticky);

            chlg = FindViewById<CheckBox>(Resource.Id.chklg);

            edten = FindViewById<EditText>(Resource.Id.edten);
            edmk = FindViewById<EditText>(Resource.Id.edmk);
            edfac = FindViewById<EditText>(Resource.Id.edfac);
            edline = FindViewById<EditText>(Resource.Id.edline);

            txtserver = FindViewById<TextView>(Resource.Id.textView6);
            txttitle = FindViewById<TextView>(Resource.Id.textView1);
            txtversion = FindViewById<TextView>(Resource.Id.txtversion);

            btexit = FindViewById<Button>(Resource.Id.btexit);
            btlogin = FindViewById<Button>(Resource.Id.btlogin);

            layout = FindViewById<RelativeLayout>(Resource.Id.layoutlogin);

            Temp.metric = Resources.DisplayMetrics;

            //string ping = "192.168.10.245";
            bool ex = await NetworkHelper.CanPing(Temp.ping);
            if (ex) Run();
            else
            {
                Android.App.AlertDialog.Builder b = new Android.App.AlertDialog.Builder(this);
                Dialog d = new Dialog(this);
                string[] item = { "A1A - VietNam", "TRX - Thailand", "TAC - Cambodia", "Outside Network" };
                b.SetSingleChoiceItems(item, -1, (s, a) =>
                {
                    switch(a.Which)
                    {
                        case 0:
                            Run();
                            break;
                        case 1:
                            Temp.chuoi = Temp.TRX_chuoi;
                            Temp.com = Temp.TRX_com;
                            Run();
                            break;
                        case 2:
                            Temp.chuoi = Temp.TAC_chuoi;
                            Temp.com = Temp.TAC_com;
                            Run();
                            break;
                        case 3:
                            Run(false);
                            break;
                    }

                    d.Dismiss();
                });

                b.SetCancelable(false);
                d = b.Create();

                d.Show();
            }

            void Run(bool update = true)
            {
                try
                {
                    ISharedPreferences pre = GetSharedPreferences("MachineDowntime", FileCreationMode.Private);

                    if (update)
                    {
                        string ch0 = pre.GetString("server1", "").ToString();
                        if (ch0 != "") Temp.chuoi = ch0;

                        string ch1 = pre.GetString("server2", "").ToString();
                        if (ch1 != "") Temp.com = ch1;
                    }
                    else
                    {
                        Temp.chuoi = Temp.out_chuoi;
                        Temp.com = Temp.out_com;
                    }

                    kn = new Connect(Temp.chuoi);

                    string ch2 = pre.GetString("user", "").ToString();
                    if (ch2 != "") edten.Text = ch2;

                    string ch3 = pre.GetString("fac", "").ToString();
                    if (ch3 != "") edfac.Text = ch3;

                    string ch4 = pre.GetString("line", "").ToString();
                    if (ch4 != "") edline.Text = ch4;

                    Temp.Newlg = 2;
                    int ch5 = pre.GetInt("lg", 0);
                    if (ch5 != 0) Temp.Newlg = ch5;
                    chlg.Text = language[Temp.Newlg - 1];
                    chlg.Checked = true;

                    Temp.NgonNgu = kn.Doc("select * from LanguageTable").Tables[0];

                    DataRow row = kn.Doc("select * from LanguageTable where ItemNO = 'UPDATE'").Tables[0].Rows[0];

                    Temp.Link = row[1].ToString();
                    if (Temp.Link != "") Temp.AppName = Temp.Link.Split('/').Last();

                    string obli = row[2].ToString();
                    if (obli == "1") Temp.Obli = true;
                    else Temp.Obli = false;

                    cn = new Connect(Temp.com);
                    Temp.FacLine = kn.Doc("exec GetData 14,'cho','',''").Tables[0];
                    string qry = update ? "select * from InlineQCSystem where STT = 8" : "select * from InlineQCSystem where STT = 105";
                    Temp.url = cn.Doc(qry).Tables[0].Rows[0][0].ToString();
                }
                catch (Exception ex)
                {
                    Toast.MakeText(this, ex.ToString(), ToastLength.Long).Show();
                }

                edfac.Focusable = false;
                edline.Focusable = false;

                edfac.Click += delegate
                {
                    try
                    {
                        edline.Text = "";
                        Android.App.AlertDialog.Builder b = new Android.App.AlertDialog.Builder(this);
                        var fac = Temp.FacLine.Select().Select(s => s[0].ToString()).Distinct().ToArray();//Temp.FacLine.Select().Select(dr => dr[0].ToString()[dr[0].ToString().IndexOf("F")..]).Distinct().ToArray();

                        b.SetSingleChoiceItems(fac, -1, (s, a) =>
                        {
                            Dialog d = s as Dialog;

                            edfac.Text = fac[a.Which];

                            d.Dismiss();
                        });
                        b.SetCancelable(false);
                        b.Create().Show();
                    }
                    catch { }
                };
                edline.Click += delegate
                {
                    try
                    {
                        if (edfac.Text != "")
                        {
                            string fac = edfac.Text.Length > 2 ? edfac.Text.Substring(3) : edfac.Text;
                            var line = Temp.FacLine.Select("FacZone like '%" + edfac.Text + "%'").Select(l => l[1].ToString()).Distinct().ToArray();
                            line = line.Concat(new string[] { fac + "PPA", fac + "JUMPER" }).ToArray();
                            Android.App.AlertDialog.Builder b = new Android.App.AlertDialog.Builder(this);

                            b.SetSingleChoiceItems(line, -1, (s, a) =>
                            {
                                Dialog d = s as Dialog;

                                edline.Text = line[a.Which];

                                d.Dismiss();
                            });
                            b.SetCancelable(false);
                            b.Create().Show();
                        }
                    }
                    catch { }
                };
                btexit.Click += delegate { System.Diagnostics.Process.GetCurrentProcess().Kill(); };
                btexit.LongClick += delegate
                {
                    StrictMode.VmPolicy.Builder builder = new StrictMode.VmPolicy.Builder();
                    StrictMode.SetVmPolicy(builder.Build());

                    string file = Android.OS.Environment.ExternalStorageDirectory + "/download/" + Temp.AppName;
                    Java.IO.File apkFile = new Java.IO.File(file);
                    Android.Net.Uri uri = Android.Net.Uri.FromFile(apkFile);//Android.Net.Uri.FromFile(apkFile);


                    //Intent intent = new Intent(Intent.ActionPackageAdded);
                    ////intent.SetAction(Intent.ActionInstallPackage);
                    //intent.SetDataAndType(uri, "application/vnd.android.package-archive");
                    //StartActivity(intent);



                    //Intent webIntent = new Intent(Intent.ActionInstallPackage);//Intent.ACTION_VIEWApplication.Context, Class);//this, Class);//
                    //webIntent.SetDataAndType(uri, "application/vnd.android.package-archive");
                    ////webIntent.SetAction("com.example.android.apis.content.SESSION_API_PACKAGE_INSTALLED");
                    //webIntent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.GrantPersistableUriPermission);
                    //webIntent.PutExtra(Intent.ExtraNotUnknownSource, true);
                    //Application.Context.StartActivity(webIntent);

                    PackageManager.CanRequestPackageInstalls();
                    var packageInstaller = PackageManager.PackageInstaller;
                    var sessionParams = new PackageInstaller.SessionParams(PackageInstallMode.InheritExisting);
                    int sessionId = packageInstaller.CreateSession(sessionParams);
                    var session = packageInstaller.OpenSession(sessionId);

                    AddApkToInstallSession(uri, session);

                    // Create an install status receiver.
                    Intent intent = new Intent(this, Class);
                    intent.SetAction("com.example.android.apis.content.SESSION_API_PACKAGE_INSTALLED");
                    PendingIntent pendingIntent = PendingIntent.GetActivity(this, 3439, intent, PendingIntentFlags.UpdateCurrent);
                    IntentSender statusReceiver = pendingIntent.IntentSender;

                    // Commit the session (this will start the installation workflow).
                    session.Commit(statusReceiver);
                    //session.Close();

                    //Finish();
                    Toast.MakeText(this, "Updated !!!", ToastLength.Long).Show();
                };
                btlogin.Click += delegate
                {
                    try
                    {
                        if (edten.Text == "")
                        {
                            Toast.MakeText(this, Temp.TT("DT37"), ToastLength.Long).Show();
                            edten.RequestFocus();
                        }
                        else if (edline.Text == "")
                        {
                            Toast.MakeText(this, Temp.TT("DT38"), ToastLength.Long).Show();
                            edten.RequestFocus();
                        }
                        else
                        {
                            if (Temp.Login.Rows.Count > 0) Temp.Login.Rows.Clear();

                            Temp.Login = cn.Doc("exec GetDataFromQuery 30,'" + edfac.Text + "','" + edline.Text + "','" + edten.Text + "','" + edmk.Text + "','cho'").Tables[0];

                            if (Temp.Login.Rows.Count == 0)
                            {
                                Toast.MakeText(this, Temp.TT("DT39"), ToastLength.Long).Show();
                                edmk.Text = "";
                                edten.RequestFocus();
                            }
                            else
                            {
                                Toast.MakeText(this, Temp.TT("DT40"), ToastLength.Long).Show();

                                DataRow r = Temp.Login.Rows[0];

                                ISharedPreferences edpre = GetSharedPreferences("MachineDowntime", FileCreationMode.Private);
                                ISharedPreferencesEditor editor = edpre.Edit();
                                editor.PutString("user", r["ID"].ToString());
                                editor.PutString("fac", edfac.Text);
                                editor.PutString("line", edline.Text);
                                editor.PutInt("lg", Temp.Newlg);
                                editor.Commit();

                                Temp.user = r["ID"].ToString();
                                Temp.dept = r["Section"].ToString();
                                Temp.facline = edline.Text;
                                Temp.fac = edfac.Text;
                                edmk.Text = "";

                                Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);
                                string[] it = { "MACHINE LOCATION", "MACHINE DOWNTIME", "LOADING DATA", /*"ACC/INPUT DOWNTIME",*/"TPM CHECKLIST", "HR CHECKLIST" };
                                b.SetSingleChoiceItems(it, -1, (s, a) =>
                                {
                                    Dialog d = s as Dialog;

                                    if (a.Which == 0)
                                    {
                                        if (Temp.Location.Rows.Count > 0) Temp.Location.Rows.Clear();
                                        Temp.Location = kn.Doc("select * from location").Tables[0];

                                        Intent location = new Intent(this, typeof(LocationActivity));
                                        StartActivity(location);
                                    }
                                    else if (a.Which == 1)
                                    {
                                        //DependencyService.Get<IStartService>().StartForegroundServiceCompat();

                                        Intent main = new Intent(this, typeof(MainActivity));
                                        StartActivity(main);
                                    }
                                    else if (a.Which == 2)
                                    {
                                        //DependencyService.Get<IStartService>().StartForegroundServiceCompat();

                                        Intent scan = new Intent(this, typeof(ScanLoadActivity));
                                        StartActivity(scan);
                                    }
                                    //else if (a.Which == 3)
                                    //{
                                    //    //DependencyService.Get<IStartService>().StartForegroundServiceCompat();

                                    //    Intent scan = new Intent(this, typeof(InputDowntimeActivity));
                                    //    StartActivity(scan);
                                    //}
                                    else if (a.Which == 3)
                                    {
                                        //DependencyService.Get<IStartService>().StartForegroundServiceCompat();

                                        Intent scan = new Intent(this, typeof(ChecklistActivity));
                                        StartActivity(scan);
                                    }
                                    else if (a.Which == 4)
                                    {
                                        //DependencyService.Get<IStartService>().StartForegroundServiceCompat();

                                        Intent scan = new Intent(this, typeof(AssetChecklistActivity));
                                        StartActivity(scan);
                                    }

                                    d.Dismiss();
                                });

                                b.Create().Show();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Toast.MakeText(this, Temp.TT("DT41") + ex.ToString(), ToastLength.Long).Show();
                        edmk.Text = "";
                    }
                };
                btlogin.LongClick += delegate { Toast.MakeText(this, "Density: " + Temp.metric.Density + "|DensityDPI: " + Temp.metric.Density + "|Height DPI: " + Temp.metric.HeightPixels + "|Width DPI: " + Temp.metric.WidthPixels + "|Y DPI: " + Temp.metric.Ydpi + "|X DPI: " + Temp.metric.Xdpi + "|Scale: " + Temp.metric.ScaledDensity, ToastLength.Long).Show(); };
                chlg.Click += Chlg_Click;
                txtserver.Click += delegate
                {
                    try
                    {
                        Android.App.AlertDialog.Builder l = new AlertDialog.Builder(this);

                        EditText ed = new EditText(this)
                        {
                            Hint = "INPUT PASSWORD",//Temp.NgonNgu.Select().Where(x => x[0].ToString() == "DT17").FirstOrDefault()[Temp.Newlg].ToString(),
                            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                            InputType = Android.Text.InputTypes.TextVariationPassword,
                            TransformationMethod = new PasswordTransformationMethod()
                        };

                        l.SetView(ed);
                        l.SetPositiveButton("NEXT", (ss, aa) =>  //Temp.NgonNgu.Select().Where(x => x[0].ToString() == "DT18").FirstOrDefault()[Temp.Newlg].ToString()
                        {
                            if (ed.Text == "pro123")
                            {
                                try
                                {
                                    Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);

                                    LinearLayout l1 = new LinearLayout(this)
                                    {
                                        Orientation = Orientation.Vertical
                                    };
                                    TextView txt1 = new TextView(this)
                                    {
                                        Text = "Machine Server"     //Temp.NgonNgu.Select().Where(x => x[0].ToString() == "DT13").FirstOrDefault()[Temp.Newlg].ToString()
                                    };
                                    EditText ed1 = new EditText(this)
                                    {
                                        Text = Temp.chuoi
                                    };
                                    TextView txt2 = new TextView(this)
                                    {
                                        Text = "Scan & Pack Server"     //Temp.NgonNgu.Select().Where(x => x[0].ToString() == "DT14").FirstOrDefault()[Temp.Newlg].ToString()
                                    };
                                    EditText ed2 = new EditText(this)
                                    {
                                        Text = Temp.com
                                    };

                                    l1.AddView(txt1); l1.AddView(ed1); l1.AddView(txt2); l1.AddView(ed2);

                                    b.SetPositiveButton("SAVE", (s, a) =>       //Temp.NgonNgu.Select().Where(x => x[0].ToString() == "DT15").FirstOrDefault()[Temp.Newlg].ToString()
                                    {
                                        Temp.chuoi = ed1.Text; Temp.com = ed2.Text; kn = new Connect(Temp.chuoi);

                                        ISharedPreferences edpre = GetSharedPreferences("MachineDowntime", FileCreationMode.Private);
                                        ISharedPreferencesEditor editor = edpre.Edit();
                                        editor.PutString("server1", Temp.chuoi);
                                        editor.PutString("server2", Temp.com);
                                        editor.Commit();
                                    });
                                    b.SetNegativeButton("EXIT", (s, a) =>       //Temp.NgonNgu.Select().Where(x => x[0].ToString() == "DT16").FirstOrDefault()[Temp.Newlg].ToString()
                                    {

                                    });

                                    b.SetCancelable(false);
                                    b.SetView(l1);

                                    b.Create().Show();
                                }
                                catch (Exception ex)
                                {
                                    Toast.MakeText(this, ex.ToString(), ToastLength.Long).Show();
                                }
                            }
                            else Toast.MakeText(this, Temp.NgonNgu.Select().Where(x => x[0].ToString() == "DT19").FirstOrDefault()[Temp.Newlg].ToString(), ToastLength.Long).Show();
                        });

                        l.Create().Show();
                    }
                    catch (Exception ex)
                    {
                        Toast.MakeText(this, ex.ToString(), ToastLength.Long).Show();
                    }
                };
                txtserver.LongClick += async delegate
                {
                    try
                    {
                        StrictMode.VmPolicy.Builder builder = new StrictMode.VmPolicy.Builder();
                        StrictMode.SetVmPolicy(builder.Build());

                        await Task.Run(() => { SetPermision(); });

                        string message1 = Temp.NgonNgu.Select().Where(x => x[0].ToString() == "DT09").FirstOrDefault()[Temp.Newlg].ToString();
                        string message2 = Temp.NgonNgu.Select().Where(x => x[0].ToString() == "DT10").FirstOrDefault()[Temp.Newlg].ToString();
                        string button1 = Temp.NgonNgu.Select().Where(x => x[0].ToString() == "DT11").FirstOrDefault()[Temp.Newlg].ToString();
                        string button2 = Temp.NgonNgu.Select().Where(x => x[0].ToString() == "DT12").FirstOrDefault()[Temp.Newlg].ToString();

                        Temp.OnUpdate(true, new Android.App.AlertDialog.Builder(this), "Machine Downtime", message1, message2, button1, button2, this);
                    }
                    catch { }
                };
                layout.LongClick += async delegate
                {
                    try
                    {
                        Toast.MakeText(this, "Start", ToastLength.Long).Show();
                        string file = Android.OS.Environment.ExternalStorageDirectory + "/download/" + Temp.AppName;

                        await Launcher.OpenAsync(new OpenFileRequest
                        {

                            File = new ReadOnlyFile(file)

                        });

                        Toast.MakeText(this, "Finish", ToastLength.Long).Show();
                    }
                    catch (Exception ex)
                    {
                        Toast.MakeText(this, ex.ToString(), ToastLength.Long).Show();
                    }
                };

                if (update) CheckUpdate();

                txtversion.Text = Temp.version;

                SetLanguage();
            }
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();

            //ShowNotification("App closed");
        }
        private void ShowNotification(string status)
        {
            // Create an intent for the main activity
            var intent = new Intent(this, typeof(LoginActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.OneShot);

            // Create a notification builder
            var notificationBuilder = new NotificationCompat.Builder(this)
                .SetSmallIcon(Resource.Drawable.application)
                .SetContentTitle("My App")
                .SetContentText(status)
                .SetAutoCancel(true)
                .SetContentIntent(pendingIntent);

            // Create a notification manager
            var notificationManager = NotificationManagerCompat.From(this);

            // Show the notification
            notificationManager.Notify(0, notificationBuilder.Build());
        }
        void WebUpdateApk()
        {
            Action update = async () =>
            {
                if (PackageManager.CanRequestPackageInstalls() == true)
                {
                    var packageInstaller = PackageManager.PackageInstaller;
                    var sessionParams = new PackageInstaller.SessionParams(PackageInstallMode.FullInstall);
                    int sessionId = packageInstaller.CreateSession(sessionParams);
                    var session = packageInstaller.OpenSession(sessionId);

                    try
                    {
                        //var url = "http://mywebsite.com/getapk.php?apk=myapp.apk";
                        var httpResponse = await new HttpClient().GetAsync(Temp.Link, HttpCompletionOption.ResponseContentRead);
                        try
                        {
                            if (httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                Stream input = await httpResponse.Content.ReadAsStreamAsync();
                                try
                                {
                                    var packageInSession = session.OpenWrite("package", 0, -1);
                                    try
                                    {
                                        if (input != null)
                                        {
                                            input.CopyTo(packageInSession);
                                        }
                                        else
                                        {
                                            throw new Exception("Inputstream is null");
                                        }
                                    }
                                    finally
                                    {
                                        packageInSession.Close();
                                    }
                                }
                                finally
                                {
                                    input.Close();
                                }
                            }
                        }
                        catch { }
                        finally
                        {
                            httpResponse.Dispose();
                        }
                    }
                    catch { }

                    Intent intent = new Intent(Application.Context, Class);
                    intent.SetAction("SESSION_API_PACKAGE_INSTALLED");
                    PendingIntent pendingIntent = PendingIntent.GetActivity(this, 0, intent, 0);
                    IntentSender statusReceiver = pendingIntent.IntentSender;
                    session.Commit(statusReceiver);
                }
                else
                {
                    Toast.MakeText(this, "No Package Installer Permission", ToastLength.Long).Show();
                }
            };
            if (PackageManager.CanRequestPackageInstalls() == false)
            {
                //RestartActivity = update;
                StartActivity(new Intent(Android.Provider.Settings.ActionManageUnknownAppSources,
                    Android.Net.Uri.Parse("package:" + Android.App.Application.Context.PackageName)));
            }
            else update();
        }
        protected override void OnNewIntent(Intent intent)
        {
            Bundle extras = intent.Extras;
            if ("SESSION_API_PACKAGE_INSTALLED".Equals(intent.Action))
            {
                var status = extras.GetInt(PackageInstaller.ExtraStatus);
                var message = extras.GetString(PackageInstaller.ExtraStatusMessage);
                switch (status)
                {
                    case (int)PackageInstallStatus.PendingUserAction:
                        // Ask user to confirm the installation
                        var confirmIntent = (Intent)extras.Get(Intent.ExtraIntent);
                        StartActivity(confirmIntent);
                        break;
                    case (int)PackageInstallStatus.Success:
                        //TODO: Handle success
                        break;
                    case (int)PackageInstallStatus.Failure:
                    case (int)PackageInstallStatus.FailureAborted:
                    case (int)PackageInstallStatus.FailureBlocked:
                    case (int)PackageInstallStatus.FailureConflict:
                    case (int)PackageInstallStatus.FailureIncompatible:
                    case (int)PackageInstallStatus.FailureInvalid:
                    case (int)PackageInstallStatus.FailureStorage:
                        //TODO: Handle failures
                        break;
                }
                Toast.MakeText(this, "OnNewIntent status = " + status.ToString(), ToastLength.Long).Show();
            }
        }
        private void AddApkToInstallSession(Android.Net.Uri apkUri, PackageInstaller.Session session)
        {
            //using (var input = ContentResolver.OpenInputStream(apkUri))//var input = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            //{
            //    using (var packageInSession = session.OpenWrite(Temp.AppName, 0, -1))
            //    {
            //        input.CopyTo(packageInSession);
            //        session.Fsync(packageInSession);
            //        packageInSession.Close();
            //    }
            //    input.Close();
            //}

            var packageInSession = session.OpenWrite("package", 0, -1);
            var input = ContentResolver.OpenInputStream(apkUri);

            try
            {
                if (input != null)
                {
                    input.CopyTo(packageInSession);
                }
                else
                {
                    throw new Exception("Inputstream is null");
                }
            }
            finally
            {
                packageInSession.Close();
                input.Close();
            }

            //That this is necessary could be a Xamarin bug.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            //That this is necessary could be a Xamarin bug.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        private void SetPermision()
        {
            Permission read = ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.ReadExternalStorage);
            Permission write = ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.WriteExternalStorage);
            Permission sv = ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.InstantAppForegroundService);
            Permission permissionResult = ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.UseFingerprint);
            Permission alert = ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.SystemAlertWindow);

            if (read != Permission.Granted) RequestPermissions(new string[] { Manifest.Permission.ReadExternalStorage, }, 0);
            if (write != Permission.Granted) RequestPermissions(new string[] { Manifest.Permission.WriteExternalStorage, }, 0);
            if (sv != Permission.Granted) RequestPermissions(new string[] { Manifest.Permission.InstantAppForegroundService, }, 0);
            if (permissionResult != Permission.Granted) RequestPermissions(new string[] { Manifest.Permission.UseFingerprint, }, 0);
            if (alert != Permission.Granted) RequestPermissions(new string[] { Manifest.Permission.SystemAlertWindow, }, 0);
        }

        private async void CheckUpdate()
        {
            try
            {
                StrictMode.VmPolicy.Builder builder = new StrictMode.VmPolicy.Builder();
                StrictMode.SetVmPolicy(builder.Build());

                await Task.Run(() => { SetPermision(); });

                string message1 = Temp.NgonNgu.Select().Where(x => x[0].ToString() == "DT09").FirstOrDefault()[Temp.Newlg].ToString();
                string message2 = Temp.NgonNgu.Select().Where(x => x[0].ToString() == "DT10").FirstOrDefault()[Temp.Newlg].ToString();
                string button1 = Temp.NgonNgu.Select().Where(x => x[0].ToString() == "DT11").FirstOrDefault()[Temp.Newlg].ToString();
                string button2 = Temp.NgonNgu.Select().Where(x => x[0].ToString() == "DT12").FirstOrDefault()[Temp.Newlg].ToString();

                Temp.OnUpdate(false, new Android.App.AlertDialog.Builder(this), "Machine Downtime", message1, message2, button1, button2, this);
            }
            catch { }
        }

        private void CheckVersion()
        {
            try
            {
                string file = Android.OS.Environment.ExternalStorageDirectory + "/download/" + Temp.AppName;

                PackageManager manager = PackageManager;
                PackageInfo info = manager.GetPackageInfo(PackageName, 0);
                PackageInfo info1 = manager.GetPackageArchiveInfo(file, 0);

                if (info1.VersionCode > info.VersionCode)
                {
                    Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);

                    b.SetMessage("Ứng dụng đã lỗi thời, cần cài đặt mới ...");
                    b.SetPositiveButton("OK", (s, a) =>
                    {
                        Java.IO.File apkFile = new Java.IO.File(file);
                        Android.Net.Uri uri = Android.Net.Uri.FromFile(apkFile);//Android.Net.Uri.FromFile(apkFile);

                        Intent webIntent = new Intent(Intent.ActionInstallPackage);//Intent.ACTION_VIEW
                        webIntent.SetDataAndType(uri, "application/vnd.android.package-archive");
                        webIntent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.GrantPersistableUriPermission);
                        webIntent.PutExtra(Intent.ExtraNotUnknownSource, true);
                        Application.Context.StartActivity(webIntent);
                    });

                    b.SetCancelable(false);
                    b.Create().Show();
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "Check Version : " + ex.ToString(), ToastLength.Long).Show();
            }
        }

        private void Chlg_Click(object sender, EventArgs e)
        {
            try
            {
                Temp.Oldlg = Temp.Newlg;
                Android.App.AlertDialog.Builder b = new Android.App.AlertDialog.Builder(this);

                b.SetSingleChoiceItems(language, Temp.Newlg - 1, (s, a) =>
                    {
                        Dialog d = s as Dialog;

                        Temp.Newlg = a.Which + 1;
                        chlg.Text = language[a.Which];
                        chlg.Checked = true;

                        SetLanguage();

                        d.Dismiss();
                    });

                b.Create().Show();
            }
            catch { }
        }

        private void SetLanguage()
        {
            try
            {
                if (Temp.Newlg != Temp.Oldlg)
                {
                    for (int i = 0; i < layout.ChildCount; i++)
                    {
                        Android.Views.View v = layout.GetChildAt(i);

                        if (tt)
                        {
                            ViewGroup.MarginLayoutParams para = (ViewGroup.MarginLayoutParams)v.LayoutParameters;

                            para.SetMargins(LayoutRequest.Widht(para.LeftMargin), LayoutRequest.Height(para.TopMargin), LayoutRequest.Widht(para.RightMargin), LayoutRequest.Height(para.BottomMargin));

                            if (para.Width > 0) para.Width = LayoutRequest.Widht(para.Width);
                            if (para.Height > 0) para.Height = LayoutRequest.Height(para.Height);

                            v.LayoutParameters = para;
                        }

                        if (v.GetType() == typeof(TextView))
                        {
                            try
                            {
                                TextView vv = v as TextView;

                                DataRow r = Temp.NgonNgu.Select().Where(d => d[Temp.Oldlg].ToString() == vv.Text).FirstOrDefault() as DataRow;

                                vv.Text = r[Temp.Newlg].ToString();

                                if (tt) vv.TextSize = LayoutRequest.TextSize(vv.TextSize);
                            }
                            catch { }
                        }
                        else if (v.GetType() == typeof(Button))
                        {
                            try
                            {
                                Button vv = v as Button;

                                DataRow r = Temp.NgonNgu.Select().Where(d => d[Temp.Oldlg].ToString() == vv.Text).FirstOrDefault() as DataRow;

                                vv.Text = r[Temp.Newlg].ToString();

                                if (tt) vv.TextSize = LayoutRequest.TextSize(vv.TextSize);
                            }
                            catch { }
                        }
                        else if (v.GetType() == typeof(EditText))
                        {
                            try
                            {
                                EditText vv = v as EditText;

                                if (tt) vv.TextSize = LayoutRequest.TextSize(vv.TextSize);
                            }
                            catch { }
                        }
                    }
                }
                tt = false;
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "Set Language failed !!!" + ex.ToString(), ToastLength.Long).Show();
            }
        }
    }
}