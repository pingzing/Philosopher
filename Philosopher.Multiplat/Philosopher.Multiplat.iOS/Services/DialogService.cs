using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Philosopher.Multiplat.Services;
using Philosopher.Multiplat.iOS.Services;

[assembly:Xamarin.Forms.Dependency(typeof(DialogService))]
namespace Philosopher.Multiplat.iOS.Services
{
    public class DialogService : IDialogService
    {
        public async Task<DialogResult> ShowPromptAsync(string message, string title = null, string okText = null, string cancelText = null,
            string placeholder = null, DialogInputType inputType = DialogInputType.Default)
        {
            InputType input;
            switch (inputType)
            {
                case DialogInputType.DecimalNumber:
                    input = InputType.DecimalNumber;
                    break;
                case DialogInputType.Default:
                    input = InputType.Default;
                    break;
                case DialogInputType.Email:
                    input = InputType.Email;
                    break;
                case DialogInputType.Name:
                    input = InputType.Name;
                    break;
                case DialogInputType.Number:
                    input = InputType.Number;
                    break;
                case DialogInputType.Password:
                    input = InputType.Password;
                    break;
                case DialogInputType.NumericPassword:
                    input = InputType.NumericPassword;
                    break;
                case DialogInputType.Phone:
                    input = InputType.Phone;
                    break;
                case DialogInputType.Url:
                    input = InputType.Url;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(inputType), inputType, null);
            }

            var result = await UserDialogs.Instance.PromptAsync(message, title, okText, cancelText, placeholder, input);
            var retResult = new DialogResult
            {
                Ok = result.Ok,
                InputText = result.Text
            };
            return retResult;
        }
    }
}
