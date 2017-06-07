using System.Collections.Generic;
using Windows.UI.Xaml;
using FormControlBaseClass;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ICS213RRFormControl
{
	[FormControl(
		FormControlName = "Message",
		FormControlMenuName = "XSC ICS-213RR Message Form",
		FormControlType = FormControlAttribute.FormType.CountyForm)
	]

	public partial class ICS213RRControl : FormControlBase
	{
		public ICS213RRControl()
		{
			this.InitializeComponent();

			ScanControls(root);

			InitializeControls();

			//ReceivedOrSent = "sent";
			//HowRecevedSent = "otherRecvdType";
		}

		public string ReceiverMsgNo
		{ get { return GetTextBoxString(receiverMsgNo); } set { SetTextBoxString(receiverMsgNo, value); } }

		public override string MessageNo
		{ get { return GetTextBoxString(messageNo); }
			set { SetTextBoxString(messageNo, value); } }

		public string SenderMsgNo
		{ get { return GetTextBoxString(senderMsgNo); } set { SetTextBoxString(senderMsgNo, value); } }


		public string Severity
		{ get { return severity.GetRadioButtonCheckedState(); } set { severity.SetRadioButtonCheckedState(value); } }

		public override string HandlingOrder
		{ get { return handlingOrder.GetRadioButtonCheckedState(); } set { handlingOrder.SetRadioButtonCheckedState(value); } }

		public string Action
		{ get { return action.GetRadioButtonCheckedState(); } set { action.SetRadioButtonCheckedState(value); } }

		public string Reply
		{ get { return reply.GetRadioButtonCheckedState(); } set { reply.SetRadioButtonCheckedState(value); } }

		public string ReplyBy
		{ get { return GetTextBoxString(replyBy); } set { SetTextBoxString(replyBy, value); } }

        public string IncidentName
        { get => GetTextBoxString(incidentName); set => SetTextBoxString(incidentName, value); }
/*
		public string ReceivedOrSent
		{ get { return receivedOrSent.GetRadioButtonCheckedState(); } set { receivedOrSent.SetRadioButtonCheckedState(value); } }

		public string HowRecevedSent
		{ get { return howRecevedSent.GetRadioButtonCheckedState(); } set { howRecevedSent.SetRadioButtonCheckedState(value); } }

		public override string OperatorCallsign
		{ get { return GetTextBoxString(operatorCallsign); } set { SetTextBoxString(operatorCallsign, value); } }

		public override string OperatorName
		{ get { return GetTextBoxString(operatorName); } set { SetTextBoxString(operatorName, value); } }

		public override string OperatorDate
		{ get { return GetTextBoxString(operatorDate); } set { SetTextBoxString(operatorDate, value); } }

		public override string OperatorTime
		{ get { return GetTextBoxString(operatorTime); } set { SetTextBoxString(operatorTime, value); } }
*/
		public override string PacFormName => "Message";

		//public override string PacFormName => FormControl.FormControlName = "Message";

		public override string CreateSubject()
		{
			return (MessageNo + "_" + Severity?.ToUpper()[0] + "/" + HandlingOrder?.ToUpper()[0] + "_EOC213RR_" + IncidentName);
		}

		protected override List<string> CreateOutpostDataFromFormFields(ref PacketMessage packetMessage, ref List<string> outpostData)
		{
			foreach (FormField formField in packetMessage.FormFieldArray)
			{
				if (formField.ControlContent == null || formField.ControlContent.Length == 0)
					continue;

				switch (formField.ControlName)
				{
                    //0: [02/02/2015]
                    case "senderMsgNo":
                        outpostData.Add($"0: [{formField.ControlContent}]");
                        break;
                    //MsgNo: [6DM-150P]
                    case "messageNo":
						outpostData.Add($"1: [{formField.ControlContent}]");
						break;
					//3: [1917]
					case "receiverMsgNo":
						outpostData.Add($"2: [{formField.ControlContent}]");
						break;
					//4.: [OTHER]
					case "severity":
						outpostData.Add($"4.: [{formField.ControlContent.ToUpper()}]");
                        outpostData.Add($"5.: [{formField.ControlContent.ToUpper()}]");
                        outpostData.Add($"6.: [{formField.ControlContent.ToUpper()}]");
                        break;
					//5.: [ROUTINE]
					case "handlingOrder":
						outpostData.Add($"7.: [{formField.ControlContent.ToUpper()}]");
                        outpostData.Add($"8.: [{formField.ControlContent.ToUpper()}]");
                        outpostData.Add($"9.: [{formField.ControlContent.ToUpper()}]");
                        break;
					//10.: [true]
					case "reply":
						outpostData.Add($"10: [{(formField.ControlContent == "replyYes" ? "true" : "")}]");
                        outpostData.Add($"11: [{(formField.ControlContent == "replyYes" ? "" : "true")}]");
                        break;
                    //12: [1234]
                    case "replyBy":
						outpostData.Add($"12: [{formField.ControlContent}]");
						break;
					//13: [Incident Name]
					case "incidentName":
						outpostData.Add($"13: [{formField.ControlContent}]");
						break;
                    //14: [Incident Number]
                    case "incidentNumber":
                        outpostData.Add($"14: [{formField.ControlContent}]");
                        break;
                    //15: [Message date]
                    case "msgDate":
                        outpostData.Add($"15: [{formField.ControlContent}]");
                        break;
                    //16: [1917]
                    case "msgTime":
                        outpostData.Add($"16: [{formField.ControlContent}]");
                        break;
                    //17: [Operational preiod]
                    case "operationalPeriod":
                        outpostData.Add($"17: [{formField.ControlContent}]");
                        break;
                    //18: [County]
                    case "trackingNoCounty":
                        outpostData.Add($"18: [{formField.ControlContent}]");
                        break;
                    //19: [Operational preiod]
                    case "trackingNoState":
                        outpostData.Add($"19: [{formField.ControlContent}]");
                        break;
                    //20: [Operational preiod]
                    case "trackingNoFederal":
                        outpostData.Add($"20: [{formField.ControlContent}]");
                        break;
                    // 21: Prepared by Name
                    case "preparedByName":
                        outpostData.Add($"21: [{formField.ControlContent}]");
                        break;
                    // 22: Prepared by Position
                    case "preparedByPosition":
                        outpostData.Add($"22: [{formField.ControlContent}]");
                        break;
                    // 23: [Approval Authority Name] 
                    case "approvalAuthorityName":
                        outpostData.Add($"23: [{formField.ControlContent}]");
                        break;
                    // 24: [Approval Authority Position] 
                    case "approvalAuthorityPosition":
                        outpostData.Add($"24: [{formField.ControlContent}]");
                        break;
                    // 25: [Approval Authority Signature] 
                    case "approvalAuthoritySignature":
                        outpostData.Add($"25: [{formField.ControlContent}]");
                        break;
                    // 26: [ResourceInfoQuantity1] 
                    case "resourceInfoQuantity1":
                        outpostData.Add($"26: [{formField.ControlContent}]");
                        break;
                    // 27: [ResourceInfoDetailedResourceDesc1] 
                    case "resourceInfoDetailedResourceDesc1":
                        outpostData.Add($"27: [{formField.ControlContent}]");
                        break;
                    // 28: [ResourceInfo Arrival1] 
                    case "resourceInfoArrival1":
                        outpostData.Add($"28: [{formField.ControlContent}]");
                        break;
                    // 29: [ResourceInfo Request Priority1] 
                    case "resourceInfoPriority1":
                        outpostData.Add($"29: [{formField.ControlContent}]");
                        outpostData.Add($"30: [{formField.ControlContent}]");
                        outpostData.Add($"31: [{formField.ControlContent}]");
                        break;
                    // 32: [ResourceInfo Cost1] 
                    case "resourceInfoCost1":
                        outpostData.Add($"32: [{formField.ControlContent}]");
                        break;
                    // 26: [ResourceInfoQuantity1] 
                    case "resourceInfoQuantity2":
                        outpostData.Add($"33: [{formField.ControlContent}]");
                        break;
                    // 34: [ResourceInfoDetailedResourceDesc1] 
                    case "resourceInfoDetailedResourceDes2":
                        outpostData.Add($"34: [{formField.ControlContent}]");
                        break;
                    // 28: [ResourceInfo Arrival1] 
                    case "resourceInfoArrival2":
                        outpostData.Add($"35: [{formField.ControlContent}]");
                        break;
                    // 29: [ResourceInfo Request Priority1] 
                    case "resourceInfoPriority2":
                        outpostData.Add($"36: [{formField.ControlContent}]");
                        outpostData.Add($"37: [{formField.ControlContent}]");
                        outpostData.Add($"38: [{formField.ControlContent}]");
                        break;
                    // 32: [ResourceInfo Cost1] 
                    case "resourceInfoCost2":
                        outpostData.Add($"39: [{formField.ControlContent}]");
                        break;
                    // 26: [ResourceInfoQuantity1] 
                    case "resourceInfoQuantity3":
                        outpostData.Add($"40: [{formField.ControlContent}]");
                        break;
                    // 34: [ResourceInfoDetailedResourceDesc1] 
                    case "resourceInfoDetailedResourceDes3":
                        outpostData.Add($"41: [{formField.ControlContent}]");
                        break;
                    // 28: [ResourceInfo Arrival1] 
                    case "resourceInfoArrival3":
                        outpostData.Add($"42: [{formField.ControlContent}]");
                        break;
                    // 29: [ResourceInfo Request Priority1] 
                    case "resourceInfoPriority3":
                        outpostData.Add($"43: [{formField.ControlContent}]");
                        outpostData.Add($"44: [{formField.ControlContent}]");
                        outpostData.Add($"45: [{formField.ControlContent}]");
                        break;
                    // 32: [ResourceInfo Cost1] 
                    case "resourceInfoCost3":
                        outpostData.Add($"46: [{formField.ControlContent}]");
                        break;
                    // 26: [ResourceInfoQuantity1] 
                    case "resourceInfoQuantity4":
                        outpostData.Add($"47: [{formField.ControlContent}]");
                        break;
                    // 34: [ResourceInfoDetailedResourceDesc1] 
                    case "resourceInfoDetailedResourceDes4":
                        outpostData.Add($"48: [{formField.ControlContent}]");
                        break;
                    // 28: [ResourceInfo Arrival1] 
                    case "resourceInfoArrival4":
                        outpostData.Add($"49: [{formField.ControlContent}]");
                        break;
                    // 29: [ResourceInfo Request Priority1] 
                    case "resourceInfoPriority4":
                        outpostData.Add($"50: [{formField.ControlContent}]");
                        outpostData.Add($"51: [{formField.ControlContent}]");
                        outpostData.Add($"52: [{formField.ControlContent}]");
                        break;
                    // 32: [ResourceInfo Cost1] 
                    case "resourceInfoCost4":
                        outpostData.Add($"53: [{formField.ControlContent}]");
                        break;
                    // 54: [Requested Delivery Location] 
                    case "deliveryLocation":
                        outpostData.Add($"54: [{formField.ControlContent}]");
                        break;
                    // 55: [ResourceInfo Cost1] 
                    case "substitute":
                        outpostData.Add($"55: [{formField.ControlContent}]");
                        break;
                    // 56: [Requested by EOC Pos] 
                    case "requestedByEOCPos":
                        outpostData.Add($"56: [{formField.ControlContent}]");
                        break;
                    // 57: [EOC Section Chief] 
                    case "sectionChief":
                        outpostData.Add($"57: [{formField.ControlContent}]");
                        break;
                    // 58: [Section Chief Signature] 
                    case "sectionChiefSignature":
                        outpostData.Add($"53: [{formField.ControlContent}]");
                        break;
                    //59. Rec-Sent: [received]
                    case "receivedOrSent":
						outpostData.Add($"59: [{formField.ControlContent}]");
                        outpostData.Add($"60: [{formField.ControlContent}]");
                        break;
					//Method: [Other]
					case "howRecevedSent":
                        outpostData.Add($"61: [{formField.ControlContent}]");
                        outpostData.Add($"62: [{formField.ControlContent}]");
                        break;
					//OpCall: [KZ6DM]
					case "operatorCallsign":
						outpostData.Add($"63: [{formField.ControlContent}]");
						break;
					//OpName: [Poul Hansen]
					case "operatorName":
						outpostData.Add($"64: [{formField.ControlContent}]");
						break;
					//OpDate: [02/02/2015]
					case "operatorDate":
						//formField.ControlContent = OperatorDate;
						outpostData.Add($"65: [{formField.ControlContent}" + "{odate]");
						break;
					//OpTime: [1920]
					case "operatorTime":
						//formField.ControlContent = OperatorTime;
						outpostData.Add($"66: [{formField.ControlContent}" + "{otime]");
						break;
				}
			}
			outpostData.Add("#EOF");

			return outpostData;
		}

		public override string CreateOutpostData(ref PacketMessage packetMessage)
		{
			List<string> outpostData = new List<string>();

			outpostData.Add("!PACF! " + packetMessage.Subject);
			outpostData.Add("# JS:EOC Resource Request (which4)");
			outpostData.Add("# JS-ver. PR-4.1-1.11, 05/23/17");
			outpostData.Add("# FORMFILENAME: EOCResourceRequest.html");

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
					case "senderMsgNo":
						formField.ControlContent = GetOutpostValue("MsgNo", ref msgLines);
						break;
					case "messagegNo":
						formField.ControlContent = msgNumber;
						break;
					case "msgDate":
						formField.ControlContent = GetOutpostValue("1a.", ref msgLines);
						break;
					case "msgTime":
						formField.ControlContent = GetOutpostValue("1b.", ref msgLines);
						break;
					case "severity":
						formField.ControlContent = GetOutpostValue("4.", ref msgLines).ToLower();
						break;
					case "handlingOrder":
						formField.ControlContent = GetOutpostValue("5.", ref msgLines).ToLower();
						break;
					case "reply":
						radioButtonContent = GetOutpostValue("6b.", ref msgLines);
						if (radioButtonContent?.Length > 0)
						{
							formField.ControlContent = (radioButtonContent == "yes" ? "replyYes" : "replyNo");
						}
						break;
					case "replyBy":
						formField.ControlContent = GetOutpostValue("6d.", ref msgLines);
						break;
					case "receivedOrSent":
						formField.ControlContent = GetOutpostValue("Rec-Sent", ref msgLines).ToLower();
						break;
					case "howRecevedSent":
						radioButtonContent = GetOutpostValue("Method", ref msgLines);
						if (radioButtonContent?.Length > 0)
						{
							if (radioButtonContent == "EOC Radio")
							{
								formField.ControlContent = "eOCRadio";
							}
							else if (radioButtonContent == "Other")
							{
								formField.ControlContent = "other";
							}
							else
							{
								formField.ControlContent = radioButtonContent;
							}
						}
						break;
					case "otherText":
						formField.ControlContent = GetOutpostValue("Other", ref msgLines);
						break;
					case "operatorCallsign":
						formField.ControlContent = GetOutpostValue("OpCall", ref msgLines);
						break;
					case "operatorName":
						formField.ControlContent = GetOutpostValue("OpName", ref msgLines);
						break;
					case "operatorDate":
						formField.ControlContent = GetOutpostValue("OpDate", ref msgLines);
						break;
					case "operatorTime":
						formField.ControlContent = GetOutpostValue("OpTime", ref msgLines);
						break;
				}
			}
			return formFields;
		}


    }
}
