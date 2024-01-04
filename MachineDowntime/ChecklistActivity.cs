using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using CSDL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Timers;
using ZXing.Mobile;
using static Android.Renderscripts.Sampler;

namespace MachineDowntime
{
    [Activity(Theme = "@style/Theme.AppCompat.Light.NoActionBar.Fullscreen", ScreenOrientation = ScreenOrientation.Portrait, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)]
    public class ChecklistActivity : Activity
    {
        TextView txtfacline, txtname, txtmcno, txtmctype, txtdate;
        Button btscan, btsave, btallgood, btallbad, btconfig, btchecked;
        ListView lschecklist;
        RadioButton q1, q2, q3, q4, daily, weekly, monthly;
        Connect kn;
        CheckBox chk;
        RelativeLayout layout;
        string q = "Q1";
        MobileBarcodeScanner scanner;
        DataTable Data = new DataTable();
        int w = 50, t = 20;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.checklist);

            MobileBarcodeScanner.Initialize(Application);
            scanner = new MobileBarcodeScanner();

            kn = new Connect(Temp.chuoi);

            layout = FindViewById<RelativeLayout>(Resource.Id.layoutchkl);

            txtfacline = FindViewById<TextView>(Resource.Id.txtfacline); txtfacline.Text = Temp.facline;
            txtname = FindViewById<TextView>(Resource.Id.txtname); txtname.Text = Temp.Login.Rows[0]["Name"].ToString();
            txtmcno = FindViewById<TextView>(Resource.Id.txtmcno); txtmcno.Text = "";
            txtmctype = FindViewById<TextView>(Resource.Id.txtmctype); txtmctype.Text = "";
            txtdate = FindViewById<TextView>(Resource.Id.txtdate);

            btscan = FindViewById<Button>(Resource.Id.btscan);
            btsave = FindViewById<Button>(Resource.Id.btsave); btsave.Visibility = ViewStates.Gone;
            btallgood = FindViewById<Button>(Resource.Id.btallgood);
            btallbad = FindViewById<Button>(Resource.Id.btallbad);
            btconfig = FindViewById<Button>(Resource.Id.btconfig);
            btchecked = FindViewById<Button>(Resource.Id.btchecked);

            chk = FindViewById<CheckBox>(Resource.Id.checkBox1);

            lschecklist = FindViewById<ListView>(Resource.Id.lschecklist);

            q1 = FindViewById<RadioButton>(Resource.Id.q1); q1.CheckedChange += delegate { if (q1.Checked) SetQ(1); };
            q2 = FindViewById<RadioButton>(Resource.Id.q2); q2.CheckedChange += delegate { if (q2.Checked) SetQ(2); };
            q3 = FindViewById<RadioButton>(Resource.Id.q3); q3.CheckedChange += delegate { if (q3.Checked) SetQ(3); };
            q4 = FindViewById<RadioButton>(Resource.Id.q4); q4.CheckedChange += delegate { if (q4.Checked) SetQ(4); };
            daily = FindViewById<RadioButton>(Resource.Id.daily); daily.CheckedChange += delegate { if (daily.Checked) SetQ(5); };
            weekly = FindViewById<RadioButton>(Resource.Id.weekly); weekly.CheckedChange += delegate { if (weekly.Checked) SetQ(6); };
            monthly = FindViewById<RadioButton>(Resource.Id.monthly); monthly.CheckedChange += delegate { if (monthly.Checked) SetQ(7); };

            void SetQ(int value)
            {
                if (value < 5) q = "Q" + value;
                else
                {
                    if (value == 5) q = "Daily";
                    else if (value == 6) q = "Weekly";
                    else if (value == 7) q = "Monthly";
                }

                Toast.MakeText(this, q, ToastLength.Long).Show();
                if (chk.Checked)
                {
                    if (Data.Rows.Count > 0)
                    {
                        foreach (DataRow row in Data.Rows)
                        {
                            row["CheckedBy"] = "";
                            row["CheckedDate"] = DBNull.Value;
                            row["Remark"] = "";
                        }

                        Data.AcceptChanges();

                        LoadList();
                    }
                }
                else LoadData();
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

            if (int.Parse(Temp.Login.Rows[0]["Levels"].ToString()) == 0) btchecked.Visibility = ViewStates.Gone;

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
                        txtmcno.Text = txt.Text.ToUpper();

                        LoadMCNo();
                    }
                });

                b.SetView(txt);
                b.Create().Show();
            };
            lschecklist.ItemClick += (s, a) =>
            {
                if (a.Position > 0)
                {
                    string[] it = { "Good", "Bad", "Input remark" };

                    Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);
                    Dialog d = new Dialog(this);

                    b.SetSingleChoiceItems(it, -1, (s, aa) =>
                    {
                        DataRow r = Data.Rows[a.Position - 1];

                        if (aa.Which == 2)
                        {
                            Android.App.AlertDialog.Builder bb = new AlertDialog.Builder(this);
                            EditText ed = new EditText(this) { Hint = "Input remark here", LayoutParameters = new ViewGroup.LayoutParams(300, ViewGroup.LayoutParams.WrapContent) };

                            bb.SetPositiveButton("OK", (s1, a1) =>
                            {
                                r["Remark"] = ed.Text;

                                Data.AcceptChanges();

                                LoadList();

                                btsave.Visibility = ViewStates.Visible;

                                d.Dismiss();
                            });

                            bb.SetView(ed);
                            bb.Create().Show();
                        }
                        else
                        {
                            r["Result"] = it[aa.Which];

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
                    string qr = "exec GetData 20,'" + q + "',N'" + txtmctype.Text + "','" + txtmcno.Text + "' \n";
                    string transdate = "";

                    switch (q)
                    {
                        case "Daily":
                            transdate = "GETDATE()";
                            break;
                        case "Weekly":
                            transdate = "DATEADD(WEEK, DATEDIFF(WEEK, 0, DATEADD(DAY, -1, GETDATE())), 0)";
                            break;
                        case "Monthly":
                            transdate = "DATEADD(MONTH, DATEDIFF(month, 0, GETDATE()), 0)";
                            break;
                        default:
                            transdate = "DATEADD(QUARTER, " + q.Substring(1, 1) + " - 1,DATEADD(yy, DATEDIFF(yy, 0, GETDATE()), 0))";
                            break;
                    }

                    foreach (DataRow row in Data.Rows)
                        qr += "insert into MachineCheckListResult values (" + row["ID"] + ",'" + txtfacline.Text + "','" + txtmcno.Text + "','" + Temp.user + "'," + transdate + ",'" + row["Result"] + "','" + Temp.user
                                + "',getdate(),'" + q + "','" + row["CheckedBy"] + "'," + (row["CheckedDate"].ToString() == "" ? "null" : "'" + DateTime.Parse(row["CheckedDate"].ToString()).ToString("yyyyMMdd") + "'") + ",N'" + row["Remark"] + "') \n";

                    kn.Ghi(qr);

                    if (kn.ErrorMessage == "")
                    {
                        Toast.MakeText(this, "Saved successfully !!!", ToastLength.Long).Show();
                        btsave.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        Toast.MakeText(this, qr, ToastLength.Long).Show();
                        Toast.MakeText(this, "Save failed !!! " + kn.ErrorMessage, ToastLength.Long).Show();
                    }
                }
            };
            btallgood.Click += delegate
            {
                if (Data.Rows.Count > 0)
                {
                    foreach (DataRow row in Data.Rows) row["Result"] = "Good";

                    Data.AcceptChanges();

                    LoadList();

                    btsave.Visibility = ViewStates.Visible;
                }
            };
            btallbad.Click += delegate
            {
                if (Data.Rows.Count > 0)
                {
                    foreach (DataRow row in Data.Rows) row["Result"] = "Bad";

                    Data.AcceptChanges();

                    LoadList();

                    btsave.Visibility = ViewStates.Visible;
                }
            };
            btchecked.Click += delegate
            {
                if (Data.Rows.Count > 0)
                {
                    foreach (DataRow row in Data.Rows)
                    {
                        if (row["Result"].ToString() != "")
                        {
                            row["CheckedBy"] = Temp.user;
                            row["CheckedDate"] = DateTime.Now;
                        }
                    }

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
            txtfacline.Click += delegate
            {
                var line = Temp.FacLine.Select("FacZone like '%" + Temp.fac + "%'").Select(l => l[1].ToString()).Distinct().ToArray();
                line = line.Concat(new string[] { Temp.fac + "PPA", Temp.fac + "JUMPER" }).ToArray();
                Android.App.AlertDialog.Builder b = new Android.App.AlertDialog.Builder(this);

                b.SetSingleChoiceItems(line, -1, (s, a) =>
                {
                    Dialog d = s as Dialog;

                    txtfacline.Text = line[a.Which];

                    d.Dismiss();
                });
                b.SetCancelable(false);
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
                    txtmcno.Text = result.Text.Contains('|') ? result.Text.Split('|').First().Trim().ToUpper() : result.Text;

                    LoadMCNo();
                }
            }
        }
        private void LoadMCNo()
        {
            DataTable d1 = kn.Doc("exec GetData 18,'" + txtmcno.Text + "','',''").Tables[0];

            txtmctype.Text = "";
            lschecklist.Adapter = null;

            if (d1.Rows.Count > 0)
            {
                Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);
                Dialog d = new Dialog(this);

                string[] it = d1.Select().Select(s => s[0].ToString()).ToArray();

                b.SetSingleChoiceItems(it, -1, (s, a) =>
                {
                    txtmctype.Text = it[a.Which];
                    LoadData();

                    d.Dismiss();
                });

                b.SetCancelable(false);
                d = b.Create();
                d.Show();
            }
            else Toast.MakeText(this, "This machine checklist was not updated, please check with Admin !!!", ToastLength.Long).Show();
        }
        private void LoadData()
        {
            if (txtmcno.Text != "" && txtmctype.Text != "")
            {
                Data = kn.Doc("exec GetData 19,'" + txtmcno.Text + "',N'" + txtmctype.Text + "','" + q + "'").Tables[0];
                LoadList();
            }
        }
        private void LoadList()
        {
            lschecklist.Adapter = new A1ATeam.ListViewAdapterWithNoLayout(Data, new List<int> { LayoutRequest.Widht(w), LayoutRequest.Widht(w * 8), LayoutRequest.Widht(w * 2), LayoutRequest.Widht(w * 3), LayoutRequest.Widht(w * 3), LayoutRequest.Widht(w * 8) }, true)
            {
                TextSize = LayoutRequest.TextSize(t),
                TextHilight = new List<A1ATeam.TextHilight> { new A1ATeam.TextHilight { ColumnIndex = 2, Condition = "Good", Color = Color.Green }, new A1ATeam.TextHilight { ColumnIndex = 2, Condition = "Bad", Color = Color.Red } }
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