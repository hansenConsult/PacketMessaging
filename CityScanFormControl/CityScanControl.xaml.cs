using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using FormControlBaseClass;
using Windows.UI.Xaml.Media;
using Windows.UI;
using MetroLog;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace CityScanFormControl
{
	[FormControl(
		FormControlName = "city_scan",
		FormControlMenuName = "City Scan/Flash Report",
		FormControlType = FormControlAttribute.FormType.CountyForm)
	]
	public partial class CityScanControl : FormControlBase
	{
		//private ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<CityScanControl>();
		public string[] CountyCityNames = new string[] {
			"Campbell",
			"Cupertino",
			"Gilroy",
			"Loma Prieta",
			"Los Altos",
			"Los Altos Hills",
			"Los Gatos/Monte Sereno",
			"Milpitas",
			"Morgan Hill",
			"Mountain View",
			"NASA-Ames",
			"Palo Alto",
			"San Jose",
			"Santa Clara",
			"Saratoga",
			"Stanford",
			"Sunnyvale",
			"Other"
		};


		public CityScanControl()
		{
			this.InitializeComponent();

			ScanControls(PrintableArea);

			InitializeControls();

			ToICSPosition = "Planning";
			city.ItemsSource = CountyCityNames;
		}

		public override string ReceiverMsgNo
		{ get { return GetTextBoxString(receiverMsgNo); } set { SetTextBoxString(receiverMsgNo, value); } }

		public override string MessageNo
		{ get { return GetTextBoxString(messageNo); } set { messageNo.Text = value; } }

		public override string SenderMsgNo
		{ get { return GetTextBoxString(senderMsgNo); } set { SetTextBoxString(senderMsgNo, value); } }

		public override string MsgDate
		{ get { return GetTextBoxString(msgDate); } set { SetTextBoxString(msgDate, value); } }

		public override string MsgTime
		{ get { return GetTextBoxString(msgTime); } set { SetTextBoxString(msgTime, value); } }

		public override string Severity
		{ get { return severity.GetRadioButtonCheckedState(); } set { severity.SetRadioButtonCheckedState(value); } }

		public override string HandlingOrder
		{ get { return handlingOrder.GetRadioButtonCheckedState(); } set { handlingOrder.SetRadioButtonCheckedState(value); } }

		public string Reply
		{ get { return reply.GetRadioButtonCheckedState(); } set { reply.SetRadioButtonCheckedState(value); } }

		public string ReplyBy
		{ get { return GetTextBoxString(replyBy); } set { SetTextBoxString(replyBy, value); } }

		//public string City
		//{ get { return GetComboBoxSelectedValuePath(city); } set { SetComboBoxString(city, value); } }

		public string CityOther
		{ get { return GetTextBoxString(cityOther); } set { SetTextBoxString(cityOther, value); } }

		public string ToICSPosition
		{ get { return toICSPosition.Text; } set { SetTextBoxString(toICSPosition, value); } }

		public string ToTitle
		{ get { return toTitle.Text; } set { SetTextBoxString(toTitle, value); } }

		public string ToName
		{ get { return toName.Text; } set { SetTextBoxString(toName, value); } }

		public string ToTelephone
		{ get { return toTelephone.Text; } set { SetTextBoxString(toTelephone, value); } }

		public string ToRadioFrequency
		{ get { return toRadioFrequency.Text; } set { SetTextBoxString(toRadioFrequency, value); } }

		public string CityImpact
		{ get { return cityImpact.GetRadioButtonCheckedState(); } set { cityImpact.SetRadioButtonCheckedState(value); } }

		public string EmergencyDeclared
		{ get { return emergencyDeclared.GetRadioButtonCheckedState(); } set { emergencyDeclared.SetRadioButtonCheckedState(value); } }

		public string EmergencyDeclaredDate
		{ get { return GetTextBoxString(emergencyDeclaredDate); } set { SetTextBoxString(emergencyDeclaredDate, value); } }

		public string EmergencyDeclaredTime
		{ get { return GetTextBoxString(emergencyDeclaredTime); } set { SetTextBoxString(emergencyDeclaredTime, value); } }

		public string EmergencyDeclaredName
		{ get { return GetTextBoxString(emergencyDeclaredName); } set { SetTextBoxString(emergencyDeclaredName, value); } }

		public string EmergencyDeclaredTitle
		{ get { return GetTextBoxString(emergencyDeclaredTitle); } set { SetTextBoxString(emergencyDeclaredTitle, value); } }

		public string EmergencyOpsCenterActivated
		{ get { return emergencyOpsCenterActivated.GetRadioButtonCheckedState(); } set { emergencyOpsCenterActivated.SetRadioButtonCheckedState(value); } }

		public string MajorIncidents
		{ get { return majorIncidents.GetRadioButtonCheckedState(); } set { majorIncidents.SetRadioButtonCheckedState(value); } }

		public string Incident
		{ get { return GetTextBoxString(incident); } set { SetTextBoxString(incident, value); } }

		public string Location
		{ get { return GetTextBoxString(location); } set { SetTextBoxString(location, value); } }

		public string Status
		{ get { return GetTextBoxString(status); } set { SetTextBoxString(status, value); } }

		public string AdditionalResourcesRequest
		{ get { return additionalResources.GetRadioButtonCheckedState(); } set { additionalResources.SetRadioButtonCheckedState(value); } }

		public override string OperatorDate
		{ get { return GetTextBoxString(operatorDate); } set { SetTextBoxString(operatorDate, value); } }

		public override string OperatorTime
		{ get { return GetTextBoxString(operatorTime); } set { SetTextBoxString(operatorTime, value); } }

		public override string OperatorCallsign
		{ get; set; }

		public override string OperatorName
		{ get; set; }

		public override string PacFormName => "city-scan";

        public override string PacFormType => "city_scan";

        public override string CreateSubject()
		{
			string cityName = city.SelectedItem as string;
			if (cityName == "Other")
			{
				cityName += (" " + GetTextBoxString(cityOther));
			}
			string emergencyDeclared = "";
			if (EmergencyDeclared != null)
			{
				emergencyDeclared = " Emergency Declared: " + (EmergencyDeclared == "emergencyDeclaredYes" ? "yes" : "no");
			}

			return (MessageNo + "_" + Severity?.ToUpper()[0] + "/" + HandlingOrder?.ToUpper()[0] + "_CityScan_" + cityName + emergencyDeclared);
		}

		////public override string CreateSubject(ref PacketMessage packetMessage)
		////{
		////	var severityField = from formField in packetMessage.FormFieldArray where formField.ControlName == "severity" select formField;
		////	var handlingOrderField = from formField in packetMessage.FormFieldArray where formField.ControlName == "handlingOrder" select formField;
		////	var emergencyDeclaredField = from formField in packetMessage.FormFieldArray where formField.ControlName == "emergencyDeclared" select formField;
		////	string s = emergencyDeclaredField.First().ControlContent;

		////	return (MessagegNo + "_" + severityField.First().ControlContent.ToUpper()[0] + "/" + handlingOrderField.First().ControlContent.ToUpper()[0] + "_CityScan_" + City + " Emergency Declared: " + (EmergencyDeclared == "emergencyDeclaredYes" ? "yes" : "no"));
		////}

		protected override List<string> CreateOutpostDataFromFormFields(ref PacketMessage packetMessage, ref List<string> outpostData)
		{
            foreach (FormField formField in packetMessage.FormFieldArray)
			{
				if (formField.ControlContent == null || formField.ControlContent.Length == 0)
					continue;

				switch (formField.ControlName)
				{
					//MsgNo: [6DM-150P]
					case "messagegNo":
						outpostData.Add($"MsgNo: [{formField.ControlContent}]");
						break;
					//D.: [OTHER]
					case "severity":
						outpostData.Add($"D.: [{formField.ControlContent.ToUpper()}]");
						break;
					//E.: [ROUTINE]
					case "handlingOrder":
						outpostData.Add($"E.: [{formField.ControlContent.ToUpper()}]");
						break;
					// F.: [Yes]
					case "reply":
						outpostData.Add($"F.: [{(formField.ControlContent == "replyYes" ? "yes" : "no")}]");
						break;
					// replyby: [1230]
					case "replyBy":
						outpostData.Add($"replyby.: [{formField.ControlContent}]");
						break;
					// 1a.: [Mountain View]
					case "city":
						outpostData.Add($"1a.: [{formField.ControlContent}]");
						break;
					// 1b.: [Fremont]
					case "cityOther":
						outpostData.Add($"1b.: [{formField.ControlContent}]");
						break;
					//2.: [08/23/2015]
					case "msgDate":
						outpostData.Add($"2.: [{formField.ControlContent}]");
						break;
					//3.: [1304]
					case "msgTime":
						outpostData.Add($"3.: [{formField.ControlContent}]");
						break;
					//4.: [Poul Hansen]
					case "toName":
						outpostData.Add($"4.: [{formField.ControlContent}]");
						break;
					//5.: [programmer]
					case "toTitle":
						outpostData.Add($"5.: [{formField.ControlContent}]");
						break;
					//6.: [000-000-0000]
					case "toTelephone":
						outpostData.Add($"6.: [{formField.ControlContent}]");
						break;
					//7.: [145.270]
					case "toRadioFrequency":
						outpostData.Add($"7.: [{formField.ControlContent}]");
						break;
					// 8.: [yes]
					case "cityImpact":
						outpostData.Add($"8.: [{(formField.ControlContent == "cityImpactYes" ? "yes" : "no")}]");
						break;
					// 9.: [no]
					case "emergencyDeclared":
						outpostData.Add($"9.: [{(formField.ControlContent == "emergencyDeclaredYes" ? "yes" : "no")}]");
						break;
					// 10.: [08/23/2015]
					case "emergencyDeclaredDate":
						outpostData.Add($"10.: [{formField.ControlContent}]");
						break;
					// 11.: [1246]
					case "emergencyDeclaredTime":
						outpostData.Add($"11.: [{formField.ControlContent}]");
						break;
					// 12.: [me]
					case "emergencyDeclaredName":
						outpostData.Add($"12.: [{formField.ControlContent}]");
						break;
					// 13.: [chef]
					case "emergencyDeclaredTitle":
						outpostData.Add($"13.: [{formField.ControlContent}]");
						break;
					// 14.: [yes]
					case "emergencyOpsCenterActivated":
						outpostData.Add($"14.: [{(formField.ControlContent == "emergencyOpsCenterActivatedYes" ? "yes" : "no")}]");
						break;
					// 15.: [no]
					case "majorIncidents":
						outpostData.Add($"15.: [{(formField.ControlContent == "majorIncidentsYes" ? "yes" : "no")}]");
						break;
					// 16a-I: [\nincident]
					case "incident":
						outpostData.Add($"16a-I: [\\n{formField.ControlContent}]");
						break;
					// 16a-L: [\nlocation]
					case "location":
						outpostData.Add($"16a-L: [\\n{formField.ControlContent}]");
						break;
					// 16a-S: [\nstatus]
					case "status":
						outpostData.Add($"16a-S: [\\n{formField.ControlContent}]");
						break;
					// OpDate: [08/23/2015]
					case "operatorDate":
						outpostData.Add($"OpDate: [{formField.ControlContent}]");   // Note: hidden field
						break;
					// OpTime: [1319]
					case "operatorTime":
						outpostData.Add($"OpTime: [{formField.ControlContent}]");   // Note: hidden field
						break;
					// 17.: [yes]
					case "additionalResources":
						outpostData.Add($"17.: [{(formField.ControlContent == "additionalResourcesYes" ? "yes" : "no")}]");
						break;
				}
			}
			outpostData.Add("#EOF");

			return outpostData;
		}

		public override string CreateOutpostData(ref PacketMessage packetMessage)
		{
			List<string> outpostData = new List<string>();

			//!PACF!6DM-123_O/R_CityScan_Mountain View Fremont Emergency Declared: no
			outpostData.Add("!PACF! " + packetMessage.Subject);
			outpostData.Add("# CITY-SCAN UPDATE FLASH REPORT ");
			outpostData.Add("# JS-ver. PR-4.1-3.11, 03/10/17");
			//# FORMFILENAME: city-scan.html
			outpostData.Add("# FORMFILENAME: city-scan.html");

			outpostData = CreateOutpostDataFromFormFields(ref packetMessage, ref outpostData);

			return CreateOutpostMessageBody(outpostData);
		}


		public override FormField[] ConvertFromOutpost(string msgNumber, ref string[] msgLines)
		{
			//!PACF!6DM - 123_O / R_CityScan_Mountain View Fremont Emergency Declared: no
			//# CITY-SCAN UPDATE FLASH REPORT 
			//# FORMFILENAME: city-scan.html

			FormField[] formFields = CreateEmptyFormFieldsArray();
			string radioButtonContent;
			foreach (FormField formField in formFields)
			{
				switch (formField.ControlName)
				{
					//MsgNo: [6DM-150P]
					case "senderMsgNo":
						formField.ControlContent = GetOutpostValue("MsgNo", ref msgLines);
						break;
					case "messagegNo":
						formField.ControlContent = msgNumber;
						break;
					//D.: [OTHER]
					case "severity":
						formField.ControlContent = GetOutpostValue("D.", ref msgLines).ToLower();
						break;
					//E.: [ROUTINE]
					case "handlingOrder":
						formField.ControlContent = GetOutpostValue("E.", ref msgLines).ToLower();
						break;
					// F.: [Yes]
					case "reply":
						radioButtonContent = GetOutpostValue("F.", ref msgLines);
						if (radioButtonContent?.Length > 0)
						{
							formField.ControlContent = (radioButtonContent == "yes" ? "replyYes" : "replyNo");
						}
						break;
					// replyby: [1230]
					case "replyBy":
						formField.ControlContent = GetOutpostValue("replyby", ref msgLines);
						break;
					// 1a.: [Mountain View]
					case "city":
						formField.ControlContent = GetOutpostValue("1a.", ref msgLines);
						break;
					// 1b.: [Fremont]
					case "cityOther":
						formField.ControlContent = GetOutpostValue("1b.", ref msgLines);
						break;
					//2.: [08/23/2015]
					case "msgDate":
						formField.ControlContent = GetOutpostValue("2.", ref msgLines);
						break;
					//3.: [1304]
					case "msgTime":
						formField.ControlContent = GetOutpostValue("3.", ref msgLines);
						break;
					//4.: [Poul Hansen]
					case "toName":
						formField.ControlContent = GetOutpostValue("4.", ref msgLines);
						break;
					//5.: [programmer]
					case "toTitle":
						formField.ControlContent = GetOutpostValue("5.", ref msgLines);
						break;
					//6.: [000-000-0000]
					case "toTelephone":
						formField.ControlContent = GetOutpostValue("6.", ref msgLines);
						break;
					//7.: [145.270]
					case "toRadioFrequency":
						formField.ControlContent = GetOutpostValue("7.", ref msgLines);
						break;
					// 8.: [yes]
					case "cityImpact":
						radioButtonContent = GetOutpostValue("8.", ref msgLines);
						if (radioButtonContent?.Length > 0)
						{
							formField.ControlContent = (radioButtonContent == "yes" ? "cityImpactYes" : "cityImpactNo");
						}
						break;
					// 9.: [no]
					case "emergencyDeclared":
						radioButtonContent = GetOutpostValue("9.", ref msgLines);
						if (radioButtonContent?.Length > 0)
						{
							formField.ControlContent = (radioButtonContent == "yes" ? "emergencyDeclaredYes" : "emergencyDeclaredNo");
						}
						break;
					// 10.: [08/23/2015]
					case "emergencyDeclaredDate":
						formField.ControlContent = GetOutpostValue("10.", ref msgLines);
						break;
					// 11.: [1246]
					case "emergencyDeclaredTime":
						formField.ControlContent = GetOutpostValue("11.", ref msgLines);
						break;
					// 12.: [me]
					case "emergencyDeclaredName":
						formField.ControlContent = GetOutpostValue("12.", ref msgLines);
						break;
					// 13.: [chef]
					case "emergencyDeclaredTitle":
						formField.ControlContent = GetOutpostValue("13.", ref msgLines);
						break;
					// 14.: [yes]
					case "emergencyOpsCenterActivated":
						radioButtonContent = GetOutpostValue("14.", ref msgLines);
						if (radioButtonContent?.Length > 0)
						{
							formField.ControlContent = (radioButtonContent == "yes" ? "emergencyOpsCenterActivatedYes" : "emergencyOpsCenterActivatedNo");
						}
						break;
					// 15.: [no]
					case "majorIncidents":
						radioButtonContent = GetOutpostValue("15.", ref msgLines);
						if (radioButtonContent?.Length > 0)
						{
							formField.ControlContent = (radioButtonContent == "yes" ? "majorIncidentsYes" : "majorIncidentsNo");
						}
						break;
					// 16a-I: [\nincident]
					case "incident":
						formField.ControlContent = GetOutpostValue("16a-I", ref msgLines);
						break;
					// 16a-L: [\nlocation]
					case "location":
						formField.ControlContent = GetOutpostValue("16a-L", ref msgLines);
						break;
					// 16a-S: [\nstatus]
					case "status":
						formField.ControlContent = GetOutpostValue("16a-S", ref msgLines);
						break;
					// OpDate: [08/23/2015]
					case "operatorDate":
						formField.ControlContent = GetOutpostValue("OpDate", ref msgLines);
						break;
					// OpTime: [1319]
					case "operatorTime":
						formField.ControlContent = GetOutpostValue("OpTime", ref msgLines);
						break;
					// 17.: [yes]
					case "additionalResources":
						radioButtonContent = GetOutpostValue("17.", ref msgLines);
						if (radioButtonContent?.Length > 0)
						{
							formField.ControlContent = (radioButtonContent == "yes" ? "additionalResourcesYes" : "additionalResourcesNo");
						}
						break;
				}
			}
			return formFields;
		}

		private void city_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//ComboBox city = (ComboBox)sender;
			string cityName = ((ComboBox)sender).SelectedItem as string;
			if (cityName == "Other")
			{
				labelOther.Foreground = _lightSalmonBrush;
				cityOther.IsEnabled = true;
				cityOther.Tag = "required";
			}
			else
			{
				labelOther.Foreground = _blackBrush;
				cityOther.IsEnabled = false;
				cityOther.Tag = "";
			}
			Subject_Changed(sender, e);
		}

	}
}
