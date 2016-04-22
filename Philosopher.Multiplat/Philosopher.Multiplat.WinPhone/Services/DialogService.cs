using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Philosopher.Multiplat.Services;
using Philosopher.Multiplat.WinPhone.Controls;
using Philosopher.Multiplat.WinPhone.Services;

[assembly: Xamarin.Forms.Dependency(typeof(DialogService))]
namespace Philosopher.Multiplat.WinPhone.Services
{
    public class DialogService : IDialogService
    {
        private static readonly SemaphoreSlim _dialogSemaphore = new SemaphoreSlim(1);

        public async Task<DialogResult> ShowPromptAsync(string message, string title = null, string okText = null, string cancelText = null,
            string placeholder = null, DialogInputType inputType = DialogInputType.Default)
        {
            var dialog = new PromptDialog(message, title, okText, cancelText, placeholder, inputType);            
            ContentDialogResult dialogResult = await ShowQueuedDialogAsync(dialog);
            if (dialogResult == ContentDialogResult.Primary)
            {
                var result = new DialogResult
                {
                    InputText = dialog.InputResult,
                    Ok = true
                };
                return result;
            }
            else
            {
                var result = new DialogResult {Ok = false};
                return result;
            }
        }

        private async Task<ContentDialogResult> ShowQueuedDialogAsync(ContentDialog dlg)
        {
            await _dialogSemaphore.WaitAsync();
            ContentDialogResult result = await dlg.ShowAsync();
            _dialogSemaphore.Release();
            return result;
        }
    }
}