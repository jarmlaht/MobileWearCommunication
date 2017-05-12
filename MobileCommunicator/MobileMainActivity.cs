using Android.App;
using Android.Widget;
using Android.OS;
using System;
using Android.Gms.Wearable;

namespace Communication
{
    [Activity(Label = "MobileCommunicator", MainLauncher = true, Icon = "@drawable/icon")]
    public class MobileMainActivity : Activity
    {
        Communicator communicator;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);

            communicator = new Communicator(this);

            var messageButton = FindViewById<Button>(Resource.Id.messageButton);
            messageButton.Click += (sender, e) => communicator.SendMessage("time: " + DateTime.Now.ToString("T"));
            communicator.MessageReceived += message => RunOnUiThread(() => messageButton.Text = message);

            var dataButton = FindViewById<Button>(Resource.Id.dataButton);
            dataButton.Click += delegate {
                var dataMap = new DataMap();
                dataMap.PutString("time", DateTime.Now.ToString("T"));
                communicator.SendData(dataMap);
            };
            communicator.DataReceived += dataMap => RunOnUiThread(() => dataButton.Text = dataMap.ToString());
        }

        protected override void OnResume()
        {
            base.OnResume();
            communicator.Resume();
        }

        protected override void OnPause()
        {
            communicator.Pause();
            base.OnPause();
        }
    }
}

