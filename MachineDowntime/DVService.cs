using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MachineDowntime
{
    [BroadcastReceiver]
    public class DVService : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action == "FOO_ACTION")
            {
                string fooString = intent.GetStringExtra("KEY_FOO_STRING");
                Toast.MakeText(context, fooString + "  " + DateTime.Now.ToString("HHmmss"), ToastLength.Short).Show();
            }
        }
    }
}