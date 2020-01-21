using Boxy.Model;
using Boxy.Model.ScryfallData;
using Boxy.Mvvm;
using Boxy.Reporting;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace Boxy.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Constructors

        public MainViewModel(IReporter reporter)
        {
            Reporter = reporter;
            Reporter.StatusReported += (sender, args) => LastStatus = args;

            if (!File.Exists(CardCatalog.SavePath))
            {
                Catalog = null;
            }
            else
            {
                try
                {
                    JsonConvert.DeserializeObject<CardCatalog>(File.ReadAllText(CardCatalog.SavePath));
                }
                catch (Exception)
                {
                    Catalog = null;
                }
            }
        }

        #endregion Constructors

        #region Fields

        private BitmapSource _cardImage;
        private BoxyStatusEventArgs _lastStatus;
        private CardCatalog _catalog;
        private DateTime? _catalogUpdated;

        #endregion Fields

        #region Properties

        public IReporter Reporter { get; }


        public CardCatalog Catalog
        {
            get
            {
                return _catalog;
            }

            set
            {
                _catalog = value;
                CatalogUpdated = Catalog?.CatalogBulkData?.UpdatedAt ?? null;
                OnPropertyChanged(nameof(Catalog));
            }
        }

        public BoxyStatusEventArgs LastStatus
        {
            get
            {
                return _lastStatus;
            }

            set
            {
                _lastStatus = value;
                OnPropertyChanged(nameof(LastStatus));
            }
        }

        public BitmapSource CardImage
        {
            get
            {
                return _cardImage;
            }

            set
            {
                _cardImage = value;
                OnPropertyChanged(nameof(CardImage));
            }
        }

        public DateTime? CatalogUpdated
        {
            get
            {
                return _catalogUpdated;
            }

            set
            {
                _catalogUpdated = value;
                OnPropertyChanged(nameof(CatalogUpdated));
            }
        }

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

            string[] lines = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            string text = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text.Trim();

            Reporter.Report(this, $"Getting card '{text}'...");
            Card card = await ScryfallService.GetCardsAsync(text);

            if (card == null)
            {
                Reporter.Report(this, $"Search term '{text}' returned no results", true);
                Reporter.StopBusy();
                return;
            }

            Reporter.Report(this, $"Getting card '{text}' artwork...");
            Bitmap bitmap = await ImageCaching.GetImageAsync(card);

            if (bitmap == null)
            {
                Reporter.Report(this, $"'{text}' art not found", true);
                Reporter.StopBusy();
                return;
            }

            CardImage = ImageHelper.LoadBitmap(bitmap);
            
            Reporter.Report(this, $"Found {1} card");
            Reporter.StopBusy();
        }

        #endregion SubmitSearch

        #endregion Commands

        #region Methods

        

        #endregion Methods

        public string TestBinding { get; set; } = "Test Binding";
    }
}
