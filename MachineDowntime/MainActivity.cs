using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidScreenStretching;
using CSDL;
using ZXing.Mobile;
using static Android.App.ActionBar;
using static Android.Content.ClipData;

namespace MachineDowntime
{
    [Activity(Label = "MACHINE DOWNTIME", Theme = "@style/Theme.AppCompat.Light.NoActionBar", ScreenOrientation = ScreenOrientation.SensorPortrait, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)]
    public class MainActivity : Activity
    {
        RelativeLayout layout;
        TextView txtfacline, txtleader, txtdate, txtten;
        ListView lstview, listtitle;
        Button btadd, btxoa, btstart, btfinish, btdoimk, btdoido;
        Timer tm = new Timer();
        Connect kn = new Connect(Temp.chuoi);
        List<ListMachineBroken> ls_broken = new List<ListMachineBroken>();
        List<string> ls_hu = new List<string>();
        List<string> ls_doido = new List<string>();
        List<string> ls_repair = new List<string>();
        MobileBarcodeScanner scanner;
        DataTable data = new DataTable();
        int fun = 0;
        Android.App.AlertDialog.Builder d_suamay, d_xoa, d_them, d_ketthuc, d_doido, d_batdau;
        List<Android.App.AlertDialog.Builder> ls_d = new List<AlertDialog.Builder>();
        string mcid = ""; string mechanic = "";
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.main);
            MobileBarcodeScanner.Initialize(Application);
            scanner = new MobileBarcodeScanner();

            Window.AddFlags(WindowManagerFlags.Fullscreen);
            Window.AddFlags(WindowManagerFlags.TranslucentNavigation);
            //Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(SystemUiFlags.HideNavigation | SystemUiFlags.Fullscreen | SystemUiFlags.LowProfile | SystemUiFlags.ImmersiveSticky);

            layout = FindViewById<RelativeLayout>(Resource.Id.layoutmain);

            txtfacline = FindViewById<TextView>(Resource.Id.txtfacline);
            txtleader = FindViewById<TextView>(Resource.Id.txtleader);
            txtdate = FindViewById<TextView>(Resource.Id.txtdate);
            txtten = FindViewById<TextView>(Resource.Id.txtten);

            btadd = FindViewById<Button>(Resource.Id.btadd);
            btxoa = FindViewById<Button>(Resource.Id.btxoa);
            btstart = FindViewById<Button>(Resource.Id.btstart);
            btfinish = FindViewById<Button>(Resource.Id.btfinish);
            btdoimk = FindViewById<Button>(Resource.Id.btchangepassword);
            btdoido = FindViewById<Button>(Resource.Id.btdoido);

            lstview = FindViewById<ListView>(Resource.Id.listView1);
            listtitle = FindViewById<ListView>(Resource.Id.listtitle);

            listtitle.Adapter = new ListMachineBroken_Adapter(new List<ListMachineBroken>
            {
                new ListMachineBroken
                {
                    MaMay = Temp.NgonNgu.Select().Where(x => x[0].ToString() == "DT29").FirstOrDefault()[Temp.Newlg].ToString(),
                    BatDauHu = Temp.NgonNgu.Select().Where(x => x[0].ToString() == "DT30").FirstOrDefault()[Temp.Newlg].ToString(),
                    BatDauSua = Temp.NgonNgu.Select().Where(x => x[0].ToString() == "DT31").FirstOrDefault()[Temp.Newlg].ToString(),
                    KetThuc = Temp.NgonNgu.Select().Where(x => x[0].ToString() == "DT32").FirstOrDefault()[Temp.Newlg].ToString(),
                    DT = "DT",
                    TrangThai = Temp.NgonNgu.Select().Where(x => x[0].ToString() == "DT33").FirstOrDefault()[Temp.Newlg].ToString(),
                    ThoMay = Temp.NgonNgu.Select().Where(x => x[0].ToString() == "DT34").FirstOrDefault()[Temp.Newlg].ToString(),
                    DoiDo = Temp.NgonNgu.Select().Where(x => x[0].ToString() == "DT35").FirstOrDefault()[Temp.Newlg].ToString(),
                    ThoiGian = Temp.NgonNgu.Select().Where(x => x[0].ToString() == "DT36").FirstOrDefault()[Temp.Newlg].ToString()
                }
            }, 1);
            //ScreenStretching.Stretching(Temp.metric, layout, this, false);
            //Temp.Density(layout);

            if (Temp.dept == "Mechanic") { btadd.Visibility = ViewStates.Gone; txtten.Text = "DT92"; }

            d_doido = new AlertDialog.Builder(this); ls_d.Add(d_doido);
            d_ketthuc = new AlertDialog.Builder(this); ls_d.Add(d_ketthuc);
            d_suamay = new AlertDialog.Builder(this); ls_d.Add(d_suamay);
            d_them = new AlertDialog.Builder(this); ls_d.Add(d_them);
            d_xoa = new AlertDialog.Builder(this); ls_d.Add(d_xoa);
            d_batdau = new AlertDialog.Builder(this); ls_d.Add(d_batdau);

            tm.Interval = 1000;
            tm.Elapsed += delegate { RunOnUiThread(() => { txtdate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"); }); };
            tm.Enabled = true;
            tm.Start();

            Temp.Oldlg = 0;

            try
            {
                txtfacline.Text = Temp.fac + "/" + Temp.facline; bool b = Temp.facline.Contains('"');
                txtleader.Text = Temp.Login.Rows[0]["Name"].ToString();
            }
            catch { }

            btadd.Click += delegate { fun = 1; Them(); };
            btxoa.Click += delegate { fun = 2; Xoa(); };
            btstart.Click += delegate { fun = 3; Start(); };
            btfinish.Click += delegate { fun = 4; KT(); };
            btdoido.Click += delegate { fun = 5; Exchange(); };
            btdoimk.Click += delegate
            {
                DoiMatKhau();
            };
            txtdate.Click += delegate { Toast.MakeText(this, Temp.msg, ToastLength.Long).Show(); };

            lstview.ItemLongClick += Lstview_ItemLongClick;
            lstview.ItemClick += Lstview_ItemClick;

            LoadData();
            //Toast.MakeText(this, Temp.msg, ToastLength.Long).Show();

            SetLanguage();
        }
        private void Lstview_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                ListMachineBroken item = ls_broken[e.Position];
                if (item.KetThuc == "")
                {
                    mcid = item.MaMay.Contains("\n") ? item.MaMay.Split("\n")[1] : item.MaMay;
                    Toast.MakeText(this,mcid + item.MaMay,ToastLength.Long).Show();
                    string[] l = { Temp.TT("DT25"), Temp.TT("DT26"), Temp.TT("DT27"), Temp.TT("DT28") };// { "XÓA", "SỬA MÁY", "KẾT THÚC", "ĐỔI ĐỒ" };
                    Android.App.AlertDialog.Builder bb = new Android.App.AlertDialog.Builder(this);

                    bb.SetSingleChoiceItems(l, -1, (ss, ee) =>
                    {
                        Dialog d = ss as Dialog;

                        switch (ee.Which)
                        {
                            case 0:
                                fun = 2;
                                d.Dismiss();
                                Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);

                                b.SetMessage(Temp.TT("DT65"));// "Bạn muốn xóa Mã Máy (" + mcid + ") này khỏi danh sách máy hư ?");
                                b.SetPositiveButton(Temp.TT("DT25"), (s, a) =>//"XÓA"
                                {
                                    kn.Ghi("delete from DowntimeReport where McSerialNo = '" + mcid + "' and FinishTime is null");

                                    LoadData();

                                });
                                b.SetNegativeButton(Temp.TT("DT63"), (s, a) => { });//"HỦY"

                                b.Create().Show();
                                break;
                            case 1:
                                fun = 3;
                                d.Dismiss();
                                SuaMay();
                                break;
                            case 2:
                                fun = 4;
                                d.Dismiss();
                                KetThuc();
                                break;
                            case 3:
                                fun = 5;
                                d.Dismiss();
                                DoiDo();
                                break;
                        }
                    });
                    bb.SetPositiveButton(Temp.TT("DT16"), (ss, ee) => { });//"THOÁT"
                    bb.SetCancelable(false);
                    Dialog dd = bb.Create();
                    dd.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(SystemUiFlags.HideNavigation | SystemUiFlags.Fullscreen | SystemUiFlags.LowProfile | SystemUiFlags.ImmersiveSticky);
                    dd.Show();
                }
            }
            catch { }
        }

        private void Start()
        {
            try
            {
                ArrayAdapter array = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItemSingleChoice, ls_hu);

                LinearLayout v = new LinearLayout(this)
                {
                    Orientation = Orientation.Vertical,
                    LayoutParameters = new LayoutParams(500, 600)
                };
                EditText edsearch = new EditText(this)
                {
                    Left = 10,
                    Top = 10,
                    InputType = Android.Text.InputTypes.TextFlagCapCharacters,
                    LayoutParameters = new LayoutParams(500, 100),
                    Hint = Temp.TT("DT42")
                };
                //edsearch.TextChanged += (s, e) => array.Filter.InvokeFilter(edsearch.Text);
                ListView lsv = new ListView(this)
                {
                    Left = 10,
                    Top = 100,
                    LayoutParameters = new LayoutParams(500, 600)
                };

                lsv.Adapter = array;

                edsearch.TextChanged += delegate
                {
                    array = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItemSingleChoice, ls_hu.FindAll(d => d.Contains(edsearch.Text)));
                    lsv.Adapter = array;
                };

                v.AddView(edsearch);
                v.AddView(lsv);
                d_suamay.SetView(v);

                d_suamay.SetPositiveButton(Temp.TT("DT16"), (ss, ee) => { });
                d_suamay.SetNeutralButton(Temp.TT("DT43"), (ss, ee) => { Scan(); });

                d_suamay.SetCancelable(false);

                Dialog dd = d_suamay.Create();
                dd.Show();

                lsv.ItemClick += (ss, ee) =>
                {
                    mcid = lsv.GetItemAtPosition(ee.Position).ToString();
                    SuaMay();
                    Temp.HideKeyBoard(this, CurrentFocus);
                    dd.Dismiss();
                };
            }
            catch { }
        }
        private void KT()
        {
            try
            {
                ArrayAdapter array = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItemSingleChoice, ls_hu);

                LinearLayout v = new LinearLayout(this)
                {
                    Orientation = Orientation.Vertical,
                    LayoutParameters = new LayoutParams(500, 600)
                };
                EditText edsearch = new EditText(this)
                {
                    Left = 10,
                    Top = 10,
                    InputType = Android.Text.InputTypes.TextFlagCapCharacters,
                    LayoutParameters = new LayoutParams(500, 100),
                    Hint = Temp.TT("DT42")
                };
                //edsearch.TextChanged += (s, e) => array.Filter.InvokeFilter(edsearch.Text);
                ListView lsv = new ListView(this)
                {
                    Left = 10,
                    Top = 100,
                    LayoutParameters = new LayoutParams(500, 600)
                };

                lsv.Adapter = array;

                edsearch.TextChanged += delegate
                {
                    array = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItemSingleChoice, ls_hu.FindAll(d => d.Contains(edsearch.Text)));
                    lsv.Adapter = array;
                };

                v.AddView(edsearch);
                v.AddView(lsv);
                d_ketthuc.SetView(v);

                d_ketthuc.SetPositiveButton(Temp.TT("DT16"), (ss, ee) => { });
                d_ketthuc.SetNeutralButton(Temp.TT("DT43"), (ss, ee) => { Scan(); });

                d_ketthuc.SetCancelable(false);
                Dialog dd = d_ketthuc.Create();
                dd.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(SystemUiFlags.HideNavigation | SystemUiFlags.Fullscreen | SystemUiFlags.LowProfile | SystemUiFlags.ImmersiveSticky);
                dd.Show();

                lsv.ItemClick += (ss, ee) =>
                {
                    mcid = lsv.GetItemAtPosition(ee.Position).ToString();
                    KetThuc();
                    Temp.HideKeyBoard(this, CurrentFocus);
                    dd.Dismiss();
                };
            }
            catch { }
        }
        private void Xoa()
        {
            try
            {
                ArrayAdapter array = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItemSingleChoice, ls_hu);

                LinearLayout v = new LinearLayout(this)
                {
                    Orientation = Orientation.Vertical,
                    LayoutParameters = new LayoutParams(500, 600)
                };
                EditText edsearch = new EditText(this)
                {
                    Left = 10,
                    Top = 10,
                    InputType = Android.Text.InputTypes.TextFlagCapCharacters,
                    LayoutParameters = new LayoutParams(500, 100),
                    Hint = Temp.TT("DT42")
                };
                //edsearch.TextChanged += (s, e) => array.Filter.InvokeFilter(edsearch.Text);
                ListView lsv = new ListView(this)
                {
                    Left = 10,
                    Top = 100,
                    LayoutParameters = new LayoutParams(500, 600)
                };

                lsv.Adapter = array;

                edsearch.TextChanged += delegate
                {
                    array = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItemSingleChoice, ls_hu.FindAll(d => d.Contains(edsearch.Text)));
                    lsv.Adapter = array;
                };

                v.AddView(edsearch);
                v.AddView(lsv);
                d_xoa.SetView(v);

                d_xoa.SetPositiveButton(Temp.TT("DT16"), (ss, ee) => { });
                d_xoa.SetNeutralButton(Temp.TT("DT43"), (ss, ee) => { Scan(); });

                d_xoa.SetCancelable(false);
                Dialog dd = d_xoa.Create();
                dd.Show();

                lsv.ItemClick += (ss, ee) =>
                {
                    mcid = lsv.GetItemAtPosition(ee.Position).ToString();
                    Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);

                    b.SetMessage(Temp.TT("DT65"));
                    b.SetPositiveButton(Temp.TT("DT64"), (s, a) =>
                    {
                        kn.Ghi("delete from DowntimeReport where McSerialNo = '" + mcid + "' and FinishTime is null");

                        LoadData();

                    });
                    b.SetNegativeButton(Temp.TT("DT63"), (s, a) => { });

                    b.Create().Show();

                    Temp.HideKeyBoard(this, CurrentFocus);
                    dd.Dismiss();
                };
            }
            catch { }
        }
        private void Exchange()
        {
            try
            {
                ArrayAdapter array = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItemSingleChoice, ls_hu);

                LinearLayout v = new LinearLayout(this)
                {
                    Orientation = Orientation.Vertical,
                    LayoutParameters = new LayoutParams(500, 600)
                };
                EditText edsearch = new EditText(this)
                {
                    Left = 10,
                    Top = 10,
                    InputType = Android.Text.InputTypes.TextFlagCapCharacters,
                    LayoutParameters = new LayoutParams(500, 100),
                    Hint = Temp.TT("DT42")
                };
                //edsearch.TextChanged += (s, e) => array.Filter.InvokeFilter(edsearch.Text);
                ListView lsv = new ListView(this)
                {
                    Left = 10,
                    Top = 100,
                    LayoutParameters = new LayoutParams(500, 600)
                };

                lsv.Adapter = array;

                edsearch.TextChanged += delegate
                {
                    array = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItemSingleChoice, ls_hu.FindAll(d => d.Contains(edsearch.Text)));
                    lsv.Adapter = array;
                };

                v.AddView(edsearch);
                v.AddView(lsv);
                d_doido.SetView(v);

                d_doido.SetPositiveButton(Temp.TT("DT16"), (ss, ee) => { });
                d_doido.SetNeutralButton(Temp.TT("DT43"), (ss, ee) => { Scan(); });

                d_doido.SetCancelable(false);
                Dialog dd = d_doido.Create();
                dd.Show();

                lsv.ItemClick += (ss, ee) =>
                {
                    mcid = lsv.GetItemAtPosition(ee.Position).ToString();
                    DoiDo();
                    Temp.HideKeyBoard(this, CurrentFocus);
                    dd.Dismiss();
                };
            }
            catch { }
        }
        private void Them()
        {
            if (Temp.dept.Contains("Sewing Line"))
            {
                try
                {
                    DataTable lstmay = kn.Doc("exec LoadMachine2 '" + Temp.fac + "','" + Temp.facline + "'").Tables[0] ?? new DataTable();

                    List<string> ls = lstmay.Rows.OfType<DataRow>().Select(dr => dr["McID"].ToString()).Distinct().ToList();

                    ArrayAdapter array = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItemSingleChoice, ls);

                    LinearLayout v = new LinearLayout(this)
                    {
                        Orientation = Orientation.Vertical,
                        LayoutParameters = new LayoutParams(500, 600)
                    };
                    EditText edsearch = new EditText(this)
                    {
                        Left = 10,
                        Top = 10,
                        InputType = Android.Text.InputTypes.TextFlagCapCharacters,
                        LayoutParameters = new LayoutParams(500, 100),
                        Hint = Temp.TT("DT42")
                    };
                    ListView lsv = new ListView(this)
                    {
                        Left = 10,
                        Top = 100,
                        LayoutParameters = new LayoutParams(500, 600)
                    };

                    lsv.Adapter = array;

                    edsearch.TextChanged += delegate
                    {
                        array = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItemSingleChoice, ls.FindAll(d => d.Contains(edsearch.Text)));
                        lsv.Adapter = array;
                    };

                    v.AddView(edsearch);
                    v.AddView(lsv);
                    d_them.SetView(v);

                    d_them.SetPositiveButton(Temp.TT("DT73"), (ss, ee) => { AddNew(edsearch.Text); });
                    d_them.SetNegativeButton(Temp.TT("DT16"), (ss, ee) => { });
                    d_them.SetNeutralButton(Temp.TT("DT43"), (ss, ee) => { Scan(); });

                    d_them.SetCancelable(false);
                    Dialog dd = d_them.Create();
                    dd.Show();

                    lsv.ItemClick += (ss, ee) =>
                    {
                        AddNew(lsv.GetItemAtPosition(ee.Position).ToString());
                        Temp.HideKeyBoard(this, CurrentFocus);
                        dd.Dismiss();
                    };
                }
                catch { }
            }
            else Toast.MakeText(this, "Your are Mechanic, you cannot do this action !!!", ToastLength.Long).Show();
        }
        private void LoadData()
        {
            try
            {
                string ch = "";
                string level = "";
                try
                {
                    string facline = "(";
                    level = Temp.Login.Rows[0]["Levels"].ToString();
                    string area = Temp.Login.Rows[0]["Area"].ToString();

                    if (area == "A")
                    {
                        for (int j = 1; j < 16; j++) area += j.ToString("00") + ",";
                        area += "33,PPA";
                    }
                    if (area == "B")
                    {
                        for (int j = 16; j < 32; j++) area += j.ToString("00") + ",";
                    }

                    if (level == "1")
                    {
                        foreach (string str in area.Split(','))
                        {
                            if (str != "") facline += "'" + Temp.facline.Substring(0, Temp.facline.Length - 3) + str + "',";
                        }

                        facline = facline.Substring(0, facline.Length - 1) + ")";
                        ch = "select * from DowntimeReport where FacLine = '" + Temp.facline + "' and FinishTime is null" +
                        " union select * from DowntimeReport where FacLine in " + facline + " and FinishTime > '" + DateTime.Now.ToString("yyyyMMdd") + "' " +
                        " order by FinishTime asc,SystemCreateDate desc";
                    }
                    else if (level == "2" || level == "10")
                    {
                        ch = "select * from DowntimeReport where FinishTime is null" +
                        " union select * from DowntimeReport where FinishTime > '" + DateTime.Now.ToString("yyyyMMdd") + "' " +
                        " order by FinishTime asc,SystemCreateDate desc";
                    }
                    else
                    {
                        ch = "select * from DowntimeReport where FacLine = '" + Temp.facline + "' and FinishTime is null" +
                        " union select * from DowntimeReport where FacLine = '" + Temp.facline + "' and FinishTime > '" + DateTime.Now.ToString("yyyyMMdd") + "' " +
                        " order by FinishTime asc,SystemCreateDate desc";
                    }
                }
                catch
                {
                    ch = "select * from DowntimeReport where FacLine = '" + Temp.facline + "' and FinishTime is null" +
                        " union select * from DowntimeReport where FacLine = '" + Temp.facline + "' and FinishTime > '" + DateTime.Now.ToString("yyyyMMdd") + "' " +
                        " order by FinishTime asc,SystemCreateDate desc";
                }

                mcid = "";
                data = kn.Doc(ch).Tables[0];
                if (ls_broken.Count > 0) ls_broken.Clear();
                if (ls_repair.Count > 0) ls_repair.Clear();
                if (ls_hu.Count > 0) ls_hu.Clear();
                if (ls_doido.Count > 0) ls_doido.Clear();

                foreach (DataRow r in data.Rows)
                {
                    ls_broken.Add(new ListMachineBroken
                    {
                        MaMay = (int.Parse(level) > 0 ? r["FacLine"].ToString() + "\n" : "") + r["McSerialNo"].ToString(),
                        BatDauHu = r["OccurTime"].ToString() == "" ? "" : DateTime.Parse(r["OccurTime"].ToString()).ToString("dd/MM HH:mm"),
                        BatDauSua = r["StartTime"].ToString() == "" ? "" : DateTime.Parse(r["StartTime"].ToString()).ToString("dd/MM HH:mm"),
                        KetThuc = r["FinishTime"].ToString() == "" ? "" : DateTime.Parse(r["FinishTime"].ToString()).ToString("dd/MM HH:mm"),
                        DT = r["DT"].ToString(),
                        TrangThai = r["Status"].ToString(),
                        ThoMay = GetName(r["Mechanic"].ToString()),
                        DoiDo = r["ExchangeTimes"].ToString(),
                        ThoiGian = r["TotalTime_Min"].ToString()
                    });
                    if (r["FinishTime"].ToString() == "") ls_hu.Add(r["McSerialNo"].ToString());

                    if (r["StartTime"].ToString() != "" && r["FinishTime"].ToString() == "") ls_repair.Add(r["McSerialNo"].ToString());
                    if (r["Status"].ToString() == "Đổi Đồ") ls_doido.Add(r["McSerialNo"].ToString());
                }
                lstview.Adapter = null;
                LoadList();

                //ch += " rows : " + data.Rows.Count;

                //Android.App.AlertDialog.Builder b= new AlertDialog.Builder(this);
                //b.SetMessage(ch);
                //b.Create().Show();
            }
            catch
            {

            }
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
        private void Lstview_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            try
            {
                Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);

                b.SetMessage(Temp.TT("DT65"));
                b.SetPositiveButton(Temp.TT("DT64"), (s, a) =>
                {
                    ListMachineBroken it = ls_broken[e.Position];
                    kn.Ghi("delete from DowntimeReport where McSerialNo = '" + (it.MaMay.Contains("\n") ? it.MaMay.Split("\n")[1] : it.MaMay) + "' and FinishTime is null");
                    LoadData();
                });
                b.SetNegativeButton(Temp.TT("DT63"), (s, a) => { });
                b.Create().Show();
            }
            catch
            {
            }
        }

        private void LoadList()
        {
            try
            {
                if (ls_broken.Count > 0)
                {
                    lstview.Adapter = new ListMachineBroken_Adapter(ls_broken, 2);
                }
            }
            catch
            {

            }
        }

        private async void Scan(bool mc = true)
        {
            try
            {
                foreach (AlertDialog.Builder b in ls_d)
                {
                    Dialog d = b.Create();
                    if (d.IsShowing) d.Dismiss();
                }

                scanner.AutoFocus();
                var result = await scanner.Scan(new MobileBarcodeScanningOptions { UseNativeScanning = true });

                HandleScanResultLogin(result, mc);
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "Scan failed !!!" + ex.ToString(), ToastLength.Long).Show();
            }
        }
        private void HandleScanResultLogin(ZXing.Result result, bool mc)
        {
            if (result != null && !string.IsNullOrEmpty(result.Text))
            {
                Vibrator vibrator = (Vibrator)GetSystemService(VibratorService);
                vibrator.Vibrate(100);

                try
                {
                    string bc = "";
                    if (result.Text.Contains('|')) bc = result.Text.Split('|').First().Trim().ToUpper();
                    else bc = result.Text.ToUpper();
                    switch (fun)
                    {
                        case 1:
                            AddNew(bc);
                            break;
                        case 2:
                            ListMachineBroken item = ls_broken.Find(d => d.MaMay.Contains(bc));

                            if (item.KetThuc == "")
                            {
                                ls_broken.Remove(item);
                                kn.Ghi("delete from DowntimeReport where McSerialNo = '" + bc + "' and FinishTime is null");

                                LoadData();
                            }
                            break;
                        case 3:
                            //if (bc.Contains(Temp.TT("DT62")))//A1A
                            if (!mc)//Employee scan
                            {
                                string name = "";

                                try
                                {
                                    string n = kn.Doc("exec GetData 15,'" + bc + "','',''").Tables[0].Rows[0][0].ToString();
                                    if (n != "") name = n;
                                }
                                catch { }

                                if (name == "")
                                {
                                    Toast.MakeText(this, Temp.TT("DT61"), ToastLength.Long).Show();
                                    mechanic = "";
                                    SuaMay();
                                }
                                else
                                {
                                    mechanic = bc;
                                    SuaMay();
                                }
                            }
                            else//machine scan
                            {
                                bool broken = ls_broken.Exists(d => d.MaMay.Contains(bc));
                                bool repair = ls_repair.Contains(bc);

                                Toast.MakeText(this, broken.ToString(), ToastLength.Long).Show();
                                if (repair) Toast.MakeText(this, Temp.TT("DT119"), ToastLength.Long).Show();//Máy này đang sửa, xin kiểm tra lại !!!
                                else
                                {
                                    if (broken)
                                    {
                                        mcid = bc;
                                        SuaMay();
                                    }
                                    else
                                    {
                                        AlertDialog.Builder b = new AlertDialog.Builder(this);

                                        b.SetMessage(Temp.TT("DT120"));//Máy này vẫn chưa trong danh sách máy hư, bạn có muốn thêm vào danh sách không ?
                                        b.SetPositiveButton(Temp.TT("DT23"), (s, a) =>
                                         {
                                             AddNew(bc);

                                             mcid = bc;
                                             SuaMay();
                                         });
                                        b.SetNegativeButton(Temp.TT("DT16"), (s, a) => { });

                                        b.Create().Show();
                                    }
                                }
                            }
                            break;
                        case 4:
                            mcid = bc;
                            KetThuc();
                            break;
                        case 5:
                            mcid = bc;
                            DoiDo();
                            break;
                    }
                }
                catch (Exception ex) { Toast.MakeText(this, "scan " + ex.ToString(), ToastLength.Long).Show(); }
            }
        }
        private void AddNew(string bc)
        {
            try
            {
                DataTable dt = kn.Doc("select * from OverView where McSerialNumber = '" + bc + "'").Tables[0];
                DataTable check = kn.Doc("select * from DowntimeReport where McSerialNo = '" + bc + "' and FinishTime is null").Tables[0];

                if (check.Rows.Count > 0) Toast.MakeText(this, Temp.TT("DT60"), ToastLength.Long).Show();
                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        string ngay = DateTime.Now.ToString("yyyyMMdd HH:mm");

                        kn.Proc("InsertDowntime", new List<string> { "@mcno=" + bc, "@fac=" + Temp.fac,
                            "@line=" + Temp.facline, "@occur=" + ngay, "@status="+Temp.TT("DT59"), "@leader=" + Temp.user });

                        if (kn.ErrorMessage != "") Toast.MakeText(this, "Add new " + kn.ErrorMessage, ToastLength.Long).Show();

                        LoadData();
                    }
                    else
                        Toast.MakeText(this, Temp.TT("DT58"), ToastLength.Long).Show();
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "Add new " + ex.ToString(), ToastLength.Long).Show();
            }
        }
        private void SuaMay()
        {
            try
            {
                LinearLayout view = new LinearLayout(this)
                {
                    Orientation = Orientation.Vertical
                };
                TextView mc = new TextView(this)
                {
                    Text = Temp.TT("DT56"),
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                };
                view.AddView(mc);
                EditText edmc = new EditText(this)
                {
                    Text = mcid,
                    LayoutParameters = new ViewGroup.LayoutParams(600, ViewGroup.LayoutParams.WrapContent),
                    Focusable = false
                };
                edmc.Click += delegate
                {
                    try
                    {
                        ArrayAdapter array = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItemSingleChoice, ls_hu);

                        Android.App.AlertDialog.Builder bb = new Android.App.AlertDialog.Builder(this);
                        LinearLayout v = new LinearLayout(this)
                        {
                            Orientation = Orientation.Vertical,
                            LayoutParameters = new LayoutParams(500, 600)
                        };
                        EditText edsearch = new EditText(this)
                        {
                            Left = 10,
                            Top = 10,
                            InputType = Android.Text.InputTypes.TextFlagCapCharacters,
                            LayoutParameters = new LayoutParams(500, 100),
                            Hint = "Search"
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
                        bb.SetView(v);

                        bb.SetPositiveButton("EXIT", (ss, ee) => { });
                        bb.SetNeutralButton("SCAN", (ss, ee) => { Scan(); });

                        bb.SetCancelable(false);
                        Dialog dd = bb.Create();
                        dd.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(SystemUiFlags.HideNavigation | SystemUiFlags.Fullscreen | SystemUiFlags.LowProfile | SystemUiFlags.ImmersiveSticky);
                        dd.Show();

                        lsv.ItemClick += (ss, ee) =>
                        {
                            edmc.Text = lsv.GetItemAtPosition(ee.Position).ToString();
                            Temp.HideKeyBoard(this, CurrentFocus);
                            dd.Dismiss();
                        };
                    }
                    catch { }
                };
                view.AddView(edmc);
                TextView tho = new TextView(this)
                {
                    Text = Temp.TT("DT57"),
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                };
                view.AddView(tho);
                EditText edtho = new EditText(this)
                {
                    Text = GetName(mechanic),
                    LayoutParameters = new ViewGroup.LayoutParams(600, ViewGroup.LayoutParams.WrapContent),
                    Focusable = false
                };
                view.AddView(edtho);
                tho.LongClick += (s, ee) => { edtho.Focusable = true; };
                d_batdau.SetView(view);

                d_batdau.SetPositiveButton(Temp.TT("DT54"), (ss, aa) =>
                {
                    if (edmc.Text == "" || edtho.Text == "")
                    {
                        Toast.MakeText(this, Temp.TT("DT55"), ToastLength.Long).Show();
                        SuaMay();
                    }
                    else
                    {
                        string ngay = DateTime.Now.ToString("yyyyMMdd HH:mm");

                        kn.Ghi("update DowntimeReport set StartTime = '" + ngay + "',Mechanic = '" + edtho.Text.Split(" ").First() + "'," +
                            "Status = N'" + Temp.TT("DT77") + "',MRT = datediff(minute,OccurTime,'" + ngay + "') where McSerialNo = '" + edmc.Text + "' and FinishTime is null");

                        if (kn.ErrorMessage != "") Toast.MakeText(this, "Repair " + kn.ErrorMessage, ToastLength.Long).Show();

                        mcid = ""; mechanic = "";

                        LoadData();
                    }
                });
                d_batdau.SetNeutralButton(Temp.TT("DT43"), (ss, aa) => { Scan(false); });
                d_batdau.SetNegativeButton(Temp.TT("DT16"), (ss, aa) => { });
                d_batdau.Create().Show();
            }
            catch (Exception ex) { Toast.MakeText(this, Temp.TT("DT31") + " !!!" + ex.ToString(), ToastLength.Long).Show(); }
        }
        private void KetThuc()
        {
            try
            {
                DataTable mc = kn.Doc("select * from DowntimeReport where McSerialNo = '" + mcid + "' and FinishTime is null").Tables[0];
                if (mc.Rows.Count > 0)
                {
                    if (ls_doido.Contains(mcid))
                    {

                        string id = mc.Rows[0][0].ToString() ?? "";
                        kn.Ghi("exec UpdateDowntimeExchangeReport 2," + id + ",'" + DateTime.Now.ToString("yyyyMMdd HH:mm") + "'");

                        if (kn.ErrorMessage == "") Toast.MakeText(this, Temp.TT("DT46"), ToastLength.Long).Show();
                        else Toast.MakeText(this, Temp.TT("DT45") + " !!!" + kn.ErrorMessage, ToastLength.Long).Show();
                    }
                    if (ls_repair.Contains(mcid))
                    {
                        Temp.mcid = mcid;
                        Temp.OccurTime = DateTime.Parse(mc.Rows[0]["OccurTime"].ToString());
                        Intent intent = new Intent(this, typeof(KetThucActivity));
                        StartActivity(intent);
                        Finish();
                    }
                    else { Toast.MakeText(this, Temp.TT("DT53"), ToastLength.Long).Show(); }
                }
            }
            catch (Exception ex) { Toast.MakeText(this, Temp.TT("DT32") + " !!!" + ex.ToString(), ToastLength.Long).Show(); }
        }
        private void DoiDo()
        {
            try
            {
                DataTable mc = kn.Doc("select * from DowntimeReport where McSerialNo = '" + mcid + "' and FinishTime is null").Tables[0] ?? new DataTable();

                if (mc.Rows.Count == 0) Toast.MakeText(this, Temp.TT("DT51"), ToastLength.Long).Show();
                else if (!ls_repair.Contains(mcid)) Toast.MakeText(this, Temp.TT("DT52"), ToastLength.Long).Show();
                else
                {
                    string id = mc.Rows[0][0].ToString() ?? "";
                    var dt = kn.Doc("select * from DowntimeExchangeReport where McID = '" + id + "' and FinishTime is null").Tables[0] ?? new DataTable();

                    AlertDialog.Builder b = new AlertDialog.Builder(this);

                    if (dt.Rows.Count == 0)
                    {
                        b.SetMessage(Temp.TT("DT49"));
                        b.SetPositiveButton(Temp.TT("DT48"), (ss, ee) =>
                        {
                            kn.Ghi("exec UpdateDowntimeExchangeReport 1," + id + ",'" + DateTime.Now.ToString("yyyyMMdd HH:mm") + "'");

                            if (kn.ErrorMessage == "") Toast.MakeText(this, Temp.TT("DT50"), ToastLength.Long).Show();
                            else Toast.MakeText(this, Temp.TT("DT45") + kn.ErrorMessage, ToastLength.Long).Show();

                            LoadData();
                        });
                        b.SetNegativeButton(Temp.TT("DT44"), (ss, ee) => { });

                        b.SetCancelable(false);
                        b.Create().Show();
                    }
                    else
                    {
                        b.SetMessage(Temp.TT("DT47"));
                        b.SetPositiveButton(Temp.TT("DT48"), (ss, ee) =>
                        {
                            kn.Ghi("exec UpdateDowntimeExchangeReport 2," + id + ",'" + DateTime.Now.ToString("yyyyMMdd HH:mm") + "'");

                            if (kn.ErrorMessage == "") Toast.MakeText(this, Temp.TT("DT46"), ToastLength.Long).Show();
                            else Toast.MakeText(this, Temp.TT("DT45") + kn.ErrorMessage, ToastLength.Long).Show();

                            LoadData();
                        });
                        b.SetNegativeButton(Temp.TT("DT44"), (ss, ee) => { });

                        b.SetCancelable(false);
                        b.Create().Show();
                    }
                }
            }
            catch (Exception ex) { Toast.MakeText(this, Temp.TT("DT28") + " !!!" + ex.ToString(), ToastLength.Long).Show(); }


        }
        private string GetName(string id)
        {
            string name = id;

            try
            {
                string n = kn.Doc("select Name from DownTimeUserList where ID = '" + id + "'").Tables[0].Rows[0][0].ToString();
                if (n != "") name += " - " + n;
            }
            catch { }

            return name;
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
                            TextView vv = v as TextView;
                            try
                            {
                                DataRow r = Temp.NgonNgu.Select().Where(d => d[Temp.Oldlg].ToString() == vv.Text).FirstOrDefault() as DataRow;

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
                                DataRow r = Temp.NgonNgu.Select().Where(d => d[Temp.Oldlg].ToString() == vv.Text).FirstOrDefault() as DataRow;

                                vv.Text = r[Temp.Newlg].ToString();
                            }
                            catch { }
                            vv.TextSize = LayoutRequest.TextSize(vv.TextSize);
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