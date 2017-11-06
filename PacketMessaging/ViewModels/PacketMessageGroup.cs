using FormControlBaseClass;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace PacketMessaging.ViewModels
{
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
	public class PacketMessageViewList
	{
		List<Field> messageViewList;

		[System.Xml.Serialization.XmlArrayItemAttribute("FormField", IsNullable = false)]
		public List<Field> MessageViewList => messageViewList;

		public PacketMessageViewList()
		{
			messageViewList = new List<Field>();
		}

		public void Add(Field field)
		{
			messageViewList.Add(field);
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

	public class Field
	{
		public string PropertyName { get; set; }
		public string Header { get; set; }
		public string HeaderShort { get; set; }
		public GridLength Width { get; set; }
		public double MinWidth { get; set; }
		public string WidthAsString { get; set; }

		public Field(string propertyName, string fieldHeader, string headerShort, string fieldWidth, double minWidth = 0)
		{
			PropertyName = propertyName;
			Header = fieldHeader;
			HeaderShort = headerShort;
			if (fieldWidth.EndsWith("*"))
				Width = new GridLength(Convert.ToDouble(fieldWidth.TrimEnd(new char[] { '*' })), GridUnitType.Star);
			else
				Width = new GridLength(Convert.ToDouble(fieldWidth));
			MinWidth = minWidth;
			WidthAsString = fieldWidth;
		}
	}
	public class Area : Field
	{
		public Area(string fieldWidth) : base(nameof(Area), "Area", "Area", fieldWidth) { }

		public Area(string headerShort, string fieldWidth) : base(nameof(Area), "Area", headerShort, fieldWidth) { }
	}

	public class BBSName : Field
	{
		public BBSName(string fieldWidth) : base(nameof(BBSName), "BBS", "BBS", fieldWidth) { }
	}

	public class JNOSDate : Field
	{
		public JNOSDate(string fieldWidth) : base(nameof(JNOSDate), "JNOSTime", "JNOSTime", fieldWidth) { }
	}

	public class SentTime : Field
	{
		public SentTime(string fieldWidth) : base(nameof(SentTime), "Sent", "Sent", fieldWidth) { }
	}

	public class CreateTime : Field
	{
		public CreateTime(string fieldWidth) : base(nameof(CreateTime), "Create Time", "Create Time", fieldWidth) { }
		public CreateTime(string headerShort, string fieldWidth) : base(nameof(CreateTime), "Create Time", headerShort, fieldWidth) { }
	}

	public class ReceivedTime : Field
	{
		public ReceivedTime(string fieldWidth) : base(nameof(ReceivedTime), "Received Time", "Received Time", fieldWidth) { }
		public ReceivedTime(string headerShort, string fieldWidth) : base(nameof(ReceivedTime), "Received Time", headerShort, fieldWidth) { }
	}

	public class MessageNumber : Field
	{
		public MessageNumber(string fieldWidth) : base(nameof(MessageNumber), "Msg Number", "Msg Number", fieldWidth) { }
		public MessageNumber(string headerShort, string fieldWidth) : base(nameof(MessageNumber), "Msg Number", headerShort, fieldWidth) { }
	}

	public class MessageFrom : Field
	{
		public MessageFrom(string fieldWidth) : base(nameof(MessageFrom), "From", "From", fieldWidth) { }
	}

	public class MessageTo : Field
	{
		public MessageTo(string fieldWidth) : base(nameof(MessageTo), "To", "To", fieldWidth) { }
	}

	public class MessageSize : Field
	{
		public MessageSize(string fieldWidth) : base(nameof(MessageSize), "Size", "Size", fieldWidth) { }
	}

	public class Subject : Field
	{
		public Subject(string fieldWidth) : base(nameof(Subject), "Subject", "Subject", fieldWidth) { }
		public Subject(string fieldWidth, double minWidth = 0) : base(nameof(Subject), "Subject", "Subject", fieldWidth, minWidth) { }
		public Subject(string headerShort, string fieldWidth, double minWidth = 0) : base(nameof(Subject), "Subject", headerShort, fieldWidth, minWidth) { }
	}

}
