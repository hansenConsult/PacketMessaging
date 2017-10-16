using FormControlBaseClass;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace OAMunicipalStatusFormControl
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 

    [FormControl(
        FormControlName = "OAMuniStatus",
        FormControlMenuName = "OA Municipal Status",
        FormControlType = FormControlAttribute.FormType.CountyForm)
    ]

    //public class ComboBoxItemConvert : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, string language)
    //    {
    //        return value;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, string language)
    //    {
    //        return value as ComboBoxItem;
    //    }
    //}

    public sealed partial class OAMunicipalStatusControl : FormControlBase
    {
        string[] Municipalities = new string[] {
                "SELECT the Municipality",
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
                "Unincorporated"
        };

        string[] UnknownYesNo = new string[] {
                "Unknown",
                "Yes",
                "No"
        };

        string[] ActivationLevel = new string[] {
                "Unknown",
                "Monitor",
                "Minimal",
                "Full"
        };

        string[] CurrentSituation = new string[] {
                "Unknown",
                "Normal",
                "Problem",
                "Failure"
        };

        public OAMunicipalStatusControl()
        {
            this.InitializeComponent();

            ScanControls(PrintableArea);

            InitializeControls();

            ReceivedOrSent = "sent";
            HowReceivedSent = "packet";

            municipalityName.ItemsSource = Municipalities;
            municipalityName.SelectedIndex = 0;

            officeStatus.ItemsSource = UnknownYesNo;
            officeStatus.SelectedIndex = 0;

            eocOpen.ItemsSource = UnknownYesNo;
            eocOpen.SelectedIndex = 0;
            activationLevel.ItemsSource = ActivationLevel;
            activationLevel.SelectedIndex = 0;
            stateOfEmergency.ItemsSource = UnknownYesNo;
            stateOfEmergency.SelectedIndex = 0;

            comboBoxCommunications.ItemsSource = CurrentSituation;
            comboBoxCommunications.SelectedIndex = 0;
            comboBoxDebris.ItemsSource = CurrentSituation;
            comboBoxDebris.SelectedIndex = 0;
            comboBoxFlooding.ItemsSource = CurrentSituation;
            comboBoxFlooding.SelectedIndex = 0;
            comboBoxHazmat.ItemsSource = CurrentSituation;
            comboBoxHazmat.SelectedIndex = 0;
            comboBoxEmergencyServices.ItemsSource = CurrentSituation;
            comboBoxEmergencyServices.SelectedIndex = 0;
            comboBoxCasualties.ItemsSource = CurrentSituation;
            comboBoxCasualties.SelectedIndex = 0;
            comboBoxUtilitiesGas.ItemsSource = CurrentSituation;
            comboBoxUtilitiesGas.SelectedIndex = 0;
            comboBoxUtilitiesElectric.ItemsSource = CurrentSituation;
            comboBoxUtilitiesElectric.SelectedIndex = 0;
            comboBoxInfrastructurePower.ItemsSource = CurrentSituation;
            comboBoxInfrastructurePower.SelectedIndex = 0;
            comboBoxInfrastructureWater.ItemsSource = CurrentSituation;
            comboBoxInfrastructureWater.SelectedIndex = 0;
            comboBoxInfrastructureSewer.ItemsSource = CurrentSituation;
            comboBoxInfrastructureSewer.SelectedIndex = 0;
            comboBoxSearchAndRescue.ItemsSource = CurrentSituation;
            comboBoxSearchAndRescue.SelectedIndex = 0;
            comboBoxTransportationsRoads.ItemsSource = CurrentSituation;
            comboBoxTransportationsRoads.SelectedIndex = 0;
            comboBoxTransportationsBridges.ItemsSource = CurrentSituation;
            comboBoxTransportationsBridges.SelectedIndex = 0;
            comboBoxCivilUnrest.ItemsSource = CurrentSituation;
            comboBoxCivilUnrest.SelectedIndex = 0;
            comboBoxAnimalIssues.ItemsSource = CurrentSituation;
            comboBoxAnimalIssues.SelectedIndex = 0;
        }

        //public string SenderMsgNo
        //{ get { return GetTextBoxString(senderMsgNo); } set { SetTextBoxString(senderMsgNo, value); } }

        //public override string MessageNo
        //{ get => GetTextBoxString(messageNo); set => SetTextBoxString(messageNo, value); }

        //public string ReceiverMsgNo
        //{ get { return GetTextBoxString(receiverMsgNo); } set { SetTextBoxString(receiverMsgNo, value); } }

        //public override string Severity
        //{ get { return severity.GetRadioButtonCheckedState(); } set { severity.SetRadioButtonCheckedState(value); } }

        //public override string HandlingOrder
        //{ get { return handlingOrder.GetRadioButtonCheckedState(); } set { handlingOrder.SetRadioButtonCheckedState(value); } }

        //public override string MsgDate
        //{ get => GetTextBoxString(msgDate); set => SetTextBoxString(msgDate, value); }

        //public override string MsgTime
        //{ get => GetTextBoxString(msgTime); set => SetTextBoxString(msgTime, value); }

        //public string ReceivedOrSent
        //{ get { return receivedOrSent.GetRadioButtonCheckedState(); } set { receivedOrSent.SetRadioButtonCheckedState(value); } }

        //public override string HowReceivedSent
        //{ get { return howRecevedSent.GetRadioButtonCheckedState(); } set { howRecevedSent.SetRadioButtonCheckedState(value); } }

        //public override string OperatorCallsign
        //{ get { return GetTextBoxString(operatorCallsign); } set { SetTextBoxString(operatorCallsign, value); } }

        //public override string OperatorDate
        //{ get { return GetTextBoxString(operatorDate); } set { SetTextBoxString(operatorDate, value); } }

        //public override string OperatorName
        //{ get { return GetTextBoxString(operatorName); } set { SetTextBoxString(operatorName, value); } }

        //public override string OperatorTime
        //{ get { return GetTextBoxString(operatorTime); } set { SetTextBoxString(operatorTime, value); } }

        public string IncidentName
        { get => GetTextBoxString(incidentName); }

        public string MunicipalityName
        { get => (municipalityName.SelectedIndex != 0 ? "" : municipalityName.SelectedItem as string); }

        public override string PacFormName => "XSC_OA_MuniStatus";

        public override string PacFormType => "OAMuniStatus";

        public override string CreateSubject()
        {
            return (MessageNo + '_' + Severity?.ToUpper()[0] + '/' + HandlingOrder?.ToUpper()[0] + "_OAMuniStat_" + MunicipalityName + '_' + IncidentName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="formControl"></param>
        /// <param name="validationState">true - invalid, false - Valid </param>
        void UpdateControlValidationInfo(FormControl formControl, bool validationState)
        {
            if (validationState)
            {
                AddToErrorString(GetTagErrorMessage(formControl.InputControl));
                formControl.InputControl.BorderBrush = _redBrush;
            }
            else
            {
                formControl.InputControl.BorderBrush = formControl.BaseBorderColor;
            }
        }

        public override string ValidateForm()
        {
            base.ValidateForm();

            foreach (FormControl formControl in FormControlsList)
            {
                bool validationState;
                Control control = formControl.InputControl;
                switch (control.Name)
                {
                    case "municipalityName":
                        validationState = (control as ComboBox).SelectedIndex == 0;
                        UpdateControlValidationInfo(formControl, validationState);
                        break;
                    case "eocOpen":
                        validationState = (control as ComboBox).SelectedIndex == 0;
                        UpdateControlValidationInfo(formControl, validationState);
                        break;
                    case "activationLevel":
                        validationState = control.IsEnabled && (control as ComboBox).SelectedIndex == 0;
                        UpdateControlValidationInfo(formControl, validationState);
                        break;
                    case "stateOfEmergency":
                        validationState = (control as ComboBox).SelectedIndex == 0;
                        UpdateControlValidationInfo(formControl, validationState);
                        break;
                    case "howSent":
                        validationState = stateOfEmergency.SelectedIndex == 1 && string.IsNullOrEmpty((control as TextBox).Text);
                        UpdateControlValidationInfo(formControl, validationState);
                        break;
                    case "commentsCommunications":
                        validationState = comboBoxCommunications.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text);
                        UpdateControlValidationInfo(formControl, validationState);
                        break;
                    case "commentsDebris":
                        validationState = comboBoxDebris.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text);
                        UpdateControlValidationInfo(formControl, validationState);
                        break;
                    case "commentsFlooding":
                        validationState = comboBoxFlooding.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text);
                        UpdateControlValidationInfo(formControl, validationState);
                        break;
                    case "commentsHazmat":
                        validationState = comboBoxHazmat.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text);
                        UpdateControlValidationInfo(formControl, validationState);
                        break;
                    case "commentsEmergencyServices":
                        validationState = comboBoxEmergencyServices.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text);
                        UpdateControlValidationInfo(formControl, validationState);
                        break;
                    case "commentsCasualties":
                        validationState = comboBoxCasualties.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text);
                        UpdateControlValidationInfo(formControl, validationState);
                        break;
                    case "commentsUtilitiesGas":
                        validationState = comboBoxUtilitiesGas.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text);
                        UpdateControlValidationInfo(formControl, validationState);
                        break;
                    case "commentsUtilitiesElectric":
                        validationState = comboBoxUtilitiesElectric.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text);
                        UpdateControlValidationInfo(formControl, validationState);
                        break;
                    case "commentsInfrastructurePower":
                        validationState = comboBoxInfrastructurePower.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text);
                        UpdateControlValidationInfo(formControl, validationState);
                        break;
                    case "commentsInfrastructureWater":
                        validationState = comboBoxInfrastructureWater.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text);
                        UpdateControlValidationInfo(formControl, validationState);
                        break;
                    case "commentsInfrastructureSewer":
                        validationState = comboBoxInfrastructureSewer.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text);
                        UpdateControlValidationInfo(formControl, validationState);
                        break;
                    case "commentsSearchAndRescue":
                        validationState = comboBoxSearchAndRescue.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text);
                        UpdateControlValidationInfo(formControl, validationState);
                        break;
                    case "commentsTransportationsRoads":
                        validationState = comboBoxTransportationsRoads.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text);
                        UpdateControlValidationInfo(formControl, validationState);
                        break;
                    case "commentsTransportationsBridges":
                        validationState = comboBoxTransportationsBridges.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text);
                        UpdateControlValidationInfo(formControl, validationState);
                        break;
                    case "commentsCivilUnrest":
                        validationState = comboBoxCivilUnrest.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text);
                        UpdateControlValidationInfo(formControl, validationState);
                        break;
                    case "commentsAnimalIssues":
                        validationState = comboBoxAnimalIssues.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text);
                        UpdateControlValidationInfo(formControl, validationState);
                        break;
                }
            }
            return ValidationResultMessage;
        }

        //protected override List<string> CreateOutpostDataFromFormFields(ref PacketMessage packetMessage, ref List<string> outpostData)
        //{
        //    foreach (FormField formField in packetMessage.FormFieldArray)
        //    {
        //        if (string.IsNullOrEmpty(formField.ControlContent))
        //            continue;

        //        string data = CreateOutpostDataString(formField);
        //        if (string.IsNullOrEmpty(data))
        //        {
        //            continue;
        //        }
        //        outpostData.Add(data);
        //    }
        //    outpostData.Add("#EOF");
        //    return outpostData;
        //}

        public override string CreateOutpostData(ref PacketMessage packetMessage)
        {
            outpostData = new List<string>
            {
                "!PACF! " + packetMessage.Subject,
                "# JS:SC-OA-Muni Status (which4)",
                "# JS-ver. PR-4.3-1.8, 09/29/17",
                "# FORMFILENAME: XSC_OA_MuniStatus_v1708.html"
            };
            outpostData = CreateOutpostDataFromFormFields(ref packetMessage, ref outpostData);

            return CreateOutpostMessageBody(outpostData);
        }

        //public override FormField[] ConvertFromOutpost(string msgNumber, ref string[] msgLines)
        //{
        //    FormField[] formFields = CreateEmptyFormFieldsArray();

        //    foreach (FormField formField in formFields)
        //    {
        //        (string id, Control control) = GetTagIndex(formField);

        //        if (control is ToggleButtonGroup)
        //        {
        //            foreach (RadioButton radioButton in ((ToggleButtonGroup)control).RadioButtonGroup)
        //            {
        //                string radioButtonIndex = GetTagIndex(radioButton);
        //                if ((GetOutpostValue(radioButtonIndex, ref msgLines)?.ToLower()) == "true")
        //                {
        //                    formField.ControlContent = radioButton.Name;
        //                }
        //            }

        //        }
        //        else if (control is CheckBox)
        //        {
        //            formField.ControlContent = (GetOutpostValue(id, ref msgLines) == "true" ? "True" : "False");
        //        }
        //        else if (control is ComboBox comboBox)
        //        {
        //            string conboBoxData = GetOutpostValue(id, ref msgLines);
        //            var comboBoxDataSet = conboBoxData.Split(new char[] { '}' }, StringSplitOptions.RemoveEmptyEntries);
        //            formField.ControlContent = comboBoxDataSet[0];
        //        }
        //        else
        //        {
        //            formField.ControlContent = GetOutpostValue(id, ref msgLines);
        //        }
        //    }
        //    return formFields;
        //}

        private void eocOpen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (activationLevel != null)
            {
                activationLevel.IsEnabled = (sender as ComboBox).SelectedIndex == 1;
            }
        }

        private void MsgDate_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = e.ToString();
            //MsgDate = msgDate.Text;
        }

    }
}

