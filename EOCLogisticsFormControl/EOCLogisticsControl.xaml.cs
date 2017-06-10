using FormControlBaseClass;
using System.Collections.Generic;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace EOCLogisticsFormControl
{
	[FormControl(
		FormControlName = "EOCLogisticsRequest",
		FormControlMenuName = "XSC EOC Logistics Request",
		FormControlType = FormControlAttribute.FormType.CountyForm)
	]

	public partial class EOCLogisticsControl : FormControlBase
	{
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
			"Sunnyvale"
		};

		public EOCLogisticsControl()
		{
			this.InitializeComponent();

			ScanControls(root);

			InitializeControls();

			ReceivedOrSent = "sent";
			HowRecevedSent = "packet";

			sectionAgencyCity.ItemsSource = CountyCityNames;
		}

		public string SenderMsgNo
		{ get { return GetTextBoxString(senderMsgNo); } set { SetTextBoxString(senderMsgNo, value); } }

		public override string MessageNo
		{ get { return GetTextBoxString(messagegNo); } set { SetTextBoxString(messagegNo, value); } }

		public string ReceiverMsgNo
		{ get { return GetTextBoxString(receiverMsgNo); } set { SetTextBoxString(receiverMsgNo, value); } }

		public override string MsgDate
		{ get; set; }

		public override string MsgTime
		{ get; set; }

		public string Severity
		{ get { return severity.GetRadioButtonCheckedState(); } set { severity.SetRadioButtonCheckedState(value); } }

		public override string HandlingOrder
		{ get { return handlingOrder.GetRadioButtonCheckedState(); } set { handlingOrder.SetRadioButtonCheckedState(value); } }

		public string Reply
		{ get { return reply.GetRadioButtonCheckedState(); } set { reply.SetRadioButtonCheckedState(value); } }

		public string ReplyBy
		{ get { return GetTextBoxString(replyBy); } set { SetTextBoxString(replyBy, value); } }

		public string ItemServiceRequested
		{ get { return GetTextBoxString(itemServiceRequested); } set { SetTextBoxString(replyBy, value); } }

		public string RequestorName
		{ get { return GetTextBoxString(requestorName); } set { SetTextBoxString(requestorName, value); } }

		public string RequestorPhone
		{ get { return GetTextBoxString(requestorPhone); } set { SetTextBoxString(requestorPhone, value); } }

		public string SectionAgencyCity
		{ get { return GetComboBoxString(sectionAgencyCity); } set { SetComboBoxString(sectionAgencyCity, value); } }

		public string SectionAgencyEmail
		{ get { return GetTextBoxString(sectionAgencyEmail); } set { SetTextBoxString(sectionAgencyEmail, value); } }

		public string ItemServiceDescription
		{ get { return GetTextBoxString(itemServiceDescription); } set { SetTextBoxString(itemServiceDescription, value); } }

		public string Purpose
		{ get { return GetTextBoxString(purpose); } set { SetTextBoxString(purpose, value); } }

		public string Quantity
		{ get { return GetTextBoxString(quantity); } set { SetTextBoxString(quantity, value); } }

		public string QuantitySize
		{ get { return GetTextBoxString(quantitySize); } set { SetTextBoxString(quantitySize, value); } }

		public string WhenNeededDate
		{ get { return GetTextBoxString(whenNeededDate); } set { SetTextBoxString(whenNeededDate, value); } }

		public string WhenNeededTime
		{ get { return GetTextBoxString(whenNeededTime); } set { SetTextBoxString(whenNeededTime, value); } }

		public string WhenNeeded
		{ get { return whenNeeded.GetRadioButtonCheckedState(); } set { whenNeeded.SetRadioButtonCheckedState(value); } }

		public string DeliveryLocationAddress
		{ get { return GetTextBoxString(deliveryLocationAddress); } set { SetTextBoxString(deliveryLocationAddress, value); } }

		public string DeliveryLocationCrossSt
		{ get { return GetTextBoxString(deliveryLocationCrossSt); } set { SetTextBoxString(deliveryLocationCrossSt, value); } }

		public string ReceiverNamePosition
		{ get { return GetTextBoxString(receiverNamePosition); } set { SetTextBoxString(receiverNamePosition, value); } }

		public string ReceiverPhoneNumber
		{ get { return GetTextBoxString(receiverPhoneNumber); } set { SetTextBoxString(receiverPhoneNumber, value); } }

		public string ReceiverEmailAddress
		{ get { return GetTextBoxString(receiverEmailAddress); } set { SetTextBoxString(receiverEmailAddress, value); } }

		public string ReceiverCellNumber
		{ get { return GetTextBoxString(receiverCellNumber); } set { SetTextBoxString(receiverCellNumber, value); } }

		public string ReceiverRadioFrequency
		{ get { return GetTextBoxString(receiverRadioFrequency); } set { SetTextBoxString(receiverRadioFrequency, value); } }

		public string AdditionalDetailsComments
		{ get { return GetTextBoxString(additionalDetailsComments); } set { SetTextBoxString(additionalDetailsComments, value); } }

		public string AuthorizationName
		{ get { return GetTextBoxString(authorizationName); } set { SetTextBoxString(authorizationName, value); } }

		public string ReceivedOrSent
		{ get { return receivedOrSent.GetRadioButtonCheckedState(); } set { receivedOrSent.SetRadioButtonCheckedState(value); } }

		public string HowRecevedSent
		{ get { return howRecevedSent.GetRadioButtonCheckedState(); } set { howRecevedSent.SetRadioButtonCheckedState(value); } }

		public override string OperatorDate
		{ get { return GetTextBlockString(operatorDate); } set { SetTextBlockString(operatorDate, value); } }

		public override string OperatorTime
		{ get { return GetTextBlockString(operatorTime); } set { SetTextBlockString(operatorTime, value); }}

		public override string OperatorCallsign
		{ get { return GetTextBlockString(operatorCallsign); } set { SetTextBlockString(operatorCallsign, value); } }


		public override string OperatorName
		{ get { return GetTextBlockString(operatorName); } set { SetTextBlockString(operatorName, value); } }

		public override string PacFormName => "EOCLogisticsRequest";



		//     public override string CreateSubject(ref PacketMessage packetMessage)
		//     {
		////1234_O / R_EOC Logistics Req_Mountain View
		//var severityField = from formField in packetMessage.FormFieldArray where formField.ControlName == "severity" select formField;
		//var handlingOrderField = from formField in packetMessage.FormFieldArray where formField.ControlName == "handlingOrder" select formField;
		//var cityField = packetMessage.FormFieldArray.Where(formField => formField.ControlName == "sectionAgencyCity").FirstOrDefault();

		//return (MessagegNo + "_" + severityField.First().ControlContent.ToUpper()[0] + "/" + handlingOrderField.First().ControlContent.ToUpper()[0] + "_EOC Logistics Req_" + SectionAgencyCity);
		//     }

		public override string CreateSubject()
		{
			return (MessageNo + "_" + Severity?.ToUpper()[0] + "/" + HandlingOrder?.ToUpper()[0] + "_EOC Logistics Req_" + sectionAgencyCity.SelectedItem);
		}

		protected override List<string> CreateOutpostDataFromFormFields(ref PacketMessage packetMessage, ref List<string> outpostData)
		{
			foreach (FormField formField in packetMessage.FormFieldArray)
			{
				if (formField.ControlContent == null || formField.ControlContent.Length == 0)
					continue;

				switch (formField.ControlName)
				{
					//1: [1234]
					case "messagegNo":
						outpostData.Add($"1: [{formField.ControlContent}]");
						break;
					//3: [true] emergency
					//4: [true] urgent
					//5: [true] other
					case "severity":
						switch (formField.ControlContent)
						{
							case "emergency":
								outpostData.Add($"4: [true]");
								break;
							case "urgent":
								outpostData.Add($"5: [true]");
								break;
							case "other":
								outpostData.Add($"6: [true]");
								break;
						}
						break;
					//7: [true] immediate
					//8: [true] priority
					//9: [true] routine
					case "handlingOrder":
						switch (formField.ControlContent)
						{
							case "immediate":
								outpostData.Add($"7: [true]");
								break;
							case "priority":
								outpostData.Add($"8: [true]");
								break;
							case "routine":
								outpostData.Add($"9: [true]");
								break;
						}
						break;
					//9: [true] yes
					//11: [true] no
					case "reply":
						switch (formField.ControlContent)
						{
							case "yes":
								outpostData.Add($"9: [true]");
								break;
							case "no":
								outpostData.Add($"11: [true]");
								break;
						}
						break;
					//10: [1345]
					case "replyBy":
						outpostData.Add($"10: [{formField.ControlContent}]");
						break;
					//12:
					//13: [Item/Service requested]
					case "itemServiceRequested":
						outpostData.Add($"13: [{formField.ControlContent}]");
						break;
					//14: [Requestor]
					case "requestorName":
						outpostData.Add($"14: [{formField.ControlContent}]");
						break;
					//15: [123-123-4567]
					case "requestorPhone":
						outpostData.Add($"15: [{formField.ControlContent}]");
						break;
					//16: [Display List of Cities]
					//17: [Mountain View]
					//19: [Mountain View}10]
					case "sectionAgencyCity":
						int itemIndex = 0;
						for (int i = 0; i < CountyCityNames.Length; i++)
						{
							if (formField.ControlContent == CountyCityNames[i])
							{
								itemIndex = i + 1;
								break;
							}
						}
						if (itemIndex > 0)
						{
							outpostData.Add($"16: [Display List of Cities]");
							outpostData.Add($"17: [{formField.ControlContent}]");
							outpostData.Add($"19: [{formField.ControlContent}" + "}" + $"{itemIndex}]");
						}
						else
						{
							outpostData.Add($"16: [Display Manual Entry]");
							outpostData.Add($"17: [{formField.ControlContent}]");
							outpostData.Add("19: [0}0]");
						}
						break;
					//18: [poul.erik@hansenca.com]
					case "sectionAgencyEmail":
						outpostData.Add($"18: [{formField.ControlContent}]");
						break;
					//20: ?
					//21: [\nService description]
					case "itemServiceDescription":
						outpostData.Add($"21: [\n{formField.ControlContent}]");
						break;
					//22: [\nPurpose]
					case "purpose":
						outpostData.Add($"22: [\n{formField.ControlContent}]");
						break;
					//23: [quantity]
					case "quantity":
						outpostData.Add($"23: [{formField.ControlContent}]");
						break;
					//24: [size]
					case "quantitySize":
						outpostData.Add($"24: [{formField.ControlContent}]");
						break;
					//25: [11/02/15]
					case "WhenNeededDate":
						outpostData.Add($"25: [{formField.ControlContent}]");
						break;
					//26: [1234]
					case "WhenNeededTime":
						outpostData.Add($"26: [{formField.ControlContent}]");
						break;
					//27: [true] pickup
					//28: [true] delivery
					case "whenNeeded":
						switch (formField.ControlContent)
						{
							case "pickup":
								outpostData.Add($"27: [true]");
								break;
							case "delivery":
								outpostData.Add($"28: [true]");
								break;
						}
						break;
					//29: [Delivery loc address]
					case "deliveryLocationAddress":
						outpostData.Add($"29: [{formField.ControlContent}]");
						break;
					//30: [Delivery x-street]
					case "deliveryLocationCrossSt":
						outpostData.Add($"30: [{formField.ControlContent}]");
						break;
					//31: [Receiver Name]
					case "receiverNamePosition":
						outpostData.Add($"31: [{formField.ControlContent}]");
						break;
					//32: [Receiver phone]
					case "receiverPhoneNumber":
						outpostData.Add($"32: [{formField.ControlContent}]");
						break;
					//33: [receiver@email.com]
					case "receiverEmailAddress":
						outpostData.Add($"33: [{formField.ControlContent}]");
						break;
					//34: [Receiver cell]
					case "receiverCellNumber":
						outpostData.Add($"34: [{formField.ControlContent}]");
						break;
					//35: [Receiver freq]
					case "receiverRadioFrequency":
						outpostData.Add($"35: [{formField.ControlContent}]");
						break;
					//36: [\nAdditional details]
					case "additionalDetailsComments":
						outpostData.Add($"36: [\n{formField.ControlContent}]");
						break;
					//37: [Authorization Name]
					case "authorizationName":
						outpostData.Add($"37: [{formField.ControlContent}]");
						break;
                    //38: [true] Received
					//39: [true] Sent
					case "receivedOrSent":
						switch (formField.ControlContent)
						{
							case "received":
								outpostData.Add($"38: [true]");
								break;
							case "sent":
								outpostData.Add($"39: [true]");
								break;
						}
						break;
					//40: [true] voice
					//41: [trye] packet
					case "howRecevedSent":
						switch (formField.ControlContent)
						{
							case "amateurRadio":
								outpostData.Add($"40: [true]");
								break;
							case "packet":
								outpostData.Add($"41: [true]");
								break;
						}
						break;
					//42: [KZ6DM]
					case "operatorCallsign":
						outpostData.Add($"42: [{formField.ControlContent.ToUpper()}]");
						break;
					//43: [Poul Hansen]
					case "operatorName":
						outpostData.Add($"43: [{formField.ControlContent}]");
						break;
					//44: [11/02/2015{odate]
					case "operatorDate":
						outpostData.Add($"44: [{formField.ControlContent}" + "{odate]");
						break;
					//45: [10:50{otime]
					case "operatorTime":
						outpostData.Add($"45: [{formField.ControlContent}" + "{otime]");
						break;
				}
			}
			outpostData.Add("#EOF");

			return outpostData;
		}

		public override string CreateOutpostData(ref PacketMessage packetMessage)
		{
            List<string> outpostData = new List<string>
            {
        
                //!PACF!1234_O / R_EOC Logistics Req_Mountain View
                "!PACF!" + packetMessage.Subject,
                //# JS:EOC Logistics Request (which4)
                "# JS:EOC Logistics Request (which4)",
                //# JS-ver. PR-4.1-2.7, 03/10/17
                "# JS-ver. PR-4.1-2.7, 03/10/17",
                //# FORMFILENAME: EOCLogisticsRequest.html
                "# FORMFILENAME: EOCLogisticsRequest.html"
            };
            outpostData = CreateOutpostDataFromFormFields(ref packetMessage, ref outpostData);

			return CreateOutpostMessageBody(outpostData);
		}

		public override FormField[] ConvertFromOutpost(string msgNumber, ref string[] msgLines)
		{
			FormField[] formFields = CreateEmptyFormFieldsArray();
			string radioButtonContent;
			foreach (FormField formField in formFields)
			{
				switch (formField.ControlName)
				{
					//1: [6DM-150P]
					case "senderMsgNo":
						formField.ControlContent = GetOutpostValue("1", ref msgLines);
						break;
					case "messagegNo":
						formField.ControlContent = msgNumber;
						break;
					//3: [true] Emergency
					//4: [true] urgent
					//5: [true] other
					case "severity":
						if ((radioButtonContent = GetOutpostValue("3", ref msgLines)?.ToLower()) == "true")
						{
							formField.ControlContent = "emergency";
						}
						else if (GetOutpostValue("4", ref msgLines)?.ToLower() == "true")
						{
							formField.ControlContent = "urgent";
						}
						else if (GetOutpostValue("5", ref msgLines)?.ToLower() == "true")
						{
							formField.ControlContent = "other";
						}
						break;
					//6: [true] Immediate
					//7: [true] priority
					//8: [true] routine
					case "handlingOrder":
						if (GetOutpostValue("6", ref msgLines)?.ToLower() == "true")
						{
							formField.ControlContent = "immediate";
						}
						else if (GetOutpostValue("7", ref msgLines)?.ToLower() == "true")
						{
							formField.ControlContent = "priority";
						}
						else if (GetOutpostValue("8", ref msgLines)?.ToLower() == "true")
						{
							formField.ControlContent = "routine";
						}
						break;
					//9: [true] Reply yes
					//11: [true] Reply no
					case "reply":
						string s = GetOutpostValue("9", ref msgLines)?.ToLower();
						string s2 = GetOutpostValue("11", ref msgLines)?.ToLower();
						if (GetOutpostValue("9", ref msgLines)?.ToLower() == "true")
						{
							formField.ControlContent = "replyYes";
						}
						else if (GetOutpostValue("11", ref msgLines)?.ToLower() == "true")
						{
							formField.ControlContent = "replyNo";
						}
						break;
					//10: [1345]
					case "replyBy":
						formField.ControlContent = GetOutpostValue("10", ref msgLines);
						break;
					//12: ?
					//13: [Item/Service requested]
					case "itemServiceRequested":
						formField.ControlContent = GetOutpostValue("13", ref msgLines);
						break;
					//14: [Requestor]
					case "requestorName":
						formField.ControlContent = GetOutpostValue("14", ref msgLines);
						break;
					//15: [123-123-4567]
					case "requestorPhone":
						formField.ControlContent = GetOutpostValue("15", ref msgLines);
						break;
					//16: [Display List of Cities]  Not used
					//17: [Mountain View]
					case "sectionAgencyCity":
						formField.ControlContent = GetOutpostValue("17", ref msgLines);
						break;
					//18: [poul.erik@hansenca.com]
					case "sectionAgencyEmail":
						formField.ControlContent = GetOutpostValue("18", ref msgLines);
						break;
					//19: [Mountain View}10] Not used
					//20: ?
					//21: [\nService description]
					case "itemServiceDescription":
						formField.ControlContent = GetOutpostValue("21", ref msgLines)?.Substring(1);
						break;
					//22: [\nPurpose]
					case "purpose":
						formField.ControlContent = GetOutpostValue("22", ref msgLines)?.Substring(1);
						break;
					//23: [quantity]
					case "quantity":
						formField.ControlContent = GetOutpostValue("23", ref msgLines);
						break;
					//24: [size]
					case "quantitySize":
						formField.ControlContent = GetOutpostValue("24", ref msgLines);
						break;
					//25: [11/02/15]
					case "WhenNeededDate":
						formField.ControlContent = GetOutpostValue("25", ref msgLines);
						break;
					//26: [1234]
					case "WhenNeededTime":
						formField.ControlContent = GetOutpostValue("26", ref msgLines);
						break;
					//27: [true] pickup
					//28: [true] delivery
					case "whenNeeded":
						if ((radioButtonContent = GetOutpostValue("27", ref msgLines)?.ToLower()) == "true")
						{
							formField.ControlContent = "whenNeededPickup";
						}
						else if ((radioButtonContent = GetOutpostValue("28", ref msgLines)?.ToLower()) == "true")
						{
							formField.ControlContent = "whenNeededDelivery";
						}
						break;
					//29: [Delivery loc address]
					case "deliveryLocationAddress":
						formField.ControlContent = GetOutpostValue("29", ref msgLines);
						break;
					//30: [Delivery x-street]
					case "deliveryLocationCrossSt":
						formField.ControlContent = GetOutpostValue("30", ref msgLines);
						break;
					//31: [Receiver Name]
					case "receiverNamePosition":
						formField.ControlContent = GetOutpostValue("31", ref msgLines);
						break;
					//32: [Receiver phone]
					case "receiverPhoneNumber":
						formField.ControlContent = GetOutpostValue("32", ref msgLines);
						break;
					//33: [receiver@email.com]
					case "receiverEmailAddress":
						formField.ControlContent = GetOutpostValue("33", ref msgLines);
						break;
					//34: [Receiver cell]
					case "receiverCellNumber":
						formField.ControlContent = GetOutpostValue("34", ref msgLines);
						break;
					//35: [Receiver freq]
					case "receiverRadioFrequency":
						formField.ControlContent = GetOutpostValue("35", ref msgLines);
						break;
					//36: [\nAdditional details]
					case "additionalDetailsComments":
						formField.ControlContent = GetOutpostValue("36", ref msgLines)?.Substring(1);
						break;
					//37: [Authorization Name]
					case "authorizationName":
						formField.ControlContent = GetOutpostValue("37", ref msgLines);
						break;
					//38: [true] Received
					//39: [true] sent
					case "receivedOrSent":
						if (GetOutpostValue("38", ref msgLines)?.ToLower() == "true")
						{
							formField.ControlContent = "received";
						}
						else if (GetOutpostValue("39", ref msgLines)?.ToLower() == "true")
						{
							formField.ControlContent = "sent";
						}
						break;
					//40: [true] v=oice
					//41: [true] packet
					case "howRecevedSent":
						if (GetOutpostValue("40", ref msgLines)?.ToLower() == "true")
						{
							formField.ControlContent = "amateurRadio";
						}
						else if (GetOutpostValue("41", ref msgLines)?.ToLower() == "true")
						{
							formField.ControlContent = "packet";
						}
						break;
					//42: [KZ6DM]
					case "operatorCallsign":
						formField.ControlContent = GetOutpostValue("42", ref msgLines);
						break;
					//43: [Poul Hansen]
					case "operatorName":
						formField.ControlContent = GetOutpostValue("43", ref msgLines);
						break;
					//44: [11/02/2015{odate]
					case "operatorDate":
						string oDate = GetOutpostValue("44", ref msgLines);
						int index = oDate.IndexOf('{');
						formField.ControlContent = oDate.Substring(0, index);
						break;
					//45: [10:50{otime]
					case "operatorTime":
						string oTime = GetOutpostValue("45", ref msgLines);
						index = oTime.IndexOf('{');
						formField.ControlContent = oTime.Substring(0, index);
						break;
				}
			}
			return formFields;
		}

	}
}
