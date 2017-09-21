using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using ToggleButtonGroupControl;

namespace FormControlBaseClass
{
	// This is for deciding at runtime which form is supported by an assembly
	[AttributeUsage(AttributeTargets.Class)]
	public class FormControlAttribute : Attribute
	{
		public enum FormType
		{
			None,
			CountyForm,
			CityForm,
			HospitalForm
		};

		public string FormControlName { get; set; }    // 
		public FormType FormControlType { get; set; }
		public string FormControlMenuName { get; set; }    // 
	}

	public sealed class FormEventArgs : EventArgs
	{
		//public FormEventArgs() { }

		//public FormEventArgs(string tacticalCallsign)
		//{
		//	TacticalCallsign = tacticalCallsign;
		//}

		//public string TacticalCallsign
		//{ get; set; }

		public string SubjectLine
		{ get; set; }
	}

	public abstract class FormControlBase : UserControl
	{
		//private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static SolidColorBrush _redBrush = new SolidColorBrush(Colors.Red);
		public static SolidColorBrush _whiteBrush = new SolidColorBrush(Colors.White);
		public static SolidColorBrush _blackBrush = new SolidColorBrush(Colors.Black);
		public static SolidColorBrush _lightSalmonBrush = new SolidColorBrush(Colors.LightSalmon);


		public event EventHandler<FormEventArgs> EventSubjectChanged;

		List<RadioButton> radioButtonsList = new List<RadioButton>();
		List<FormControl> formControlsList = new List<FormControl>();

		public FormControlBase()
		{
		}

		//delegate string GetTextBoxText(TextBox name);
		//string GetText(TextBox name) => name.Text;
		public string GetTextBlockString(TextBlock textBlock) => textBlock.Text;

		public void SetTextBlockString(TextBlock textBlock, string text)
		{
			textBlock.Text = text;
		}


		public string GetTextBoxString(TextBox textBox)
		{
			//	if ((textBox.Dispatcher.HasThreadAccess))
			//	{
			//	//return textBox.Text;
			return textBox.Text;
			//	}
			//else
			//{
			//	object retval = textBox.Dispatcher.Invoke(DispatcherPriority.Normal, new GetTextBoxText(GetText), textBox);
			//	return (string)retval;
		}

		//delegate void SetTextBoxText(TextBox name, string text);
		//void SetText(TextBox textBox, string text) => textBox.Text = text;

		public void SetTextBoxString(TextBox textBox, string text)
		{
			//	{
			//		if ((textBox.Dispatcher.CheckAccess()))
			//		{
			textBox.Text = text;
			//		}
			//		else
			//		{
			//			textBox.Dispatcher.Invoke(DispatcherPriority.Normal, new SetTextBoxText(SetText), textBox, text);
			//		}
			//	}
		}

		//delegate ComboBoxItem GetCBxSelection(ComboBox comboBox);
		ComboBoxItem GetCBSelectedItem(ComboBox comboBox) => (ComboBoxItem)comboBox.SelectedItem;

		public ComboBoxItem GetComboBoxSelectedItem(ComboBox comboBox)
		{
			//	{
			//		if ((comboBox.Dispatcher.CheckAccess()))
			//		{
			return GetCBSelectedItem(comboBox);
			//		}
			//		else
			//		{
			//			return (ComboBoxItem)comboBox.Dispatcher.Invoke(DispatcherPriority.Normal, new GetCBxSelection(GetCBSelectedItem), comboBox);
			//		}
			//	}
		}

		//delegate string GetComboBoxText(ComboBox comboBox);
		//string GetText(ComboBox comboBox) => comboBox.Text;

		public string GetComboBoxString(ComboBox comboBox)
		{
			//	{
			//		if ((comboBox.Dispatcher.CheckAccess()))
			//		{
			return comboBox.SelectionBoxItem.ToString();
			//		}
			//		else
			//		{
			//			object retval = comboBox.Dispatcher.Invoke(DispatcherPriority.Normal, new GetComboBoxText(GetText), comboBox);
			//			return (string)retval;
			//		}
			//	}
		}

		//delegate void SetComboBoxText(ComboBox comboBox, string text);
		//void SetText(ComboBox comboBox, string text) => comboBox.Text = text;

		public void SetComboBoxString(ComboBox comboBox, string text)
		{
			//	if ((comboBox.Dispatcher.CheckAccess()))
			//	{
			comboBox.PlaceholderText = text;
			//	}
			//	else
			//	{
			//		comboBox.Dispatcher.Invoke(DispatcherPriority.Normal, new SetComboBoxText(SetText), comboBox, text);
			//	}
		}

		//delegate bool? GetCheckBoxChecked(CheckBox checkBox);
		//bool? GetCheckBox(CheckBox checkBox) => checkBox.IsChecked;

		public bool? GetCheckBoxCheckedState(CheckBox checkBox)
		{
			//	if ((checkBox.Dispatcher.CheckAccess()))
			//	{
			return checkBox.IsChecked;
			//	}
			//	else
			//	{
			//		return (bool?)checkBox.Dispatcher.Invoke(DispatcherPriority.Normal, new GetCheckBoxChecked(GetCheckBox), checkBox);
			//	}
		}

		//delegate void SetCheckBoxChecked(CheckBox checkBox, bool? isChecked);
		//void SetCheckBox(CheckBox checkBox, bool? isChecked) => checkBox.IsChecked = isChecked;

		public void SetCheckBoxCheckedState(CheckBox checkBox, bool? isChecked)
		{
			//	if ((checkBox.Dispatcher.CheckAccess()))
			//	{
			checkBox.IsChecked = isChecked;
			//	}
			//	else
			//	{
			//		checkBox.Dispatcher.Invoke(DispatcherPriority.Normal, new SetCheckBoxChecked(SetCheckBox), checkBox, isChecked);
			//	}
		}

		//public void Arrange()
		//{
		//	throw new NotImplementedException();
		//}

		//delegate void SetFocusDelegate(UIElement element);
		//void SetFocus(UIElement element)
		//{
		//	element.Focus();
		//}

		//public void SetControlFocus(UIElement element)
		//{
		//	{
		//		if ((element.Dispatcher.CheckAccess()))
		//		{
		//SetFocus(element);
		//		}
		//		else
		//		{
		//			element.Dispatcher.Invoke(DispatcherPriority.Normal, new SetFocusDelegate(SetFocus), element);
		//		}
		//	}
		//}

		//delegate IInputElement GetFocusDelegate(DependencyObject element);
		//IInputElement GetFocus(DependencyObject element) => FocusManager.GetFocusedElement(element);

		//public IInputElement GetFocusedControl(DependencyObject element)
		//{
		//	if ((element.Dispatcher.CheckAccess()))
		//	{
		//return GetFocus(element);
		//	}
		//	else
		//	{
		//		return (IInputElement)element.Dispatcher.Invoke(DispatcherPriority.Normal, new GetFocusDelegate(GetFocus), element);
		//	}
		//}

		//delegate void SetFocusManagerDelegate(DependencyObject element, IInputElement value);
		//void SetFocus(DependencyObject element, IInputElement value) => FocusManager.SetFocusedElement(element, value);

		//public void SetFocusedControl(DependencyObject element, IInputElement value)
		//{
		//	if ((element.Dispatcher.CheckAccess()))
		//	{
		//		SetFocus(element, value);
		//	}
		//	else
		//	{
		//		element.Dispatcher.Invoke(DispatcherPriority.Normal, new SetFocusManagerDelegate(SetFocus), element, value);
		//	}
		//}

		//delegate string GetFrameworkNameDelegate(FrameworkElement element);
		//string GetName(FrameworkElement element) => element.Name;

		//public string GetFrameworkElementName(FrameworkElement element)
		//{
		//	if ((element.Dispatcher.CheckAccess()))
		//	{
		//		return GetName(element);
		//	}
		//	else
		//	{
		//		return (string)element.Dispatcher.Invoke(DispatcherPriority.Normal, new GetFrameworkNameDelegate(GetName), element);
		//	}
		//}

		//delegate string GetFormControlName(Control control);
		//string GetName(Control control) => control.Name;

		public string GetControlName(Control control)
		{
			//	{
			//		if ((control.Dispatcher.CheckAccess()))
			//		{
			return control.Name;
			//		}
			//		else
			//		{
			//			return (string)control.Dispatcher.Invoke(DispatcherPriority.Normal, new GetFormControlName(GetName), control);
			//		}
			//	}
		}

		public void ScanControls(DependencyObject panelName)
		{
			var count = VisualTreeHelper.GetChildrenCount(panelName);

			for (int i = 0; i < count; i++)
			{
				DependencyObject control = VisualTreeHelper.GetChild(panelName, i);

				if (control is StackPanel || control is Grid || control is Border || control is RelativePanel)
				{
					ScanControls(control);
				}
				else if (control is TextBox || control is ComboBox || control is CheckBox || control is ToggleButtonGroup)
				{
					FormControl formControl = new FormControl((Control)control);
					formControlsList.Add(formControl);
				}
				else if (control is RadioButton)
				{
                    FormControl formControl = new FormControl((Control)control);
                    formControlsList.Add(formControl);

                    radioButtonsList.Add((RadioButton)control);
				}
			}
		}

		public void InitializeControls()
		{
			foreach (FormControl formControl in formControlsList)
			{
				Control control = formControl.InputControl;

				if (control is TextBox)
				{
					//((TextBox)control).Text = "";
					control.BorderBrush = formControl.BaseBorderColor;
				}
				else if (control is ComboBox)
				{
					//((ComboBox)control).PlaceholderText = "";
					control.BorderBrush = formControl.BaseBorderColor;
				}
				else if (control is ToggleButtonGroup toggleButtonGroup)
				{
                    toggleButtonGroup.Initialize(radioButtonsList, control.Name);
				}
				else if (control is CheckBox checkBox)
				{
                    checkBox.IsChecked = false;
				}
			}
		}

		public bool ValidateForm()
		{
			bool result = true;
			foreach (FormControl formControl in formControlsList)
			{
				Control control = formControl.InputControl;
				if (control.Tag != null && control.IsEnabled && control.Tag.ToString().Contains("required"))
				{
                    //if (control.GetType() == typeof(TextBox))
                    if (control is TextBox textBox)
                    {
                        //if (((TextBox)(Control)control).Text.Length == 0)
                        if (textBox.Text.Length == 0)
                        {
                            textBox.BorderBrush = _redBrush;
							result &= false;
						}
						else
						{
                            textBox.BorderBrush = formControl.BaseBorderColor;
						}
					}
                    else if (control is ComboBox comboBox)
                    {
                        if (string.IsNullOrEmpty((string)comboBox.SelectionBoxItem))
                        //if (comboBox.SelectionBoxItem == null || comboBox.SelectionBoxItem?.ToString().Length == 0)
                        {
                            //control.SetValue(Border.BorderBrushProperty, _redBrush);
                            //((TextBox)control).BorderBrush = _redBrush;
                            //Border.
                            comboBox.BorderBrush = _redBrush;
							result &= false;
						}
						else
						{
							control.BorderBrush = formControl.BaseBorderColor;
						}
					}
                    else if (control is ToggleButtonGroup toggleButtonGroup)
                    {
						result &= toggleButtonGroup.Validate();
					}
				}
			}
			return result;
		}

		public virtual string OperatorCallsign
		{ get; set; }

		public virtual string OperatorName
		{ get; set; }

		public static string DefaultMessageTo
		{ get; set; }

		public virtual string MessageNo
		{ get; set; }

		public virtual string MsgDate
		{ get; set; }

		public virtual string MsgTime
		{ get; set; }

		public virtual string OperatorDate
		{ get; set; }

		public virtual string OperatorTime
		{ get; set; }

		public virtual string HandlingOrder
		{ get; set; }

		public abstract string PacFormName
		{ get; }


		public abstract string CreateSubject();

		public abstract string CreateOutpostData(ref PacketMessage packetMessage);

		protected abstract List<string> CreateOutpostDataFromFormFields(ref PacketMessage packetMessage, ref List<string> outpostData);

		public abstract FormField[] ConvertFromOutpost(string msgNumber, ref string[] msgLines);

        public (string id, Control control) GetTagIndex(FormField formField)
        {
            Control control = null;
            string id = "";
            try
            {
                FormControl formControl = formControlsList.Find(x => x.InputControl.Name == formField.ControlName);
                control = formControl?.InputControl;

                string tag = (string)control.Tag;
                string[] tags = tag.Split(new char[] { ',' });
                id = tags[0];
                // Test if id is an integer
                int index = Convert.ToInt32(id);
            }
            catch
            {
                return ("", control);
            }
            return (id, control);
        }

        public string CreateOutpostDataString(FormField formField)
        {
            (string id, Control control) = GetTagIndex(formField);
            if (string.IsNullOrEmpty(id))
                return "";

            if (control is TextBox)
            {
                if (((TextBox)control).AcceptsReturn)
                {
                    return $"{id}: [\\n{formField.ControlContent}]";
                }
                else
                {
                    if (formField.ControlName == "operatorDate")
                    {
                        return $"{id}: [{formField.ControlContent}" + "{odate]";
                    }
                    else if (formField.ControlName == "operatorTime")
                    {
                        return $"{id}: [{formField.ControlContent}" + "{otime]";
                    }
                    else
                    {
                        return $"{id}: [{formField.ControlContent}]";
                    }
                }
            }
            else if (control is RadioButton || control is CheckBox)
            {
                if (formField.ControlContent == "True")
                {
                    return $"{id}: [true]";
                }
                else
                {
                    return "";
                }
            }
            return "";
        }

        protected string CreateOutpostMessageBody(List<string> outpostData)
		{
			StringBuilder sb = new StringBuilder();
			foreach (string s in outpostData)
			{
				sb.Append(s + "\r");
				//Console.WriteLine(s);
			}
			string outpostDataMessage = sb.ToString();
			return outpostDataMessage;
		}

		//public static string CompressString(string str)
		//{
		//    var bytes = Encoding.UTF8.GetBytes(str);
		//    //var bytes = str.ToArray.ToByteArray();
		//    using (var msi = new MemoryStream(bytes))
		//    {
		//        using (var mso = new MemoryStream())
		//        {
		//            using (var gs = new GZipStream(mso, CompressionMode.Compress))
		//            {
		//                msi.CopyTo(gs);
		//            }
		//            return Convert.ToBase64String(mso.ToArray());
		//            //return Convert.ToString(mso.ToArray());
		//        }
		//    }
		//}

		//public static string UnCompressString(string str)
		//{
		//    byte[] bytes = Convert.FromBase64String(str);
		//    //byte[] bytes = Convert.(str);
		//    using (var msi = new MemoryStream(bytes))
		//    using (var mso = new MemoryStream())
		//    {
		//        using (var gs = new GZipStream(msi, CompressionMode.Decompress))
		//        {
		//            gs.CopyTo(mso);
		//        }
		//        return Encoding.UTF8.GetString(mso.ToArray());
		//    }
		//}

		public FormField[] CreateEmptyFormFieldsArray()
		{
			FormField[] formFields = new FormField[formControlsList.Count];

            for (int i = 0; i < formControlsList.Count; i++)
            {
                FormField formField = new FormField()
                {
                    ControlName = formControlsList[i].InputControl.Name,
                    ControlContent = "",
                };
                formFields.SetValue(formField, i);

            }
            return formFields;
		}

		public FormField[] CreateFormFieldsInXML()
		{
			FormField[] formFields = new FormField[formControlsList.Count];

			for (int i = 0; i < formControlsList.Count; i++)
			{
                FormField formField = new FormField() { ControlName = formControlsList[i].InputControl.Name };

                if (formControlsList[i].InputControl is TextBox)
                {
					formField.ControlContent = ((TextBox)formControlsList[i].InputControl).Text;
					//if (((TextBox)formFieldsList[i]).IsSpellCheckEnabled)
					//{
					//	for (int j = 0; j < ((TextBox)formFieldsList[i]).Text.Length; j++)
					//	{
					//		int spellingErrorIndex = ((TextBox)formFieldsList[i]).GetSpellingErrorStart(j);
					//		if (spellingErrorIndex < 0)
					//		{
					//			continue;
					//		}
					//		else
					//		{
					//			int spellingErrorLength = ((TextBox)formFieldsList[i]).GetSpellingErrorLength(spellingErrorIndex);
					//			string misSpelledWord = ((TextBox)formFieldsList[i]).Text.Substring(spellingErrorIndex, spellingErrorLength);
					//			j += spellingErrorLength;
					//			if (formField.MisSpells == null || formField.MisSpells.Length == 0)
					//				formField.MisSpells = misSpelledWord;
					//			else
					//				formField.MisSpells += (", " + misSpelledWord);
					//		}
					//	}
					//}
				}
                else if (formControlsList[i].InputControl is ComboBox)
                {
					formField.ControlContent = ((ComboBox)(Control)formControlsList[i].InputControl).SelectionBoxItem?.ToString();
				}
                else if (formControlsList[i].InputControl is ToggleButtonGroup)
                {
					formField.ControlContent = ((ToggleButtonGroup)(Control)formControlsList[i].InputControl).GetRadioButtonCheckedState();
				}
                else if (formControlsList[i].InputControl is CheckBox)
                {
					formField.ControlContent = ((CheckBox)(Control)formControlsList[i].InputControl).IsChecked.ToString();
				}
                else if (formControlsList[i].InputControl is RadioButton)
                {
                    formField.ControlContent = ((RadioButton)formControlsList[i].InputControl).IsChecked.ToString();
                }
                formFields.SetValue(formField, i);
			}
			return formFields;
		}

		public void FillFormFromFormFields(FormField[] formFields)
		{
			foreach (FormField formField in formFields)
			{
				FormControl formControl = formControlsList.Find(x => x.InputControl.Name == formField.ControlName);

				Control control = formControl?.InputControl;

				if (control is null)
					continue;

				if (control is TextBox textBox)
				{
                    textBox.Text = formField.ControlContent;
				}
				else if (control is ComboBox comboBox)
				{
                    comboBox.SelectedItem = formField.ControlContent;
				}
				else if (control is ToggleButtonGroup toggleButtonGroup)
				{
                    toggleButtonGroup.SetRadioButtonCheckedState(formField.ControlContent);
				}
				else if (control is CheckBox checkBox)
				{
                    checkBox.IsChecked = formField.ControlContent == "True" ? true : false;
				}
			}
		}

		protected string GetOutpostFieldValue(string field)
		{
			int startIndex = field.IndexOf('[');
			int endIndex = field.IndexOf(']');
			if (startIndex != -1 && endIndex != -1)
			{
                if (field.Substring(startIndex + 1, endIndex - startIndex - 1).StartsWith("\\n"))
                    return field.Substring(startIndex + 3, endIndex - startIndex - 3);
                else
                    return field.Substring(startIndex + 1, endIndex - startIndex - 1);
			}
			else
			{
				return "";
			}
		}

		protected string GetOutpostValue(string fieldIdent, ref string[] msgLines)
		{
			for (int i = 4; i < msgLines.Length; i++)
			{
				int index = msgLines[i].IndexOf(':');
				if (index == -1)
				{
					continue;
				}
				if (fieldIdent == msgLines[i].Substring(0, index))
				{
					return GetOutpostFieldValue(msgLines[i]);
				}
			}
			return null;
		}

		protected void Subject_Changed(object sender, RoutedEventArgs e)
		{
			if (sender is RadioButton radioButton)
			{
				if (radioButton.Name == "emergency")
				{
					HandlingOrder = "immediate";
				}
			}
			EventHandler<FormEventArgs> OnSubjectChange = EventSubjectChanged;
            FormEventArgs formEventArgs = new FormEventArgs() { SubjectLine = MessageNo };
			OnSubjectChange?.Invoke(this, formEventArgs);
		}

		protected string ConvertTabsToSpaces(string text, int tabWidth)
		{
			StringBuilder sb = new StringBuilder();
			var lines = text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
			string[] convertedLines = new string[lines.Length];
			foreach (string line in lines)
			{
				string convertedLine = ConvertLineTabsToSpaces(line, tabWidth);
				sb.AppendLine(convertedLine);
			}
			return sb.ToString();
		}

		protected string ConvertLineTabsToSpaces(string line, int tabWidth)
		{
			string convertedLine;
			StringBuilder sb = new StringBuilder();
			int j = 0;
			for (int i = 0; i < line.Length; i++, j++)
			{
				if (line[i] != '\t')
				{
					sb.Append(line[i]);
				}
				else
				{
					j--;
					int spaceCount = tabWidth - j % tabWidth;
					for (int k = 0; k < spaceCount; k++)
					{
						sb.Append(' ');
						j++;
					}
				}
			}
			convertedLine = sb.ToString();
			//Console.WriteLine(convertedLine);

			return convertedLine;
		}
	}
}


