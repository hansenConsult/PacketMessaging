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
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
	//[System.SerializableAttribute()]
	//[System.Diagnostics.DebuggerStepThroughAttribute()]
	//[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
	public partial class ListViewParametersArray
	{
		private static volatile ListViewParametersArray _instance;
		private static object _syncRoot = new Object();

		private Dictionary<string, ListViewParameters> _listViewDefinitionsDict = new Dictionary<string, ListViewParameters>();

		private ListViewParametersArray() {	}

		const string fileName = "ListViewDefinitions.xml";

		private ListViewParameters[] listViewParametersField;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("ListViewParameters")]
		public ListViewParameters[] ArrayOfListViewParameters
		{
			get => this.listViewParametersField;
			set => this.listViewParametersField = value;
		}

		[XmlIgnore]
		public Dictionary<string, ListViewParameters> ListViewDefinitionsDict
		{
			get => _listViewDefinitionsDict;
		}

		public static ListViewParametersArray Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (_syncRoot)
					{
						if (_instance == null)
							_instance = new ListViewParametersArray();
					}
				}
				return _instance;
			}
		}

		public async Task SaveAsync()
		{
			StorageFolder localFolder = ApplicationData.Current.LocalFolder;
			try
			{
				StorageFile file = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
				using (StreamWriter writer = new StreamWriter(new FileStream(file.Path, FileMode.OpenOrCreate)))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(ListViewParametersArray));
					serializer.Serialize(writer, this);
				}
			}
			catch (Exception e)
			{
				//log.Error($"Error saving {fileName} {e}");
			}
		}

		public async Task OpenAsync()
		{
			StorageFolder localFolder = ApplicationData.Current.LocalFolder;
			try
			{
				StorageFile file = await localFolder.GetFileAsync(fileName);
				using (FileStream reader = new FileStream(file.Path, FileMode.Open))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(ListViewParametersArray));
					_instance = (ListViewParametersArray)serializer.Deserialize(reader);
				}
			}
			catch (Exception e)
			{
				;
			}
		}

		public Dictionary<string, ListViewParameters> BuildDictionary()
		{
			foreach (ListViewParameters parms in ArrayOfListViewParameters)
			{
				_listViewDefinitionsDict.Add(parms.PivotListViewName, parms);
			}
			return _listViewDefinitionsDict;
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
	//[System.SerializableAttribute()]
	//[System.Diagnostics.DebuggerStepThroughAttribute()]
	//[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public class ListViewParameters
	{
		private ListViewColumns listViewColumnsField;

		private string listViewField;

		/// <remarks/>
		public ListViewColumns ColumnDefinitions
		{
			get => this.listViewColumnsField;
			set => this.listViewColumnsField = value;
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string PivotListViewName
		{
			get => this.listViewField;
			set => listViewField = value;
		}

		public ListViewParameters() { }

		public ListViewParameters(string listViewName, ListViewColumns listViewColumns)
		{
			PivotListViewName = listViewName;
			ColumnDefinitions = listViewColumns;
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
	//[System.SerializableAttribute()]
	//[System.Diagnostics.DebuggerStepThroughAttribute()]
	//[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public class ListViewColumns
	{
		private ColumnDescription[] columnDescriptionBaseField;

		private bool listViewHeaderCreatedField;

		private Views.MainPage.SortDirection sortOrderField;

		private string sortPropertyNameField;

		[System.Xml.Serialization.XmlElementAttribute("ColumnDescriptionBase")]
		public ColumnDescription[] ListViewColumnsArray
		{
			get => columnDescriptionBaseField;
			set => this.columnDescriptionBaseField = value;
		}

		//[System.Xml.Serialization.XmlAttributeAttribute()]
		[XmlIgnore]
		public bool ListViewHeaderCreated
		{
			get => this.listViewHeaderCreatedField;
			set => this.listViewHeaderCreatedField = value;
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public Views.MainPage.SortDirection SortOrder
		{
			get => this.sortOrderField;
			set => this.sortOrderField = value;
		}

		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string SortPropertyName
		{ 
			get => this.sortPropertyNameField;
			set => this.sortPropertyNameField = value;
		}

		public ListViewColumns() { }

		public ListViewColumns(int columnCount)
		{
			ListViewColumnsArray = new ColumnDescription[columnCount];
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

	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
	//[System.SerializableAttribute()]
	//[System.Diagnostics.DebuggerStepThroughAttribute()]
	//[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public partial class ColumnDescription
	{
		private string propertyNameField;

		private string headerField;

		private string headershortField;

		private GridLength widthField;

		private double minWidthField;

		private string widthAsStringField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string PropertyName
		{ 
			get => this.propertyNameField;
			set => this.propertyNameField = value;
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Header
		{
			get => this.headerField;
			set => this.headerField = value;
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string HeaderShort
		{
			get => this.headershortField;
			set => this.headershortField = value;
		}

		/// <remarks/>
		//[System.Xml.Serialization.XmlAttributeAttribute()]
		[XmlIgnore]
		public GridLength Width
		{
			get => this.widthField;
			set => this.widthField = value;
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public double MinWidth
		{
			get => this.minWidthField;
			set => this.minWidthField = value;
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string WidthAsString
		{
			get => this.widthAsStringField;
			set => this.widthAsStringField = value;
		}

		public ColumnDescription() { }

		public ColumnDescription(string propertyName, string header, string headerShort, string width, double minWidth = 0)
		{
			PropertyName = propertyName;
			Header = header;
			HeaderShort = headerShort;
			//if (width.EndsWith("*"))
			//{
			//	Width = new GridLength(Convert.ToDouble(width.TrimEnd(new char[] { '*' })), GridUnitType.Star);
			//}
			//else
			//	Width = new GridLength(Convert.ToDouble(width));
			MinWidth = minWidth;
			WidthAsString = width;
		}

		public static ColumnDescription CreateColumnDescrioption(string propertyName)
		{
			ColumnDescription columnDescription = null;
			switch (propertyName)
			{
				case "Area":
					columnDescription = new ColumnDescription(propertyName, "Area", "Area", "40");
					break;
				case "BBS":
					columnDescription = new ColumnDescription(propertyName, "BBS", "BBS", "60");
					break;
				case "JNOSDate":
					columnDescription = new ColumnDescription(propertyName, "Sent Time", "Sent Time", "120");
					break;
				case "SentTime":
					columnDescription = new ColumnDescription(propertyName, "Sent", "Sent Time", "120");
					break;
				case "CreateTime":
					columnDescription = new ColumnDescription(propertyName, "Create Time", "Create Time", "120");
					break;
				case "ReceivedTime":
					columnDescription = new ColumnDescription(propertyName, "Received Time", "Received Time", "120");
					break;
				case "MessageNumber":
					columnDescription = new ColumnDescription(propertyName, "Msg Number", "Msg. No.", "110");
					break;
				case "MessageFrom":
					columnDescription = new ColumnDescription(propertyName, "From", "From", "70");
					break;
				case "MessageTo":
					columnDescription = new ColumnDescription(propertyName, "To", "To", "70");
					break;
				case "MessageSize":
					columnDescription = new ColumnDescription(propertyName, "Size", "Size", "40");
					break;
				case "Subject":
					columnDescription = new ColumnDescription(propertyName, "Subject", "Subject", "1*", 70);
					break;
			}
			return columnDescription;
		}
	}
	//public class Area : ColumnDescription
	//{
	//	public Area() { }
	//	public Area(string width = "40") : base(nameof(Area), header: "Area", headerShort: "Area", width: width) { }

	//	public Area(string headerShort, string fieldWidth = "40") : base(nameof(Area), "Area", headerShort, fieldWidth) { }
	//}

	//public class BBS : ColumnDescription
	//{
	//	public BBS() { }
	//	public BBS(string fieldWidth = "60") : base(nameof(BBS), "BBS", "BBS", fieldWidth) { }
	//}

	//public class JNOSDate : ColumnDescription
	//{
	//	public JNOSDate() { }
	//	public JNOSDate(string fieldWidth = "120") : base(nameof(JNOSDate), "Sent Time", "Sent Time", fieldWidth) { }
	//}

	//public class SentTime : ColumnDescription
	//{
	//	public SentTime(string fieldWidth = "120") : base(nameof(SentTime), "Sent", "Sent Time", fieldWidth) { }
	//}

	//public class CreateTime : ColumnDescription
	//{
	//	public CreateTime() { }
	//	public CreateTime(string fieldWidth = "120") : base(nameof(CreateTime), "Create Time", "Create Time", fieldWidth) { }
	//	public CreateTime(string headerShort, string fieldWidth = "120") : base(nameof(CreateTime), "Create Time", headerShort, fieldWidth) { }
	//}

	//public class ReceivedTime : ColumnDescription
	//{
	//	public ReceivedTime() { }
	//	public ReceivedTime(string fieldWidth = "120") : base(nameof(ReceivedTime), "Received Time", "Received Time", fieldWidth) { }
	//	public ReceivedTime(string headerShort, string fieldWidth = "120") : base(nameof(ReceivedTime), "Received Time", headerShort, fieldWidth) { }
	//}

	//public class MessageNumber : ColumnDescription
	//{
	//	public MessageNumber() { }
	//	public MessageNumber(string fieldWidth = "110") : base(nameof(MessageNumber), "Msg Number", "Msg. No.", fieldWidth) { }
	//	public MessageNumber(string headerShort, string fieldWidth = "110") : base(nameof(MessageNumber), "Msg Number", headerShort, fieldWidth) { }
	//}

	//public class MessageFrom : ColumnDescription
	//{
	//	public MessageFrom() { }
	//	public MessageFrom(string fieldWidth = "70") : base(nameof(MessageFrom), "From", "From", fieldWidth) { }
	//}

	//public class MessageTo : ColumnDescription
	//{
	//	public MessageTo() { }
	//	public MessageTo(string fieldWidth = "70") : base(nameof(MessageTo), "To", "To", fieldWidth) { }
	//}

	//public class MessageSize : ColumnDescription
	//{
	//	public MessageSize() { }
	//	public MessageSize(string fieldWidth = "40") : base(nameof(MessageSize), "Size", "Size", fieldWidth) { }
	//}

	public class Subject : ColumnDescription
	{
		public Subject() { }
		public Subject(string width = "1*", double minWidth = 0) : base(nameof(Subject), "Subject", "Subject", width, minWidth) { }
		public Subject(string headerShort, string width, double minWidth = 0) : base(nameof(Subject), "Subject", headerShort, width, minWidth) { }
	}

}
