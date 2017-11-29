﻿using FormControlBaseClass;
using PacketMessaging.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Template10.Common;
using MetroLog;
using System.Reflection;
using Windows.UI.Xaml.Markup;
using System.Text;
using PacketMessaging.Models;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Input;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;
using PacketMessaging.Controls.GridSplitter;


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

		public class ListViewParameters
		{
			public ListView PivotListView
			{ get; set; }

			public ListViewColumns ColumnDefinitions
			{ get; set; }

			public ListViewParameters(ListView listView, ListViewColumns listViewColumns)
			{
				PivotListView = listView;
				ColumnDefinitions = listViewColumns;
			}
		}
		Dictionary<string, ListViewParameters> _listViewDefinitionsDict = new Dictionary<string, ListViewParameters>();

        public static StorageFolder _unsentMessagesFolder = null;
        public static StorageFolder _sentMessagesFolder = null;
        public static StorageFolder receivedMessagesFolder = null;
        public static StorageFolder _draftMessagesFolder = null;
        public static StorageFolder _archivedMessagesFolder = null;
        public static StorageFolder _deletedMessagesFolder = null;
        public static StorageFolder LogsFolder = null;
		public static StorageFolder _MetroLogsFolder = null;

		ListView _currentListView = null;

		ListViewColumns _listViewColumDefinitionsInBox;
		ListViewColumns _listViewColumDefinitionsSent;
		ListViewColumns _listViewColumDefinitionsOutBox;
		ListViewColumns _listViewColumnDefinitionsDrafts;
		ListViewColumns _listViewColumDefinitionsArchive;
		ListViewColumns _listViewColumnDefinitionsDeleted;
		string _sortOnPropertyName;
		SortDirection _sortDirection = SortDirection.Ascending;

		public static MainPage Current;

		CoreCursor _appCursor;
		enum PointerState
		{
			None,
			SetFieldWidthLeft,
			SetFieldWidthRight
		}
		PointerState _pointerState;
		double _FieldWidthRefPoint;
		ColumnDefinitionCollection _columnDefinitions;
		int _columnIndex = 0;

		//PacketMessage _selectedMessage = null;
		List<PacketMessage> _selectedMessages = new List<PacketMessage>();

		public Dictionary<string, ListViewParameters> ListViewDefinitionsDict
		{ get => _listViewDefinitionsDict; }

		public ListView CurrentListView
		{ get => _currentListView; }

		public MainPage()
        {
            this.InitializeComponent();

            NavigationCacheMode = NavigationCacheMode.Enabled;
            _SerializationService = Template10.Services.SerializationService.SerializationService.Json;
            _settings = Services.SettingsServices.SettingsService.Instance;

            Current = this;

			_appCursor = CoreApplication.MainView.CoreWindow.PointerCursor;

			log.Info("");
			log.Info("Initializing Packet Messaging Application");
			foreach (PivotItem item in MyPivot.Items)
			{
				switch (item.Name)
				{
					case "InBox":
						item.Tag = receivedMessagesFolder;
						break;
					case "Sent":
						item.Tag = _sentMessagesFolder;
						break;
					case "OutBox":
						item.Tag = _unsentMessagesFolder;
						break;
					case "Drafts":
						item.Tag = _draftMessagesFolder;
						break;
					case "Archive":
						item.Tag = _archivedMessagesFolder;
						break;
					case "Deleted":
						item.Tag = _deletedMessagesFolder;
						break;
					case "testDataGrid":
						item.Tag = _deletedMessagesFolder;
						break;
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


			AddressBook.Instance.CreateAddressBook();

			ObservableCollection<ColumnDescriptionBase> draftsPropertiesList = new ObservableCollection<ColumnDescriptionBase>();

			_listViewColumDefinitionsInBox = new ListViewColumns();
			_listViewColumDefinitionsInBox.Add(new Area("40"));
			_listViewColumDefinitionsInBox.Add(new ReceivedTime("120"));
			_listViewColumDefinitionsInBox.Add(new JNOSDate("120"));
			_listViewColumDefinitionsInBox.Add(new Subject("1*", 70));
			_listViewColumDefinitionsInBox.Add(new MessageNumber("110"));
			_listViewColumDefinitionsInBox.Add(new MessageTo("70"));
			_listViewColumDefinitionsInBox.Add(new MessageFrom("70"));
			_listViewColumDefinitionsInBox.Add(new BBS("60"));
			_listViewColumDefinitionsInBox.Add(new MessageSize("40"));
			_listViewDefinitionsDict.Add(listViewInBox.Name, new ListViewParameters(listViewInBox, _listViewColumDefinitionsInBox));

			_listViewColumDefinitionsSent = new ListViewColumns();
			_listViewColumDefinitionsSent.Add(new ReceivedTime("120"));
			_listViewColumDefinitionsSent.Add(new SentTime("120"));
			_listViewColumDefinitionsSent.Add(new Subject("1*", 70));
			_listViewColumDefinitionsSent.Add(new MessageNumber("110"));
			_listViewColumDefinitionsSent.Add(new MessageTo("70"));
			_listViewColumDefinitionsSent.Add(new MessageFrom("70"));
			_listViewColumDefinitionsSent.Add(new BBS("60"));
			_listViewColumDefinitionsSent.Add(new MessageSize("40"));
			_listViewDefinitionsDict.Add(listViewSentItems.Name, new ListViewParameters(listViewSentItems, _listViewColumDefinitionsSent));

			_listViewColumDefinitionsOutBox = new ListViewColumns();
			_listViewColumDefinitionsOutBox.Add(new CreateTime("120"));
			_listViewColumDefinitionsOutBox.Add(new Subject("1*", 70));
			_listViewColumDefinitionsOutBox.Add(new MessageNumber("110"));
			_listViewColumDefinitionsOutBox.Add(new MessageTo("70"));
			_listViewColumDefinitionsOutBox.Add(new MessageFrom("70"));
			_listViewColumDefinitionsOutBox.Add(new BBS("60"));
			_listViewColumDefinitionsOutBox.Add(new MessageSize("40"));
			_listViewDefinitionsDict.Add(listViewOutBox.Name, new ListViewParameters(listViewOutBox, _listViewColumDefinitionsOutBox));

			_listViewColumnDefinitionsDrafts = new ListViewColumns();
			_listViewColumnDefinitionsDrafts.Add(new CreateTime("120"));
			_listViewColumnDefinitionsDrafts.Add(new Subject("1*", 70));
			_listViewColumnDefinitionsDrafts.Add(new MessageNumber("110"));
			_listViewColumnDefinitionsDrafts.Add(new MessageTo("70"));
			_listViewColumnDefinitionsDrafts.Add(new MessageFrom("70"));
			_listViewColumnDefinitionsDrafts.Add(new BBS("60"));
			_listViewDefinitionsDict.Add(listViewDrafts.Name, new ListViewParameters(listViewDrafts, _listViewColumnDefinitionsDrafts));

			_listViewColumDefinitionsArchive = new ListViewColumns();
			_listViewColumDefinitionsArchive.Add(new Area("40"));
			_listViewColumDefinitionsArchive.Add(new ReceivedTime("120"));
			_listViewColumDefinitionsArchive.Add(new CreateTime("120"));
			_listViewColumDefinitionsArchive.Add(new Subject("1*", 70));
			_listViewColumDefinitionsArchive.Add(new MessageNumber("110"));
			_listViewColumDefinitionsArchive.Add(new MessageTo("70"));
			_listViewColumDefinitionsArchive.Add(new MessageFrom("70"));
			_listViewColumDefinitionsArchive.Add(new BBS("60"));
			_listViewColumDefinitionsArchive.Add(new MessageSize("40"));
			_listViewDefinitionsDict.Add(listViewArchivedItems.Name, new ListViewParameters(listViewArchivedItems, _listViewColumDefinitionsArchive));

			_listViewColumnDefinitionsDeleted = new ListViewColumns();
			_listViewColumnDefinitionsDeleted.Add(new CreateTime("120"));
			_listViewColumnDefinitionsDeleted.Add(new Subject("1*", 70));
			_listViewColumnDefinitionsDeleted.Add(new MessageNumber("110"));
			_listViewColumnDefinitionsDeleted.Add(new MessageTo("70"));
			_listViewColumnDefinitionsDeleted.Add(new MessageFrom("70"));
			_listViewColumnDefinitionsDeleted.Add(new BBS("60"));
			_listViewDefinitionsDict.Add(listViewDeletedItems.Name, new ListViewParameters(listViewDeletedItems, _listViewColumnDefinitionsDeleted));

			foreach (ColumnDescriptionBase field in _listViewColumnDefinitionsDrafts.MessageViewList)
			{
				draftsPropertiesList.Add(field);
			}
			listViewProperties.Source = draftsPropertiesList;

			if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
			{
				ObservableCollection<ColumnDescriptionBase> deletedPropertiesList = new ObservableCollection<ColumnDescriptionBase>();

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

		public async Task SetlistViewColumnDefinitionWidthAsync(int columnIndex, GridLength width)
		{
			_listViewDefinitionsDict[_currentListView.Name].ColumnDefinitions.MessageViewList[columnIndex].Width = width;
			_listViewDefinitionsDict[_currentListView.Name].ColumnDefinitions.MessageViewList[columnIndex].WidthAsString = width.ToString();

			await CreateGridItemTemplateAsync(_listViewDefinitionsDict[_currentListView.Name]);
		}

		void SetPointerState(PointerState pointerState)
		{
			if (pointerState != PointerState.None && _pointerState == PointerState.None)
			{
				CoreApplication.MainView.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeWestEast, 0);
			}
			else if (pointerState == PointerState.None)
			{
				CoreApplication.MainView.CoreWindow.PointerCursor = _appCursor;
			}
			_pointerState = pointerState;
		}
		/*
				StringReader reader = new StringReader(
					@"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
						<Ellipse Width=""300.5"" Height=""200"" Fill=""Red""/>
					</DataTemplate>");
				var template = XamlReader.Load(await reader.ReadToEndAsync());
				ListView lv = new ListView();
				lv.ItemTemplate = template as DataTemplate;
				ObservableCollection<int> coll = new ObservableCollection<int>();
				for (int i = 0; i< 20; i++)
				{
					coll.Add(i);
				}
				lv.ItemsSource = coll;
				rootGrid.Children.Add(lv);
		*/
		public async Task CreateGridItemTemplateAsync(ListViewParameters listViewParameters)
		{
			ListViewColumns packetMessageViewList = listViewParameters.ColumnDefinitions;

			if (packetMessageViewList == null)
				return;

			ListView listView = listViewParameters.PivotListView;

			StringBuilder header = new StringBuilder();
			header.Append("<DataTemplate");
			header.Append(" xmlns =\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
			header.Append(" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"");
			header.Append(" xmlns:converters=\"using:PacketMessaging.Converters\"");
			header.Append(" xmlns:data=\"using:FormControlBaseClass\"");
			//header.Append(" x:DataType=\"data:PacketMessage\"");
			//header.Append(" Name=\"DeletedListViewTemplate\"");
			header.Append(">");
			//header.Append(" <Resources>");
			//header.Append(" <converters:DateTimeConverter x:Key=\"DatetimeConverter\"/>");
			//header.Append(" </Resources>");
			//header.Append(" <Binding Converter="converters:DateTimeConverter"/>"); 			
			header.Append("<Grid><Grid.ColumnDefinitions>");
			foreach (var field in packetMessageViewList.MessageViewList)
			{
				header.Append($"<ColumnDefinition Width=\"{field.WidthAsString}\" MinWidth =\"{field.MinWidth}\"/>");
			}
			header.Append("</Grid.ColumnDefinitions>");
			for (int i = 0; i < packetMessageViewList.MessageViewList.Count; i++)
			{
				ColumnDescriptionBase field = packetMessageViewList.MessageViewList[i];
				//if (field.PropertyName == "ReceivedTime"
				//		|| field.PropertyName == "JNOSDate")
				//{
				//	header.Append($"<TextBlock Grid.Column=\"{i}\" Text=\"" + "{Binding " + $"{field.PropertyName}" + ", Converter=converters:DatetimeConverter}\" HorizontalAlignment=\"Center\"/>");
				//	//header.Append("<TextBlock Grid.Column=\"" + $"{i}" + "\" Text=\"{Binding " + $"{field.PropertyName}" + "}\" HorizontalAlignment=\"Center\"/>");
				//}
				//else
				if (field.PropertyName == "Subject")
				{
					header.Append($"<TextBlock Grid.Column=\"{i}\" Text=\"" + "{Binding " + $"{field.PropertyName}" + "}\" Width=\"1024\" Padding=\"5,0,5,0\"/>");
				}
				else if (field.PropertyName == "MessageTo" || field.PropertyName == "MessageFrom")
				{ 
					header.Append($"<TextBlock Grid.Column=\"{i}\" Text=\"" + "{Binding " + $"{field.PropertyName}" + "}\" Padding=\"5,0,5,0\"/>");
				}
				else
				{
					header.Append($"<TextBlock Grid.Column=\"{i}\" Text=\"" + "{Binding " + $"{field.PropertyName}" + "}\"  HorizontalAlignment=\"Center\" Padding=\"5,0,5,0\" />");
				}
			}
			header.Append("</Grid>");
			header.Append("</DataTemplate>");
			string headerString = header.ToString();
			
			StringReader reader = new StringReader(headerString);
			DataTemplate template = XamlReader.Load(await reader.ReadToEndAsync()) as DataTemplate;
			listView.ItemTemplate = template;
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

		private void CreateListViewHeader(ListView listView, ListViewColumns packetMessageViewList)
		{
			if (!packetMessageViewList.ListViewHeaderCreated)
			{
				packetMessageViewList.ListViewHeaderCreated = true;

				Grid header = (Grid)listView.Header;

				for (int i = 0; i < packetMessageViewList.MessageViewList.Count; i++)
				{
					ColumnDefinition columnDefinition = new ColumnDefinition()
					{
						Width = packetMessageViewList.MessageViewList[i].Width,
						MinWidth = packetMessageViewList.MessageViewList[i].MinWidth
					};

					header.ColumnDefinitions.Add(columnDefinition);
					Border columnBorder = new Border();
					Grid.SetColumn(columnBorder, i);
					header.Children.Add(columnBorder);   // Not sure what this does, Add to visual tree?
					columnBorder.BorderBrush = FormControlBasics._blackBrush;
					if (i == 0)
						columnBorder.BorderThickness = new Thickness(1, 1, 1, 1);
					else
						columnBorder.BorderThickness = new Thickness(0, 1, 1, 1);

					TextBlock columnTextBlock = new TextBlock();
					Grid.SetColumn(columnTextBlock, i);
					//header.Children.Add(columnTextBlock);   // Not sure what this does, Add to visual tree?
					columnBorder.Child = columnTextBlock;   // Not sure what this does, Add to visual tree?
					columnTextBlock.Text = packetMessageViewList.MessageViewList[i].HeaderShort;
					columnTextBlock.FontSize = 17;
					if (columnTextBlock.Text == "Subject")
					{
						columnTextBlock.Padding = new Thickness(5, 5, 0, 0);
					}
					else
					{
						columnTextBlock.Width = Convert.ToDouble(packetMessageViewList.MessageViewList[i].WidthAsString);
						columnTextBlock.TextAlignment = TextAlignment.Center;
						columnTextBlock.Padding = new Thickness(0, 5, 0, 0);
					}
					columnTextBlock.Tag = packetMessageViewList.MessageViewList[i].PropertyName;
					columnTextBlock.AddHandler(DoubleTappedEvent, new DoubleTappedEventHandler(TextBlock_DoubleTapped), true);
					//columnTextBlock.AddHandler(PointerMovedEvent, new PointerEventHandler(TextBlock_PointerMoved), true);
					//columnTextBlock.AddHandler(PointerPressedEvent, new PointerEventHandler(TextBlock_PointerPressed), true);
					//columnTextBlock.AddHandler(PointerReleasedEvent, new PointerEventHandler(TextBlock_PointerReleased), true);
					//columnTextBlock.AddHandler(PointerEnteredEvent, new PointerEventHandler(TextBlock_PointerEntered), true);
				}
			}
		}

		private void CreateListViewHeader(ListViewParameters listViewParameters)
		{
			ListViewColumns packetMessageViewList = listViewParameters.ColumnDefinitions;
			if (!packetMessageViewList.ListViewHeaderCreated)
			{
				packetMessageViewList.ListViewHeaderCreated = true;
				ListView listView = listViewParameters.PivotListView;

				Grid header = (Grid)listView.Header;

				for (int i = 0; i < packetMessageViewList.MessageViewList.Count; i++)
				{
					ColumnDefinition columnDefinition = new ColumnDefinition()
					{
						Width = packetMessageViewList.MessageViewList[i].Width,
						MinWidth = packetMessageViewList.MessageViewList[i].MinWidth
					};
					header.ColumnDefinitions.Add(columnDefinition);
					Border columnBorder = new Border();
					Grid.SetColumn(columnBorder, i);
					header.Children.Add(columnBorder);   // Not sure what this does, Add to visual tree?
					columnBorder.BorderBrush = FormControlBasics._blackBrush;
					if (i == 0)
						columnBorder.BorderThickness = new Thickness(1, 1, 1, 1);
					else
						columnBorder.BorderThickness = new Thickness(0, 1, 1, 1);

					TextBlock columnTextBlock = new TextBlock();
					Grid.SetColumn(columnTextBlock, i);
					//header.Children.Add(columnTextBlock);   // Not sure what this does, Add to visual tree?
					columnBorder.Child = columnTextBlock;   // Not sure what this does, Add to visual tree?
					columnTextBlock.Text = packetMessageViewList.MessageViewList[i].HeaderShort;
					columnTextBlock.FontSize = 17;
					if (columnTextBlock.Text == "Subject")
					{
						columnTextBlock.Padding = new Thickness(5, 5, 0, 0);
					}
					else
					{
						columnTextBlock.Width = Convert.ToDouble(packetMessageViewList.MessageViewList[i].WidthAsString);
						columnTextBlock.TextAlignment = TextAlignment.Center;
						columnTextBlock.Padding = new Thickness(0, 5, 0, 0);
					}
					if (i > 0)
					{
						GridSplitter gridSplitter = new GridSplitter()
						{
							Margin = new Thickness(-5, 0, 0, 0),
							Width = 10,
							Opacity = 0,
							HorizontalAlignment = HorizontalAlignment.Left,
							ResizeBehavior = GridSplitter.GridResizeBehavior.BasedOnAlignment
						};
						Grid.SetColumn(gridSplitter, i);
						header.Children.Add(gridSplitter);
					}
					columnTextBlock.Tag = packetMessageViewList.MessageViewList[i].PropertyName;
					columnTextBlock.AddHandler(DoubleTappedEvent, new DoubleTappedEventHandler(TextBlock_DoubleTapped), true);
				}
			}
		}

		private async void MyPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			_selectedMessages?.Clear();

			PivotItem pivotItem = (PivotItem)e.AddedItems[0];

			if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop")
			{
				switch (pivotItem.Name)
				{
					case "InBox":
						_currentListView = listViewInBox;
						//CreateListViewHeader(_listViewDefinitionsDict[_currentListView.Name]);
						//await CreateGridItemTemplateAsync(_currentListView, _listViewColumDefinitionsInBox);
						break;

					case "Sent":
						_currentListView = listViewSentItems;
						CreateListViewHeader(_listViewDefinitionsDict[_currentListView.Name]);
						//await CreateGridItemTemplateAsync(_listViewDefinitionsDict[_currentListView.Name]);
						break;

					case "OutBox":
						_currentListView = listViewOutBox;
						CreateListViewHeader(_listViewDefinitionsDict[_currentListView.Name]);
						await CreateGridItemTemplateAsync(_listViewDefinitionsDict[_currentListView.Name]);
						break;

					case "Drafts":
						_currentListView = listViewDrafts;
						CreateListViewHeader(_listViewDefinitionsDict[_currentListView.Name]);
						await CreateGridItemTemplateAsync(_listViewDefinitionsDict[_currentListView.Name]);

						break;

					case "Archive":
						_currentListView = listViewArchivedItems;
						CreateListViewHeader(_listViewDefinitionsDict[_currentListView.Name]);
						//await CreateGridItemTemplateAsync(_listViewDefinitionsDict[_currentListView.Name]);
						break;

					case "Deleted":
						_currentListView = listViewDeletedItems;
						CreateListViewHeader(_listViewDefinitionsDict[_currentListView.Name]);
						await CreateGridItemTemplateAsync(_listViewDefinitionsDict[_currentListView.Name]);
						break;
				}
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

			List<Object> selectedMessages = null;
			if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
			{
				if (pivotItem.Name == "Drafts")
				{
					//selectedMessages = _currentGridView.SelectedItems;
					selectedMessages = new List<Object>();

					foreach (var message in _selectedMessages)
					{
						selectedMessages.Add(message);
					}
				}
				else
					selectedMessages = (List<Object>)_currentListView.SelectedItems;
			}
			else
			{
				selectedMessages = (List<Object>)_currentListView.SelectedItems;
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
                catch (Exception ex)
                {
					var file = await folder.CreateFileAsync(packetMessage.FileName, CreationCollisionOption.OpenIfExists);
					await file?.DeleteAsync();

					string s = ex.ToString();
                    continue;
                }
            }
			RefreshListViewAsync(pivotItem);
        }

		// DeviceFamily Mobile
		private void ComboBoxSelectListProperty_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			string selection = e.AddedItems[0] as string;
			_sortOnPropertyName = ((ColumnDescriptionBase)e.AddedItems[0]).PropertyName as string;
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
			int addedCount = e.AddedItems.Count;
			foreach (PacketMessage  message in e.AddedItems)
			{
				_selectedMessages.Add(message);
			}
			//_selectedMessages = e.AddedItems as List<PacketMessage>;
		}

		private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			RefreshListView();
		}

		//private void TextBlock_PointerEntered(object sender, PointerRoutedEventArgs e)
		//{
		//	PointerPoint point = e.GetCurrentPoint(sender as UIElement);
		//	Type type = sender.GetType();
		//	TextBlock textBlock = sender as TextBlock;
		//	var gridColumnBorder = textBlock.Parent as Border;
		//	var width = gridColumnBorder.ActualWidth;
		//	//Size size = textBlock.RenderSize;
		//	CoreApplicationView view = CoreApplication.MainView;
		//	if (point.Position.X > width - 5)
		//	{
		//		// Right side
		//		CoreApplication.MainView.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeWestEast, 0);
		//	}
		//	else if (point.Position.X < 5)
		//	{
		//		// left side

		//	}
		//	else
		//	{
		//		CoreApplication.MainView.CoreWindow.PointerCursor = _appCursor;
		//	}
		//}

		////private void TextBlock_PointerMoved(object sender, PointerRoutedEventArgs e)
		////{
		////	TextBlock textBlock = sender as TextBlock;
		////	PointerPoint pointerPoint = e.GetCurrentPoint(textBlock);
		////	var gridColumnBorder = textBlock.Parent as Border;
		////	var width = gridColumnBorder.ActualWidth;
		////	//Size size = textBlock.RenderSize;
		////	Point rawPos = pointerPoint.RawPosition;

		////	if (pointerPoint.Position.X > width - 5)
		////	{
		////		// Change mouse pointer
		////		SetPointerState(PointerState.SetFieldWidthLeft);
		////	}
		////	else if (pointerPoint.Position.X < 5)
		////	{
		////		SetPointerState(PointerState.SetFieldWidthRight);
		////	}
		////	else if (!pointerPoint.Properties.IsLeftButtonPressed)
		////	{
		////		SetPointerState(PointerState.None);
		////	}

		////}

		////private void ListViewHeader_PointerExited(object sender, PointerRoutedEventArgs e)
		////{
		////	SetPointerState(PointerState.None);
		////}

		////private void TextBlock_PointerPressed(object sender, PointerRoutedEventArgs e)
		////{
		////	if (_pointerState != PointerState.None)
		////	{
		////		TextBlock textBlock = sender as TextBlock;
		////		var gridColumnBorder = textBlock.Parent as Border;
		////		var gridHeader = gridColumnBorder.Parent as Grid;
		////		_columnDefinitions = gridHeader.ColumnDefinitions;
		////		//Grid grid = gridHeader.Parent as Grid;
		////		PointerPoint pointerPoint = e.GetCurrentPoint(textBlock);
		////		if (pointerPoint.Properties.IsLeftButtonPressed)
		////		{
		////			_FieldWidthRefPoint = pointerPoint.Position.X;
		////			// Must know column and left or right side of column
		////			int i = 0;
		////			for (; i < _listViewDefinitionsDict[_currentListView.Name].ColumnDefinitions.MessageViewList.Count; i++)
		////			{
		////				if (_listViewDefinitionsDict[_currentListView.Name].ColumnDefinitions.MessageViewList[i].HeaderShort == textBlock.Text)
		////				{
		////					break;
		////				}
		////			}
		////			_columnIndex = i;
		////		}
		////	}
		////}

		////private void TextBlock_PointerReleased(object sender, PointerRoutedEventArgs e)
		////{
		////	if (_pointerState != PointerState.None)
		////	{
		////		TextBlock textBlock = sender as TextBlock;
		////		PointerPoint pointerPoint = e.GetCurrentPoint(textBlock);
		////		double fieldWidthDifference = pointerPoint.Position.X - _FieldWidthRefPoint;
		////		textBlock.Width += fieldWidthDifference;

		////		var gridColumnBorder = textBlock.Parent as Border;
		////		var width = 150;
		////		gridColumnBorder.Width = 150;

		////		textBlock.Width = width;
		////		_columnDefinitions[_columnIndex].Width = new GridLength(width);
		////		ListViewParameters listViewParms = _listViewDefinitionsDict[_currentListView.Name];
		////		ListViewColumns columnDefinitions = listViewParms.ColumnDefinitions;
		////		ColumnDescriptionBase columnDescription = columnDefinitions.MessageViewList[_columnIndex];
		////		columnDescription.Width = new GridLength(width);
		////		listViewParms = new ListViewParameters(_currentListView, columnDefinitions);
		////		_listViewDefinitionsDict[_currentListView.Name] = listViewParms;

		////		SetPointerState(PointerState.None);
		////	}
		////}
	}
}
