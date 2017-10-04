using FormControlBaseClass;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ToggleButtonGroupControl;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace OAMunicipalStatusFormControl
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 

    [FormControl(
        //FormControlName = "XSC_OA_MuniStatus_v1708",
        FormControlName = "OAMuniStatus",
        FormControlMenuName = "XSC OA Municopal Status",
        FormControlType = FormControlAttribute.FormType.CountyForm)
    ]

    public sealed partial class OAMunicipalStatusControl : FormControlBase
    {
        public OAMunicipalStatusControl()
        {
            this.InitializeComponent();

            ScanControls(PrintableArea);

            InitializeControls();

            ReceivedOrSent = "sent";
            HowRecevedSent = "packet";

            //foreach (FormControl formControl in FormControlsList)
            //{
            //    Control control = formControl.InputControl;
            //    if (control is ComboBox comboBox)
            //    {
            //        //SetComboBoxString(comboBox, "Unknown");
            //        comboBox.SelectedIndex = 0;
            //    }
            //}
            //municipalityName.SelectedIndex = -1;
        }

        public string SenderMsgNo
        { get { return GetTextBoxString(senderMsgNo); } set { SetTextBoxString(senderMsgNo, value); } }

        public override string MessageNo
        { get { return GetTextBoxString(messageNo); } set { SetTextBoxString(messageNo, value); } }

        public string ReceiverMsgNo
        { get { return GetTextBoxString(receiverMsgNo); } set { SetTextBoxString(receiverMsgNo, value); } }

        public string Severity
        { get { return severity.GetRadioButtonCheckedState(); } set { severity.SetRadioButtonCheckedState(value); } }

        public override string HandlingOrder
        { get { return handlingOrder.GetRadioButtonCheckedState(); } set { handlingOrder.SetRadioButtonCheckedState(value); } }

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

        public override string OperatorDate
        { get { return GetTextBoxString(operatorDate); } set { SetTextBoxString(operatorDate, value); } }

        public override string OperatorName
        { get { return GetTextBoxString(operatorName); } set { SetTextBoxString(operatorName, value); } }

        public override string OperatorTime
        { get { return GetTextBoxString(operatorTime); } set { SetTextBoxString(operatorTime, value); } }

        public string IncidentName
        { get => GetTextBoxString(incidentName); }

        public string MunicipalityName
        { get => GetComboBoxSelectedItem(municipalityName)?.Content.ToString(); }

        public override string PacFormName => "OAMuniStatus";

        public override string CreateSubject()
        {
            return (MessageNo + "_" + Severity?.ToUpper()[0] + "/" + HandlingOrder?.ToUpper()[0] + "_OAMuniStat_" + MunicipalityName + '_' + IncidentName);
        }

        public override string ValidateForm()
        {
            ErrorMessage = "";
            foreach (FormControl formControl in FormControlsList)
            {
                Control control = formControl.InputControl;
                if (!string.IsNullOrEmpty(control.Tag as string) && control.IsEnabled && control.Tag.ToString().Contains("conditionallyrequired"))
                {
                    continue;
                }

                if (control.Tag != null && control.IsEnabled && control.Visibility == Visibility.Visible && control.Tag.ToString().Contains("required"))
                {
                    if (control is TextBox textBox)
                    {
                        if (string.IsNullOrEmpty(textBox.Text))
                        {
                            AddToErrorString(GetTagErrorMessage(control));
                            control.BorderBrush = _redBrush;
                        }
                        else
                        {
                            control.BorderBrush = formControl.BaseBorderColor;
                        }
                    }
                    else if (control is ComboBox comboBox)
                    {
                        if (string.IsNullOrEmpty(GetComboBoxString(comboBox)))
                        {
                            AddToErrorString(GetTagErrorMessage(control));
                            control.BorderBrush = _redBrush;
                        }
                        else
                        {
                            control.BorderBrush = formControl.BaseBorderColor;
                        }
                    }
                    else if (control is ToggleButtonGroup toggleButtonGroup)
                    {
                        if (!toggleButtonGroup.Validate())
                        {
                            AddToErrorString(GetTagErrorMessage(control));
                        }
                    }
                }
            }

            foreach (FormControl formControl in FormControlsList)
            {
                Control control = formControl.InputControl;
                switch (control.Name)
                {
                    case "eocOpen":
                        if ((control as ComboBox).SelectedIndex == 0)
                        {
                            AddToErrorString(GetTagErrorMessage(control));
                            control.BorderBrush = _redBrush;
                        }
                        else
                        {
                            control.BorderBrush = formControl.BaseBorderColor;
                        }
                        break;
                    case "activationLevel":                        
                        if (control.IsEnabled && (control as ComboBox).SelectedIndex == 0)
                        {
                            AddToErrorString(GetTagErrorMessage(control));
                            control.BorderBrush = _redBrush;
                        }
                        else
                        {
                            control.BorderBrush = formControl.BaseBorderColor;
                        }
                        break;
                    case "stateOfEmergency":
                        if ((control as ComboBox).SelectedIndex == 0)
                        {
                            AddToErrorString(GetTagErrorMessage(control));
                            control.BorderBrush = _redBrush;
                        }
                        else
                        {
                            control.BorderBrush = formControl.BaseBorderColor;
                        }
                        break;
                    case "howSent":
                        if (stateOfEmergency.SelectedIndex == 1 && string.IsNullOrEmpty((control as TextBox).Text))
                        {
                            AddToErrorString(GetTagErrorMessage(control));
                            control.BorderBrush = _redBrush;
                        }
                        else
                        {
                            control.BorderBrush = formControl.BaseBorderColor;
                        }
                        break;
                    case "commentsCommunications":
                        if (comboBoxCommunications.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text))
                        {
                            AddToErrorString(GetTagErrorMessage(control));
                            control.BorderBrush = _redBrush;
                        }
                        else
                        {
                            control.BorderBrush = formControl.BaseBorderColor;
                        }
                        break;
                    case "commentsDebris":
                        if (comboBoxDebris.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text))
                        {
                            AddToErrorString(GetTagErrorMessage(control));
                            control.BorderBrush = _redBrush;

                        }
                        else
                        {
                            control.BorderBrush = formControl.BaseBorderColor;
                        }
                        break;
                    case "commentsFlooding":
                        if (comboBoxFlooding.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text))
                        {
                            AddToErrorString(GetTagErrorMessage(control));
                            control.BorderBrush = _redBrush;
                        }
                        else
                        {
                            control.BorderBrush = formControl.BaseBorderColor;
                        }
                        break;
                    case "commentsHazmat":
                        if (comboBoxHazmat.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text))
                        {
                            AddToErrorString(GetTagErrorMessage(control));
                            control.BorderBrush = _redBrush;
                        }
                        else
                        {
                            control.BorderBrush = formControl.BaseBorderColor;
                        }
                        break;
                    case "commentsEmergencyServices":
                        if (comboBoxEmergencyServices.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text))
                        {
                            AddToErrorString(GetTagErrorMessage(control));
                            control.BorderBrush = _redBrush;
                        }
                        else
                        {
                            control.BorderBrush = formControl.BaseBorderColor;
                        }
                        break;
                    case "commentsCasualties":
                        if (comboBoxCasualties.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text))
                        {
                            AddToErrorString(GetTagErrorMessage(control));
                            control.BorderBrush = _redBrush;
                        }
                        else
                        {
                            control.BorderBrush = formControl.BaseBorderColor;
                        }
                        break;
                    case "commentsUtilitiesGas":
                        if (comboBoxUtilitiesGas.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text))
                        {
                            AddToErrorString(GetTagErrorMessage(control));
                            control.BorderBrush = _redBrush;
                        }
                        else
                        {
                            control.BorderBrush = formControl.BaseBorderColor;
                        }
                        break;
                    case "commentsUtilitiesElectric":
                        if (comboBoxUtilitiesElectric.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text))
                        {
                            AddToErrorString(GetTagErrorMessage(control));
                            control.BorderBrush = _redBrush;
                        }
                        else
                        {
                            control.BorderBrush = formControl.BaseBorderColor;
                        }
                        break;
                    case "commentsInfrastructurePower":
                        if (comboBoxInfrastructurePower.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text))
                        {
                            AddToErrorString(GetTagErrorMessage(control));
                            control.BorderBrush = _redBrush;
                        }
                        else
                        {
                            control.BorderBrush = formControl.BaseBorderColor;
                        }
                        break;
                    case "commentsInfrastructureWater":
                        if (comboBoxInfrastructureWater.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text))
                        {
                            AddToErrorString(GetTagErrorMessage(control));
                            control.BorderBrush = _redBrush;
                        }
                        else
                        {
                            control.BorderBrush = formControl.BaseBorderColor;
                        }
                        break;
                    case "commentsInfrastructureSewer":
                        if (comboBoxInfrastructureSewer.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text))
                        {
                            AddToErrorString(GetTagErrorMessage(control));
                            control.BorderBrush = _redBrush;
                        }
                        else
                        {
                            control.BorderBrush = formControl.BaseBorderColor;
                        }
                        break;
                    case "commentsSearchAndRescue":
                        if (comboBoxSearchAndRescue.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text))
                        {
                            AddToErrorString(GetTagErrorMessage(control));
                            control.BorderBrush = _redBrush;
                        }
                        else
                        {
                            control.BorderBrush = formControl.BaseBorderColor;
                        }
                        break;
                    case "commentsTransportationsRoads":
                        if (comboBoxTransportationsRoads.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text))
                        {
                            AddToErrorString(GetTagErrorMessage(control));
                            control.BorderBrush = _redBrush;
                        }
                        else
                        {
                            control.BorderBrush = formControl.BaseBorderColor;
                        }
                        break;
                    case "commentsTransportationsBridges":
                        if (comboBoxTransportationsBridges.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text))
                        {
                            AddToErrorString(GetTagErrorMessage(control));
                            control.BorderBrush = _redBrush;
                        }
                        else
                        {
                            control.BorderBrush = formControl.BaseBorderColor;
                        }
                        break;
                    case "commentsCivilUnrest":
                        if (comboBoxCivilUnrest.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text))
                        {
                            AddToErrorString(GetTagErrorMessage(control));
                            control.BorderBrush = _redBrush;
                        }
                        else
                        {
                            control.BorderBrush = formControl.BaseBorderColor;
                        }
                        break;
                    case "commentsAnimalIssues":
                        if (comboBoxAnimalIssues.SelectedIndex > 1 && string.IsNullOrEmpty((control as TextBox).Text))
                        {
                            AddToErrorString(GetTagErrorMessage(control));
                            control.BorderBrush = _redBrush;
                        }
                        else
                        {
                            control.BorderBrush = formControl.BaseBorderColor;
                        }
                        break;
                }
            }
            return ErrorMessage;
        }

        protected override List<string> CreateOutpostDataFromFormFields(ref PacketMessage packetMessage, ref List<string> outpostData)
        {
            foreach (FormField formField in packetMessage.FormFieldArray)
            {
                if (string.IsNullOrEmpty(formField.ControlContent))
                    continue;

                string data = CreateOutpostDataString(formField);
                if (string.IsNullOrEmpty(data))
                {
                    continue;
                }
                outpostData.Add(data);
            }
            outpostData.Add("#EOF");
            return outpostData;
        }

        public override string CreateOutpostData(ref PacketMessage packetMessage)
        {
            List<string> outpostData = new List<string>
            {
                "!PACF! " + packetMessage.Subject,
                "# JS:SC-OA-Muni Status (which4)",
                "# JS-ver. PR-4.3-1.8, 09/29/17",
                "# FORMFILENAME: XSC_OA_MuniStatus_v1708.html"
            };
            outpostData = CreateOutpostDataFromFormFields(ref packetMessage, ref outpostData);

            return CreateOutpostMessageBody(outpostData);
        }

        public override FormField[] ConvertFromOutpost(string msgNumber, ref string[] msgLines)
        {
            FormField[] formFields = CreateEmptyFormFieldsArray();

            foreach (FormField formField in formFields)
            {
                (string id, Control control) = GetTagIndex(formField);

                if (control is ToggleButtonGroup)
                {
                    foreach (RadioButton radioButton in ((ToggleButtonGroup)control).RadioButtonGroup)
                    {
                        string radioButtonIndex = GetTagIndex(radioButton);
                        if ((GetOutpostValue(radioButtonIndex, ref msgLines)?.ToLower()) == "true")
                        {
                            formField.ControlContent = radioButton.Name;
                        }
                    }

                }
                else if (control is CheckBox)
                {
                    formField.ControlContent = (GetOutpostValue(id, ref msgLines) == "true" ? "True" : "False");
                }
                else if (control is ComboBox comboBox)
                {
                    string conboBoxData = GetOutpostValue(id, ref msgLines);
                    var comboBoxDataSet = conboBoxData.Split(new char[] { '}' }, StringSplitOptions.RemoveEmptyEntries);
                    formField.ControlContent = comboBoxDataSet[0];
                }
                else
                {
                    formField.ControlContent = GetOutpostValue(id, ref msgLines);
                }
            }
            return formFields;
        }

        private void eocOpen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (activationLevel != null)
            {
                activationLevel.IsEnabled = (sender as ComboBox).SelectedIndex == 1;
            }
        }
    }
}

