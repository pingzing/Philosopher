using System.Threading.Tasks;

namespace Philosopher.Multiplat.Services
{
    public interface IDialogService
    {
        Task<DialogResult> ShowPromptAsync(string message, string title = null, string okText = null, string cancelText = null, string placeholder = null,
            DialogInputType inputType = DialogInputType.Default);
    }

    public class DialogResult
    {
        public bool Ok { get; set; }
        public string InputText { get; set; }
    }

    public enum DialogInputType
    {
        Default,
        Email,
        Name,
        Number,
        DecimalNumber,
        Password,
        NumericPassword,
        Phone,
        Url
    }
}