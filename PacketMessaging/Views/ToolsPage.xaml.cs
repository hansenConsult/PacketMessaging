using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PacketMessaging.Views
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class ToolsPage : Page
	{
		StorageFile _selectedFile;
		private int _selectedFileIndex;

		public ToolsPage()
		{
			this.InitializeComponent();
		}

		private async Task UpdateFileListAsync()
		{
			List<string> fileTypeFilter = new List<string>() { ".log" };
			QueryOptions queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, fileTypeFilter);

			// Get the files in the user's archive folder
			StorageFileQueryResult results = MainPage._MetroLogsFolder.CreateFileQueryWithOptions(queryOptions);
			// Iterate over the results
			IReadOnlyList<StorageFile> files = await results.GetFilesAsync();

			var observableCollection = new ObservableCollection<StorageFile>(files);
			LogFilesCollection.Source = observableCollection.OrderByDescending(f => f.Name);

			logFilesComboBox.SelectedIndex = _selectedFileIndex;
		}

		private async void toolsPagePivot_SelectionChangedAsync(object sender, SelectionChangedEventArgs e)
		{
			PivotItem pivotItem = (PivotItem)e.AddedItems[0];

			if (pivotItem.Name == "logFile")
			{
				await UpdateFileListAsync();
			}
		}

		private async void logFilesComboBox_SelectionChangedAsync(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				_selectedFile = (StorageFile)e.AddedItems[0];
				logFileTextBox.Text = await FileIO.ReadTextAsync(_selectedFile);
			}
			catch (UnauthorizedAccessException )
			{
				StorageFile fileCopy;
				try
				{
					// Delete any file copy that for some reason was not deleted
					fileCopy = await StorageFile.GetFileFromPathAsync(_selectedFile.Path + "-Copy");
					await fileCopy.DeleteAsync();
				}
				catch
				{ }
				// Create a copy of an open log file because it can not be read directly
				await _selectedFile.CopyAsync(MainPage._MetroLogsFolder, _selectedFile.Name + "-Copy");
				fileCopy = await StorageFile.GetFileFromPathAsync(_selectedFile.Path + "-Copy");
				logFileTextBox.Text = await FileIO.ReadTextAsync(fileCopy);
				await fileCopy.DeleteAsync();
			}
			catch (COMException )
			{

			}
		}

		private void AppBarButton_SaveFile(object sender, RoutedEventArgs e)
		{

		}

		private async void AppBarButton_DeleteFileAsync(object sender, RoutedEventArgs e)
		{
			var selection = _selectedFile;
			logFilesComboBox.SelectedIndex = Math.Max(0, logFilesComboBox.SelectedIndex - 1);
			_selectedFileIndex = logFilesComboBox.SelectedIndex;

			await selection.DeleteAsync();

			await UpdateFileListAsync();
		}
	}
}
