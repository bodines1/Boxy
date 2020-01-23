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
using System.Deployment.Application;
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

        /// <summary>
        /// Creates a new instance of <see cref="MainViewModel"/>.
        /// </summary>
        /// <param name="dialogService">Service for resolving and showing dialog windows from viewmodels.</param>
        /// <param name="reporter">Reports status and progress events to subscribers.</param>
        public MainViewModel(IDialogService dialogService, IReporter reporter)
        {
            DialogService = dialogService;
            Reporter = reporter;
            SoftwareVersion = ApplicationDeployment.IsNetworkDeployed ? ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString() : "Debug";
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

        private string _softwareVersion;
        private BoxyStatusEventArgs _lastStatus;
        private BoxyProgressEventArgs _lastProgress;
        private CardCatalog _catalog;
        private DateTime? _catalogTimestamp;
        private int _zoomPercent;
        private ObservableCollection<CardViewModel> _displayedCards;
        private ObservableCollection<string> _errorsWhileBuildingCards;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Service for resolving and showing dialog windows from viewmodels.
        /// </summary>
        private IDialogService DialogService { get; }

        /// <summary>
        /// Card catalog contains all possible oracle cards locally to avoid querying Scryfall.
        /// </summary>
        private CardCatalog Catalog
        {
            get
            {
                return _catalog;
            }

            set
            {
                _catalog = value;
                CatalogTimestamp = Catalog?.Metadata?.UpdatedAt;
                OnPropertyChanged(nameof(Catalog));
            }
        }

        /// <summary>
        /// Reports status and progress events to subscribers.
        /// </summary>
        public IReporter Reporter { get; }

        /// <summary>
        /// The version of the software currently running.
        /// </summary>
        public string SoftwareVersion
        {
            get
            {
                return _softwareVersion;
            }

            set
            {
                _softwareVersion = value;
                OnPropertyChanged(nameof(SoftwareVersion));
            }
        }

        /// <summary>
        /// Last status args received from the <see cref="Reporter"/>.
        /// </summary>
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

        /// <summary>
        /// Last progress args received from the <see cref="Reporter"/>.
        /// </summary>
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

        /// <summary>
        /// The timestamp of the catalog (when the catalog was last updated by Scryfall).
        /// </summary>
        public DateTime? CatalogTimestamp
        {
            get
            {
                return _catalogTimestamp;
            }

            set
            {
                _catalogTimestamp = value;
                OnPropertyChanged(nameof(CatalogTimestamp));
            }
        }

        /// <summary>
        /// How big, as a percent, to make the card images compared to their default size.
        /// </summary>
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

        /// <summary>
        /// A collection of card view models to display, and later to build the PDF.
        /// </summary>
        public ObservableCollection<CardViewModel> DisplayedCards
        {
            get
            {
                return _displayedCards ?? (_displayedCards = new ObservableCollection<CardViewModel>());
            }
        }

        /// <summary>
        /// Error messages generated during the card building process, stored to display to user.
        /// </summary>
        public ObservableCollection<string> ErrorsWhileBuildingCards
        {
            get
            {
                return _errorsWhileBuildingCards ?? (_errorsWhileBuildingCards = new ObservableCollection<string>());
            }
        }

        #endregion Properties

        #region Commands

        #region BuildCards

        private AsyncCommand _buildCards;

        /// <summary>
        /// Command which builds the list of displayed cards from the user's entered data.
        /// </summary>
        public AsyncCommand BuildCards
        {
            get
            {
                return _buildCards ?? (_buildCards = new AsyncCommand(BuildCards_ExecuteAsync));
            }
        }

        private async Task BuildCards_ExecuteAsync(object parameter)
        {
            if (!(parameter is string str))
            {
                return;
            }

            if (Catalog == null)
            {
                var yesNoDialog = new YesNoDialogViewModel("Card Catalog must be updated before continuing. Would you like to update the Card Catalog (~65 MB) now?");
                if (!(DialogService.ShowDialog(yesNoDialog) ?? false))
                {
                    return;
                }

                await UpdateCatalog_ExecuteAsync();
            }

            DisplayedCards.Clear();

            string[] lines = str.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            Reporter.StatusReported += BuildingCardsErrors;

            foreach (string line in lines)
            {
                Reporter.StartBusy();
                Reporter.Report(this, $"Searching [{line}] in local catalog");
                Card card = Catalog.FindExactCard(line);

                if (card == null)
                {
                    Reporter.Report(this, $"[{line}] returned no results", true);
                    continue;
                }

                List<Card> allPrintings = await ScryfallService.GetAllPrintingsAsync(card.OracleId, Reporter);

                //TODO: Qty
                var cardVm = new CardViewModel(Reporter, allPrintings, 1);
                cardVm.ScaleToPercent(ZoomPercent);
                DisplayedCards.Add(cardVm);
                cardVm.SelectPreferredPrinting();
            }

            Reporter.StatusReported -= BuildingCardsErrors;
            Reporter.Report(this, $"Built {DisplayedCards.Count} cards");
            Reporter.StopBusy();
        }

        private void BuildingCardsErrors(object sender, BoxyStatusEventArgs e)
        {
            if (!e.IsError)
            {
                return;
            }

            ErrorsWhileBuildingCards.Add(e.Message);
        }

        #endregion BuildCards

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
