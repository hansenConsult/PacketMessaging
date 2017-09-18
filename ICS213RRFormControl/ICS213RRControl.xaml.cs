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

			ScanControls(PrintableArea);

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
					//6.: [OTHER]
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
					//9.: [ROUTINE]
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
                                outpostData.Add($"12: [true]");
                                break;
                        }
                        break;
                    //11: [1234]
                    case "replyBy":
						outpostData.Add($"11: [{formField.ControlContent}]");
						break;
					//13: [Incident Name]
					case "incidentName":
						outpostData.Add($"13: [{formField.ControlContent}]");
						break;
                    //14: [Message date]
                    case "msgDate":
                        outpostData.Add($"14: [{formField.ControlContent}]");
                        break;
                    //15: [1917]
                    case "msgTime":
                        outpostData.Add($"15: [{formField.ControlContent}]");
                        break;
                    //16: [\nRequested By]
                    case "requestedBy":
                        outpostData.Add($"16: [\\n{formField.ControlContent}]");
                        break;
                    //17: [\nPrepared by]
                    case "preparedBy":
                        outpostData.Add($"17: [\\n{formField.ControlContent}]");
                        break;
                    //18: [\nApproved by]
                    case "approvedBy":
                        outpostData.Add($"18: [\\n{formField.ControlContent}]");
                        break;
                    //19: [\nQty]
                    case "resourceInfoQuantity1":
                        outpostData.Add($"19: [\\n{formField.ControlContent}]");
                        break;
                    //20: [\nResource descr]
                    case "resourceInfoDetailedResourceDesc1":
                        outpostData.Add($"20: [\\n{formField.ControlContent}]");
                        break;
                    // 21: [\nArrival]
                    case "resourceInfoArrival1":
                        outpostData.Add($"21: [\\n{formField.ControlContent}]");
                        break;
                    // 22: [true]
                    case "resourceInfoPriority1":
                        switch (formField.ControlContent)
                        {
                            case "resourceInfoNow":
                                outpostData.Add($"22: [true]");
                                break;
                            case "resourceInfoHigh":
                                outpostData.Add($"23: [true]");
                                break;
                            case "resourceInfoMedium":
                                outpostData.Add($"24: [true]");
                                break;
                            case "resourceInfoLow":
                                outpostData.Add($"25: [true]");
                                break;
                        }
                        break;
                    // 26: [\nCost] 
                    case "resourceInfoCost1":
                        outpostData.Add($"26: [\\n{formField.ControlContent}]");
                        break;
                    // 27: [\nDelivery to] 
                    case "deliveryTo":
                        outpostData.Add($"27: [\\n{formField.ControlContent}]");
                        break;
                    // 28: [\nLocation] 
                    case "deliveryLocation":
                        outpostData.Add($"28: [\\n{formField.ControlContent}]");
                        break;
                    // 29: [\nSubstitutes] 
                    case "substitutes":
                        outpostData.Add($"29: [\\n{formField.ControlContent}]");
                        break;
                    // 30: [true] 
                    case "suppReqEquipmentOperator":
                        outpostData.Add($"30: [{formField.ControlContent}]");
                        break;
                    // 31: [true] 
                    case "suppReqLodinng":
                        outpostData.Add($"31: [{formField.ControlContent}]");
                        break;
                    // 32: [truw] 
                    case "suppReqFuel":
                        outpostData.Add($"32: [{formField.ControlContent}]");
                        break;
                    // 33: [Jet] 
                    case "suppReqFuelType":
                        outpostData.Add($"33: [{formField.ControlContent}]");
                        break;
                    // 34: [truw] 
                    case "suppReqPower":
                        outpostData.Add($"34: [{formField.ControlContent}]");
                        break;
                    // 35: [truw] 
                    case "suppReqMeals":
                        outpostData.Add($"35: [{formField.ControlContent}]");
                        break;
                    // 36: [truw] 
                    case "suppReqMaintenance":
                        outpostData.Add($"36: [{formField.ControlContent}]");
                        break;
                    // 37: [truw] 
                    case "suppReqWater":
                        outpostData.Add($"37: [{formField.ControlContent}]");
                        break;
                    // 38: [truw] 
                    case "suppReqOther":
                        outpostData.Add($"38: [{formField.ControlContent}]");
                        break;
                    // 39: [\nSpecial Instructions] 
                    case "specialInstructions":
                        outpostData.Add($"39: [\\n{formField.ControlContent}]");
                        break;
                    //40. Rec-Sent: [received]
                    case "receivedOrSent":
                        switch (formField.ControlContent)
                        {
                            case "received":
                                outpostData.Add($"40: [true]");
                                break;
                            case "sent":
                                outpostData.Add($"41: [true]");
                                break;
                        }
                        break;
					//Method: [Other]
					case "howRecevedSent":
                        switch (formField.ControlContent)
                        {
                            case "voice":
                                outpostData.Add($"42: [true]");
                                break;
                            case "packet":
                                outpostData.Add($"43: [true]");
                                break;
                        }
                        break;
					//OpCall: [KZ6DM]
					case "operatorCallsign":
						outpostData.Add($"44: [{formField.ControlContent}]");
						break;
					//OpName: [Poul Hansen]
					case "operatorName":
						outpostData.Add($"45: [{formField.ControlContent}]");
						break;
					//OpDate: [02/02/2015]
					case "operatorDate":
						outpostData.Add($"46: [{formField.ControlContent}" + "{odate]");
						break;
					//OpTime: [1920]
					case "operatorTime":
						outpostData.Add($"47: [{formField.ControlContent}" + "{otime]");
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
                "!PACF! " + packetMessage.Subject,
                "# JS:EOC Resource Request (which4)",
                "# JS-ver. PR-4.3-2.7, 08/14/17",
                "# FORMFILENAME: XSC_EOC-213RR_v1706.html"
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
                    //0: [sen-123]
                    case "senderMsgNo":
                        formField.ControlContent = GetOutpostValue("0", ref msgLines);
                        break;
                    //MsgNo: [6DM-150P]
                    case "messageNo":
                        formField.ControlContent = msgNumber;
                        break;
                    //Received msg no: [6DM-150P]
                    case "receiverMsgNo":
                        formField.ControlContent = GetOutpostValue("2", ref msgLines);
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
                        if (GetOutpostValue("10", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "replyYes";
                        }
                        else if (GetOutpostValue("12", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "replyNo";
                        }
                        break;
                    //11: [1234]
                    case "replyBy":
                        formField.ControlContent = GetOutpostValue("11", ref msgLines);
                        break;
                    //13: [Incident Name]
                    case "incidentName":
                        formField.ControlContent = GetOutpostValue("13", ref msgLines);
                        break;
                    //14: [Message date]
                    case "msgDate":
                        formField.ControlContent = GetOutpostValue("14", ref msgLines);
                        break;
                    //15: [1917]
                    case "msgTime":
                        formField.ControlContent = GetOutpostValue("15", ref msgLines);
                        break;
                    // 16: [\nRequested by]
                    case "requestedBy":
                        formField.ControlContent = GetOutpostValue("16", ref msgLines);
                        break;
                    // 17: [\nPrepared by]
                    case "preparedBy":
                        formField.ControlContent = GetOutpostValue("17", ref msgLines);
                        break;
                    // 18: [Approved by] 
                    case "approvedBy":
                        formField.ControlContent = GetOutpostValue("18", ref msgLines);
                        break;
                    // 19: [ResourceInfoQuantity1] 
                    case "resourceInfoQuantity1":
                        formField.ControlContent = GetOutpostValue("19", ref msgLines);
                        break;
                    // 20: [ResourceInfoDetailedResourceDesc1] 
                    case "resourceInfoDetailedResourceDesc1":
                        formField.ControlContent = GetOutpostValue("20", ref msgLines);
                        break;
                    // 28: [ResourceInfo Arrival1] 
                    case "resourceInfoArrival1":
                        formField.ControlContent = GetOutpostValue("28", ref msgLines);
                        break;
                    // 29: [ResourceInfo Request Priority1] 
                    case "resourceInfoPriority1":
                        if (GetOutpostValue("22", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "resourceInfoUrgent1";
                        }
                        else if (GetOutpostValue("23", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "resourceInfoRoutine1";
                        }
                        else if (GetOutpostValue("24", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "resourceInfoRoutine1";
                        }
                        else if (GetOutpostValue("25", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "resourceInfoLow1";
                        }
                        break;
                    // 26: [\nCost] 
                    case "resourceInfoCost1":
                        formField.ControlContent = GetOutpostValue("26", ref msgLines);
                        break;
                    // 27: [\nDelivery to] 
                    case "deliveryTo":
                        formField.ControlContent = GetOutpostValue("27", ref msgLines);
                        break;
                    // 28: [\nLocation] 
                    case "deliveryLocation":
                        formField.ControlContent = GetOutpostValue("28", ref msgLines);
                        break;
                    // 29: [\nSubstitutes] 
                    case "substitutes":
                        formField.ControlContent = GetOutpostValue("29", ref msgLines);
                        break;
                    // 30: [true] 
                    case "suppReqEquipmentOperator":
                        formField.ControlContent = GetOutpostValue("30", ref msgLines);
                        break;
                    // 31: [true] 
                    case "suppReqLodinng":
                        formField.ControlContent = GetOutpostValue("31", ref msgLines);
                        break;
                    // 32: [true] 
                    case "suppReqFuel":
                        formField.ControlContent = GetOutpostValue("32", ref msgLines);
                        break;
                    // 33: [Jet] 
                    case "suppReqFuelType":
                        formField.ControlContent = GetOutpostValue("33", ref msgLines);
                        break;
                    // 34: [true] 
                    case "suppReqPower":
                        formField.ControlContent = GetOutpostValue("34", ref msgLines);
                        break;
                    // 35: [true] 
                    case "suppReqMeals":
                        formField.ControlContent = GetOutpostValue("35", ref msgLines);
                        break;
                    // 36: [true] 
                    case "suppReqMaintenance":
                        formField.ControlContent = GetOutpostValue("36", ref msgLines);
                        break;
                    // 37: [true] 
                    case "suppReqWater":
                        formField.ControlContent = GetOutpostValue("37", ref msgLines);
                        break;
                    // 38: [true] 
                    case "suppReqOther":
                        formField.ControlContent = GetOutpostValue("38", ref msgLines);
                        break;
                    // 39: [\nSpecial Instructions] 
                    case "specialInstructions":
                        formField.ControlContent = GetOutpostValue("39", ref msgLines);
                        break;
                    //40. Rec-Sent: [received]
                    case "receivedOrSent":
                        if (GetOutpostValue("40", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "received";
                        }
                        else if (GetOutpostValue("41", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "sent";
                        }
                        break;
                    //Method: [Other]
                    case "howRecevedSent":
                        if (GetOutpostValue("42", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "voice";
                        }
                        else if (GetOutpostValue("43", ref msgLines)?.ToLower() == "true")
                        {
                            formField.ControlContent = "packet";
                        }
                        break;
                    //OpCall: [KZ6DM]
                    case "operatorCallsign":
                        formField.ControlContent = GetOutpostValue("44", ref msgLines);
                        break;
                    //OpName: [Poul Hansen]
                    case "operatorName":
                        formField.ControlContent = GetOutpostValue("45", ref msgLines);
                        break;
                    //OpDate: [02/02/2015]
                    case "operatorDate":
                        formField.ControlContent = GetOutpostValue("46", ref msgLines);
                        break;
                    //OpTime: [1920]
                    case "operatorTime":
                        formField.ControlContent = GetOutpostValue("47", ref msgLines);
                        break;
                }
            }
            return formFields;
		}

    }
}
