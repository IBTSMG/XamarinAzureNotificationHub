using Android.App;
using Android.Content;
using Android.Util;
using Gcm.Client;
using System;
using System.Collections.Generic;
using System.Text;
using WindowsAzure.Messaging;

namespace AzurePush.Droid
{
    [Service]
    public class PushHandlerService : GcmServiceBase
    {
        public static string RegistrationID { get; private set; }
        private NotificationHub Hub { get; set; }

        public PushHandlerService() : base(App.SenderID)
        {
            Log.Info(MyBroadcastReceiver.TAG, "PushHandlerService() constructor");
        }
        protected override void OnUnRegistered(Context context, string registrationId)
        {
            Log.Verbose(MyBroadcastReceiver.TAG, "GCM Unregistered: " + registrationId);

            createNotification("GCM Unregistered...", "The device has been unregistered!");
        }

        protected override bool OnRecoverableError(Context context, string errorId)
        {
            Log.Warn(MyBroadcastReceiver.TAG, "Recoverable Error: " + errorId);

            return base.OnRecoverableError(context, errorId);
        }

        protected override void OnError(Context context, string errorId)
        {
            Log.Error(MyBroadcastReceiver.TAG, "GCM Error: " + errorId);
        }
        protected override void OnMessage(Context context, Intent intent)
        {
            Log.Info(MyBroadcastReceiver.TAG, "GCM Message Received!");

            var msg = new StringBuilder();

            if (intent != null && intent.Extras != null)
            {
                foreach (var key in intent.Extras.KeySet())
                    msg.AppendLine(key + "=" + intent.Extras.Get(key).ToString());
            }

            string messageText = intent.Extras.GetString("message");
            if (!string.IsNullOrEmpty(messageText))
            {
                createNotification("New hub message!", messageText);
            }
            else
            {
                createNotification("Unknown message details", msg.ToString());
            }
        }

        /*

            GcmServiceBase implements methods OnRegistered(), OnUnRegistered(), OnMessage(), OnRecoverableError(), and OnError(). 
            Our PushHandlerService implementation class must override these methods, and these methods will fire in response to interacting with the notification hub.

            source:https://docs.microsoft.com/en-us/azure/notification-hubs/xamarin-notification-hubs-push-notifications-android-gcm
        */
        protected override void OnRegistered(Context context, string registrationId)
        {
            Log.Verbose(MyBroadcastReceiver.TAG, "GCM Registered: " + registrationId);
            RegistrationID = registrationId;

            createNotification("PushHandlerService-GCM Registered...",
                                "The device has been Registered!");

            Hub = new NotificationHub(App.NotificationHubName,
                App.ListenConnectionString, context);
            try
            {
                Hub.UnregisterAll(registrationId);
            }
            catch (Exception ex)
            {
                Log.Error(MyBroadcastReceiver.TAG, ex.Message);
            }


            // create tags if you want. We didn't use.
            // In the OnRegistered() code above, you should note the ability to specify tags to register for specific messaging channels.
            //var tags = new List<string>() { };
            // Hub.Register(registrationId, tags.ToArray());

            try
            {
                var hubRegistration = Hub.Register(registrationId);
            }
            catch (Exception ex)
            {
                Log.Error(MyBroadcastReceiver.TAG, ex.Message);
            }
        }

        void createNotification(string title, string desc)
        {
            var notificationManager = GetSystemService(Context.NotificationService) as NotificationManager;
            var uiIntent = new Intent(this, typeof(MainActivity));
            var notification = new Notification(Android.Resource.Drawable.SymActionEmail, title);
            notification.Flags = NotificationFlags.AutoCancel;
            notification.SetLatestEventInfo(this, title, desc, PendingIntent.GetActivity(this, 0, uiIntent, 0));
            notificationManager.Notify(1, notification);
            dialogNotify(title, desc);
        }

        protected void dialogNotify(String title, String message)
        {
            MainActivity.instance.RunOnUiThread(() =>
            {
                AlertDialog.Builder dlg = new AlertDialog.Builder(MainActivity.instance);
                AlertDialog alert = dlg.Create();
                alert.SetTitle(title);
                alert.SetButton("Ok", delegate
                {
                    alert.Dismiss();
                });
                alert.SetMessage(message);
                alert.Show();
            });
        }
    }

    [BroadcastReceiver(Permission = Constants.PERMISSION_GCM_INTENTS)]
    [IntentFilter(new string[] { Constants.INTENT_FROM_GCM_MESSAGE },
         Categories = new string[] { "com.ibtech.xamarinfcmsample" })]
    [IntentFilter(new string[] { Constants.INTENT_FROM_GCM_REGISTRATION_CALLBACK },
         Categories = new string[] { "com.ibtech.xamarinfcmsample" })]
    [IntentFilter(new string[] { Constants.INTENT_FROM_GCM_LIBRARY_RETRY },
         Categories = new string[] { "com.ibtech.xamarinfcmsample" })]
    public class MyBroadcastReceiver : GcmBroadcastReceiverBase<PushHandlerService>
    {
        public static string[] SENDER_IDS = new string[] { App.SenderID };

        public const string TAG = "MyBroadcastReceiver-GCM";
    }
}