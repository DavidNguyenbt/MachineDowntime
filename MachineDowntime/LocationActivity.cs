using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
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
using ZXing.Mobile;

namespace MachineDowntime
{
    [Activity(Label = "Machine Location", Theme = "@style/Theme.AppCompat.Light.NoActionBar", ScreenOrientation = ScreenOrientation.SensorPortrait)]
    public class LocationActivity : Activity
    {
        Connect kn = new Connect(Temp.chuoi);
        TextView txtmechanic, txtdate, txtqty, txtlevels;
        RelativeLayout layout;
        RadioGroup rdg;
        RadioButton rd1;
        Button btchange, btscan, btsieuthi, btchangepass;
        Timer timer;
        EditText edarea;
        ListView ls;
        string xuong = Temp.facline, level = "0", f = "", d = "", s = "", facline = "", user = "";//f : factory, d : dept, s : section
        View curly = null;
        Color curcl;
        DataTable dt = new DataTable();
        DataTable Main = new DataTable();
        MobileBarcodeScanner scanner;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.location);

            MobileBarcodeScanner.Initialize(Application);
            scanner = new MobileBarcodeScanner();

            txtlevels = FindViewById<TextView>(Resource.Id.txtlevel2); txtlevels.Text = level = Temp.Login.Rows[0]["Levels"].ToString();
            txtmechanic = FindViewById<TextView>(Resource.Id.txtten2); txtmechanic.Text = Temp.Login.Rows[0]["Name"].ToString();
            txtdate = FindViewById<TextView>(Resource.Id.txtdate); txtdate.Text = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
            txtqty = FindViewById<TextView>(Resource.Id.txtqty);

            edarea = FindViewById<EditText>(Resource.Id.edarea);

            ls = FindViewById<ListView>(Resource.Id.lsmachine);

            layout = FindViewById<RelativeLayout>(Resource.Id.layoutlocation);

            btchange = FindViewById<Button>(Resource.Id.btchange);
            btscan = FindViewById<Button>(Resource.Id.btscan);
            btsieuthi = FindViewById<Button>(Resource.Id.btsieuthimay);
            btchangepass = FindViewById<Button>(Resource.Id.btdoipass);

            rdg = FindViewById<RadioGroup>(Resource.Id.radioGroup1); rdg.Visibility = ViewStates.Gone;
            rd1 = FindViewById<RadioButton>(Resource.Id.radioButton1);

            if (Temp.user.Length > 5) user = Temp.user.Substring(Temp.user.Length - 5, 5) + " - " + txtmechanic.Text.Split(' ').Last();
            else user = Temp.user;

            //SetLanguage();

            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += delegate
            {
                RunOnUiThread(() =>
                {
                    txtdate.Text = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                });
            };
            timer.Start();

            try
            {
                DataRow[] rs = Temp.Location.Select("facline1 = '" + xuong + "'");
                if (rs.Length > 0) facline = rs[0]["facline2"].ToString(); else facline = Temp.facline;
                edarea.Text = facline;

                LoadList("exec GetData 10,'" + facline + "','',''");

                btchange.Click += delegate { SelectDept(1); rdg.Visibility = ViewStates.Gone; };
                btchange.LongClick += delegate { LoadList("exec GetData 10,'" + facline + "','',''"); rdg.Visibility = ViewStates.Gone; };
                btchangepass.Click += delegate { DoiMatKhau(); rdg.Visibility = ViewStates.Gone; };
                btscan.Click += delegate { Scan(); };
                btscan.LongClick += delegate
                {
                    Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);
                    EditText ed = new EditText(this) { LayoutParameters = new ViewGroup.LayoutParams(300, -2) };
                    ed.InputType = Android.Text.InputTypes.TextFlagCapCharacters;

                    b.SetPositiveButton("OK", (s, a) =>
                    {
                        if (ed.Text != "") ShowData(ed.Text.Substring(0,8));
                    });

                    b.SetView(ed);
                    b.Create().Show();
                };
                btsieuthi.Click += delegate { LoadList("exec GetData 13,'','',''"); rdg.Visibility = ViewStates.Visible; rd1.Checked = true; };

                ls.ItemClick += (s, a) =>
                {
                    if (a.Position > 0)
                    {
                        DataRow r = dt.Rows[a.Position - 1];

                        Toast.MakeText(this, r["McSerialNumber"].ToString(), ToastLength.Short).Show();

                        View ly = GetViewByPosition(a.Position, ls);

                        if (curly != null) curly.SetBackgroundColor(curcl);

                        curly = ly;
                        curcl = ((ColorDrawable)ly.Background).Color;

                        ly.SetBackgroundColor(Color.LightGreen);

                        ShowData(r["McSerialNumber"].ToString());
                    }
                };

                ls.ItemLongClick += (s, a) =>
                {
                    Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);

                    DataTable qty = kn.Doc("exec GetData 11,'" + facline + "','',''").Tables[0];

                    ListView lst = new ListView(this);
                    lst.Adapter = new A1ATeam.ListViewAdapterWithNoLayout(qty, new List<int> { }, true) { TextSize = 10/*LayoutRequest.TextSize(15)*/ };

                    b.SetView(lst);
                    b.Create().Show();
                };

                for (int i = 0; i < rdg.ChildCount; i++)
                {
                    RadioButton rd = rdg.GetChildAt(i) as RadioButton;

                    rd.Click += delegate
                    {
                        curly = null; dt.Rows.Clear();
                        switch (rd.Text)
                        {
                            case "All":
                                //var dv = Main.DefaultView;
                                //dt = dv.ToTable();
                                dt.Merge(Main);
                                break;
                            case "F1":
                                var dv1 = Main.DefaultView;
                                dv1.RowFilter = "CurrentLocation like 'Fac1%'";
                                dt = dv1.ToTable();
                                break;
                            case "F2":
                                var dv2 = Main.DefaultView;
                                dv2.RowFilter = "CurrentLocation like 'Fac2%'";
                                dt = dv2.ToTable();
                                break;
                            case "F3":
                                var dv3 = Main.DefaultView;
                                dv3.RowFilter = "CurrentLocation like 'Fac3%'";
                                dt = dv3.ToTable();
                                break;
                            case "F4":
                                var dv4 = Main.DefaultView;
                                dv4.RowFilter = "CurrentLocation like 'Fac4%'";
                                dt = dv4.ToTable();
                                break;
                            case "F5":
                                var dv5 = Main.DefaultView;
                                dv5.RowFilter = "CurrentLocation like 'Fac5%'";
                                dt = dv5.ToTable();
                                break;
                            case "RnD":
                                var dv6 = Main.DefaultView;
                                dv6.RowFilter = "CurrentLocation like 'R&D%'";
                                dt = dv6.ToTable();
                                break;
                        }
                        Toast.MakeText(this, rd.Text + dt.Rows.Count + "/" + Main.Rows.Count, ToastLength.Long).Show();
                        BaseAdapter adapter = new A1ATeam.ListViewAdapterWithNoLayout(dt.DefaultView.ToTable(false, "McSerialNumber", "McName", "McType", "McBrand", "McModel", "Status", "ManuSerialNo", "Factory", "CurrentLocation"), new List<int> { }, true)
                        { TextSize = 10/* LayoutRequest.TextSize(20)*/ };

                        ls.Adapter = adapter; txtqty.Text = "Machine Qty : " + dt.Rows.Count;

                        Toast.MakeText(this, "Found Machine : " + dt.Rows.Count, ToastLength.Long).Show();
                    };
                }
            }
            catch { }


        }
        private void DoiMatKhau()
        {
            try
            {
                Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);

                LinearLayout v = new LinearLayout(Application.Context)
                {
                    Orientation = Orientation.Vertical
                };
                TextView user = new TextView(Application.Context)
                {
                    LayoutParameters = new ViewGroup.LayoutParams(300, ViewGroup.LayoutParams.WrapContent),
                    Text = Temp.TT("DT70") + Temp.user
                };
                v.AddView(user);
                EditText oldpass = new EditText(Application.Context)
                {
                    LayoutParameters = new ViewGroup.LayoutParams(300, ViewGroup.LayoutParams.WrapContent),
                    Hint = Temp.TT("DT71")
                };
                v.AddView(oldpass);
                EditText newpass = new EditText(Application.Context)
                {
                    LayoutParameters = new ViewGroup.LayoutParams(300, ViewGroup.LayoutParams.WrapContent),
                    Hint = Temp.TT("DT72")
                };
                v.AddView(newpass);

                b.SetView(v);
                b.SetPositiveButton(Temp.TT("DT66"), (s, a) =>
                {
                    try
                    {
                        DataTable login = new DataTable();
                        login = kn.Doc("select * from DownTimeUserList where ID = '" + Temp.user + "'").Tables[0];

                        if (login.Rows.Count == 0)
                        {
                            kn.Ghi("insert into DownTimeUserList values ('" + Temp.user + "',N'" + Temp.Login.Rows[0]["Name"] + "','" + newpass.Text + "','" + Temp.facline + "','" + Temp.Login.Rows[0]["Section"] + "'," +
                                "'" + Temp.Login.Rows[0]["Position"] + "','" + Temp.Login.Rows[0]["Levels"] + "','" + Temp.facline.Substring(Temp.facline.Length - 2, 2) + "')");
                            if (kn.ErrorMessage == "")
                            {
                                Toast.MakeText(this, Temp.TT("DT68"), ToastLength.Long).Show();
                                Finish();
                            }
                            else Toast.MakeText(this, kn.ErrorMessage, ToastLength.Long).Show();
                        }
                        else
                        {
                            DataRow[] r = login.Select("Password = '" + oldpass.Text + "'");

                            if (r.Length == 0)
                            {
                                Toast.MakeText(this, Temp.TT("DT67"), ToastLength.Long).Show();
                                DoiMatKhau();
                            }
                            else
                            {
                                kn.Ghi("update DownTimeUserList set Password = '" + newpass.Text + "' where ID = '" + Temp.user + "'");
                                if (kn.ErrorMessage == "")
                                {
                                    Toast.MakeText(this, Temp.TT("DT68"), ToastLength.Long).Show();
                                    Finish();
                                }
                                else Toast.MakeText(this, kn.ErrorMessage, ToastLength.Long).Show();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Toast.MakeText(this, Temp.TT("DT69") + ex.ToString(), ToastLength.Long).Show();
                    }

                });
                b.SetNegativeButton(Temp.TT("DT16"), (s, a) =>
                {

                });
                b.SetCancelable(false);

                Dialog d = b.Create();
                d.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(SystemUiFlags.HideNavigation | SystemUiFlags.Fullscreen | SystemUiFlags.LowProfile | SystemUiFlags.ImmersiveSticky);
                d.Show();
            }
            catch { }
        }
        private async void Scan()
        {
            try
            {
                scanner.AutoFocus();
                var result = await scanner.Scan(new MobileBarcodeScanningOptions { UseNativeScanning = true });

                HandleScanResultLogin(result);
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "Scan failed !!!" + ex.ToString(), ToastLength.Long).Show();
            }
        }
        private void HandleScanResultLogin(ZXing.Result result)
        {
            if (result != null && !string.IsNullOrEmpty(result.Text))
            {
                Vibrator vibrator = (Vibrator)GetSystemService(VibratorService);
                vibrator.Vibrate(100);

                ShowData(result.Text.Substring(0,8));
            }
        }
        private void SelectDept(int i)
        {
            Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);
            Dialog dg = new Dialog(this);

            List<string> ls = new List<string>();

            switch (i)
            {
                case 1:
                    ls = Temp.Location.Select().Select(s => s[0].ToString()).Distinct().ToList();
                    break;
                case 2:
                    ls = Temp.Location.Select("fac = '" + f + "'").Select(s => s[1].ToString()).Distinct().ToList();
                    break;
                case 3:
                    ls = Temp.Location.Select("fac = '" + f + "' and dept = '" + d + "'").Select(s => s[2].ToString()).Distinct().ToList();
                    break;
            }

            b.SetSingleChoiceItems(ls.ToArray(), -1, (s, a) =>
            {
                switch (i)
                {
                    case 1:
                        f = ls[a.Which];

                        SelectDept(2);
                        break;
                    case 2:
                        d = ls[a.Which];

                        SelectDept(3);
                        break;
                    case 3:
                        s = ls[a.Which];

                        facline = f + "/" + d + "/" + s;
                        edarea.Text = facline;

                        LoadList("exec GetData 10,'" + facline + "','',''");
                        break;
                }

                dg.Dismiss();
            });

            dg = b.Create();
            dg.Show();
        }
        private void LoadList(string query)
        {
            if (facline != "")
            {
                curly = null; if (dt.Rows.Count > 0) { dt.Rows.Clear(); Main.Rows.Clear(); }

                DataTable d = kn.Doc(query).Tables[0];

                Main = d; dt = Main.DefaultView.ToTable();

                BaseAdapter adapter = new A1ATeam.ListViewAdapterWithNoLayout(dt.DefaultView.ToTable(false, "McSerialNumber", "McName", "McType", "McBrand", "McModel", "Status", "ManuSerialNo", "Factory", "CurrentLocation"), new List<int> { }, true)
                { TextSize = 10/* LayoutRequest.TextSize(20)*/ };

                ls.Adapter = adapter; txtqty.Text = "Machine Qty : " + dt.Rows.Count;

                Toast.MakeText(this, "Found Machine : " + dt.Rows.Count, ToastLength.Long).Show();
            }
        }
        private void ShowData(string mcid, DataRow row = null)
        {
            DataRow r = row;

            if (r is null)
            {
                DataTable d = kn.Doc("exec GetData 12,'" + mcid + "','',''").Tables[0];

                if (d.Rows.Count > 0) r = d.Rows[0];
            }

            if (r != null)
            {
                string mc = r["McSerialNumber"].ToString();
                DataTable spmk = kn.Doc("select * from MachineSPMK where McSerialNumber = '" + mc + "' and NewLocation is null").Tables[0];

                Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);
                Dialog dg = new Dialog(this);

                ScrollView main = new ScrollView(this);
                LinearLayout ln = new LinearLayout(this) { Orientation = Orientation.Vertical }; main.AddView(ln);

                ViewGroup.MarginLayoutParams mr = new ViewGroup.MarginLayoutParams(-2, -2);
                mr.SetMargins(LayoutRequest.Widht(5), LayoutRequest.Height(5), 0, 0);

                for (int i = 0; i < r.Table.Columns.Count; i++)
                {
                    LinearLayout l = new LinearLayout(this) { Orientation = Orientation.Horizontal };

                    TextView txt1 = new TextView(this) { Text = r.Table.Columns[i].ColumnName + " : ", LayoutParameters = mr }; txt1.SetTextColor(Color.Blue); l.AddView(txt1);
                    TextView txt2 = new TextView(this) { Text = r[i].ToString(), LayoutParameters = mr }; txt2.SetTextColor(Color.DeepPink); l.AddView(txt2);

                    ln.AddView(l);
                }

                LinearLayout ln2 = new LinearLayout(this) { Orientation = Orientation.Horizontal };
                Button bt1 = new Button(this) { Text = "ADD TO LINE", LayoutParameters = mr }; bt1.Enabled = false; ln2.AddView(bt1);
                Button bt2 = new Button(this) { Text = "MOVE TO SPMK", LayoutParameters = mr }; bt2.Enabled = false; ln2.AddView(bt2);
                Button bt3 = new Button(this) { Text = "EXIT", LayoutParameters = mr }; ln2.AddView(bt3);
                ln.AddView(ln2);

                string location = r["CurrentLocation"].ToString();
                if (facline.Equals(location))
                {
                    if (level != "0") bt2.Enabled = true;
                }
                else
                {
                    if (location.Contains("/"))
                    {
                        if (location.Split('/')[1] == "Sewing Line")
                        {
                            if (location.Substring(0, 4) == facline.Substring(0, 4)) bt1.Enabled = true;
                            else if (spmk.Rows.Count > 0 && level != "0") bt1.Enabled = true;
                        }
                        else bt1.Enabled = true;
                    }
                    else bt1.Enabled = true;
                }

                bt3.Click += delegate { dg.Dismiss(); };
                bt1.Click += delegate
                {
                    string qr = "update Overview set CurrentLocation = '" + facline + "',Remark = N'" + user + "' where McSerialNumber = '" + mc + "' \n";

                    if (spmk.Rows.Count > 0) qr += " insert into LocaHistory values (getdate(),'" + mc + "','MC SPMK','" + facline + "',N'" + user + "') \n" +
                                                    " update MachineSPMK set NewLocation = '" + facline + "',ReceivedBy = N'" + user + "',ReceivedDate = getdate() where McSerialNumber = '" + mc + "'";
                    else qr += " insert into LocaHistory values (getdate(),'" + mc + "','" + location + "','" + facline + "',N'" + user + "')";

                    kn.Ghi(qr);

                    if (kn.ErrorMessage == "")
                    {
                        Toast.MakeText(this, "Done !!!", ToastLength.Long).Show();

                        LoadList("exec GetData 10,'" + facline + "','',''"); rdg.Visibility = ViewStates.Gone;
                    }
                    else Toast.MakeText(this, "Failed !!! " + kn.ErrorMessage, ToastLength.Long).Show();

                    dg.Dismiss();
                };
                bt2.Click += delegate
                {
                    if (spmk.Rows.Count == 0)
                    {
                        string qr = "update Overview set CurrentLocation = '',Remark = N'" + user + "' where McSerialNumber = '" + mc + "' \n" +
                                " insert into LocaHistory values (getdate(),'" + mc + "','" + facline + "','MC SPMK',N'" + user + "') \n" +
                                "insert into MachineSPMK values ('" + DateTime.Now.ToString("yyyyMMdd") + "','" + mc + "','" + facline + "',N'" + user + "',null,null,null,getdate())";

                        kn.Ghi(qr);

                        if (kn.ErrorMessage == "")
                        {
                            Toast.MakeText(this, "Done !!!", ToastLength.Long).Show();

                            LoadList("exec GetData 10,'" + facline + "','',''"); rdg.Visibility = ViewStates.Gone;
                        }
                        else Toast.MakeText(this, "Failed !!! " + kn.ErrorMessage, ToastLength.Long).Show();
                    }

                    dg.Dismiss();
                };

                b.SetCancelable(false);
                b.SetView(main);

                dg = b.Create();
                dg.Show();
            }
            else Toast.MakeText(this, "Not found Machine ID : " + mcid + " !!!", ToastLength.Long).Show();
        }
        public View GetViewByPosition(int pos, ListView listView)
        {
            try
            {
                int firstListItemPosition = listView.FirstVisiblePosition;
                int lastListItemPosition = firstListItemPosition + listView.Adapter.Count - 1;

                if (pos < firstListItemPosition || pos > lastListItemPosition)
                {
                    return listView.Adapter.GetView(pos, null, listView);
                }
                else
                {
                    int childIndex = pos - firstListItemPosition;
                    return listView.GetChildAt(childIndex);
                }
            }
            catch (System.Exception ex)
            {
                Toast.MakeText(this, ex.ToString(), ToastLength.Long).Show();
                return null;
            }
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
                        try
                        {
                            TextView vv = v as TextView;

                            DataRow r = Temp.NgonNgu.Select().Where(d => d[Temp.Oldlg].ToString() == vv.Text).FirstOrDefault() as DataRow;

                            vv.Text = r[Temp.Newlg].ToString();

                            vv.TextSize = LayoutRequest.TextSize(vv.TextSize);
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

                            vv.TextSize = LayoutRequest.TextSize(vv.TextSize);
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
            catch (Exception ex)
            {
                Toast.MakeText(this, "Set Language failed !!!" + ex.ToString(), ToastLength.Long).Show();
            }
        }
    }
}