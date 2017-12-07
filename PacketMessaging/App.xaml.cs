using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Xaml;
using PacketMessaging.Services.SettingsServices;
using MetroLog;
using Template10.Controls;
using MetroLog.Targets;
using PacketMessaging.ViewModels;
using PacketMessaging.Models;

namespace PacketMessaging
{
	sealed partial class App : Template10.Common.BootStrapper
	{
		public static Dictionary<string, TacticalCallsignData> _tacticalCallsignDataDictionary;

		public App()
		{
			InitializeComponent();
			SplashFactory = (e) => new Views.Splash(e);

			#region App settings

			var _settings = SettingsService.Instance;
			RequestedTheme = _settings.AppTheme;
			CacheMaxDuration = _settings.CacheMaxDuration;
			ShowShellBackButton = _settings.UseShellBackButton;

			#endregion

			// Initialize MetroLog
			LogManagerFactory.DefaultConfiguration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new StreamingFileTarget());

			GlobalCrashHandler.Configure();

			// Initialize Tactical call signs data
			_tacticalCallsignDataDictionary = new Dictionary<string, TacticalCallsignData>();

			TacticalCallsignData tacticalCallsignData = new TacticalCallsignData()
			{
				AreaName = "County Agencies",
				FileName = "CountyTacticalCallsigns.xml",
				StartString = "Santa Clara County Cities/Agencies",
				BulletinFileName = "SCCo Packet Tactical Calls"
			};
			_tacticalCallsignDataDictionary.Add(tacticalCallsignData.FileName, tacticalCallsignData);

			tacticalCallsignData = new TacticalCallsignData()
			{
				AreaName = "non County Agencies",
				FileName = "NonCountyTacticalCallsigns.xml",
				StartString = "Other (non-SCCo) Agencies",
				BulletinFileName = "SCCo Packet Tactical Calls"
			};
			_tacticalCallsignDataDictionary.Add(tacticalCallsignData.FileName, tacticalCallsignData);

			tacticalCallsignData = new TacticalCallsignData()
			{
				AreaName = "Local Mountain View",
				FileName = "MTVTacticalCallsigns.xml",
				TacticallWithBBS = "MTVEOC",
				StartString = "#Mountain View Tactical Call List",
				StopString = "#MTV001 thru MTV010 also permissible",
				RawDataFileName = "Tactical_Calls.txt"
			};
			_tacticalCallsignDataDictionary.Add(tacticalCallsignData.FileName, tacticalCallsignData);

			tacticalCallsignData = new TacticalCallsignData()
			{
				AreaName = "Local Cupertino",
				FileName = "CUPTacticalCallsigns.xml",
				TacticallWithBBS = "CUPEOC",
				StartString = "# Cupertino OES",
				StopString = "# City of Palo Alto",
				RawDataFileName = "Tactical_Calls.txt"
			};
			_tacticalCallsignDataDictionary.Add(tacticalCallsignData.FileName, tacticalCallsignData);

			tacticalCallsignData = new TacticalCallsignData()
			{
				AreaName = "County Hospitals",
				FileName = "HospitalsTacticalCallsigns.xml",
				TacticallWithBBS = "HOSDOC",
				StartString = "# SCCo Hospitals Packet Tactical Call Signs",
				StopString = "# HOS001 - HOS010",
				RawDataFileName = "Tactical_Calls.txt"
			};
			_tacticalCallsignDataDictionary.Add(tacticalCallsignData.FileName, tacticalCallsignData);

			tacticalCallsignData = new TacticalCallsignData()
			{
				AreaName = "All County",
				FileName = "AllCountyTacticalCallsigns.xml",
				StartString = "",
				BulletinFileName = "https://scc-ares-races.org/activities/showtacticalcalls.php"
			};
			_tacticalCallsignDataDictionary.Add(tacticalCallsignData.FileName, tacticalCallsignData);

			tacticalCallsignData = new TacticalCallsignData()
			{
				AreaName = "User Address Book",
				FileName = "UserAddressBook.xml",
				StartString = "",
				BulletinFileName = ""
			};

			//Debug.WriteLine("Application Started");
		}

		public override async Task OnInitializeAsync(IActivatedEventArgs args)
		{
			if (Window.Current.Content as ModalDialog == null)
			{
				// create a new frame 
				var nav = NavigationServiceFactory(BackButton.Attach, ExistingContent.Include);
				// create modal root
				Window.Current.Content = new ModalDialog
				{
					DisableBackButtonWhenModal = true,
					Content = new Views.Shell(nav),
					ModalContent = new Views.Busy(),
				};
			}
			await Task.CompletedTask;
		}

		public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
		{
			SharedData sharedData = SharedData.SharedDataInstance;

			if (args.Kind == ActivationKind.Launch)
			{
				BackgroundTransfer backgroundTransfer = BackgroundTransfer.CreateBackgroundTransfer();
				await backgroundTransfer.DiscoverActiveDownloadsAsync();
			}

			//long-running startup tasks go here
			StorageFolder localFolder = ApplicationData.Current.LocalFolder;
			Views.MainPage._unsentMessagesFolder = await localFolder.CreateFolderAsync("UnsentMessages", CreationCollisionOption.OpenIfExists);
			Views.MainPage._sentMessagesFolder = await localFolder.CreateFolderAsync("SentMessages", CreationCollisionOption.OpenIfExists);
			Views.MainPage._receivedMessagesFolder = await localFolder.CreateFolderAsync("ReceivedMessages", CreationCollisionOption.OpenIfExists);
			Views.MainPage._draftMessagesFolder = await localFolder.CreateFolderAsync("DraftMessages", CreationCollisionOption.OpenIfExists);
			Views.MainPage._archivedMessagesFolder = await localFolder.CreateFolderAsync("ArchivedMessages", CreationCollisionOption.OpenIfExists);
			Views.MainPage._deletedMessagesFolder = await localFolder.CreateFolderAsync("DeletedMessages", CreationCollisionOption.OpenIfExists);
			Views.MainPage.LogsFolder = await localFolder.CreateFolderAsync("Logs", CreationCollisionOption.OpenIfExists);
			Views.MainPage._MetroLogsFolder = await localFolder.CreateFolderAsync("MetroLogs", CreationCollisionOption.OpenIfExists);

			SharedData.filesInInstalledLocation = await Package.Current.InstalledLocation.GetFilesAsync();

			sharedData.BbsArray = await BBSDefinitions.OpenAsync(localFolder);  //"ms-appx:///Assets/pdffile.pdf"
			sharedData.TncDeviceArray = await TNCDeviceArray.OpenAsync(localFolder);

			sharedData.ProfileArray = await ProfileArray.OpenAsync(localFolder);

			foreach (var tacticalCallsignType in _tacticalCallsignDataDictionary.Values)
			{
				tacticalCallsignType.TacticalCallsigns = await TacticalCallsigns.OpenAsync(tacticalCallsignType.FileName);
			}
			await AddressBook.Instance.OpenAsync();

			await DistributionListArray.Instance.OpenAsync();

			await ListViewParametersArray.Instance.OpenAsync();

			NavigationService.Navigate(typeof(Views.MainPage));
			await Task.CompletedTask;
		}

		public override async Task OnSuspendingAsync(object s, SuspendingEventArgs e, bool prelaunchActivated)
		{
			// Save ListView state
			await ListViewParametersArray.Instance.SaveAsync();

			await base.OnSuspendingAsync(s, e, prelaunchActivated);
			return; 
		}
		///// <summary>
		///// Provides application-specific behavior to supplement the default Application class.
		///// </summary>
		//sealed partial class App : Application
		//{
		//    /// <summary>
		//    /// Initializes the singleton application object.  This is the first line of authored code
		//    /// executed, and as such is the logical equivalent of main() or WinMain().
		//    /// </summary>
		//    public App()
		//    {
		//        this.InitializeComponent();
		//        this.Suspending += OnSuspending;
		//    }

		//    /// <summary>
		//    /// Invoked when the application is launched normally by the end user.  Other entry points
		//    /// will be used such as when the application is launched to open a specific file.
		//    /// </summary>
		//    /// <param name="e">Details about the launch request and process.</param>
		//    protected override void OnLaunched(LaunchActivatedEventArgs e)
		//    {
		//        Frame rootFrame = Window.Current.Content as Frame;

		//        // Do not repeat app initialization when the Window already has content,
		//        // just ensure that the window is active
		//        if (rootFrame == null)
		//        {
		//            // Create a Frame to act as the navigation context and navigate to the first page
		//            rootFrame = new Frame();

		//            rootFrame.NavigationFailed += OnNavigationFailed;

		//            if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
		//            {
		//                //TODO: Load state from previously suspended application
		//            }

		//            // Place the frame in the current Window
		//            Window.Current.Content = rootFrame;
		//        }

		//        if (e.PrelaunchActivated == false)
		//        {
		//            if (rootFrame.Content == null)
		//            {
		//                // When the navigation stack isn't restored navigate to the first page,
		//                // configuring the new page by passing required information as a navigation
		//                // parameter
		//                rootFrame.Navigate(typeof(MainPage), e.Arguments);
		//            }
		//            // Ensure the current window is active
		//            Window.Current.Activate();
		//        }
		//    }

		//    /// <summary>
		//    /// Invoked when Navigation to a certain page fails
		//    /// </summary>
		//    /// <param name="sender">The Frame which failed navigation</param>
		//    /// <param name="e">Details about the navigation failure</param>
		//    void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
		//    {
		//        throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
		//    }

		/// <summary>
		/// Invoked when application execution is being suspended.  Application state is saved
		/// without knowing whether the application will be terminated or resumed with the contents
		/// of memory still intact.
		/// </summary>
		/// <param name="sender">The source of the suspend request.</param>
		/// <param name="e">Details about the suspend request.</param>
		//public override async Task App.OnSuspendingAsync(object sender, SuspendingEventArgs e)
		//{
		//	var deferral = e.SuspendingOperation.GetDeferral();
		//	//TODO: Save application state and stop any background activity
		//	deferral.Complete();
		//}
	}
}

