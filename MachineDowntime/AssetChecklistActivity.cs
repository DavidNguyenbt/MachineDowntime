using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CSDL;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using Xamarin.Essentials;
using ZXing.Mobile;
using static AndroidX.Navigation.Navigator;

namespace MachineDowntime
{
    [Activity(Theme = "@style/Theme.AppCompat.Light.NoActionBar.Fullscreen", ScreenOrientation = ScreenOrientation.Portrait, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)]
    public class AssetChecklistActivity : Activity
    {
        TextView txtname, txtmcno, txtmctype, txtdate;
        Button btscan, btsave, btallgood, btallbad, btconfig;
        ListView lschecklist;
        RadioButton weekly, monthly, half_yearly, yearly;
        Connect kn;
        CheckBox chk, chklg;
        RelativeLayout layout;
        string q = "Weekly";
        MobileBarcodeScanner scanner;
        DataTable Data = new DataTable();
        int w = 50, t = 20;
        bool en = false;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.assetchecklist);

            MobileBarcodeScanner.Initialize(Application);
            scanner = new MobileBarcodeScanner();

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            kn = new Connect(Temp.chuoi);

            layout = FindViewById<RelativeLayout>(Resource.Id.layoutchkl);

            txtname = FindViewById<TextView>(Resource.Id.txtname); txtname.Text = Temp.Login.Rows[0]["Name"].ToString();
            txtmcno = FindViewById<TextView>(Resource.Id.txtmcno); txtmcno.Text = "";
            txtmctype = FindViewById<TextView>(Resource.Id.txtmctype); txtmctype.Text = "";
            txtdate = FindViewById<TextView>(Resource.Id.txtdate);

            btscan = FindViewById<Button>(Resource.Id.btscan);
            btsave = FindViewById<Button>(Resource.Id.btsave); btsave.Visibility = ViewStates.Gone;
            btallgood = FindViewById<Button>(Resource.Id.btallgood);
            btallbad = FindViewById<Button>(Resource.Id.btallbad);
            btconfig = FindViewById<Button>(Resource.Id.btconfig);

            chk = FindViewById<CheckBox>(Resource.Id.checkBox1);
            chklg = FindViewById<CheckBox>(Resource.Id.chklg);

            if (Temp.Newlg == 2)
            {
                en = true;
                chklg.Checked = en;
            }
            chklg.CheckedChange += delegate { if (chklg.Checked) en = true; else en = false; LoadList(); };

            lschecklist = FindViewById<ListView>(Resource.Id.lschecklist);

            weekly = FindViewById<RadioButton>(Resource.Id.q1); weekly.CheckedChange += delegate { if (weekly.Checked) { q = weekly.Text; SetQ(); } };
            monthly = FindViewById<RadioButton>(Resource.Id.q2); monthly.CheckedChange += delegate { if (monthly.Checked) { q = monthly.Text; SetQ(); } };
            half_yearly = FindViewById<RadioButton>(Resource.Id.q3); half_yearly.CheckedChange += delegate { if (half_yearly.Checked) { q = half_yearly.Text; SetQ(); } };
            yearly = FindViewById<RadioButton>(Resource.Id.q4); yearly.CheckedChange += delegate { if (yearly.Checked) { q = yearly.Text; SetQ(); } };

            void SetQ()
            {
                Toast.MakeText(this, q, ToastLength.Long).Show();
                LoadData();
            }

            Timer tm = new Timer();
            tm.Interval = 1000;
            tm.Enabled = true;
            tm.Elapsed += delegate
            {
                RunOnUiThread(() =>
                {
                    txtdate.Text = DateTime.Now.ToString("G");
                });
            };

            btscan.Click += async delegate
            {
                scanner.AutoFocus();
                var result = await scanner.Scan(new MobileBarcodeScanningOptions { UseNativeScanning = true });

                HandleScanResultLogin(result);
            };
            btscan.LongClick += delegate
            {
                Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);

                EditText txt = new EditText(this);
                txt.InputType = Android.Text.InputTypes.TextFlagCapCharacters;

                b.SetPositiveButton("OK", (s, a) =>
                {
                    if (txt.Text != "")
                    {
                        ReadMCNo(txt.Text);
                    }
                });

                b.SetView(txt);
                b.Create().Show();
            };
            lschecklist.ItemClick += (s, a) =>
            {
                if (a.Position > 0)
                {
                    string[] it = { Temp.TT("DT128"), Temp.TT("DT129"), Temp.TT("DT130"), Temp.TT("DT132") }; //"Good", "Bad", "Input remark"

                    Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);
                    Dialog d = new Dialog(this);

                    b.SetSingleChoiceItems(it, -1, async (s, aa) =>
                    {
                        DataRow r = Data.Rows[a.Position - 1];

                        if (aa.Which == 2)
                        {
                            Android.App.AlertDialog.Builder bb = new AlertDialog.Builder(this);
                            EditText ed = new EditText(this) { Hint = Temp.TT("DT131"), LayoutParameters = new ViewGroup.LayoutParams(300, ViewGroup.LayoutParams.WrapContent) };//"Input remark here"

                            bb.SetPositiveButton("OK", (s1, a1) =>
                            {
                                r["Remark"] = ed.Text;

                                Data.AcceptChanges();

                                LoadList();

                                btsave.Visibility = ViewStates.Visible;

                                d.Dismiss();
                            });
                            bb.SetNeutralButton("ALL", (s1, a1) =>
                            {
                                foreach (DataRow dr in Data.Rows) dr["Remark"] = ed.Text;

                                Data.AcceptChanges();

                                LoadList();

                                btsave.Visibility = ViewStates.Visible;

                                d.Dismiss();
                            });

                            bb.SetView(ed);
                            bb.Create().Show();
                        }
                        else if (aa.Which == 3)
                        {
                            var result = await MediaPicker.CapturePhotoAsync(new MediaPickerOptions { Title = "Please take your picture" });

                            if (result != null)
                            {
                                var stream = await result.OpenReadAsync();

                                string imgname = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg";

                                var file = System.IO.Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath, imgname);

                                using (FileStream fs = File.Open(file, FileMode.CreateNew))
                                {
                                    await stream.CopyToAsync(fs);
                                    await fs.FlushAsync();

                                    r["ImageName"] = imgname;
                                    r["ImagePath"] = file;

                                    Data.AcceptChanges();

                                    LoadList();

                                    btsave.Visibility = ViewStates.Visible;

                                    d.Dismiss();
                                }
                            }
                        }
                        else
                        {
                            if (aa.Which == 1) r["Result"] = "0";
                            else r["Result"] = "1";

                            Data.AcceptChanges();

                            LoadList();

                            btsave.Visibility = ViewStates.Visible;

                            d.Dismiss();
                        }
                    });

                    b.SetCancelable(false);
                    d = b.Create();
                    d.Show();
                }
            };
            btsave.Click += delegate
            {
                if (Data.Rows.Count > 0)
                {
                    string qry = "";
                    foreach (DataRow row in Data.Rows)
                    {
                        if (row["Result"].ToString() != "")
                        {
                            qry += "exec [dbo].[UpdateHRAssetChecklistResult] '" + txtmcno.Text + "',N'" + txtmctype.Text + "','" + q + "'," + row["ID"] + "," + row["Result"] + ",N'" + row["Remark"] + "','" + row["ImageName"] + "','" + Temp.user + "' \n";

                            if (row["ImagePath"].ToString() != "")
                            {
                                System.Net.WebClient Client = new System.Net.WebClient();
                                Client.Headers.Add("Content-Type", "binary/octet-stream");
                                byte[] result = Client.UploadFile(Temp.url + "hrassetchecklist.php", "POST", row["ImagePath"].ToString());
                                string Result_msg = System.Text.Encoding.UTF8.GetString(result, 0, result.Length);
                                Toast.MakeText(Application.Context, Result_msg, ToastLength.Long).Show();
                            }
                        }
                    }

                    if (qry != "")
                    {
                        kn.Ghi(qry);

                        if (kn.ErrorMessage == "")
                        {
                            Toast.MakeText(Application.Context, "Done !!!", ToastLength.Long).Show();
                            btsave.Visibility = ViewStates.Gone;
                        }
                        else Toast.MakeText(Application.Context, kn.ErrorMessage, ToastLength.Long).Show();
                    }

                }
            };
            btallgood.Click += delegate
            {
                if (Data.Rows.Count > 0)
                {
                    foreach (DataRow row in Data.Rows) row["Result"] = "1";

                    Data.AcceptChanges();

                    LoadList();

                    btsave.Visibility = ViewStates.Visible;
                }
            };
            btallbad.Click += delegate
            {
                if (Data.Rows.Count > 0)
                {
                    foreach (DataRow row in Data.Rows) row["Result"] = "0";

                    Data.AcceptChanges();

                    LoadList();

                    btsave.Visibility = ViewStates.Visible;
                }
            };
            btconfig.Click += delegate
            {
                Toast.MakeText(this, "w=" + w + " s=" + t, ToastLength.Long).Show();
                Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);

                LinearLayout ln = new LinearLayout(this) { Orientation = Orientation.Vertical };
                LinearLayout ln1 = new LinearLayout(this) { Orientation = Orientation.Horizontal }; ln.AddView(ln1);
                LinearLayout ln2 = new LinearLayout(this) { Orientation = Orientation.Horizontal }; ln.AddView(ln2);

                TextView txt1 = new TextView(this) { Text = "Width : " }; ln1.AddView(txt1);
                SeekBar sb1 = new SeekBar(this) { Progress = w, Max = 1000, LayoutParameters = new ViewGroup.LayoutParams(300, ViewGroup.LayoutParams.WrapContent), TextAlignment = TextAlignment.Gravity }; ln1.AddView(sb1);
                TextView txt_1 = new TextView(this) { Text = w.ToString() }; ln1.AddView(txt_1);
                sb1.ProgressChanged += delegate { txt_1.Text = sb1.Progress.ToString(); };

                TextView txt2 = new TextView(this) { Text = "Text Size : " }; ln2.AddView(txt2);
                SeekBar sb2 = new SeekBar(this) { Progress = t, Max = 200, LayoutParameters = new ViewGroup.LayoutParams(300, ViewGroup.LayoutParams.WrapContent), TextAlignment = TextAlignment.Gravity }; ln2.AddView(sb2);
                TextView txt_2 = new TextView(this) { Text = t.ToString() }; ln2.AddView(txt_2);
                sb2.ProgressChanged += delegate { txt_2.Text = sb2.Progress.ToString(); };

                b.SetPositiveButton("APPLY", (s, a) =>
                {
                    w = sb1.Progress; t = sb2.Progress;
                    Toast.MakeText(this, "w=" + w + " s=" + t, ToastLength.Long).Show();
                    LoadList();
                });
                b.SetNegativeButton("CANCEL", (s, a) => { });

                b.SetCancelable(false);
                b.SetView(ln);
                b.Create().Show();
            };

            SetLanguage();
        }
        private void HandleScanResultLogin(ZXing.Result result)
        {
            if (result != null && !string.IsNullOrEmpty(result.Text))
            {
                Vibrator vibrator = (Vibrator)GetSystemService(VibratorService);
                vibrator.Vibrate(100);

                if (result.Text != "")
                {
                    string rs = result.Text.Contains('|') ? result.Text.Split('|').First().Trim().ToUpper() : result.Text;

                    ReadMCNo(rs);
                }
            }
        }
        private void ReadMCNo(string mc)
        {
            if (chk.Checked)
            {
                if (txtmcno.Text == "") run();
                else
                {
                    if (mc.Substring(0, 4) == txtmcno.Text.Substring(0, 4))
                    {
                        run(false);
                        btsave.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);
                        b.SetMessage(Temp.TT("DT133"));

                        b.SetPositiveButton(Temp.TT("DT48"), (s, a) =>
                        {
                            run();
                        });
                        b.SetNegativeButton(Temp.TT("DT44"), (s, a) => { });

                        b.SetCancelable(false);
                        b.Create().Show();
                    }
                }
            }
            else run();

            void run(bool load = true)
            {
                txtmcno.Text = mc;

                LoadMCNo(load);
            }
        }
        private void LoadMCNo(bool load)
        {
            DataTable d1 = kn.Doc("exec GetData 60,'" + txtmcno.Text + "','',''").Tables[0];

            txtmctype.Text = "";

            if (d1.Rows.Count > 0)
            {
                if (d1.Rows.Count == 1)
                {
                    txtmctype.Text = d1.Rows[0][0].ToString();
                    if (load) LoadData();
                }
                else
                {
                    Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);
                    Dialog d = new Dialog(this);

                    string[] it = d1.Select().Select(s => s[0].ToString()).ToArray();

                    b.SetSingleChoiceItems(it, -1, (s, a) =>
                    {
                        txtmctype.Text = it[a.Which];
                        if (load) LoadData();

                        d.Dismiss();
                    });

                    b.SetCancelable(false);
                    d = b.Create();
                    d.Show();
                }
            }
            else Toast.MakeText(this, "This machine checklist was not updated, please check with Admin !!!", ToastLength.Long).Show();
        }
        private void LoadData()
        {
            if (txtmcno.Text != "" && txtmctype.Text != "")
            {
                Data.Rows.Clear();

                Data = kn.Doc("exec GetData 61,'" + txtmcno.Text + "',N'" + txtmctype.Text + "','" + q + "'").Tables[0];
                LoadList();
            }
        }
        private void LoadList()
        {
            DataTable dt = new DataTable();

            if (en) dt = Data.DefaultView.ToTable(true, "ID", "ChecklistEN", "Result", "Remark", "ImageName");
            else dt = Data.DefaultView.ToTable(true, "ID", "ChecklistVN", "Result", "Remark", "ImageName");

            foreach (DataRow dr in dt.Rows)
            {
                if (dr["Result"].ToString() == "1") dr["Result"] = Temp.TT("DT128");
                else if (dr["Result"].ToString() == "0") dr["Result"] = Temp.TT("DT129");

                dt.AcceptChanges();
            }

            lschecklist.Adapter = new A1ATeam.ListViewAdapterWithNoLayout(dt, new List<int> { LayoutRequest.Widht(w), LayoutRequest.Widht(w * 8), LayoutRequest.Widht(w * 3), LayoutRequest.Widht(w * 4), LayoutRequest.Widht(w * 4) }, true)
            {
                TextSize = LayoutRequest.TextSize(t),
                TextHilight = new List<A1ATeam.TextHilight> { new A1ATeam.TextHilight { ColumnIndex = 2, Condition = Temp.TT("DT128"), Color = Color.DarkGreen }, new A1ATeam.TextHilight { ColumnIndex = 2, Condition = Temp.TT("DT129"), Color = Color.Red } }
            };
        }
        private void SetLanguage()
        {
            try
            {
                for (int i = 0; i < layout.ChildCount; i++)
                {
                    View v = layout.GetChildAt(i);

                    ViewGroup.MarginLayoutParams para = (ViewGroup.MarginLayoutParams)v.LayoutParameters;

                    para.SetMargins(LayoutRequest.Widht(para.LeftMargin), LayoutRequest.Height(para.TopMargin), LayoutRequest.Widht(para.RightMargin), LayoutRequest.Height(para.BottomMargin));

                    if (para.Width > 0) para.Width = LayoutRequest.Widht(para.Width);
                    if (para.Height > 0) para.Height = LayoutRequest.Height(para.Height);

                    v.LayoutParameters = para;

                    if (v.GetType() == typeof(TextView))
                    {
                        TextView vv = v as TextView;
                        try
                        {
                            DataRow r = Temp.NgonNgu.Select().Where(d => d[0].ToString() == vv.Text).FirstOrDefault() as DataRow;

                            vv.Text = r[Temp.Newlg].ToString();
                        }
                        catch { }
                        vv.TextSize = LayoutRequest.TextSize(vv.TextSize);
                    }
                    else if (v.GetType() == typeof(Button))
                    {
                        Button vv = v as Button;
                        try
                        {
                            DataRow r = Temp.NgonNgu.Select().Where(d => d[0].ToString() == vv.Text).FirstOrDefault() as DataRow;

                            vv.Text = r[Temp.Newlg].ToString();
                        }
                        catch { }
                        vv.TextSize = LayoutRequest.TextSize(vv.TextSize);
                    }
                    else if (v.GetType() == typeof(CheckBox))
                    {
                        CheckBox vv = v as CheckBox;
                        try
                        {
                            DataRow r = Temp.NgonNgu.Select().Where(d => d[0].ToString() == vv.Text).FirstOrDefault() as DataRow;

                            vv.Text = r[Temp.Newlg].ToString();
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "Set Language failed !!!" + ex.ToString(), ToastLength.Long).Show();
            }
        }
    }
}