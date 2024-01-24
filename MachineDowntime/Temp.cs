using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Security.Keystore;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V4.Hardware.Fingerprint;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Java.Lang;
using Java.Lang.Ref;
using Java.Security;
using Javax.Crypto;
using UpdateManager;

namespace MachineDowntime
{
    class Temp
    {
        public static string chuoi = "Data Source=192.168.1.245;Initial Catalog=Maintenance;Integrated Security=False;User ID=prog4;Password=DeS;Connect Timeout=30;Encrypt=False;";
        public static string com = "Data Source=192.168.1.245;Initial Catalog=DtradeProduction;Integrated Security=False;User ID=prog4;Password=DeS;Connect Timeout=30;Encrypt=False;";
        //public static string chuoi = "Data Source=192.168.54.8;Initial Catalog=Maintenance;Integrated Security=False;User ID=sa;Password=Admin@168*;Connect Timeout=30;Encrypt=False;";
        //public static string com = "Data Source=192.168.54.8;Initial Catalog=DtradeProduction;Integrated Security=False;User ID=sa;Password=Admin@168*;Connect Timeout=30;Encrypt=False;";
        //public static string chuoi = "Data Source=108.181.157.253,18697;Initial Catalog=Maintenance;Integrated Security=False;User ID=David;Password=Vancho1988;Connect Timeout=30;Encrypt=False;";
        public static string user = "", facline = "", mcid = "", msg = "Service is not run", service = "", version = "V4.6", dept = "", fac = "";
        public static string Link = "", AppName = "";
        public static int i = 0, Newlg = 0, Oldlg = 0;
        public static DataTable NgonNgu = new DataTable();
        public static DataTable ListMachine = new DataTable();
        public static DataTable FacLine = new DataTable();
        public static DataTable Login = new DataTable();
        public static DataTable Location = new DataTable();
        public static DisplayMetrics metric;
        public static bool Obli = true;
        public static DateTime OccurTime = DateTime.Now;

        public static void OnUpdate(bool click, Android.App.AlertDialog.Builder b, string title, string message1, string message2, string buttonOK, string buttonNO, Context ct)
        {
            Update.CheckUpdate(ct, Obli, click, b, Link, AppName, title, message1, message2, buttonOK, buttonNO);
        }
        public static void HideKeyBoard(Context context, View cur)
        {
            InputMethodManager inputManager = (InputMethodManager)context.GetSystemService(Context.InputMethodService);
            View currentFocus = cur;
            if (currentFocus != null)
            {
                inputManager.HideSoftInputFromWindow(currentFocus.WindowToken, HideSoftInputFlags.None);
            }
        }
        public static void Density(ViewGroup vg)
        {
            try
            {
                var widthInInches = metric.WidthPixels / metric.Xdpi;
                var heightInInches = metric.HeightPixels / metric.Ydpi;

                var inch = System.Math.Round(System.Math.Sqrt(System.Math.Pow(widthInInches, 2) + System.Math.Pow(heightInInches, 2)), 2);

                var persent = System.Math.Round(3 / metric.Density);
                //Toast.MakeText(Application.Context, inch + "|" + persent + "|density" + metric.Density, ToastLength.Long).Show();

                View layout = vg as View;
                RelativeLayout.LayoutParams rlLayout = (RelativeLayout.LayoutParams)layout.LayoutParameters;
                rlLayout.SetMargins((int)(persent * rlLayout.LeftMargin), (int)(persent * rlLayout.TopMargin), (int)(persent * rlLayout.RightMargin), (int)(persent * rlLayout.BottomMargin));
                layout.LayoutParameters = rlLayout;
                layout.LayoutParameters.Width = (int)(persent * rlLayout.Width);
                layout.LayoutParameters.Height = (int)(persent * rlLayout.Height);

                for (int i = 0; i < vg.ChildCount; i++)
                {
                    var childView = vg.GetChildAt(i);
                    RelativeLayout.LayoutParams rlLayoutTv = (RelativeLayout.LayoutParams)childView.LayoutParameters;
                    rlLayoutTv.SetMargins((int)(persent * rlLayoutTv.LeftMargin), (int)(persent * rlLayoutTv.TopMargin), (int)(persent * rlLayoutTv.RightMargin), (int)(persent * rlLayoutTv.BottomMargin));
                    childView.LayoutParameters = rlLayoutTv;
                    childView.LayoutParameters.Width = (int)(persent * rlLayoutTv.Width);
                    childView.LayoutParameters.Height = (int)(persent * rlLayoutTv.Height);

                    //Type tp = childView.GetType();

                    //if (tp.Equals(typeof(EditText)))
                    //{
                    //    EditText mojt = childView as EditText;
                    //    mojt.TextSize = (float)(mojt.TextSize * persent);
                    //}
                    //else if (tp.Equals(typeof(TextView)))
                    //{
                    //    TextView mojt = childView as TextView;
                    //    mojt.TextSize = (float)(mojt.TextSize * persent);
                    //}
                    //else if (tp.Equals(typeof(CheckBox)))
                    //{
                    //    CheckBox mojt = childView as CheckBox;
                    //    mojt.TextSize = (float)(mojt.TextSize * persent);
                    //}
                    //else if (tp.Equals(typeof(RadioButton)))
                    //{
                    //    RadioButton mojt = childView as RadioButton;
                    //    mojt.TextSize = (float)(mojt.TextSize * persent);
                    //}
                    //else if (tp.Equals(typeof(Button)))
                    //{
                    //    Button mojt = childView as Button;
                    //    mojt.TextSize = (float)(mojt.TextSize * persent);
                    //}
                }
            }
            catch
            {

            }
        }

        public static void Stretching(DisplayMetrics _metric, ViewGroup _viewGroup, Context _context, bool _show)
        {
            var metric = _metric;
            var viewGroup = _viewGroup;
            var t = _context;
            var s = _show;
            try
            {
                var width = metric.WidthPixels;
                var height = metric.HeightPixels;
                var density = metric.Density;
                var SizingScrRt = width / (density * 1024); //CSDL.density *
                float TextRatio = 1;
                if (density != 1) TextRatio = 2 - density;
                if (s) Toast.MakeText(t, "Res=" + width + "x" + height + " | Density=" + density + " | TextRatio=" + TextRatio + " | SizingScrRt=" + SizingScrRt, ToastLength.Short).Show();

                //var viewGroup = (ViewGroup)FindViewById<RelativeLayout>(Resource.Id.rlMnPlanLoadLayout);
                for (int i = 0; i < viewGroup.ChildCount; i++)
                {
                    var childView = viewGroup.GetChildAt(i);
                    RelativeLayout.LayoutParams rlLayoutTv = (RelativeLayout.LayoutParams)childView.LayoutParameters;
                    rlLayoutTv.SetMargins((int)(SizingScrRt * rlLayoutTv.LeftMargin), (int)(SizingScrRt * rlLayoutTv.TopMargin), (int)(SizingScrRt * rlLayoutTv.RightMargin), (int)(SizingScrRt * rlLayoutTv.BottomMargin));
                    childView.LayoutParameters = rlLayoutTv;
                    childView.LayoutParameters.Width = (int)(SizingScrRt * rlLayoutTv.Width);
                    childView.LayoutParameters.Height = (int)(SizingScrRt * rlLayoutTv.Height);

                    Type tp = childView.GetType();

                    if (tp.Equals(typeof(EditText)))
                    {
                        EditText mojt = childView as EditText;
                        mojt.TextSize = (int)(mojt.TextSize * SizingScrRt * TextRatio);
                    }
                    else if (tp.Equals(typeof(TextView)))
                    {
                        TextView mojt = childView as TextView;
                        mojt.TextSize = (int)(mojt.TextSize * SizingScrRt * TextRatio);
                    }
                    else if (tp.Equals(typeof(CheckBox)))
                    {
                        CheckBox mojt = childView as CheckBox;
                        mojt.TextSize = (int)(mojt.TextSize * SizingScrRt * TextRatio);
                    }
                    else if (tp.Equals(typeof(RadioButton)))
                    {
                        RadioButton mojt = childView as RadioButton;
                        mojt.TextSize = (int)(mojt.TextSize * SizingScrRt * TextRatio);
                    }
                    else if (tp.Equals(typeof(Button)))
                    {
                        Button mojt = childView as Button;
                        mojt.TextSize = (int)(mojt.TextSize * SizingScrRt * TextRatio);
                    }
                }
            }
            catch { }
        }

        public static string TT(string code)
        {
            return NgonNgu.Select().Where(x => x[0].ToString() == code).FirstOrDefault()[Newlg].ToString();
        }
        public static void ShowMessage(View view, string msg, int time = 5000)
        {
            var sn = Snackbar.Make(view, msg, time);
            sn.Show();
        }
    }
    class LayoutRequest
    {
        static Android.Util.DisplayMetrics metric = Application.Context.Resources.DisplayMetrics;

        static float he = metric.HeightPixels / metric.Density;
        static float wi = metric.WidthPixels / metric.Density;
        static float ohe = 1368 / 2;
        static float owi = 720 / 2;
        //static A1ATeam.LayoutRequest rq = new A1ATeam.LayoutRequest(1080, 1920, 3);
        public static int Height(int h)
        {
            return (int)(h * (he / ohe));//rq.eWidht(h);//
        }
        public static int Widht(int w)
        {
            return (int)(w * (wi / owi));//rq.eWidht(w);//
        }
        public static float TextSize(float s)
        {
            return s * (wi / owi) / metric.Density;//rq.eTextSize(s);//
        }
    }
    //public class CryptoObjectHelper
    //{
    //    // This can be key name you want. Should be unique for the app.
    //    static readonly string KEY_NAME = "com.xamarin.android.sample.fingerprint_authentication_key";

    //    // We always use this keystore on Android.
    //    static readonly string KEYSTORE_NAME = "AndroidKeyStore";

    //    // Should be no need to change these values.
    //    static readonly string KEY_ALGORITHM = KeyProperties.KeyAlgorithmAes;
    //    static readonly string BLOCK_MODE = KeyProperties.BlockModeCbc;
    //    static readonly string ENCRYPTION_PADDING = KeyProperties.EncryptionPaddingPkcs7;
    //    static readonly string TRANSFORMATION = KEY_ALGORITHM + "/" +
    //                                            BLOCK_MODE + "/" +
    //                                            ENCRYPTION_PADDING;
    //    readonly KeyStore _keystore;

    //    public CryptoObjectHelper()
    //    {
    //        _keystore = KeyStore.GetInstance(KEYSTORE_NAME);
    //        _keystore.Load(null);
    //    }

    //    public FingerprintManagerCompat.CryptoObject BuildCryptoObject()
    //    {
    //        Cipher cipher = CreateCipher();
    //        return new FingerprintManagerCompat.CryptoObject(cipher);
    //    }

    //    Cipher CreateCipher(bool retry = true)
    //    {
    //        IKey key = GetKey();
    //        Cipher cipher = Cipher.GetInstance(TRANSFORMATION);
    //        try
    //        {
    //            cipher.Init(CipherMode.EncryptMode, key);
    //        }
    //        catch (KeyPermanentlyInvalidatedException e)
    //        {
    //            _keystore.DeleteEntry(KEY_NAME);
    //            if (retry)
    //            {
    //                CreateCipher(false);
    //            }
    //            else
    //            {
    //                throw new System.Exception("Could not create the cipher for fingerprint authentication.", e);
    //            }
    //        }
    //        return cipher;
    //    }

    //    IKey GetKey()
    //    {
    //        IKey secretKey;
    //        if (!_keystore.IsKeyEntry(KEY_NAME))
    //        {
    //            CreateKey();
    //        }

    //        secretKey = _keystore.GetKey(KEY_NAME, null);
    //        return secretKey;
    //    }

    //    void CreateKey()
    //    {
    //        KeyGenerator keyGen = KeyGenerator.GetInstance(KeyProperties.KeyAlgorithmAes, KEYSTORE_NAME);
    //        KeyGenParameterSpec keyGenSpec =
    //            new KeyGenParameterSpec.Builder(KEY_NAME, KeyStorePurpose.Encrypt | KeyStorePurpose.Decrypt)
    //                .SetBlockModes(BLOCK_MODE)
    //                .SetEncryptionPaddings(ENCRYPTION_PADDING)
    //                .SetUserAuthenticationRequired(true)
    //                .Build();
    //        keyGen.Init(keyGenSpec);
    //        keyGen.GenerateKey();
    //    }
    //}
    //class MyAuthCallbackSample : FingerprintManagerCompat.AuthenticationCallback
    //{
    //    // Can be any byte array, keep unique to application.
    //    static readonly byte[] SECRET_BYTES = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
    //    // The TAG can be any string, this one is for demonstration.
    //    static readonly string TAG = "X:";// + typeof(SimpleAuthCallback).Name;

    //    public MyAuthCallbackSample()
    //    {
    //    }

    //    public override void OnAuthenticationSucceeded(FingerprintManagerCompat.AuthenticationResult result)
    //    {
    //        if (result.CryptoObject.Cipher != null)
    //        {
    //            try
    //            {
    //                // Calling DoFinal on the Cipher ensures that the encryption worked.
    //                byte[] doFinalResult = result.CryptoObject.Cipher.DoFinal(SECRET_BYTES);

    //                // No errors occurred, trust the results.              
    //            }
    //            catch (BadPaddingException bpe)
    //            {
    //                // Can't really trust the results.
    //                Log.Error(TAG, "Failed to encrypt the data with the generated key." + bpe);
    //            }
    //            catch (IllegalBlockSizeException ibse)
    //            {
    //                // Can't really trust the results.
    //                Log.Error(TAG, "Failed to encrypt the data with the generated key." + ibse);
    //            }
    //        }
    //        else
    //        {
    //            // No cipher used, assume that everything went well and trust the results.
    //        }
    //    }

    //    public override void OnAuthenticationError(int errMsgId, ICharSequence errString)
    //    {
    //        // Report the error to the user. Note that if the user canceled the scan,
    //        // this method will be called and the errMsgId will be FingerprintState.ErrorCanceled.
    //    }

    //    public override void OnAuthenticationFailed()
    //    {
    //        // Tell the user that the fingerprint was not recognized.
    //    }

    //    public override void OnAuthenticationHelp(int helpMsgId, ICharSequence helpString)
    //    {
    //        // Notify the user that the scan failed and display the provided hint.
    //    }
    //}
}