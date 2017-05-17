using FormControlBaseClass;
using PacketMessaging.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Template10.Common;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth.Rfcomm;
using MetroLog;
using System.Reflection;
using System.Numerics;
using Windows.UI.Xaml.Markup;
using System.Text;
using System.Xml;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

//if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")

namespace PacketMessaging.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
		private ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<MainPage>();

		Template10.Services.SerializationService.ISerializationService _SerializationService;
        Services.SettingsServices.SettingsService _settings;

        public static StorageFolder unsentMessagesFolder = null;
        public static StorageFolder _sentMessagesFolder = null;
        public static StorageFolder receivedMessagesFolder = null;
        public static StorageFolder _draftMessagesFolder = null;
        public static StorageFolder _archivedMessagesFolder = null;
        public static StorageFolder _deletedMessagesFolder = null;
        public static StorageFolder LogsFolder = null;
		public static StorageFolder _MetroLogsFolder = null;

		ListView _currentListView = null;
		GridView _currentGridView = null;
		PacketMessageViewList _packetMessageViewListDrafts;
		PacketMessageViewList _packetMessageViewListDeleted;
		string _sortOnPropertyName;
		SortDirection _sortDirection = SortDirection.Ascending;

		public static MainPage Current;

		public double _gridViewItemWidth
		{ get => 218; }

		//PacketMessage _selectedMessage = null;
		List<PacketMessage> _selectedMessages = new List<PacketMessage>();


		public MainPage()
        {
            this.InitializeComponent();

            NavigationCacheMode = NavigationCacheMode.Enabled;
            _SerializationService = Template10.Services.SerializationService.SerializationService.Json;
            _settings = Services.SettingsServices.SettingsService.Instance;

            Current = this;

			log.Info("");
			log.Info("Initializing Packet Messaging Application");
            foreach (PivotItem item in MyPivot.Items)
            {
                if (item.Name == "InBox")
                {
                    item.Tag = receivedMessagesFolder;
                }
                else if (item.Name == "Sent")
                {
                    item.Tag = _sentMessagesFolder;
                }
                else if (item.Name == "Outbox")
                {
                    item.Tag = unsentMessagesFolder;
                }
                else if (item.Name == "Drafts")
                {
                    item.Tag = _draftMessagesFolder;
                }
                else if (item.Name == "Archive")
                {
                    item.Tag = _archivedMessagesFolder;
                }
                else if (item.Name == "Deleted")
                {
                    item.Tag = _deletedMessagesFolder;
                }
            }

            SharedData sharedData = SharedData.SharedDataInstance;

            sharedData.CurrentProfile = null;
            foreach (Profile profile in sharedData.ProfileArray.Profiles)
            {
                if (profile.Selected)
                {
                    sharedData.CurrentProfile = profile;
                }
            }

            if (sharedData.CurrentProfile == null)
            {
                sharedData.CurrentProfile = sharedData.ProfileArray.Profiles[0];
            }

            foreach (BBSData bbs in sharedData.BbsArray.BBSDataArray)
            {
                if (sharedData.CurrentProfile.BBS == bbs.Name)
                {
                    sharedData.CurrentBBS = bbs;
                }
            }

            foreach (TNCDevice tncDevice in sharedData.TncDeviceArray.TNCDevices)
            {
                if (sharedData.CurrentProfile.TNC == tncDevice.Name)
                {
                    sharedData.CurrentTNCDevice = tncDevice;
                }
            }

            string areas = _settings.JNOSAreas;
            SharedData._Areas = areas.Split(new char[] { ',', ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

			AddressBook.CreateAddressBook();

			ObservableCollection<Field> draftsPropertiesList = new ObservableCollection<Field>();

			_packetMessageViewListDrafts = new PacketMessageViewList();
			_packetMessageViewListDrafts.Add(new CreateTime("120"));
			_packetMessageViewListDrafts.Add(new Subject("1*"));
			_packetMessageViewListDrafts.Add(new MessageNumber("Number", "110"));
			_packetMessageViewListDrafts.Add(new MessageTo("70"));
			_packetMessageViewListDrafts.Add(new MessageFrom("70"));
			_packetMessageViewListDrafts.Add(new BBSName("60"));

			foreach (Field field in _packetMessageViewListDrafts.MessageViewList)
			{
				draftsPropertiesList.Add(field);
			}
			listViewProperties.Source = draftsPropertiesList;

			_packetMessageViewListDeleted = _packetMessageViewListDrafts;

			if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
			{


				ObservableCollection<Field> deletedPropertiesList = new ObservableCollection<Field>();

				viewColumnsDeleted.Source = draftsPropertiesList;
			}
		}

		private void OpenMessage()
        {
            if (_selectedMessages.Count == 1)
            {
                string folder = ((StorageFolder)((PivotItem)MyPivot.SelectedItem).Tag).Path;
                string packetMessagePath = folder + @"\" + _selectedMessages[0].FileName;
                var nav = WindowWrapper.Current().NavigationServices.FirstOrDefault();
                nav.Navigate(typeof(FormsPage), packetMessagePath);
            }
        }

		public enum SortDirection
		{
			Ascending,
			Descending
		}

		public List<T> Sort_List<T>(SortDirection sortDirection, string sortProperty, List<T> data)
		{
			if (string.IsNullOrEmpty(sortProperty) || typeof(SortDirection) != sortDirection.GetType())
				return data;

			List<T> data_sorted = new List<T>();

			if (sortDirection == SortDirection.Ascending)
			{
				data_sorted = (from n in data
							   orderby GetDynamicSortProperty(n, sortProperty) ascending
							   select n).ToList();
			}
			else if (sortDirection == SortDirection.Descending)
			{
				data_sorted = (from n in data
							   orderby GetDynamicSortProperty(n, sortProperty) descending
							   select n).ToList();
			}
			return data_sorted;
		}

		public object GetDynamicSortProperty(object item, string propName)
		{
			//Use reflection to get order type
			return item.GetType().GetProperty(propName).GetValue(item);
		}

		private void RefreshListView()
		{
			if ((PivotItem)MyPivot.SelectedItem == null)
				return;

			RefreshListViewAsync((PivotItem)MyPivot.SelectedItem);
		}

		private async void RefreshListViewAsync(PivotItem pivotItem)
        {
			ListView listView = new ListView();

			double gridViewItemWidth = 0;
			//if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
			//{
			//	//if (pivotItem.Name == "Drafts")
			//	//{
			//		if (MyPivot.ActualWidth > MyPivot.ActualHeight)
			//			gridViewItemWidth = (MyPivot.ActualWidth - 40) / 2;
			//		else
			//			gridViewItemWidth = MyPivot.ActualWidth - 36;

			//		//MainPageViewModel._MainPageViewModel.GridViewItemWidth = gridViewItemWidth;
			//	//}
			//}

			var messagesInFolder = await PacketMessage.GetPacketMessages((StorageFolder)pivotItem.Tag);
			List<PacketMessage> sortedList = Sort_List(_sortDirection, _sortOnPropertyName, messagesInFolder);
			if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
			{
				if (MyPivot.ActualWidth > MyPivot.ActualHeight)
					gridViewItemWidth = (MyPivot.ActualWidth - 40) / 2;
				else
					gridViewItemWidth = MyPivot.ActualWidth - 36;

				if (sortedList.Count > 0)
					sortedList[0].GridWidth = gridViewItemWidth;
			}

			messageFolderCollection.Source = new ObservableCollection<PacketMessage>(sortedList);
		}

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter == null)
                return;

            var index = int.Parse(_SerializationService.Deserialize(e.Parameter?.ToString()).ToString());
            MyPivot.SelectedIndex = index;

			RefreshListViewAsync((PivotItem)MyPivot.Items[index]);
        }

		public async Task CreateGridHeaderAsync(PacketMessageViewList packetMessageViewList)
		{
			if (packetMessageViewList == null)
				return;

			StringBuilder header=new StringBuilder(@"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">");
			//StringBuilder header = new StringBuilder(@"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml"">");
			//StringBuilder header = new StringBuilder();
			header.Append(@"<Grid Margin=""12,0,12,0""><Grid.ColumnDefinitions>");
			foreach (var field in packetMessageViewList.MessageViewList)
			{
				header.Append(@"<ColumnDefinition Width=""" + $"{field.Width}" + @"""/>");
			}
			header.Append("</Grid.ColumnDefinitions>");
			for (int i = 0; i < packetMessageViewList.MessageViewList.Count; i++)
			{
				if (packetMessageViewList.MessageViewList[i].WidthAsString.Contains("*"))
					header.Append(@"<TextBlock Grid.Column=""" + $"{i}" + @""" Text=""" + $"{packetMessageViewList.MessageViewList[i].HeaderShort}" + @""" FontSize=""17"" HorizontalAlignment=""Left"" DoubleTapped=""TextBlock_DoubleTapped""/>");
				else
					header.Append(@"<TextBlock DoubleTapped=""TextBlock_DoubleTapped"" Grid.Column=""" + $"{i}" + @""" Text=""" + $"{packetMessageViewList.MessageViewList[i].HeaderShort}" + @""" FontSize=""17"" HorizontalAlignment=""Center""/>");
			}
			header.Append("</Grid>");
			header.Append("</DataTemplate>");
			string headerString = header.ToString();

			StringReader reader = new StringReader(headerString);
			var template = XamlReader.Load(await reader.ReadToEndAsync());
			listViewDrafts.HeaderTemplate = template as DataTemplate;
			//listViewDrafts.Header = template;
		}

		public sealed class DateTimeConverter : IValueConverter
		{
			object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
			{
				if (value == null)
				{
					return "";
				}
				DateTime dateTime = (DateTime)value;
				string date = $"{dateTime.Month:d2}/{dateTime.Day:d2}/{dateTime.Year - 2000:d2} {dateTime.Hour:d2}:{dateTime.Minute:d2}";
				return date;
			}

			object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
			{
				throw new NotImplementedException();
			}
		}

		public async Task CreateGridItemTemplateAsync(PacketMessageViewList packetMessageViewList)
		{
			if (packetMessageViewList == null)
				return;

			StringBuilder header = new StringBuilder();
			header.Append("<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
			//header.Append(" xmlns:data=\"using:FormControlBaseClass\"");
			//header.Append(" Name=\"DeletedListViewTemplate\"");
			header.Append(">");
			header.Append("<Grid ><Grid.ColumnDefinitions>");
			foreach (var field in packetMessageViewList.MessageViewList)
			{
				header.Append("<ColumnDefinition Width=\"" + $"{field.WidthAsString}" + "\"/>");
			}
			header.Append("</Grid.ColumnDefinitions>");
			for (int i = 0; i < packetMessageViewList.MessageViewList.Count; i++)
			{
				//if (packetMessageViewList.MessageViewList[i].PropertyName.Contains("Time"))
				//{
				//	header.Append("<TextBlock Grid.Column=\"" + $"{i}" + "\" Text=\"{Binding " + $"{packetMessageViewList.MessageViewList[i].PropertyName}" + ", Converter={ DatetimeConverter}}\" HorizontalAlignment=\"Center\"/>");
				//}
				//else
				//{
					header.Append("<TextBlock Grid.Column=\"" + $"{i}" + "\" Text=\"{Binding " + $"{packetMessageViewList.MessageViewList[i].PropertyName}" + "}\" HorizontalAlignment=\"Center\"/>");
				//}
			}
			header.Append("</Grid>");
			header.Append("</DataTemplate>");
			string headerString = header.ToString();

			//headerString = @"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
   //<Ellipse Width=""300.5"" Height=""200"" Fill=""Red""/>
   //</DataTemplate>";


			StringReader reader = new StringReader(headerString);
			var template = XamlReader.Load(await reader.ReadToEndAsync());
			listViewDeletedItems.ItemTemplate = template as DataTemplate;
		}

		private void ListBox_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            OpenMessage();
        }

		public void ShowMessageBox(string message)
		{
			MessageDialog messageDialog = new MessageDialog(message);
		}

		//public delegate void DoubleTappedEventHandler(object sender, DoubleTappedRoutedEventArgs e);

		private async void MyPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			_selectedMessages?.Clear();

			PivotItem pivotItem = (PivotItem)e.AddedItems[0];

			if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop")
			{
				PacketMessageViewList messageViewList = null;
				switch (pivotItem.Name)
				{
					case "InBox":
						_currentListView = listViewInBox;
						break;

					case "Sent":
						_currentListView = listViewSentItems;
						break;

					case "OutBox":
						_currentListView = listViewOutBox;
						break;

					case "Drafts":
						_currentListView = listViewDrafts;
						messageViewList = _packetMessageViewListDrafts;
						break;

					case "Archive":
						_currentListView = listViewArchivedItems;
						break;

					case "Deleted":
						_currentListView = listViewDeletedItems;
						Grid header = (Grid)listViewDeletedItems.Header;
						//UIElementCollection columns = header.Children;

						ColumnDefinition columnDefinition;
						for (int i = 0; i < _packetMessageViewListDeleted.MessageViewList.Count; i++)
						{
							columnDefinition = new ColumnDefinition()
							{
								Width = _packetMessageViewListDeleted.MessageViewList[i].Width
							};
							header.ColumnDefinitions.Add(columnDefinition);
							TextBlock columnTextBlock = new TextBlock();
							header.Children.Add(columnTextBlock);
							columnTextBlock.Text = _packetMessageViewListDeleted.MessageViewList[i].HeaderShort;
							columnTextBlock.FontSize = 17;
							columnTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
							columnTextBlock.Tag = _packetMessageViewListDeleted.MessageViewList[i].PropertyName;
							columnTextBlock.AddHandler(DoubleTappedEvent, new DoubleTappedEventHandler(TextBlock_DoubleTapped), true);
							Grid.SetColumn(columnTextBlock, i);
						}

						//DataTemplate itemTemplate = new DataTemplate();
						//itemTemplate.LoadContent();
						////DataTemplate itemTemplate = listViewDeletedItems.ItemTemplate;
						//itemTemplate.;
						//for (int i = 0; i < _packetMessageViewListDeleted.MessageViewList.Count; i++)
						//{
						//	columnDefinition = new ColumnDefinition()
						//	{
						//		Width = _packetMessageViewListDeleted.MessageViewList[i].Width
						//	};
						//	itemTemplate.ColumnDefinitions.Add(columnDefinition);
						//	TextBlock columnTextBlock = new TextBlock();
						//	itemTemplate.Children.Add(columnTextBlock);
						//}
						await CreateGridItemTemplateAsync(_packetMessageViewListDeleted);
						break;
				}
				//await CreateGridHeaderAsync(messageViewList);
			}
			else
			{
				switch (pivotItem.Name)
				{
					case "InBox":
						_currentListView = listViewInBox;
						break;

					case "Sent":
						_currentListView = listViewSentItems;
						break;

					case "OutBox":
						_currentListView = listViewOutBox;
						break;

					case "Drafts":
						_currentListView = listViewDrafts;
						break;

					case "Archive":
						_currentListView = listViewArchivedItems;
						break;

					case "Deleted":
						_currentListView = listViewDeletedItems;
						break;
				}
			}
			RefreshListViewAsync(pivotItem);
		}

		private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
			_currentListView = (ListView)sender;

			_selectedMessages.Clear();

			foreach (var message in e.AddedItems)
			{
				_selectedMessages.Add(message as PacketMessage);
			}

			var count = _selectedMessages.Count;
		}

		private void AppBar_SendReceive(object sender, RoutedEventArgs e)
		{
			Services.CommunicationsService.CommunicationsService communicationsService = Services.CommunicationsService.CommunicationsService.CreateInstance();
			communicationsService.BBSConnectAsync();
		}

		private void AppBarButton_OpenMessage(object sender, RoutedEventArgs e)
        {
            OpenMessage();
        }

        private async void AppBar_DeleteItemAsync(object sender, RoutedEventArgs e)
        {
            PivotItem pivotItem = (PivotItem)MyPivot.SelectedItem;
            StorageFolder folder = (StorageFolder)pivotItem.Tag;
            bool permanentlyDelete = false;
            if (folder == _deletedMessagesFolder)
            {
                permanentlyDelete = true;
            }

			IList<Object> selectedMessages = null;
			if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
			{
				if (pivotItem.Name == "Drafts")
					selectedMessages = _currentGridView.SelectedItems;
				else
					selectedMessages = _currentListView.SelectedItems;
			}
			else
			{
				selectedMessages = _currentListView.SelectedItems;
			}
			foreach (PacketMessage packetMessage in selectedMessages)
            {
                try
                {
                    var file = await folder.CreateFileAsync(packetMessage.FileName, CreationCollisionOption.OpenIfExists);
                    if (permanentlyDelete)
                    {
                        await file?.DeleteAsync();
                    }
                    else
                    {
                        await file?.MoveAsync(_deletedMessagesFolder);
                    }
                }
                catch
                {
                    continue;
                }
            }
			RefreshListViewAsync(pivotItem);
        }

		// DeviceFamily Mobile
		private void ComboBoxSelectListProperty_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			string selection = e.AddedItems[0] as string;
			_sortOnPropertyName = ((Field)e.AddedItems[0]).PropertyName as string;
			RefreshListView();
		}

		private void SearchAutoSelectBox_TextChanged(object sender, AutoSuggestBoxTextChangedEventArgs e)
		{
			/* The TextChanged event occurs whenever the content of the text box is updated. 
			 * Use the event args Reason property to determine whether the change was due to 
			 * user input. If the change reason is UserInput, filter your data based on the input. 
			 * Then, set the filtered data as the ItemsSource of the AutoSuggestBox to update the suggestion list.
			*/
			if (e.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
			{

			}
		}

		private void SearchAutoSelectBox_QuerySubmitted(object sender, AutoSuggestBoxQuerySubmittedEventArgs e)
		{
			/* The QuerySubmitted event occurs when a user commits a query string. 
			 * The user can commit a query in one of these ways:
			 *		While the focus is in the text box, press Enter or click the query icon. 
			 *			The event args ChosenSuggestion property is null.
			 *		While the focus is in the suggestion list, press Enter, click, or tap an item. 
			 *			The event args ChosenSuggestion property contains the item that was selected from the list.
			 *	In all cases, the event args QueryText property contains the text from the text box.
			*/
		}

		private void ListViewMessages_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
		{
			OpenMessage();
		}

		private void ListViewMessages_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			_selectedMessages.Clear();

			foreach (var message in e.AddedItems)
			{
				_selectedMessages.Add(message as PacketMessage);
			}
		}

		public void TextBlock_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
		{
			// Get the sprting property
			_sortOnPropertyName = (string)((TextBlock)sender).Tag;
			// Switch sorting order and sort on this column
			_sortDirection = _sortDirection == SortDirection.Ascending ? SortDirection.Descending : SortDirection.Ascending;
			RefreshListView();
		}

		private void sortDirectionButton_Click(object sender, RoutedEventArgs e)
		{
			_sortDirection = _sortDirection == SortDirection.Ascending ? SortDirection.Descending : SortDirection.Ascending;
			if (_sortDirection == SortDirection.Ascending)
			{
				((Button)sender).Content = "\xE96D";
			}
			else
			{
				((Button)sender).Content = "\xE96E";
			}
			RefreshListView();
		}

		private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var removedItems = e.RemovedItems;
			int removedCount = removedItems.Count;
			_selectedMessages = e.AddedItems as List<PacketMessage>;
		}

		private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			RefreshListView();
		}
	}
}
