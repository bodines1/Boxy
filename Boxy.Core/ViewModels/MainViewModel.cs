using Boxy.Model;
using Boxy.Model.ScryfallData;
using Boxy.Resources.DialogService;
using Boxy.Resources.Mvvm;
using Boxy.Resources.Reporting;
using Boxy.ViewModels.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable ClassNeverInstantiated.Global

namespace Boxy.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Constructors

        public MainViewModel(IDialogService dialogService, IReporter reporter)
        {
            DialogService = dialogService;
            Reporter = reporter;
            ZoomPercent = 100;

            Reporter.StatusReported += (sender, args) => LastStatus = args;
            Reporter.ProgressReported += (sender, args) => LastProgress = args;
            
            try
            {
                Catalog = JsonConvert.DeserializeObject<CardCatalog>(File.ReadAllText(CardCatalog.SavePath));
            }
            catch (Exception)
            {
                Catalog = null;
            }

            ArtworkPreferences.Initialize();
        }

        #endregion Constructors

        #region Fields

        private BoxyStatusEventArgs _lastStatus;
        private BoxyProgressEventArgs _lastProgress;
        private CardCatalog _catalog;
        private DateTime? _catalogUpdated;
        private ObservableCollection<CardViewModel> _displayedCards;

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
                CatalogUpdated = Catalog?.Metadata?.UpdatedAt;
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

        public BoxyProgressEventArgs LastProgress
        {
            get
            {
                return _lastProgress;
            }

            set
            {
                _lastProgress = value;
                OnPropertyChanged(nameof(LastProgress));
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

        public int ZoomPercent
        {
            get
            {
                return _zoomPercent;
            }

            set
            {
                _zoomPercent = value;

                foreach (CardViewModel card in DisplayedCards)
                {
                    card.ScaleToPercent(ZoomPercent);
                }

                OnPropertyChanged(nameof(ZoomPercent));
            }
        }

        public ObservableCollection<CardViewModel> DisplayedCards
        {
            get
            {
                return _displayedCards ?? (_displayedCards = new ObservableCollection<CardViewModel>());
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
            if (!(parameter is string str))
            {
                return;
            }

            if (Catalog == null)
            {
                DialogService.ShowDialog(new MessageDialogViewModel("Card Catalog must be updated before continuing."));
                return;
            }

            DisplayedCards.Clear();

            string[] lines = str.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                Reporter.StartBusy();
                Reporter.Report(this, $"Finding card '{line}' in local catalog");
                Card card = Catalog.FindExactCard(line);

                if (card == null)
                {
                    Reporter.Report(this, $"Search term '{line}' returned no results", true);
                    Reporter.StopBusy();
                    continue;
                }

                List<Card> allPrintings = await ScryfallService.GetAllPrintingsAsync(card.OracleId, Reporter);
                Reporter.Report(this, $"Found {allPrintings} prints");

                //TODO: Qty
                var cardVm = new CardViewModel(Reporter, allPrintings, 1);
                cardVm.ScaleToPercent(ZoomPercent);
                DisplayedCards.Add(cardVm);
                cardVm.SelectPreferredPrinting();
            }

            Reporter.Report(this, $"Built {DisplayedCards.Count} cards");
            Reporter.StopBusy();
        }

        #endregion SubmitSearch

        #region UpdateCatalog

        private AsyncCommand _updateCatalog;
        private int _zoomPercent;

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

            BulkData oracleBulkData = (await ScryfallService.GetBulkDataInfo(Reporter)).Data.Single(bd => bd.Name.Contains("Oracle"));
            int catalogSize = oracleBulkData.CompressedSize;

            List<Card> cards = await ScryfallService.GetBulkCards(oracleBulkData.PermalinkUri, Reporter);

            if (cards == null)
            {
                Reporter.Report(this, "Cards not found or empty", true);
                Reporter.StopBusy();
                return;
            }

            Reporter.Report(this, "Converting catalog to JSON");
            var catalog = new CardCatalog(oracleBulkData, cards);
            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(catalog));

            try
            {
                Reporter.Report(this, "Saving locally");
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

        /// <inheritdoc />
        public override void Cleanup()
        {
            ArtworkPreferences.SavePreferencesToFile();
            base.Cleanup();
        }

        #endregion Methods
    }
}
