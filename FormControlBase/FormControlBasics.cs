using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToggleButtonGroupControl;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace FormControlBaseClass
{
    public class FormControlBasics : UserControl
    {
        public static SolidColorBrush _redBrush = new SolidColorBrush(Colors.Red);
        public static SolidColorBrush _whiteBrush = new SolidColorBrush(Colors.White);
        public static SolidColorBrush _blackBrush = new SolidColorBrush(Colors.Black);
        public static SolidColorBrush _lightSalmonBrush = new SolidColorBrush(Colors.LightSalmon);

        protected List<FormControl> formControlsList = new List<FormControl>();

        protected string validationResultMessage;

        protected void AddToErrorString(string errorText)
        {
            validationResultMessage += ($"\n{errorText}");
        }

        public static string GetTagErrorMessage(Control control)
        {
            string tag = control.Tag as string;
            if (string.IsNullOrEmpty(tag))
                return "";

            string[] tags = tag.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (tags.Length == 2 && tags[0] == "required")
            {
                return tags[1];
            }
            else if (tags.Length == 3)
            {
                return tags[2];
            }
            else
            {
                return "";
            }
        }

        public virtual string ValidateForm()
        {
            validationResultMessage = "";
            //bool result = true;
            foreach (FormControl formControl in formControlsList)
            {
                Control control = formControl.InputControl;
                string tag = control.Tag as string;
                if (!string.IsNullOrEmpty(tag) && control.IsEnabled && tag.Contains("conditionallyrequired"))
                {
                    continue;
                }

                //if (control.Name == "comboBoxToICSPosition" || control.Name == "textBoxToICSPosition" || control.Name == "comboBoxFromICSPosition" || control.Name == "textBoxFromICSPosition")
                //{
                //    int a = 0;
                //}
                if (!string.IsNullOrEmpty(tag) && control.IsEnabled && control.Visibility == Visibility.Visible && tag.Contains("required"))
                {
                    if (control is TextBox textBox)
                    {
                        if (textBox.Text.Length == 0)
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
                        if (string.IsNullOrEmpty((string)comboBox.SelectionBoxItem))
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
            return validationResultMessage;
        }

    }
}
