using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using CSDL;
using static Android.App.ActionBar;

namespace MachineDowntime
{
    [Activity(Theme = "@style/Theme.AppCompat.Light.NoActionBar.Fullscreen", ScreenOrientation = ScreenOrientation.Portrait, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)]
    public class KetThucActivity : Activity
    {
        RelativeLayout layout;
        EditText edgiocom, edngaynghi, edkhac, edcategory, edmotra, edhanhdong, edest, edcode, edmctype;
        Button btthoat, btcapnhat;
        TextView mcid;
        DataTable dtcategory = new DataTable();
        DataTable dtcode = new DataTable();
        Connect kn = new Connect(Temp.chuoi);
        string ComCode = "", mctype = "", dfcode = "";
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.ketthuc);

            //Window.AddFlags(WindowManagerFlags.Fullscreen);
            //Window.AddFlags(WindowManagerFlags.TranslucentNavigation);
            //Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(SystemUiFlags.HideNavigation | SystemUiFlags.Fullscreen | SystemUiFlags.LowProfile | SystemUiFlags.ImmersiveSticky);

            layout = FindViewById<RelativeLayout>(Resource.Id.layoutketthuc1);

            edgiocom = FindViewById<EditText>(Resource.Id.edgiocom);
            edngaynghi = FindViewById<EditText>(Resource.Id.edngaynghi);
            edkhac = FindViewById<EditText>(Resource.Id.edkhac);
            edcategory = FindViewById<EditText>(Resource.Id.edcategory);
            edmotra = FindViewById<EditText>(Resource.Id.edmotra);
            edhanhdong = FindViewById<EditText>(Resource.Id.edhanhdong);
            edcode = FindViewById<EditText>(Resource.Id.edcode);
            edest = FindViewById<EditText>(Resource.Id.edest); edest.Focusable = false;
            edmctype = FindViewById<EditText>(Resource.Id.edmctype);

            btthoat = FindViewById<Button>(Resource.Id.btthoat);
            btcapnhat = FindViewById<Button>(Resource.Id.btcapnhat);

            mcid = FindViewById<TextView>(Resource.Id.textView0);
            mcid.Text = Temp.TT("DT56") + " : " + Temp.mcid;

            try
            {
                DataTable mc = kn.Doc("select McType from Overview where McSerialNumber = '" + Temp.mcid + "'").Tables[0];

                if (mc.Rows.Count > 0) mctype = mc.Rows[0][0].ToString();

                edmctype.Text = mctype;

                GetEstimate();
            }
            catch { }

            btthoat.Click += delegate
            {
                Intent it = new Intent(this, typeof(MainActivity));
                StartActivity(it);
                Finish();
            };
            btcapnhat.Click += Btcapnhat_Click;

            edcategory.Focusable = false;
            edcategory.Click += Edcategory_Click;

            edcode.Focusable = false;
            edcode.Click += Edcode_Click;

            edmctype.Focusable = false;
            edmctype.Click += Edmctype_Click;

            edmotra.Focusable = false;
            edmotra.Click += Edmotra_Click;

            edhanhdong.Focusable = false;
            edhanhdong.Click += Edhanhdong_Click;

            edgiocom.TextChanged += delegate { GetEstimate(); };
            edngaynghi.TextChanged += delegate { GetEstimate(); };
            edkhac.TextChanged += delegate { GetEstimate(); };

            LoadCategory();

            SetLanguage();
        }

        private void Edmctype_Click(object sender, EventArgs e)
        {
            try
            {
                List<string> ls = dtcode.Select().Select(x => x[2].ToString()).Distinct().ToList();

                ArrayAdapter array = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItemSingleChoice, ls);

                ShowDialog(array, edmctype, false);
            }
            catch { }
        }

        private void Edcode_Click(object sender, EventArgs e)
        {
            try
            {
                List<string> ls = dtcode.Select().Select(x => x[1].ToString()).Distinct().ToList();

                ArrayAdapter array = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItemSingleChoice, ls);

                ShowDialog(array, edcode, false);
            }
            catch { }
        }

        private void GetEstimate()
        {
            try
            {
                int giocom = string.IsNullOrEmpty(edgiocom.Text) ? 0 : int.Parse(edgiocom.Text);
                int ngaynghi = string.IsNullOrEmpty(edngaynghi.Text) ? 0 : int.Parse(edngaynghi.Text);
                int khac = string.IsNullOrEmpty(edkhac.Text) ? 0 : int.Parse(edkhac.Text);

                TimeSpan ts = DateTime.Now - Temp.OccurTime;

                edest.Text = ((int)ts.TotalMinutes - giocom - ngaynghi * 24 * 60 - khac).ToString();
            }
            catch { }
        }
        private void LoadCategory()
        {
            try
            {
                dtcategory = kn.Doc("select * from DowtimeDescription where Language = " + Temp.Newlg).Tables[0];

                dtcode = kn.Doc("select * from DowntimeCode where McGroup = '" + Temp.mcid[..4] + "'").Tables[0];
            }
            catch { }
        }
        private void Edhanhdong_Click(object sender, EventArgs e)
        {
            try
            {
                if (edmotra.Text.Contains("-"))
                {
                    dfcode = edmotra.Text.Split('-')[0];

                    DataTable dt = kn.Doc("select Action from DowntimeReport where McDTCode = " + dfcode + " order by Action").Tables[0];
                    List<string> ls = dt.Select().Select(s => s[0].ToString()).Distinct().ToList();

                    ArrayAdapter array = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItemSingleChoice, ls);

                    ShowDialog(array, edhanhdong, true);
                }
            }
            catch { }
        }

        private void Edmotra_Click(object sender, EventArgs e)
        {
            try
            {
                List<string> ls = dtcode.Select("AfectiveCode = '" + edcode.Text + "' and McType = '" + edmctype.Text + "'").Select(x => x[0] + " - " + x[3]).Distinct().ToList();

                ArrayAdapter array = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItemSingleChoice, ls);

                ShowDialog(array, edmotra, false);
            }
            catch { }
        }

        private void Edcategory_Click(object sender, EventArgs e)
        {
            try
            {
                List<string> ls = dtcategory.Rows.OfType<DataRow>().Select(dr => dr["Category"].ToString()).Distinct().ToList();

                ArrayAdapter array = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItemSingleChoice, ls);

                ShowDialog(array, edcategory, false);
            }
            catch { }
        }
        private void ShowDialog(ArrayAdapter array, EditText ed, bool add)
        {
            try
            {
                Android.App.AlertDialog.Builder b = new Android.App.AlertDialog.Builder(this);
                LinearLayout v = new LinearLayout(this)
                {
                    Orientation = Orientation.Vertical,
                    LayoutParameters = new LayoutParams(500, 600)
                };
                EditText edsearch = new EditText(this)
                {
                    Left = 10,
                    Top = 10,
                    LayoutParameters = new LayoutParams(500, 100),
                    Hint = Temp.TT("DT42")
                };
                edsearch.TextChanged += (s, e) => array.Filter.InvokeFilter(edsearch.Text);
                ListView lsv = new ListView(this)
                {
                    Left = 10,
                    Top = 100,
                    LayoutParameters = new LayoutParams(500, 600)
                };

                lsv.Adapter = array;

                v.AddView(edsearch);
                v.AddView(lsv);
                b.SetView(v);

                if (add) b.SetPositiveButton(Temp.TT("DT73"), (ss, ee) => { ed.Text = edsearch.Text; });
                b.SetNegativeButton(Temp.TT("DT16"), (ss, ee) => { });

                b.SetCancelable(false);
                Dialog dd = b.Create();
                dd.Show();

                lsv.ItemClick += (ss, ee) =>
                {
                    ed.Text = lsv.GetItemAtPosition(ee.Position).ToString();
                    Temp.HideKeyBoard(this, CurrentFocus);
                    dd.Dismiss();
                };
            }
            catch { }
        }
        private void Btcapnhat_Click(object sender, EventArgs e)
        {
            try
            {
                if (edcategory.Text == "")
                {
                    Toast.MakeText(this, Temp.TT("DT74"), ToastLength.Long).Show();
                    edcategory.RequestFocus();
                }
                else if (edmotra.Text == "")
                {
                    Toast.MakeText(this, Temp.TT("DT75"), ToastLength.Long).Show();
                    edmotra.RequestFocus();
                }
                else if (edhanhdong.Text == "")
                {
                    Toast.MakeText(this, Temp.TT("DT76"), ToastLength.Long).Show();
                    edhanhdong.RequestFocus();
                }
                else
                {
                    int giocom = string.IsNullOrEmpty(edgiocom.Text) ? 0 : int.Parse(edgiocom.Text);
                    int ngaynghi = string.IsNullOrEmpty(edngaynghi.Text) ? 0 : int.Parse(edngaynghi.Text);
                    int khac = string.IsNullOrEmpty(edkhac.Text) ? 0 : int.Parse(edkhac.Text);

                    int dt = giocom + ngaynghi * 24 * 60 + khac;
                    DateTime ngay = DateTime.Now;

                    try
                    {
                        string catagory = Temp.NgonNgu.Select().Where(x => x[Temp.Newlg].ToString() == edcategory.Text).FirstOrDefault()[1].ToString();
                        switch (catagory)
                        {
                            case "Máy Móc":
                                ComCode = "M679";
                                break;
                            case "Công Nhân":
                                ComCode = "M680";
                                break;
                            case "Kỹ Thuật":
                                ComCode = "M681";
                                break;
                            case "Điện":
                                ComCode = "M682";
                                break;
                            default:
                                ComCode = "M679";
                                break;
                        }
                    }
                    catch { ComCode = "M679"; }

                    Connect cnn = new Connect(Temp.com);
                    DataTable dtcmtid1 = cnn.Doc("select * from spcomment where FacLine = '" + Temp.facline + "' and FDDate = '" + DateTime.Now.AddDays(-1).ToString("yyyyMMdd") + "'").Tables[0];
                    DataTable dtcmtid = cnn.Doc("select * from spcomment where FacLine = '" + Temp.facline + "' and FDDate = '" + DateTime.Now.ToString("yyyyMMdd") + "'").Tables[0];
                    DataTable mayhu = kn.Doc("select * from DowntimeReport where McSerialNo = '" + Temp.mcid + "' and FinishTime is null").Tables[0];

                    if (mayhu.Rows.Count > 0)
                    {
                        DateTime fn = DateTime.Parse(mayhu.Rows[0]["StartTime"].ToString());
                        DataRow r = mayhu.Rows[0];
                        DateTime occur = DateTime.Parse(r["OccurTime"].ToString());

                        if (ngay.Day > fn.Day)
                        {
                            string st = ngay.ToString("yyyyMMdd");
                            string fnn = fn.ToString("yyyyMMdd");

                            Android.App.AlertDialog.Builder b1 = new AlertDialog.Builder(this);

                            LinearLayout ln1 = new LinearLayout(this) { Orientation = Orientation.Vertical };

                            TextView txt1 = new TextView(this) { Text = fn.ToString("dd-MM-yyyy") + " end working at : " };

                            TimePicker t1 = new TimePicker(this) { Hour = 16, Minute = 30 };

                            ln1.AddView(txt1); ln1.AddView(t1);

                            b1.SetView(ln1);
                            b1.SetCancelable(false);

                            b1.SetPositiveButton("OK", (s1, a1) =>
                            {
                                fnn += " " + t1.Hour.ToString("00") + ":" + t1.Minute.ToString("00");

                                Android.App.AlertDialog.Builder b2 = new AlertDialog.Builder(this);

                                LinearLayout ln2 = new LinearLayout(this) { Orientation = Orientation.Vertical };

                                TextView txt2 = new TextView(this) { Text = ngay.ToString("dd-MM-yyyy") + " begin working at : " };

                                TimePicker t2 = new TimePicker(this) { Hour = 7, Minute = 30 };

                                ln2.AddView(txt2); ln2.AddView(t2);

                                b2.SetView(ln2);
                                b2.SetCancelable(false);

                                b2.SetPositiveButton("OK", (s2, a2) =>
                                {
                                    st += " " + t2.Hour.ToString("00") + ":" + t2.Minute.ToString("00");

                                    List<string> ls1 = new List<string>
                                    {
                                        "@mcno=" + Temp.mcid,
                                        "@giocom=" + giocom,
                                        "@holiday=" + ngaynghi,
                                        "@other=" + khac,
                                        "@dt=" + dt,
                                        "@code=" + dfcode,
                                        "@action=" + edhanhdong.Text,
                                        "@ngay=" + fnn
                                    };
                                    List<string> l1 = new List<string>()
                                    {
                                        "@fac=" + Temp.fac,
                                        "@line=" + Temp.facline,
                                        "@date=" + fn.ToString("yyyyMMdd"),
                                        "@cmid=" + (dtcmtid1.Rows.Count + 1).ToString(),
                                        "@first=" + occur.ToString("yyyyMMdd HH:mm"),
                                        "@last=" + fnn,
                                        "@code=" + ComCode,
                                        "@remark=" + edmotra.Text + ", " + edhanhdong.Text,
                                        "@user=" + Temp.user
                                    };
                                    cnn.Proc("DowntimeInsert", l1);
                                    kn.Proc("UpdateDowntime2", ls1);

                                    //string code = kn.Doc("select top 1 RecNo from DowtimeDescription where Category = '" + edcategory.Text + "' and Description = N'" + edmotra.Text + "' and Solution = N'" + edhanhdong.Text + "'").Tables[0].Rows[0][0].ToString();
                                    kn.Ghi("insert into DowntimeReport values ('" + Temp.mcid + "','" + Temp.fac + "','" + Temp.facline + "','" + st + "','" + st + "','" + ngay.ToString("yyyyMMdd HH:mm") + "',N'Sửa Xong',0," +
                                            "datediff(minute,'" + st + "','" + ngay.ToString("yyyyMMdd HH:mm") + "'),0,datediff(minute,'" + st + "','" + ngay.ToString("yyyyMMdd HH:mm") + "')," + r["MTBFHrs"].ToString().Replace(',', '.') + "," +
                                            "'" + r["Mechanic"].ToString() + "',NULL,NULL,'" + dfcode + "','" + Temp.user + "',getdate(),0,0,0,N'" + edhanhdong.Text + "')");

                                    List<string> l2 = new List<string>()
                                    {
                                        "@fac=" + Temp.fac,
                                        "@line=" + Temp.facline,
                                        "@date=" + ngay.ToString("yyyyMMdd"),
                                        "@cmid=" + (dtcmtid.Rows.Count + 2).ToString(),
                                        "@first=" + st,
                                        "@last=" + ngay.ToString("yyyyMMdd HH:mm"),
                                        "@code=" + ComCode,
                                        "@remark=" + edmotra.Text + ", " + edhanhdong.Text,
                                        "@user=" + Temp.user
                                    };
                                    cnn.Proc("DowntimeInsert", l2);

                                    if (kn.ErrorMessage == "")
                                    {
                                        Toast.MakeText(this, Temp.TT("DT82"), ToastLength.Long).Show();

                                        Intent it = new Intent(this, typeof(MainActivity));
                                        StartActivity(it);
                                        Finish();
                                    }
                                    else Toast.MakeText(this, Temp.TT("DT45") + kn.ErrorMessage, ToastLength.Long).Show();
                                });

                                b2.Create().Show();
                            });
                            b1.Create().Show();
                        }
                        else
                        {
                            List<string> ls = new List<string>
                            {
                                "@mcno=" + Temp.mcid,
                                "@giocom=" + giocom,
                                "@holiday=" + ngaynghi,
                                "@other=" + khac,
                                "@dt=" + dt,
                                "@code=" + dfcode,
                                "@action=" + edhanhdong.Text,
                                "@ngay=" + ngay.ToString("yyyyMMdd HH:mm")
                            };

                            List<string> l = new List<string>()
                            {
                                "@fac=" + Temp.fac,
                                "@line=" + Temp.facline,
                                "@date=" + DateTime.Now.ToString("yyyyMMdd"),
                                "@cmid=" + (dtcmtid.Rows.Count + 1).ToString(),
                                "@first=" + occur.ToString("yyyyMMdd HH:mm"),
                                "@last=" + ngay.AddMinutes(-dt).ToString("yyyyMMdd HH:mm"),
                                "@code=" + ComCode,
                                "@remark=" + edmotra.Text + ", " + edhanhdong.Text,
                                "@user=" + Temp.user
                            };

                            //update downtime scan&pack
                            cnn.Proc("DowntimeInsert", l);

                            kn.Proc("UpdateDowntime2", ls);

                            if (kn.ErrorMessage == "")
                            {
                                Toast.MakeText(this, Temp.TT("DT82"), ToastLength.Long).Show();

                                Intent it = new Intent(this, typeof(MainActivity));
                                StartActivity(it);
                                Finish();
                            }
                            else Toast.MakeText(this, Temp.TT("DT45") + kn.ErrorMessage, ToastLength.Long).Show();
                        }
                    }
                    else Toast.MakeText(this, "No Record inserted " + Temp.mcid, ToastLength.Long).Show();
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.ToString(), ToastLength.Long).Show();
            }
        }

        private void SetLanguage()
        {
            try
            {
                if (Temp.Newlg != Temp.Oldlg)
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
                            try
                            {
                                TextView vv = v as TextView;

                                vv.TextSize = LayoutRequest.TextSize(vv.TextSize);

                                DataRow r = Temp.NgonNgu.Select().Where(d => d[Temp.Oldlg].ToString() == vv.Text).FirstOrDefault() as DataRow;

                                vv.Text = r[Temp.Newlg].ToString();
                            }
                            catch { }
                        }
                        else if (v.GetType() == typeof(Button))
                        {
                            try
                            {
                                Button vv = v as Button;

                                vv.TextSize = LayoutRequest.TextSize(vv.TextSize);

                                DataRow r = Temp.NgonNgu.Select().Where(d => d[Temp.Oldlg].ToString() == vv.Text).FirstOrDefault() as DataRow;

                                vv.Text = r[Temp.Newlg].ToString();
                            }
                            catch { }
                        }
                        else if (v.GetType() == typeof(EditText))
                        {
                            try
                            {
                                EditText vv = v as EditText;

                                vv.TextSize = LayoutRequest.TextSize(vv.TextSize);
                            }
                            catch { }
                        }
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