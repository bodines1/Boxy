using Boxy.DialogService;
using Boxy.Model;
using Boxy.Model.ScryfallData;
using Boxy.Model.SerializedData;
using Boxy.Mvvm;
using Boxy.Reporting;
using Boxy.ViewModels.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Deployment.Application;
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
        /// <param name="cardCatalog">Holds a queryable set of all oracle cards (no duplicate printings) to prevent excess queries to ScryfallAPI.</param>
        /// <param name="artworkPreferences">Holds a mapping of Oracle Ids to Card Ids to store and retrieve a user's preferred printing of a card.</param>
        public MainViewModel(IDialogService dialogService, IReporter reporter, CardCatalog cardCatalog, ArtworkPreferences artworkPreferences)
        {
            DialogService = dialogService;
            Reporter = reporter;
            OracleCatalog = cardCatalog;
            ArtPreferences = artworkPreferences;
            
            SoftwareVersion = ApplicationDeployment.IsNetworkDeployed ? ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString() : "Debug";
            ZoomPercent = 100;

            Reporter.StatusReported += (sender, args) => LastStatus = args;
            Reporter.ProgressReported += (sender, args) => LastProgress = args;
        }

        #endregion Constructors

        #region Fields

        private string _softwareVersion;
        private BoxyStatusEventArgs _lastStatus;
        private BoxyProgressEventArgs _lastProgress;
        private CardCatalog _oracleCatalog;
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
        /// Holds a mapping of Oracle Ids to Card Ids to store and retrieve a user's preferred printing of a card.
        /// </summary>
        private ArtworkPreferences ArtPreferences { get; }

        /// <summary>
        /// Card catalog contains all possible oracle cards locally to avoid querying Scryfall.
        /// </summary>
        public CardCatalog OracleCatalog
        {
            get
            {
                return _oracleCatalog;
            }

            set
            {
                _oracleCatalog = value;
                OnPropertyChanged(nameof(OracleCatalog));
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

        #region SearchSubmit

        private AsyncCommand _searchSubmit;

        /// <summary>
        /// Command which builds the list of displayed cards from the user's entered data.
        /// </summary>
        public AsyncCommand SearchSubmit
        {
            get
            {
                return _searchSubmit ?? (_searchSubmit = new AsyncCommand(SearchSubmit_ExecuteAsync));
            }
        }

        private async Task SearchSubmit_ExecuteAsync(object parameter)
        {
            if (!(parameter is string str))
            {
                return;
            }

            ErrorsWhileBuildingCards.Clear();
            TimeSpan? timeSinceUpdate = DateTime.Now - OracleCatalog.UpdateTime;

            if (timeSinceUpdate == null)
            {
                var yesNoDialog = new YesNoDialogViewModel("Card Catalog must be updated before continuing. Would you like to update the Card Catalog (~65 MB) now?", "Update?");
                if (!(DialogService.ShowDialog(yesNoDialog) ?? false))
                {
                    return;
                }

                await UpdateCatalog_ExecuteAsync();
            }

            if (timeSinceUpdate > TimeSpan.FromDays(7))
            {
                var yesNoDialog = new YesNoDialogViewModel("Card Catalog is out of date, it is recommended you get a new catalog now." +
                                                           "If you don't, cards may not appear in search results or you may receive old " +
                                                           "imagery. Click 'Yes' to update the Card Catalog (~65 MB) now or 'No' use the old catalog.", "Update?");
                if (!(DialogService.ShowDialog(yesNoDialog) ?? false))
                {
                    return;
                }

                await UpdateCatalog_ExecuteAsync();
            }

            DisplayedCards.Clear();
            Reporter.StartBusy();
            Reporter.StartProgress();
            Reporter.Report("Building viewable cards");
            Reporter.StatusReported += BuildingCardsErrors;

            string[] lines = str.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                
            }

            for (var i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                Reporter.Report(this, $"Searching [{line}] in local catalog");
                Card card = OracleCatalog.FindExactCard(line);

                if (card == null)
                {
                    Reporter.Report(this, $"[{line}] returned no results", true);
                    continue;
                }

                List<Card> allPrintings = await ScryfallService.GetAllPrintingsAsync(card.OracleId, Reporter);

                //TODO: Qty
                var cardVm = new CardViewModel(Reporter, ArtPreferences, allPrintings, 1);
                cardVm.ScaleToPercent(ZoomPercent);
                DisplayedCards.Add(cardVm);
                cardVm.SelectPreferredPrinting();
                Reporter.Progress(this, i, 0, lines.Length - 1);
            }

            Reporter.StatusReported -= BuildingCardsErrors;
            Reporter.Report(this, $"Built {DisplayedCards.Count} cards");
            Reporter.StopBusy();
            Reporter.StopProgress();
        }

        private void BuildingCardsErrors(object sender, BoxyStatusEventArgs e)
        {
            if (!e.IsError)
            {
                return;
            }

            ErrorsWhileBuildingCards.Add(e.Message);
        }

        #endregion SearchSubmit

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
            List<Card> cards = await ScryfallService.GetBulkCards(oracleBulkData.PermalinkUri, Reporter);

            if (cards == null || cards.Count == 0)
            {
                Reporter.Report(this, "Cards not found or empty", true);
                Reporter.StopBusy();
                return;
            }

            Reporter.Report(this, "Converting catalog to JSON");
            var catalog = new CardCatalog(oracleBulkData, cards, DateTime.Now);

            try
            {
                Reporter.Report(this, "Saving locally");
                catalog.SaveToFile();
            }
            catch (Exception e)
            {
                DisplayError(e, "Could not update Card Catalog.");
                Reporter.Report(this, "Failed to save");
                Reporter.StopBusy();
                return;
            }

            OracleCatalog = catalog;
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
            ArtPreferences.SaveToFile();
            base.Cleanup();
        }

        #endregion Methods
    }
}
