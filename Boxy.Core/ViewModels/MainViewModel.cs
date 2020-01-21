using Boxy.Model;
using Boxy.Model.ScryfallData;
using Boxy.Mvvm;
using Boxy.Reporting;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Boxy.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Constructors

        public MainViewModel(IReporter reporter)
        {
            Reporter = reporter;
        }

        #endregion Constructors

        #region Properties

        public IReporter Reporter { get; }

        #endregion Properties

        #region Commands

        #region SubmitSearch

        private AsyncCommand _submitSearch;

        public AsyncCommand SubmitSearch
        {
            get
            {
                return _submitSearch ?? (_submitSearch = new AsyncCommand(SubmitSearch_ExecuteAsync));
            }
        }

        private async Task SubmitSearch_ExecuteAsync(object parameter)
        {
            if (!(parameter is RichTextBox rtb))
            {
                return;
            }

            Reporter.StartBusy();

            string text = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text; 
            Card card = await ScryfallService.GetCardsAsync(text);
            Bitmap bitmap = await ImageCaching.GetImageAsync(card);
            ImageDisplay.Source = ImageHelper.LoadBitmap(bitmap);

            Reporter.StopBusy();
        }

        #endregion SubmitSearch

        #endregion Commands

        #region Methods

        #region Command Execution

        

        #endregion Command Execution

        #endregion Methods

        

        

        

        

        public string TestBinding { get; set; } = "Test Binding";
    }
}
