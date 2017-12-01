using FormControlBaseClass;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace PacketMessaging.ViewModels
{
	public class ListViewParametersArray
	{
		const string fileName = "ListViewDefinitions.xml";

		private ListViewParameters[] _listViewParametersField;

		[System.Xml.Serialization.XmlElementAttribute("ListViewParameters")]
		public ListViewParameters[] ArrayOfListViewParameters
		{ 
			get => _listViewParametersField; 
			set => _listViewParametersField = value;
		}

		public static async Task SaveAsync(Dictionary<string, ListViewParameters> listViewDefinitionsDict)
		{
			ListViewParametersArray listViewParametersArray = new ListViewParametersArray();
			ListViewParameters[] listViewParmsArray = new ListViewParameters[listViewDefinitionsDict.Count];
			for (int i = 0; i < listViewParmsArray.Length; i++)
			{
				listViewParmsArray[i] = listViewDefinitionsDict.ElementAt(i).Value;
				ColumnDescriptionBase[] columnDescriptions = new ColumnDescriptionBase[listViewDefinitionsDict.ElementAt(i).Value.ColumnDefinitions.ListViewColumnsArray.Length];
				columnDescriptions = listViewDefinitionsDict.ElementAt(i).Value.ColumnDefinitions.ListViewColumnsArray;
			}
			listViewParametersArray.ArrayOfListViewParameters = listViewParmsArray;

			StorageFolder localFolder = ApplicationData.Current.LocalFolder;
			try
			{
				StorageFile file = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
				using (StreamWriter writer = new StreamWriter(new FileStream(file.Path, FileMode.OpenOrCreate)))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(ListViewParametersArray));
					serializer.Serialize(writer, listViewParametersArray);
				}
			}
			catch (Exception e)
			{
				//log.Error($"Error saving {fileName} {e}");
			}

		}
	}

	public class ListViewParameters
	{

		public string PivotListViewName
		{ get; set; }

		public ListViewColumns ColumnDefinitions
		{ get; set; }

		public ListViewParameters() { }

		public ListViewParameters(string listViewName, ListViewColumns listViewColumns)
		{
			PivotListViewName = listViewName;
			ColumnDefinitions = listViewColumns;
		}

	}

	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
	public class ListViewColumns
	{
		ColumnDescriptionBase[] _listViewColumns;

		[System.Xml.Serialization.XmlArrayItemAttribute("FormField", IsNullable = false)]
		public ColumnDescriptionBase[] ListViewColumnsArray => _listViewColumns;

		public bool ListViewHeaderCreated
		{ get; set; }

		public Views.MainPage.SortDirection SortOrder
		{ get; set; }

		public string SortPropertyName
		{ get; set; }

		public ListViewColumns()
		{

		}

		public ListViewColumns(int columnCount)
		{
			_listViewColumns = new ColumnDescriptionBase[columnCount];
		}

		/*
StringReader reader = new StringReader(
   @" < DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
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
	}

	public class ColumnDescriptionBase
	{
		public string PropertyName { get; set; }
		public string Header { get; set; }
		public string HeaderShort { get; set; }

		public GridLength Width
		{ get ; set; }

		public double MinWidth
		{ get; set; }

		public string WidthAsString { get; set; }


		public ColumnDescriptionBase(string propertyName, string header, string headerShort, string width, double minWidth = 0)
		{
			PropertyName = propertyName;
			Header = header;
			HeaderShort = headerShort;
			if (width.EndsWith("*"))
			{
				Width = new GridLength(Convert.ToDouble(width.TrimEnd(new char[] { '*' })), GridUnitType.Star);
			}
			else
				Width = new GridLength(Convert.ToDouble(width));
			MinWidth = minWidth;
			WidthAsString = width;
		}
	}
	public class Area : ColumnDescriptionBase
	{
		public Area(string width = "40") : base(nameof(Area), header: "Area", headerShort: "Area", width: width) { }

		public Area(string headerShort, string fieldWidth = "40") : base(nameof(Area), "Area", headerShort, fieldWidth) { }
	}

	public class BBS : ColumnDescriptionBase
	{
		public BBS(string fieldWidth = "60") : base(nameof(BBS), "BBS", "BBS", fieldWidth) { }
	}

	public class JNOSDate : ColumnDescriptionBase
	{
		public JNOSDate(string fieldWidth = "120") : base(nameof(JNOSDate), "Sent Time", "Sent Time", fieldWidth) { }
	}

	public class SentTime : ColumnDescriptionBase
	{
		public SentTime(string fieldWidth = "120") : base(nameof(SentTime), "Sent", "Sent Time", fieldWidth) { }
	}

	public class CreateTime : ColumnDescriptionBase
	{
		public CreateTime(string fieldWidth = "120") : base(nameof(CreateTime), "Create Time", "Create Time", fieldWidth) { }
		public CreateTime(string headerShort, string fieldWidth = "120") : base(nameof(CreateTime), "Create Time", headerShort, fieldWidth) { }
	}

	public class ReceivedTime : ColumnDescriptionBase
	{
		public ReceivedTime(string fieldWidth = "120") : base(nameof(ReceivedTime), "Received Time", "Received Time", fieldWidth) { }
		public ReceivedTime(string headerShort, string fieldWidth = "120") : base(nameof(ReceivedTime), "Received Time", headerShort, fieldWidth) { }
	}

	public class MessageNumber : ColumnDescriptionBase
	{
		public MessageNumber(string fieldWidth = "110") : base(nameof(MessageNumber), "Msg Number", "Msg. No.", fieldWidth) { }
		public MessageNumber(string headerShort, string fieldWidth = "110") : base(nameof(MessageNumber), "Msg Number", headerShort, fieldWidth) { }
	}

	public class MessageFrom : ColumnDescriptionBase
	{
		public MessageFrom(string fieldWidth = "70") : base(nameof(MessageFrom), "From", "From", fieldWidth) { }
	}

	public class MessageTo : ColumnDescriptionBase
	{
		public MessageTo(string fieldWidth = "70") : base(nameof(MessageTo), "To", "To", fieldWidth) { }
	}

	public class MessageSize : ColumnDescriptionBase
	{
		public MessageSize(string fieldWidth = "40") : base(nameof(MessageSize), "Size", "Size", fieldWidth) { }
	}

	public class Subject : ColumnDescriptionBase
	{
		public Subject(string width = "1*", double minWidth = 0) : base(nameof(Subject), "Subject", "Subject", width, minWidth) { }
		public Subject(string headerShort, string width, double minWidth = 0) : base(nameof(Subject), "Subject", headerShort, width, minWidth) { }
	}

}
