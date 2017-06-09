using System.Collections.Generic;
using Windows.UI.Xaml;
using FormControlBaseClass;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace EOC213RRFormControl
{
	[FormControl(
		FormControlName = "EOCResourceRequest",
		FormControlMenuName = "XSC EOC Resource Request Form",
		FormControlType = FormControlAttribute.FormType.CountyForm)
	]

	public partial class EOC213RRControl : FormControlBase
	{
		public EOC213RRControl()
		{
			this.InitializeComponent();

			ScanControls(root);

			InitializeControls();

            ReceivedOrSent = "sent";
            HowRecevedSent = "packet";
        }

		public string ReceiverMsgNo
		{ get { return GetTextBoxString(receiverMsgNo); } set { SetTextBoxString(receiverMsgNo, value); } }

		public override string MessageNo
		{ get { return GetTextBoxString(messageNo); } set { SetTextBoxString(messageNo, value); } }

		public string SenderMsgNo
		{ get { return GetTextBoxString(senderMsgNo); } set { SetTextBoxString(senderMsgNo, value); } }

		public string Severity
		{ get { return severity.GetRadioButtonCheckedState(); } set { severity.SetRadioButtonCheckedState(value); } }

		public override string HandlingOrder
		{ get { return handlingOrder.GetRadioButtonCheckedState(); } set { handlingOrder.SetRadioButtonCheckedState(value); } }

		public string Reply
		{ get { return reply.GetRadioButtonCheckedState(); } set { reply.SetRadioButtonCheckedState(value); } }

		public string ReplyBy
		{ get { return GetTextBoxString(replyBy); } set { SetTextBoxString(replyBy, value); } }

        public string IncidentName
        { get => GetTextBoxString(incidentName); set => SetTextBoxString(incidentName, value); }

        public override string MsgDate
        { get => GetTextBoxString(msgDate); set => SetTextBoxString(msgDate, value); }

        public override string MsgTime
        { get => GetTextBoxString(msgTime); set => SetTextBoxString(msgTime, value); }

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

		public override string PacFormName => "EOCResourceRequest";

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
					//5.: [ROUTINE]
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
					//10.: [true]
					case "reply":
                        switch (formField.ControlContent)
                        {
                            case "yes":
                                outpostData.Add($"10: [true]");
                                break;
                            case "no":
                                outpostData.Add($"11: [true]");
                                break;
                        }
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
                        switch (formField.ControlContent)
                        {
                            case "resourceInfoUrgent1":
                                outpostData.Add($"29: [true]");
                                break;
                            case "resourceInfoRoutine1":
                                outpostData.Add($"30: [true]");
                                break;
                            case "resourceInfoLow1":
                                outpostData.Add($"31: [true]");
                                break;
                        }
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
                        switch (formField.ControlContent)
                        {
                            case "resourceInfoUrgent2":
                                outpostData.Add($"36: [true]");
                                break;
                            case "resourceInfoRoutine2":
                                outpostData.Add($"37: [true]");
                                break;
                            case "resourceInfoLow2":
                                outpostData.Add($"38: [true]");
                                break;
                        }
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
                        switch (formField.ControlContent)
                        {
                            case "resourceInfoUrgent3":
                                outpostData.Add($"43: [true]");
                                break;
                            case "resourceInfoRoutine3":
                                outpostData.Add($"44: [true]");
                                break;
                            case "resourceInfoLow3":
                                outpostData.Add($"45: [true]");
                                break;
                        }
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
                        switch (formField.ControlContent)
                        {
                            case "resourceInfoUrgent4":
                                outpostData.Add($"50: [true]");
                                break;
                            case "resourceInfoRoutine4":
                                outpostData.Add($"51: [true]");
                                break;
                            case "resourceInfoLow4":
                                outpostData.Add($"52: [true]");
                                break;
                        }
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
                        outpostData.Add($"58: [{formField.ControlContent}]");
                        break;
                    //59. Rec-Sent: [received]
                    case "receivedOrSent":
                        switch (formField.ControlContent)
                        {
                            case "received":
                                outpostData.Add($"59: [true]");
                                break;
                            case "sent":
                                outpostData.Add($"60: [true]");
                                break;
                        }
                        break;
					//Method: [Other]
					case "howRecevedSent":
                        switch (formField.ControlContent)
                        {
                            case "voice":
                                outpostData.Add($"61: [true]");
                                break;
                            case "packet":
                                outpostData.Add($"62: [true]");
                                break;
                        }
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
                    //0: [02/02/2015]
                    case "senderMsgNo":
                        formField.ControlContent = GetOutpostValue("0", ref msgLines);
                        break;
                    //MsgNo: [6DM-150P]
                    case "messageNo":
                        formField.ControlContent = msgNumber;
                        break;
                    //4.: [OTHER]
                    case "severity":
                        if ((radioButtonContent = GetOutpostValue("4", ref msgLines)?.ToLower()) == "true")
                        {
                            formField.ControlContent = "emergency";
                        }
                        else if (GetOutpostValue("5", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "urgent";
                        }
                        else if (GetOutpostValue("6", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "other";
                        }
                        break;
                    //5.: [ROUTINE]
                    case "handlingOrder":
                        if (GetOutpostValue("7", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "immediate";
                        }
                        else if (GetOutpostValue("8", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "priority";
                        }
                        else if (GetOutpostValue("9", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "routine";
                        }
                        break;
                    //10.: [true]
                    case "reply":
                        //string s = GetOutpostValue("10", ref msgLines)?.ToLower();
                        //string s2 = GetOutpostValue("11", ref msgLines)?.ToLower();
                        if (GetOutpostValue("10", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "replyYes";
                        }
                        else if (GetOutpostValue("11", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "replyNo";
                        }
                        break;
                    //12: [1234]
                    case "replyBy":
                        formField.ControlContent = GetOutpostValue("12", ref msgLines);
                        break;
                    //13: [Incident Name]
                    case "incidentName":
                        formField.ControlContent = GetOutpostValue("13", ref msgLines);
                        break;
                    //14: [Incident Number]
                    case "incidentNumber":
                        formField.ControlContent = GetOutpostValue("14", ref msgLines);
                        break;
                    //15: [Message date]
                    case "msgDate":
                        formField.ControlContent = GetOutpostValue("15", ref msgLines);
                        break;
                    //16: [1917]
                    case "msgTime":
                        formField.ControlContent = GetOutpostValue("16", ref msgLines);
                        break;
                    //17: [Operational preiod]
                    case "operationalPeriod":
                        formField.ControlContent = GetOutpostValue("17", ref msgLines);
                        break;
                    //18: [County]
                    case "trackingNoCounty":
                        formField.ControlContent = GetOutpostValue("18", ref msgLines);
                        break;
                    //19: [Operational preiod]
                    case "trackingNoState":
                        formField.ControlContent = GetOutpostValue("19", ref msgLines);
                        break;
                    //20: [Operational preiod]
                    case "trackingNoFederal":
                        formField.ControlContent = GetOutpostValue("20", ref msgLines);
                        break;
                    // 21: Prepared by Name
                    case "preparedByName":
                        formField.ControlContent = GetOutpostValue("21", ref msgLines);
                        break;
                    // 22: Prepared by Position
                    case "preparedByPosition":
                        formField.ControlContent = GetOutpostValue("22", ref msgLines);
                        break;
                    // 23: [Approval Authority Name] 
                    case "approvalAuthorityName":
                        formField.ControlContent = GetOutpostValue("23", ref msgLines);
                        break;
                    // 24: [Approval Authority Position] 
                    case "approvalAuthorityPosition":
                        formField.ControlContent = GetOutpostValue("24", ref msgLines);
                        break;
                    // 25: [Approval Authority Signature] 
                    case "approvalAuthoritySignature":
                        formField.ControlContent = GetOutpostValue("25", ref msgLines);
                        break;
                    // 26: [ResourceInfoQuantity1] 
                    case "resourceInfoQuantity1":
                        formField.ControlContent = GetOutpostValue("26", ref msgLines);
                        break;
                    // 27: [ResourceInfoDetailedResourceDesc1] 
                    case "resourceInfoDetailedResourceDesc1":
                        formField.ControlContent = GetOutpostValue("27", ref msgLines);
                        break;
                    // 28: [ResourceInfo Arrival1] 
                    case "resourceInfoArrival1":
                        formField.ControlContent = GetOutpostValue("28", ref msgLines);
                        break;
                    // 29: [ResourceInfo Request Priority1] 
                    case "resourceInfoPriority1":
                        if (GetOutpostValue("29", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "resourceInfoUrgent1";
                        }
                        else if (GetOutpostValue("30", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "resourceInfoRoutine1";
                        }
                        else if (GetOutpostValue("31", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "resourceInfoLow1";
                        }
                        break;
                    // 32: [ResourceInfo Cost1] 
                    case "resourceInfoCost1":
                        formField.ControlContent = GetOutpostValue("32", ref msgLines);
                        break;
                    // 26: [ResourceInfoQuantity1] 
                    case "resourceInfoQuantity2":
                        formField.ControlContent = GetOutpostValue("33", ref msgLines);
                        break;
                    // 34: [ResourceInfoDetailedResourceDesc1] 
                    case "resourceInfoDetailedResourceDes2":
                        formField.ControlContent = GetOutpostValue("34", ref msgLines);
                        break;
                    // 28: [ResourceInfo Arrival1] 
                    case "resourceInfoArrival2":
                        formField.ControlContent = GetOutpostValue("35", ref msgLines);
                        break;
                    // 29: [ResourceInfo Request Priority1] 
                    case "resourceInfoPriority2":
                        if (GetOutpostValue("36", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "resourceInfoUrgent2";
                        }
                        else if (GetOutpostValue("37", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "resourceInfoRoutine2";
                        }
                        else if (GetOutpostValue("38", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "resourceInfoLow2";
                        }
                        break;
                    // 32: [ResourceInfo Cost1] 
                    case "resourceInfoCost2":
                        formField.ControlContent = GetOutpostValue("39", ref msgLines);
                        break;
                    // 26: [ResourceInfoQuantity1] 
                    case "resourceInfoQuantity3":
                        formField.ControlContent = GetOutpostValue("40", ref msgLines);
                        break;
                    // 34: [ResourceInfoDetailedResourceDesc1] 
                    case "resourceInfoDetailedResourceDes3":
                        formField.ControlContent = GetOutpostValue("41", ref msgLines);
                        break;
                    // 28: [ResourceInfo Arrival1] 
                    case "resourceInfoArrival3":
                        formField.ControlContent = GetOutpostValue("42", ref msgLines);
                        break;
                    // 29: [ResourceInfo Request Priority1] 
                    case "resourceInfoPriority3":
                        if (GetOutpostValue("43", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "resourceInfoUrgent3";
                        }
                        else if (GetOutpostValue("44", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "resourceInfoRoutine3";
                        }
                        else if (GetOutpostValue("45", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "resourceInfoLow3";
                        }
                        break;
                    // 32: [ResourceInfo Cost1] 
                    case "resourceInfoCost3":
                        formField.ControlContent = GetOutpostValue("46", ref msgLines);
                        break;
                    // 26: [ResourceInfoQuantity1] 
                    case "resourceInfoQuantity4":
                        formField.ControlContent = GetOutpostValue("47", ref msgLines);
                        break;
                    // 34: [ResourceInfoDetailedResourceDesc1] 
                    case "resourceInfoDetailedResourceDes4":
                        formField.ControlContent = GetOutpostValue("48", ref msgLines);
                        break;
                    // 28: [ResourceInfo Arrival1] 
                    case "resourceInfoArrival4":
                        formField.ControlContent = GetOutpostValue("49", ref msgLines);
                        break;
                    // 29: [ResourceInfo Request Priority1] 
                    case "resourceInfoPriority4":
                        if (GetOutpostValue("50", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "resourceInfoUrgent4";
                        }
                        else if (GetOutpostValue("51", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "resourceInfoRoutine4";
                        }
                        else if (GetOutpostValue("52", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "resourceInfoLow4";
                        }
                        break;
                    // 32: [ResourceInfo Cost1] 
                    case "resourceInfoCost4":
                        formField.ControlContent = GetOutpostValue("53", ref msgLines);
                        break;
                    // 54: [Requested Delivery Location] 
                    case "deliveryLocation":
                        formField.ControlContent = GetOutpostValue("54", ref msgLines);
                        break;
                    // 55: [ResourceInfo Cost1] 
                    case "substitute":
                        formField.ControlContent = GetOutpostValue("55", ref msgLines);
                        break;
                    // 56: [Requested by EOC Pos] 
                    case "requestedByEOCPos":
                        formField.ControlContent = GetOutpostValue("56", ref msgLines);
                        break;
                    // 57: [EOC Section Chief] 
                    case "sectionChief":
                        formField.ControlContent = GetOutpostValue("57", ref msgLines);
                        break;
                    // 58: [Section Chief Signature] 
                    case "sectionChiefSignature":
                        formField.ControlContent = GetOutpostValue("58", ref msgLines);
                        break;
                    //59. Rec-Sent: [received]
                    case "receivedOrSent":
                        if (GetOutpostValue("59", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "received";
                        }
                        else if (GetOutpostValue("60", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "sent";
                        }
                        break;
                    //Method: [Other]
                    case "howRecevedSent":
                        if (GetOutpostValue("61", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "voice";
                        }
                        else if (GetOutpostValue("62", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "packet";
                        }
                        break;
                    //OpCall: [KZ6DM]
                    case "operatorCallsign":
                        formField.ControlContent = GetOutpostValue("63", ref msgLines);
                        break;
                    //OpName: [Poul Hansen]
                    case "operatorName":
                        formField.ControlContent = GetOutpostValue("64", ref msgLines);
                        break;
                    //OpDate: [02/02/2015]
                    case "operatorDate":
                        //formField.ControlContent = OperatorDate;
                        formField.ControlContent = GetOutpostValue("65", ref msgLines);
                        break;
                    //OpTime: [1920]
                    case "operatorTime":
                        //formField.ControlContent = OperatorTime;
                        formField.ControlContent = GetOutpostValue("66", ref msgLines);
                        break;
                }
            }
            return formFields;
		}


    }
}
