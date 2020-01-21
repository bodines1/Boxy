using Boxy.DialogService;
using Boxy.Model;
using Boxy.Model.ScryfallData;
using Boxy.Mvvm;
using Boxy.Reporting;
using Boxy.ViewModels.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace Boxy.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Constructors

        public MainViewModel(IDialogService dialogService, IReporter reporter)
        {
            DialogService = dialogService;
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

        private IDialogService DialogService { get; }

        private CardCatalog Catalog
        {
            get
            {
                return _catalog;
            }

            set
            {
                _catalog = value;
                CatalogUpdated = Catalog?.UpdatedAt;
                OnPropertyChanged(nameof(Catalog));
            }
        }

        public IReporter Reporter { get; }

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
                return _submitSearch ?? (_submitSearch = new AsyncCommand(param =>
                {
                    
                    Task task = SubmitSearch_ExecuteAsync(param);
                    
                    return task;
                } ));
            }
        }

        private async Task SubmitSearch_ExecuteAsync(object parameter)
        {
            if (!(parameter is RichTextBox rtb))
            {
                return;
            }

            if (Catalog == null)
            {
                DialogService.ShowDialog(new MessageDialogViewModel("Card Catalog must be updated before continuing."));
                return;
            }

            string[] lines = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            string text = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text.Trim();

            Reporter.StartBusy();
            Reporter.Report(this, $"Getting card '{text}'");
            Card card = Catalog.FindCard(text);

            if (card == null)
            {
                Reporter.Report(this, $"Search term '{text}' returned no results", true);
                Reporter.StopBusy();
                return;
            }

            Reporter.Report(this, $"Getting card '{text}' artwork");
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

        #region UpdateCatalog

        private AsyncCommand _updateCatalog;

        public AsyncCommand UpdateCatalog
        {
            get
            {
                return _updateCatalog ?? (_updateCatalog = new AsyncCommand(UpdateCatalog_ExecuteAsync));
            }
        }

        private async Task UpdateCatalog_ExecuteAsync()
        {
            Reporter.StartBusy();
            Reporter.Report(this, "Getting cards for catalog");
            List<Card> cards = await ScryfallService.GetBulkDataCatalog(CardCatalog.ScryfallUri);

            if (cards == null)
            {
                Reporter.Report(this, "Cards not found or empty", true);
                Reporter.StopBusy();
                return;
            }

            Reporter.Report(this, "Converting catalog to JSON");
            var catalog = new CardCatalog(DateTime.Now, cards);
            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(catalog));

            try
            {
                Reporter.Report(this, "Saving locally");

                if (!Directory.Exists(CardCatalog.SavePath))
                {
                    Directory.CreateDirectory(CardCatalog.SavePath);
                }

                using (var fileStream = new FileStream(CardCatalog.SavePath, FileMode.Create))
                {
                    await fileStream.WriteAsync(data, 0, data.Length);
                    await fileStream.FlushAsync();
                }
            }
            catch (Exception e)
            {
                DisplayError(e, "Could not update Card Catalog.");
                Reporter.Report(this, "Failed to save");
                Reporter.StopBusy();
                return;
            }

            Catalog = catalog;
            Reporter.Report(this, "Catalog updated");
            Reporter.StopBusy();
        }

        #endregion UpdateCatalog

        #endregion Commands

        #region Methods

        private void DisplayError(Exception exc, string additionalInfo)
        {
            var message = new StringBuilder($"{additionalInfo}\r\n\r\n");

            while (exc != null)
            {
                message.AppendLine(exc.Message);
                exc = exc.InnerException;
            }

            DialogService.ShowDialog(new MessageDialogViewModel(message.ToString()));
        }

        #endregion Methods
    }
}
