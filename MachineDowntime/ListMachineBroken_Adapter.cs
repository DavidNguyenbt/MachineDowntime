using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MachineDowntime
{
    class ListMachineBroken_Adapter : BaseAdapter<ListMachineBroken>
    {
        List<ListMachineBroken> ls = new List<ListMachineBroken>();
        int i = 0;
        public ListMachineBroken_Adapter(List<ListMachineBroken> l,int _i)
        {
            ls = l;
            i = _i;
        }

        public override ListMachineBroken this[int position]
        {
            get
            {
                return ls[position];
            }
        }

        public override int Count
        {
            get
            {
                return ls.Count;
            }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            LinearLayout v;

            TextView mamay, batdauhu, batdausua, ketthuc,dt, trangthai, thomay, doido, thoigian;

            v = new LinearLayout(Application.Context)
            {
                Orientation = Orientation.Horizontal
            };
            mamay = new TextView(Application.Context)
            {
                //LayoutParameters = new ViewGroup.LayoutParams(180, ViewGroup.LayoutParams.WrapContent)
                LayoutParameters = new ViewGroup.LayoutParams(LayoutRequest.Widht(180), ViewGroup.LayoutParams.WrapContent),
                TextSize=LayoutRequest.TextSize(20)
            };
            mamay.SetTextColor(Color.White);
            v.AddView(mamay);
            batdauhu = new TextView(Application.Context)
            {
                //LayoutParameters = new ViewGroup.LayoutParams(150, ViewGroup.LayoutParams.WrapContent),
                LayoutParameters = new ViewGroup.LayoutParams(LayoutRequest.Widht(150), ViewGroup.LayoutParams.WrapContent),
                TextSize = LayoutRequest.TextSize(20),
                TextAlignment = TextAlignment.Center
            };
            batdauhu.SetTextColor(Color.White);
            v.AddView(batdauhu);
            batdausua = new TextView(Application.Context)
            {
                //LayoutParameters = new ViewGroup.LayoutParams(150, ViewGroup.LayoutParams.WrapContent),
                LayoutParameters = new ViewGroup.LayoutParams(LayoutRequest.Widht(150), ViewGroup.LayoutParams.WrapContent),
                TextSize = LayoutRequest.TextSize(20),
                TextAlignment = TextAlignment.Center
            };
            batdausua.SetTextColor(Color.White);
            v.AddView(batdausua);
            ketthuc = new TextView(Application.Context)
            {
                //LayoutParameters = new ViewGroup.LayoutParams(150, ViewGroup.LayoutParams.WrapContent),
                LayoutParameters = new ViewGroup.LayoutParams(LayoutRequest.Widht(150), ViewGroup.LayoutParams.WrapContent),
                TextSize = LayoutRequest.TextSize(20),
                TextAlignment = TextAlignment.Center
            };
            ketthuc.SetTextColor(Color.White);
            v.AddView(ketthuc);
            dt = new TextView(Application.Context)
            {
                //LayoutParameters = new ViewGroup.LayoutParams(150, ViewGroup.LayoutParams.WrapContent),
                LayoutParameters = new ViewGroup.LayoutParams(LayoutRequest.Widht(150), ViewGroup.LayoutParams.WrapContent),
                TextSize = LayoutRequest.TextSize(20),
                TextAlignment = TextAlignment.Center
            };
            dt.SetTextColor(Color.White);
            v.AddView(dt);
            trangthai = new TextView(Application.Context)
            {
                //LayoutParameters = new ViewGroup.LayoutParams(200, ViewGroup.LayoutParams.WrapContent),
                LayoutParameters = new ViewGroup.LayoutParams(LayoutRequest.Widht(200), ViewGroup.LayoutParams.WrapContent),
                TextSize = LayoutRequest.TextSize(20),
                TextAlignment = TextAlignment.Center
            };
            trangthai.SetTextColor(Color.White);
            v.AddView(trangthai);
            thomay = new TextView(Application.Context)
            {
                //LayoutParameters = new ViewGroup.LayoutParams(250, ViewGroup.LayoutParams.WrapContent),
                LayoutParameters = new ViewGroup.LayoutParams(LayoutRequest.Widht(250), ViewGroup.LayoutParams.WrapContent),
                TextSize = LayoutRequest.TextSize(20),
                TextAlignment = TextAlignment.Center
            };
            thomay.SetTextColor(Color.White);
            v.AddView(thomay);
            doido = new TextView(Application.Context)
            {
                //LayoutParameters = new ViewGroup.LayoutParams(150, ViewGroup.LayoutParams.WrapContent),
                LayoutParameters = new ViewGroup.LayoutParams(LayoutRequest.Widht(150), ViewGroup.LayoutParams.WrapContent),
                TextSize = LayoutRequest.TextSize(20),
                TextAlignment = TextAlignment.Center
            };
            doido.SetTextColor(Color.White);
            v.AddView(doido);
            thoigian = new TextView(Application.Context)
            {
                //LayoutParameters = new ViewGroup.LayoutParams(150, ViewGroup.LayoutParams.WrapContent),
                LayoutParameters = new ViewGroup.LayoutParams(LayoutRequest.Widht(150), ViewGroup.LayoutParams.WrapContent),
                TextSize = LayoutRequest.TextSize(20),
                TextAlignment = TextAlignment.Center
            };
            thoigian.SetTextColor(Color.White);
            v.AddView(thoigian);

            if (i == 1)
            {
                mamay.Text = ls[position].MaMay;
                batdauhu.Text = ls[position].BatDauHu;
                batdausua.Text = ls[position].BatDauSua;
                ketthuc.Text = ls[position].KetThuc;
                dt.Text = ls[position].DT;
                trangthai.Text = ls[position].TrangThai;
                thomay.Text = ls[position].ThoMay;
                doido.Text = ls[position].DoiDo;
                thoigian.Text = ls[position].ThoiGian;

                v.SetBackgroundColor(Color.SeaGreen);
            }
            else
            {
                mamay.Text = ls[position].MaMay;
                batdauhu.Text = ls[position].BatDauHu == "" ? "" : ls[position].BatDauHu.Split(" ").First() + "\n" + ls[position].BatDauHu.Split(" ").Last();
                batdausua.Text = ls[position].BatDauSua == "" ? "" : ls[position].BatDauSua.Split(" ").First() + "\n" + ls[position].BatDauSua.Split(" ").Last();
                ketthuc.Text = ls[position].KetThuc == "" ? "" : ls[position].KetThuc.Split(" ").First() + "\n" + ls[position].KetThuc.Split(" ").Last();
                dt.Text = ls[position].DT;
                trangthai.Text = ls[position].TrangThai;
                thomay.Text = ls[position].ThoMay == "" ? "" : ls[position].ThoMay.Split(" ").Last();
                doido.Text = ls[position].DoiDo;
                thoigian.Text = ls[position].ThoiGian;
            }

            try
            {
                string catagory = ls[position].TrangThai;//Temp.NgonNgu.Select().Where(x => x[Temp.Newlg].ToString() == ls[position].TrangThai).FirstOrDefault()[1].ToString();
                switch (catagory)
                {
                    case "Chờ Sửa":
                        v.SetBackgroundColor(Color.LightPink);
                        break;
                    case "Wait":
                        v.SetBackgroundColor(Color.LightPink);
                        break;
                    case "Đang Sửa":
                        v.SetBackgroundColor(Color.Green);
                        break;
                    case "Repairing":
                        v.SetBackgroundColor(Color.Green);
                        break;
                    case "Đổi Đồ":
                        v.SetBackgroundColor(Color.LightYellow);
                        break;
                    case "Exchange":
                        v.SetBackgroundColor(Color.LightYellow);
                        break;
                    case "Sửa Xong":
                        v.SetBackgroundColor(Color.LightBlue);
                        break;
                    case "Finished":
                        v.SetBackgroundColor(Color.LightBlue);
                        break;
                }
            }
            catch { }

            return v;
        }
    }
}