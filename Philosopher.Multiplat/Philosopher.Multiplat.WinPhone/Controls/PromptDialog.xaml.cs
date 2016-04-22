using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Philosopher.Multiplat.Services;

namespace Philosopher.Multiplat.WinPhone.Controls
{
    public sealed partial class PromptDialog : ContentDialog
    {
        public string InputResult { get; private set; }

        public static readonly DependencyProperty MessageTextProperty = DependencyProperty.Register(
            "MessageText", typeof(string), typeof(PromptDialog), new PropertyMetadata(default(string)));
        public string MessageText
        {
            get { return (string)GetValue(MessageTextProperty); }
            set { SetValue(MessageTextProperty, value); }
        }

        public static readonly DependencyProperty InputTextProperty = DependencyProperty.Register(
            "InputText", typeof (string), typeof (PromptDialog), new PropertyMetadata(default(string)));

        public string InputText
        {
            get { return (string) GetValue(InputTextProperty); }
            set { SetValue(InputTextProperty, value); }
        }

        public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
            "PlaceholderText", typeof (string), typeof (PromptDialog), new PropertyMetadata(default(string)));
        public string PlaceholderText
        {
            get { return (string) GetValue(PlaceholderTextProperty); }
            set { SetValue(PlaceholderTextProperty, value); }
        }        

        public PromptDialog(string message, string title, string okText, string cancelText,
            string placeholderText, DialogInputType inputType = DialogInputType.Default)
        {
            this.InitializeComponent();
            MessageText = message;
            if (!String.IsNullOrEmpty(title))
            {
                Title = title;
            }
            if (!String.IsNullOrEmpty(okText))
            {
                PrimaryButtonText = okText;
            }
            else
            {
                PrimaryButtonText = "ok";
            }
            if (!String.IsNullOrEmpty(cancelText))
            {
                SecondaryButtonText = cancelText.ToLower();
            }
            else
            {
                SecondaryButtonText = "cancel";
            }
            if (!String.IsNullOrEmpty(placeholderText))
            {
                PlaceholderText = placeholderText.ToLower();
            }

            //No distinction in the caller between a password dialog and regular dialog, 
            //so just swap visibility of the two input boxes. They use the same bindings.
            if (inputType == DialogInputType.Password)
            {
                InputBox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;
            }

            InputScope scope = new InputScope();            
            switch(inputType)
            {
                case DialogInputType.Url:
                    scope.Names.Add(new InputScopeName(InputScopeNameValue.Url));
                    break;
                case DialogInputType.Email:
                    scope.Names.Add(new InputScopeName(InputScopeNameValue.EmailSmtpAddress));
                    break;
                case DialogInputType.Name:
                        scope.Names.Add(new InputScopeName(InputScopeNameValue.NameOrPhoneNumber));
                    break;
                case DialogInputType.Phone:
                    scope.Names.Add(new InputScopeName(InputScopeNameValue.TelephoneNumber));
                    break;
                case DialogInputType.Number:
                case DialogInputType.NumericPassword:
                case DialogInputType.DecimalNumber:
                    scope.Names.Add(new InputScopeName(InputScopeNameValue.Number));                    
                    break;
                case DialogInputType.Default:
                default:
                    scope.Names.Add(new InputScopeName(InputScopeNameValue.Default));                    
                    break;                                        
            }
            InputBox.InputScope = scope;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            
            InputResult = InputText;
            
            //this.Hide();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            InputResult = null;
            //this.Hide();
        }
    }
}
