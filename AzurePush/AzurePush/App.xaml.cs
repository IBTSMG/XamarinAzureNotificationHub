using Xamarin.Forms;

namespace AzurePush
{
	public partial class App : Application
	{
		public const string SenderID = "xxxxxx"; // Google API Project Number
		public const string ListenConnectionString = "Endpoint=sb://ibtxamarin.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=xxxxxxx";
		public const string NotificationHubName = "IBTXamarinNotificationHub";
		public App()
		{
			InitializeComponent();

			MainPage = new AzurePush.MainPage();
		}

		protected override void OnStart()
		{
			// Handle when your app starts
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}
	}
}
