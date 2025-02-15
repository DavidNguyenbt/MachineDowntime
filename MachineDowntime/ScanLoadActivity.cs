using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using CSDL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Xamarin.Essentials;
using ZXing.Mobile;

namespace MachineDowntime
{
    [Activity(Theme = "@style/Theme.AppCompat.Light.NoActionBar", ScreenOrientation = ScreenOrientation.SensorPortrait)]
    public class ScanLoadActivity : Activity
    {
        RelativeLayout layout;
        Button btscanvao, btchuyenline, bttimpo, btspmk, btchangepo, btcombinepo;
        TextView txtname;
        ListView lst;
        Connect kn = new Connect(Temp.com);
        MobileBarcodeScanner scanner;
        DataSet ds = new DataSet();
        string ou = "TR", url = "";
        int load = 0;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.scanload);
            MobileBarcodeScanner.Initialize(Application);
            scanner = new MobileBarcodeScanner();

            Window.AddFlags(WindowManagerFlags.Fullscreen);
            Window.AddFlags(WindowManagerFlags.TranslucentNavigation);

            layout = FindViewById<RelativeLayout>(Resource.Id.layoutscanload);

            btscanvao = FindViewById<Button>(Resource.Id.btscanin); btscanvao.Text = Temp.TT("DT96");
            btchuyenline = FindViewById<Button>(Resource.Id.btscanout); btchuyenline.Text = Temp.TT("DT97");
            bttimpo = FindViewById<Button>(Resource.Id.bttimpo); bttimpo.Text = Temp.TT("DT98");
            btspmk = FindViewById<Button>(Resource.Id.btspmk);
            btchangepo = FindViewById<Button>(Resource.Id.btchangepo); btchangepo.Text = Temp.TT("DT101");
            btcombinepo = FindViewById<Button>(Resource.Id.btcombinepo); btcombinepo.Text = Temp.TT("DT102");

            txtname = FindViewById<TextView>(Resource.Id.txtname); txtname.Text = Temp.TT("DT05") + " " + Temp.facline + " | " + Temp.TT("DT02") + " " + Temp.user;

            lst = FindViewById<ListView>(Resource.Id.listView1);

            SetLanguage();

            load = Intent.GetIntExtra("Load", 0);
            RefreshData();

            string dept = Temp.facline.Substring(0, 2);

            switch (dept)
            {
                case "F1":
                    ou = "TP";
                    break;
                case "F2":
                    ou = "TR";
                    break;
                case "F3":
                    ou = "F3";
                    break;
                default:
                    ou = dept;
                    break;
            }

            btscanvao.Click += delegate { CheckPO(); };
            btscanvao.LongClick += delegate
            {
                //Scan();
            };
            btchuyenline.Click += delegate { PO(1); };
            bttimpo.Click += delegate { PO(2); };
            btspmk.Click += delegate
            {
                SPMK();
            };
            btchangepo.Click += delegate
              {
                  try
                  {
                      Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);

                      LinearLayout ln = new LinearLayout(this) { Orientation = Orientation.Vertical };

                      ViewGroup.MarginLayoutParams mr = new ViewGroup.MarginLayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                      mr.SetMargins(LayoutRequest.Widht(5), LayoutRequest.Widht(5), 0, 0);

                      TextView txt1 = new TextView(this) { Text = Temp.TT("DT105"), LayoutParameters = mr }; txt1.SetTextColor(Color.Blue);
                      TextView txt2 = new TextView(this) { Text = Temp.TT("DT103"), LayoutParameters = mr }; txt2.SetTextColor(Color.Blue);
                      TextView txt3 = new TextView(this) { Text = Temp.TT("DT106"), LayoutParameters = mr }; txt3.SetTextColor(Color.Blue);
                      TextView txt4 = new TextView(this) { Text = Temp.TT("DT104"), LayoutParameters = mr }; txt4.SetTextColor(Color.Blue);

                      EditText ed1 = new EditText(this) { Hint = Temp.TT("DT109"), LayoutParameters = mr }; ed1.SetTextColor(Color.ParseColor("#FF7F24")); ed1.InputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextFlagCapCharacters;
                      EditText ed2 = new EditText(this) { Hint = Temp.TT("DT107"), LayoutParameters = mr }; ed2.SetTextColor(Color.ParseColor("#FF7F24")); ed2.InputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextFlagCapCharacters;
                      EditText ed3 = new EditText(this) { Hint = Temp.TT("DT110"), LayoutParameters = mr }; ed3.SetTextColor(Color.ParseColor("#FF7F24")); ed3.InputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextFlagCapCharacters;
                      EditText ed4 = new EditText(this) { Hint = Temp.TT("DT108"), LayoutParameters = mr }; ed4.SetTextColor(Color.ParseColor("#FF7F24")); ed4.InputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextFlagCapCharacters;

                      ln.AddView(txt1); ln.AddView(ed1);
                      ln.AddView(txt2); ln.AddView(ed2);
                      ln.AddView(txt3); ln.AddView(ed3);
                      ln.AddView(txt4); ln.AddView(ed4);

                      Button bt = new Button(this) { Text = Temp.TT("DT66"), LayoutParameters = mr, TextAlignment = TextAlignment.Center }; ln.AddView(bt);

                      b.SetPositiveButton(Temp.TT("DT93"), (s, a) =>
                      {

                      });

                      b.SetCancelable(false);
                      b.SetView(ln);
                      Dialog d = b.Create();
                      d.Show();

                      string mss = "";

                      bt.Click += delegate
                      {
                          if (ed3.Text == "" || ed4.Text == "" || ed2.Text == "")
                          {
                              Toast.MakeText(this, Temp.TT("DT95"), ToastLength.Long).Show();
                              ed3.RequestFocusFromTouch();
                          }
                          else
                          {
                              if (CheckPO(ed2.Text))
                              {
                                  kn.Ghi("exec EndlineScanOutputChangePO '" + ou + "','" + Temp.facline + "','" + ed1.Text + "','" + ed2.Text + "','" + ed4.Text + "','" + ed3.Text + "','" + Temp.user + "'");

                                  if (kn.ErrorMessage == "")
                                  {
                                      Toast.MakeText(this, Temp.TT("DT94"), ToastLength.Long).Show();
                                      d.Dismiss();
                                  }
                                  else Toast.MakeText(this, Temp.TT("DT95") + kn.ErrorMessage, ToastLength.Long).Show();
                              }
                              else
                              {
                                  Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);

                                  b.SetMessage(mss);
                                  b.Create().Show();
                              }
                          }
                      };

                      bool CheckPO(string str)
                      {
                          bool b = true;
                          if (str.Contains(","))
                          {
                              string[] p = str.Split(",");

                              foreach (var item in p)
                              {
                                  if (item.Length > 5) b = Check(item);
                                  else
                                  {
                                      b = false;
                                      mss += "PO " + item + " is not correct, please check it again !!! \n";
                                      break;
                                  }
                              }
                          }
                          else b = Check(str);

                          return b;
                      }

                      bool Check(string po)
                      {
                          DataTable dt = kn.Doc("select * from ScanBarcodesew where LINEST = '" + Temp.facline + "' and PONO like '%" + po + "'").Tables[0];

                          if (dt.Rows.Count > 0) return true;
                          else
                          {
                              mss += "PO " + po + " is not exist in this line, please check it again !!! \n";
                              return false;
                          }
                      }
                  }
                  catch { }
              };
            btcombinepo.Click += delegate
              {
                  try
                  {
                      Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);

                      LinearLayout ln = new LinearLayout(this) { Orientation = Orientation.Vertical };

                      ViewGroup.MarginLayoutParams mr = new ViewGroup.MarginLayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                      mr.SetMargins(LayoutRequest.Widht(5), LayoutRequest.Widht(5), 0, 0);

                      TextView txt1 = new TextView(this) { Text = "Style : ", LayoutParameters = mr }; txt1.SetTextColor(Color.Blue);
                      TextView txt2 = new TextView(this) { Text = "Job : ", LayoutParameters = mr }; txt2.SetTextColor(Color.Blue);
                      TextView txt3 = new TextView(this) { Text = "PONO : ", LayoutParameters = mr }; txt3.SetTextColor(Color.Blue);
                      TextView txt4 = new TextView(this) { Text = "Color : ", LayoutParameters = mr }; txt4.SetTextColor(Color.Blue);

                      EditText ed1 = new EditText(this) { Hint = "Input Style", LayoutParameters = mr }; ed1.SetTextColor(Color.ParseColor("#FF7F24")); ed1.InputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextFlagCapCharacters;
                      EditText ed2 = new EditText(this) { Hint = "Input Job", LayoutParameters = mr }; ed2.SetTextColor(Color.ParseColor("#FF7F24")); ed2.InputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextFlagCapCharacters;
                      EditText ed3 = new EditText(this) { Hint = "Input PO", LayoutParameters = mr }; ed3.SetTextColor(Color.ParseColor("#FF7F24")); ed3.InputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextFlagCapCharacters;
                      EditText ed4 = new EditText(this) { Hint = "Input Color", LayoutParameters = mr }; ed4.SetTextColor(Color.ParseColor("#FF7F24")); ed4.InputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextFlagCapCharacters;

                      ln.AddView(txt1); ln.AddView(ed1);
                      ln.AddView(txt2); ln.AddView(ed2);
                      ln.AddView(txt3); ln.AddView(ed3);
                      ln.AddView(txt4); ln.AddView(ed4);

                      Button bt = new Button(this) { Text = Temp.TT("DT66"), LayoutParameters = mr, TextAlignment = TextAlignment.Center }; ln.AddView(bt);

                      b.SetPositiveButton(Temp.TT("DT93"), (s, a) =>
                      {

                      });

                      b.SetCancelable(false);
                      b.SetView(ln);
                      Dialog d = b.Create();
                      d.Show();

                      bt.Click += delegate
                      {
                          if (ed3.Text == "" || ed4.Text == "" || ed1.Text == "" || ed2.Text == "")
                          {
                              Toast.MakeText(this, Temp.TT("DT95"), ToastLength.Long).Show();
                              ed3.RequestFocusFromTouch();
                          }
                          else
                          {
                              kn.Ghi("insert into ScanBarcodeOrderQty values ('" + ed1.Text + "','" + ed2.Text + "','" + ed3.Text + "','" + ed4.Text + "',getdate(),'" + Temp.user + "')");

                              if (kn.ErrorMessage == "")
                              {
                                  Toast.MakeText(this, Temp.TT("DT94"), ToastLength.Long).Show();
                                  d.Dismiss();
                              }
                              else Toast.MakeText(this, Temp.TT("DT95") + kn.ErrorMessage, ToastLength.Long).Show();
                          }
                      };
                  }
                  catch { }
              };
        }
        private void CheckPO()
        {
            DataTable dt = new DataTable();
            dt = kn.Doc("exec GetLoadData 78,'" + Temp.facline + "',''").Tables[0];

            if (dt.Rows.Count > 0)
            {
                Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);

                HorizontalScrollView sh = new HorizontalScrollView(this);
                LinearLayout ln = new LinearLayout(this) { Orientation = Orientation.Vertical }; sh.AddView(ln);
                LinearLayout head = new LinearLayout(this) { Orientation = Orientation.Horizontal }; ln.AddView(head);
                TextView txt = new TextView(this) { Text = "PO : " }; head.AddView(txt);
                EditText ed = new EditText(this) { LayoutParameters = new ViewGroup.LayoutParams(LayoutRequest.Widht(300), ViewGroup.LayoutParams.WrapContent) }; head.AddView(ed);
                Button bt = new Button(this) { Text = "SEARCH" }; head.AddView(bt);
                Button btall = new Button(this) { Text = "SEARCH ALL LINE" }; head.AddView(btall);
                ListView lsv = new ListView(this); ln.AddView(lsv);

                A1ATeam.ListViewAdapterWithNoLayout adapter = new A1ATeam.ListViewAdapterWithNoLayout(dt, new List<int> { LayoutRequest.Widht(200), LayoutRequest.Widht(200), LayoutRequest.Widht(200), LayoutRequest.Widht(100), LayoutRequest.Widht(200) }, true)
                {
                    TextSize = LayoutRequest.Widht(10),
                    SingleClicked = true,
                    ClickedItemColor = Color.LimeGreen,
                };

                lsv.Adapter = adapter;

                b.SetPositiveButton("SELECT", (s, a) =>
                {
                    string po = adapter.GetFirstValue();
                    //Toast.MakeText(this, url + "2/" + Temp.facline + "/" + po, ToastLength.Long).Show();
                    CheckPOStatus(po);
                });
                b.SetNegativeButton("EXIT", (s, a) => { });

                b.SetView(sh);
                b.SetCancelable(false);

                b.Create().Show();

                bt.Click += delegate
                {
                    if (ed.Text != "")
                    {
                        DataView dv = dt.DefaultView;
                        dv.RowFilter = "PO_NO like '%" + ed.Text + "%'";

                        DataTable dt2 = new DataTable();
                        dt2 = dv.ToTable();

                        adapter = new A1ATeam.ListViewAdapterWithNoLayout(dt2, new List<int> { LayoutRequest.Widht(200), LayoutRequest.Widht(200), LayoutRequest.Widht(200), LayoutRequest.Widht(100), LayoutRequest.Widht(200) }, true)
                        {
                            TextSize = LayoutRequest.Widht(10),
                            SingleClicked = true,
                            ClickedItemColor = Color.LimeGreen,
                        };

                        lsv.Adapter = adapter;

                        Toast.MakeText(this, dt2.Rows.Count.ToString(), ToastLength.Long).Show();
                    }
                };
                btall.Click += delegate
                {
                    if (ed.Text != "")
                    {
                        dt.Clear();

                        dt = kn.Doc("exec GetLoadData 81,'" + ed.Text.ToUpper() + "',''").Tables[0];
                    }
                };
            }
            else
            {
                Toast.MakeText(this, "No data", ToastLength.Long).Show();
            }
        }
        private void CheckPOStatus(string po)
        {
            try
            {
                DataSet ds = new DataSet();
                ds = kn.Doc("exec GetLoadData 79,'" + Temp.facline + "','" + po + "'");

                if (ds.Tables[0].Rows.Count > 0)
                    Toast.MakeText(this, Temp.TT("DT121"), ToastLength.Long).Show();
                else if (ds.Tables[1].Rows.Count > 0)
                {
                    kn.Ghi("exec GetLoadData 80,'" + Temp.facline + "','" + po + "'");

                    if (kn.ErrorMessage == "") Toast.MakeText(this, Temp.TT("DT122"), ToastLength.Long).Show();
                    else Toast.MakeText(this, "Insert PO error !!! " + kn.ErrorMessage, ToastLength.Long).Show();
                }
                else
                {
                    if (ds.Tables[2].Rows.Count == 0)
                    {
                        kn.Ghi("exec GetLoadData 80,'" + Temp.facline + "','" + po + "'");

                        if (kn.ErrorMessage == "") Toast.MakeText(this, Temp.TT("DT122"), ToastLength.Long).Show();
                        else Toast.MakeText(this, "Insert PO error !!! " + kn.ErrorMessage, ToastLength.Long).Show();
                    }
                    else
                    {
                        DataRow dr = ds.Tables[2].Rows[0];
                        Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);
                        string msg = string.Format(Temp.TT("DT123"), dr[0].ToString(), dr[1].ToString(), po, dr[2].ToString());
                        b.SetMessage(msg);

                        b.SetPositiveButton("YES", (s, e) =>
                        {
                            if (ds.Tables[1].Rows.Count == 0)
                            {
                                Toast.MakeText(this, "Sending mail", ToastLength.Long).Show();

                                WebView wb = new WebView(this);
                                //string url = "http://192.168.1.108:8084/EndlineOuputUnlockMultiPO/public/api/UpdatePoEndLine/1/A1A" + strSelectedFac + "/" + strSelectedLine;
                                string str = url + "2/" + Temp.facline + "/" + po;
                                wb.LoadUrl(str);
                                wb.Settings.JavaScriptEnabled = true;
                                //wb.Visibility = ViewStates.Gone;

                                //Toast.MakeText(this, CSDL.Language("M00082"), ToastLength.Long).Show();
                                //Toast.MakeText(this, url, ToastLength.Long).Show();
                                Clipboard.SetTextAsync(url);

                                AlertDialog.Builder bb = new AlertDialog.Builder(this);//,Resource.Style.ShapeAppearanceOverlay_MaterialComponents_MaterialCalendar_Window_Fullscreen);
                                bb.SetView(wb);
                                bb.SetPositiveButton("EXIT", (k, l) => { });
                                bb.SetCancelable(false);
                                bb.Create().Show();
                            }
                        });
                        b.SetNegativeButton("NO", (s, e) => { });

                        b.Create().Show();
                    }
                }
            }
            catch { }
        }
        private void PO(int i)
        {
            try
            {
                DataSet dts = new DataSet();
                A1ATeam.ListViewAdapterWithNoLayout adapter = null;
                ViewGroup.MarginLayoutParams mr = new ViewGroup.MarginLayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
                Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);

                EditText edline = new EditText(this) { Hint = "Line", TextAlignment = TextAlignment.Center, Focusable = false };

                LinearLayout main = new LinearLayout(this) { Orientation = Orientation.Vertical };
                CheckBox chk = new CheckBox(this) { Text = "All Factory", Checked = false }; main.AddView(chk);
                LinearLayout head = new LinearLayout(this) { Orientation = Orientation.Horizontal }; main.AddView(head);

                TextView txtqty = new TextView(this) { Text = "Qty : ", LayoutParameters = mr };

                EditText edqty = new EditText(this) { Text = "0", LayoutParameters = mr, Focusable = false };
                edqty.SetTextColor(Color.ParseColor("#FF3030"));

                mr.SetMargins(LayoutRequest.Widht(5), LayoutRequest.Height(10), 0, 0);
                TextView txtpo = new TextView(this) { Text = "PO : ", LayoutParameters = mr }; head.AddView(txtpo);

                mr.SetMargins(LayoutRequest.Widht(5), 0, 0, 0); mr.Width = LayoutRequest.Widht(200);
                EditText edpo = new EditText(this) { Hint = "PO", TextAlignment = TextAlignment.Center, LayoutParameters = mr }; head.AddView(edpo);
                edpo.SetTextColor(Color.ParseColor("#FF3030"));

                mr.Width = ViewGroup.LayoutParams.WrapContent;
                mr.SetMargins(LayoutRequest.Widht(5), LayoutRequest.Height(10), 0, 0);
                Button btpo = new Button(this) { Text = Temp.TT("DT98"), LayoutParameters = mr }; head.AddView(btpo);

                mr.Width = ViewGroup.LayoutParams.MatchParent;
                mr.SetMargins(LayoutRequest.Widht(5), LayoutRequest.Height(10), 0, 0);
                ListView lsv = new ListView(this) { LayoutParameters = mr };

                if (i == 1)
                {
                    LinearLayout toline = new LinearLayout(this) { Orientation = Orientation.Horizontal }; main.AddView(toline);

                    mr.Width = ViewGroup.LayoutParams.WrapContent;
                    mr.SetMargins(LayoutRequest.Widht(5), LayoutRequest.Height(10), 0, 0);
                    TextView txtline = new TextView(this) { Text = "Line : ", LayoutParameters = mr }; toline.AddView(txtline);

                    mr.SetMargins(LayoutRequest.Widht(5), 0, 0, 0); mr.Width = LayoutRequest.Widht(300);
                    edline.LayoutParameters = mr;
                    toline.AddView(edline);
                    edline.SetTextColor(Color.ParseColor("#FF3030"));
                    edline.Click += delegate
                    {
                        string edfac = Temp.facline.Substring(0, 2);
                        var line = chk.Checked ? Temp.FacLine.Select().Select(l => l[1].ToString()).Distinct().ToArray() : Temp.FacLine.Select("FacZone like '%" + edfac + "%'").Select(l => l[1].ToString()).Distinct().ToArray();
                        line = line.Concat(new string[] { edfac + "PPA", edfac + "JUMPER" }).ToArray();
                        Android.App.AlertDialog.Builder b = new Android.App.AlertDialog.Builder(this);

                        b.SetSingleChoiceItems(line, -1, (s, a) =>
                        {
                            Dialog d = s as Dialog;

                            edline.Text = line[a.Which];

                            d.Dismiss();
                        });
                        b.SetCancelable(false);
                        b.Create().Show();
                    };

                    mr.Width = ViewGroup.LayoutParams.WrapContent;
                    mr.SetMargins(LayoutRequest.Widht(10), LayoutRequest.Height(10), 0, 0);
                    toline.AddView(txtqty);
                    mr.SetMargins(LayoutRequest.Widht(5), 0, 0, 0);
                    toline.AddView(edqty); edqty.Focusable = false;

                    b.SetPositiveButton(Temp.TT("DT66"), (s, a) =>
                    {
                        Confirm(edline.Text);
                    });
                    b.SetNegativeButton(Temp.TT("DT93"), (s, a) =>
                    {

                    });
                }
                else
                {
                    mr.Width = ViewGroup.LayoutParams.WrapContent;
                    mr.SetMargins(LayoutRequest.Widht(5), LayoutRequest.Height(10), 0, 0);
                    Button btspmk = new Button(this) { Text = "SPMK STATUS", LayoutParameters = mr }; head.AddView(btspmk);

                    b.SetPositiveButton(Temp.TT("DT93"), (s, a) =>
                    {

                    });

                    btspmk.Click += delegate
                    {
                        DataTable dt = new DataTable();

                        dt = kn.Doc("exec GetLoadData 75,'" + edpo.Text + "','" + ou + "'").Tables[0];

                        if (dt.Rows.Count > 0)
                        {
                            lsv.Adapter = new A1ATeam.ListViewAdapterWithNoLayout(dt, new List<int> { LayoutRequest.Widht(200) }, true, true, true)
                            { TextSize = LayoutRequest.Widht(10) };
                        }
                        else Toast.MakeText(this, Temp.TT("DT99"), ToastLength.Long).Show();
                    };
                }

                HorizontalScrollView sh = new HorizontalScrollView(this); main.AddView(sh);
                sh.AddView(lsv);

                b.SetCancelable(false);
                b.SetView(main);
                AlertDialog d = b.Create();
                d.Show();

                if (i == 1) d.GetButton((int)DialogButtonType.Positive).Enabled = false;

                btpo.Click += delegate
                {
                    if (i == 1)
                    {
                        dts = kn.Doc("exec GetLoadData 14,'" + edpo.Text + "','" + ou + ";" + Temp.facline + "'");

                        if (dts.Tables[0].Rows.Count > 0)
                        {
                            adapter = new A1ATeam.ListViewAdapterWithNoLayout(dts.Tables[0], new List<int> { LayoutRequest.Widht(150), LayoutRequest.Widht(100), LayoutRequest.Widht(100), LayoutRequest.Widht(100), LayoutRequest.Widht(200), LayoutRequest.Widht(200) }, true)
                            {
                                CheckBox = true,
                                TextSize = LayoutRequest.Widht(10),
                                SumValueOfColumn = "QTY",
                                ShowValue = edqty
                            };
                            lsv.Adapter = adapter;

                            if (edline.Text != "") d.GetButton((int)DialogButtonType.Positive).Enabled = true;
                        }
                        else Toast.MakeText(this, Temp.TT("DT99"), ToastLength.Long).Show();
                    }
                    else
                    {
                        DataTable dt = new DataTable();

                        if (chk.Checked) dt = kn.Doc("exec GetLoadData 67,'" + edpo.Text + "','" + ou + "'").Tables[0];
                        else dt = kn.Doc("exec GetLoadData 15,'" + edpo.Text + "','" + ou + "'").Tables[0];

                        if (dt.Rows.Count > 0)
                        {
                            lsv.Adapter = new A1ATeam.ListViewAdapterWithNoLayout(dt, new List<int> { LayoutRequest.Widht(200) }, true, true, true)
                            { TextSize = LayoutRequest.Widht(10) };
                        }
                        else Toast.MakeText(this, Temp.TT("DT99"), ToastLength.Long).Show();
                    }
                };
                edline.TextChanged += delegate
                {
                    if (edline.Text != "" && adapter != null) d.GetButton((int)DialogButtonType.Positive).Enabled = true;
                };

                void Run(DataTable dt)
                {
                    kn.SqlBulkCopy(dt, "ScanBarcodeSPMK");

                    if (kn.ErrorMessage == "")
                    {
                        Toast.MakeText(this, Temp.TT("DT94"), ToastLength.Long).Show();
                        RefreshData();

                        string qry = "";
                        foreach (DataRow r in dt.Rows)
                        {
                            qry += "delete from ScanBarcodesew where LINEST = '" + Temp.facline + "' and STYLE_NO = '" + r["STYLE_NO"] + "' and BARCODE = '" + r["BARCODE"] + "' and OU_CODE = '" + r["OU_CODE"] + "' and PONO = '" + r["PONO"] + "' \n";
                        }

                        if (qry != "") kn.Ghi(qry);
                    }
                    else
                    {
                        Android.App.AlertDialog.Builder er = new AlertDialog.Builder(this);
                        er.SetMessage(kn.ErrorMessage);

                        er.Create().Show();
                    }
                }

                void Confirm(string line)
                {
                    if (adapter == null)
                        Toast.MakeText(this, "No Data !!!" + adapter.CheckedBox.Count.ToString(), ToastLength.Long).Show();
                    else
                    {
                        if (adapter.CheckedBox.Count > 0 && line != "")
                        {
                            DataTable dt = dts.Tables[0];
                            DataTable up = dts.Tables[1];

                            if (up.Rows[0]["COLOR"].ToString() == "") Temp.ShowMessage(layout, "This PO has been changed from another PO !!!");
                            else
                            {
                                up.Columns.Add("TOLINE", typeof(string));
                                up.Columns.Add("RECORDDATE", typeof(DateTime));
                                up.Columns.Add("USERBY", typeof(string));

                                up.Select().ToList().ForEach(r => { r["TOLINE"] = line; r["RECORDDATE"] = DateTime.Now; r["USERBY"] = Temp.user; r["STATUS"] = 0; });

                                if (adapter.SelectAll)
                                {
                                    Run(up);
                                }
                                else
                                {
                                    List<int> lstchk = adapter.CheckedBox;

                                    foreach (int i in lstchk)
                                    {
                                        DataRow r1 = dt.Rows[i - 1];

                                        DataView dv = up.DefaultView;
                                        dv.RowFilter = "STYLE_NO = '" + r1[4] + "' and JOB_NO = '" + r1[5] + "' and PONO = '" + r1[0] + "' and CUT_SIZE = '" + r1[1] + "' and BUNDLE = '" + r1[2] + "'";

                                        Run(dv.ToTable());
                                    }
                                }
                            }
                        }
                        else Toast.MakeText(this, "No Data !!!" + adapter.CheckedBox.Count.ToString(), ToastLength.Long).Show();
                    }
                }
            }
            catch { }
        }

        private void SPMK()
        {
            try
            {
                A1ATeam.ListViewAdapterWithNoLayout adapter = null;
                Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);

                HorizontalScrollView sh = new HorizontalScrollView(this);
                ListView lsv = new ListView(this); sh.AddView(lsv);

                DataSet dts = kn.Doc("exec GetLoadData 16,'','" + Temp.facline + "'");

                if (dts.Tables[0].Rows.Count > 0)
                {
                    adapter = new A1ATeam.ListViewAdapterWithNoLayout(dts.Tables[0], new List<int> { LayoutRequest.Widht(100), LayoutRequest.Widht(200), LayoutRequest.Widht(100), LayoutRequest.Widht(100), LayoutRequest.Widht(200), LayoutRequest.Widht(200) }, true)
                    {
                        CheckBox = true,
                        TextSize = LayoutRequest.Widht(10)
                    };
                    lsv.Adapter = adapter;

                    b.SetPositiveButton(Temp.TT("DT66"), (s, a) =>
                    {
                        Confirm();
                    });
                    b.SetNegativeButton(Temp.TT("DT93"), (s, a) =>
                    {

                    });

                    b.SetCancelable(false);
                    b.SetView(sh);
                    b.Create().Show();

                    void Confirm()
                    {
                        if (adapter == null)
                            Toast.MakeText(this, "No Data !!!" + adapter.CheckedBox.Count.ToString(), ToastLength.Long).Show();
                        else
                        {
                            if (adapter.CheckedBox.Count > 0)
                            {
                                DataTable dt = dts.Tables[0];
                                DataTable up = dts.Tables[1];

                                up.Columns.Remove(up.Columns["TOLINE"]);
                                up.Columns.Remove(up.Columns["RECORDDATE"]);
                                up.Columns.Remove(up.Columns["USERBY"]);

                                if (adapter.SelectAll)
                                {
                                    Run(up);
                                }
                                else
                                {
                                    List<int> lstchk = adapter.CheckedBox;
                                    DataTable d = up.Clone();

                                    foreach (int i in lstchk)
                                    {
                                        DataRow r1 = dt.Rows[i - 1];

                                        DataView dv = up.DefaultView;
                                        dv.RowFilter = "STYLE_NO = '" + r1[4] + "' and JOB_NO = '" + r1[5] + "' and PONO = '" + r1[1] + "' and CUT_SIZE = '" + r1[2] + "'";

                                        //Run(dv.ToTable());
                                        foreach (DataRow r2 in dv.ToTable().Rows) d.ImportRow(r2);
                                    }

                                    if (d.Rows.Count > 0) Run(d);
                                }
                            }
                            else Toast.MakeText(this, "No Data !!!" + adapter.CheckedBox.Count.ToString(), ToastLength.Long).Show();
                        }
                    }

                    void Run(DataTable dt)
                    {
                        foreach (DataRow r in dt.Rows)
                        {
                            kn.Ghi("delete from ScanBarcodesew where BARCODE = '" + r["BARCODE"] + "' and OU_CODE = '" + r["OU_CODE"] + "' and PONO = '" + r["PONO"] + "' and LINEST = '" + r["LINEST"] + "'");
                        }

                        dt.Select().ToList().ForEach(r => { r["STATUS"] = 1; r["OU_CODE"] = ou; r["LINEST"] = Temp.facline; r["UPD_BY"] = Temp.user; });

                        kn.SqlBulkCopy(dt, "ScanBarcodesew");

                        if (kn.ErrorMessage == "")
                        {
                            Toast.MakeText(this, Temp.TT("DT94"), ToastLength.Long).Show();
                            RefreshData();

                            foreach (DataRow r in dt.Rows)
                            {
                                kn.Ghi("update ScanBarcodeSPMK set STATUS = 1 where BARCODE = '" + r["BARCODE"] + "' and TOLINE = '" + r["LINEST"] + "' and PONO = '" + r["PONO"] + "'");
                            }
                        }
                        else
                        {
                            Android.App.AlertDialog.Builder er = new AlertDialog.Builder(this);
                            er.SetMessage(kn.ErrorMessage);

                            er.Create().Show();
                        }
                    }
                }
                else Toast.MakeText(this, Temp.TT("DT100"), ToastLength.Long).Show();
            }
            catch { }
        }
        private void RefreshData()
        {
            try
            {
                if (ds.Tables.Count > 0) ds.Clear();

                ds = kn.Doc("USE [DtradeProduction] exec GetLoadData 13,'" + DateTime.Now.ToString("yyyyMMdd") + "','" + Temp.facline + "'");

                lst.Adapter = new A1ATeam.ListViewAdapterWithNoLayout(ds.Tables[2], new List<int> { LayoutRequest.Widht(200), LayoutRequest.Widht(100), LayoutRequest.Widht(100), LayoutRequest.Widht(200), LayoutRequest.Widht(200) }, true, true, true)
                { TextSize = LayoutRequest.Widht(10) };

                //if (load == 0) { if (ds.Tables[0].Rows.Count > 0) ShowData(); }
                //else SPMK();

                SPMK();

                url = ds.Tables[4].Rows[0][0].ToString();
            }
            catch { }
        }
        private void ShowData()
        {
            try
            {
                Android.App.AlertDialog.Builder b = new AlertDialog.Builder(this);

                HorizontalScrollView main = new HorizontalScrollView(this);
                LinearLayout ln = new LinearLayout(this); main.AddView(ln);
                ListView lv = new ListView(this); ln.AddView(lv);

                DataTable dt = ds.Tables[0];
                ds.Tables[1].Select().ToList().ForEach(r => { r["UPD_BY"] = Temp.user; });

                A1ATeam.ListViewAdapterWithNoLayout adapter = new A1ATeam.ListViewAdapterWithNoLayout(dt, new List<int> { LayoutRequest.Widht(200), LayoutRequest.Widht(100), LayoutRequest.Widht(100), LayoutRequest.Widht(200), LayoutRequest.Widht(200) }, true)
                {
                    CheckBox = true,
                    TextSize = LayoutRequest.Widht(10)
                };
                lv.Adapter = adapter;

                b.SetPositiveButton(Temp.TT("DT66"), (s, a) =>
                {
                    if (adapter.SelectAll)
                    {
                        Run(ds.Tables[1]);
                    }
                    else
                    {
                        List<int> lstchk = adapter.CheckedBox;

                        foreach (int i in lstchk)
                        {
                            int cr = dt.Rows.Count;
                            DataRow r1 = dt.Rows[i - 1];

                            DataView dv = ds.Tables[1].DefaultView;
                            dv.RowFilter = "STYLE_NO = '" + r1[3] + "' and JOB_NO = '" + r1[4] + "' and PO_NO = '" + r1[0] + "' and CUT_SIZE = '" + r1[1] + "'";

                            Run(dv.ToTable());
                        }
                    }
                });
                b.SetNegativeButton(Temp.TT("DT93"), (s, a) =>
                {

                });

                b.SetView(main);
                b.SetCancelable(false);
                b.Create().Show();

                void Run(DataTable dt)
                {
                    kn.SqlBulkCopy(dt, "ScanBarcodesew");

                    if (kn.ErrorMessage == "") { Toast.MakeText(this, Temp.TT("DT94"), ToastLength.Long).Show(); RefreshData(); }
                    else
                    {
                        Android.App.AlertDialog.Builder er = new AlertDialog.Builder(this);
                        er.SetMessage(kn.ErrorMessage);

                        er.Create().Show();
                    }
                }
            }
            catch { }
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

                        vv.TextSize = LayoutRequest.TextSize(vv.TextSize);
                    }
                    else if (v.GetType() == typeof(Button))
                    {
                        Button vv = v as Button;

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
            catch (Exception ex)
            {
                Toast.MakeText(this, "Set Language failed !!!" + ex.ToString(), ToastLength.Long).Show();
            }
        }
    }
}