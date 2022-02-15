using CardMimic.DialogService;
using CardMimic.Model;
using CardMimic.Model.ScryfallData;
using CardMimic.Model.SerializedData;
using CardMimic.Mvvm;
using CardMimic.Properties;
using CardMimic.Reporting;
using CardMimic.Utilities;
using CardMimic.ViewModels.Dialogs;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable ClassNeverInstantiated.Global

namespace CardMimic.ViewModels
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
            ZoomPercent = Settings.Default.ZoomPercent;

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                Version version = ApplicationDeployment.CurrentDeployment.CurrentVersion;
                SoftwareVersion = $"{version.Major}.{version.Minor}.{version.Build}";
            }
            else
            {
                SoftwareVersion = "Debug";
            }

            Reporter.StatusReported += (sender, args) => LastStatus = args;
            Reporter.ProgressReported += (sender, args) => LastProgress = args;
        }

        #endregion Constructors

        #region Fields

        private string _importDeckUri;
        private string _decklistText;
        private string _softwareVersion;
        private CardMimicStatusEventArgs _lastStatus;
        private CardMimicProgressEventArgs _lastProgress;
        private CardCatalog _oracleCatalog;
        private int _zoomPercent;
        private ObservableCollection<CardViewModel> _displayedCards;
        private int _totalCards;
        private double _totalPrice;
        private bool _isFormatLegal;
        private ObservableCollection<string> _errorsWhileBuildingCards;
        private ObservableCollection<string> _savedPdfFilePaths;

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
        /// User entered URI to use for importing a deck. Can be a web url from supported websites, or a local file path.
        /// </summary>
        public string ImportDeckUri
        {
            get
            {
                return _importDeckUri;
            }

            set
            {
                _importDeckUri = value;
                OnPropertyChanged(nameof(ImportDeckUri));
            }
        }

        /// <summary>
        /// Text in the decklist text box.
        /// </summary>
        public string DecklistText
        {
            get
            {
                return _decklistText ?? (_decklistText = string.Empty);
            }

            set
            {
                _decklistText = value;
                OnPropertyChanged(nameof(DecklistText));
            }
        }

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
        public CardMimicStatusEventArgs LastStatus
        {
            get
            {
                return _lastStatus;
            }

            private set
            {
                _lastStatus = value;
                OnPropertyChanged(nameof(LastStatus));
            }
        }

        /// <summary>
        /// Last progress args received from the <see cref="Reporter"/>.
        /// </summary>
        public CardMimicProgressEventArgs LastProgress
        {
            get
            {
                return _lastProgress;
            }

            private set
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
                    card.ScaleToPercent(_zoomPercent);
                }

                Settings.Default.ZoomPercent = _zoomPercent;
                Settings.Default.Save();
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
                if (_displayedCards != null)
                {
                    return _displayedCards;
                }

                _displayedCards = new ObservableCollection<CardViewModel>();
                _displayedCards.CollectionChanged += OnDisplayedCardsOnCollectionChanged;
                return _displayedCards;
            }
        }

        private void OnDisplayedCardsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.NewItems != null && args.NewItems.Count > 0)
            {
                foreach (object item in args.NewItems)
                {
                    if (!(item is CardViewModel cvm))
                    {
                        continue;
                    }

                    cvm.PropertyChanged += PriceWatcher;
                }
            }

            if (args.OldItems != null && args.OldItems.Count > 0)
            {
                foreach (object item in args.OldItems)
                {
                    if (!(item is CardViewModel cvm))
                    {
                        continue;
                    }

                    cvm.PropertyChanged -= PriceWatcher;
                }
            }

            if (!DisplayedCards.Any())
            {
                TotalCards = 0;
                TotalPrice = 0;
                IsFormatLegal = true;
                return;
            }

            TotalCards = DisplayedCards.Select(cvm => cvm.Quantity).Sum();
            IsFormatLegal = DisplayedCards.Select(cvm => cvm.IsLegal).All(boolVal => boolVal);
        }

        private void PriceWatcher(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(CardViewModel.LowestPrice):
                    TotalPrice = DisplayedCards.Select(cvm => cvm.TotalPrice).Sum();
                    break;
                case nameof(CardViewModel.IsLegal):
                    IsFormatLegal = DisplayedCards.Select(cvm => cvm.IsLegal).All(boolVal => boolVal);
                    break;
            }
        }

        /// <summary>
        /// Total price of all the cards generated to be displayed in <see cref="DisplayedCards"/>.
        /// </summary>
        public double TotalPrice
        {
            get
            {
                return _totalPrice;
            }

            set
            {
                _totalPrice = value;
                OnPropertyChanged(nameof(TotalPrice));
                OnPropertyChanged(nameof(IsPriceTooHigh));
            }
        }

        /// <summary>
        /// Total number of cards in the <see cref="DisplayedCards"/>, taking quantity into account.
        /// </summary>
        public int TotalCards
        {
            get
            {
                return _totalCards;
            }

            set
            {
                _totalCards = value;
                OnPropertyChanged(nameof(TotalCards));
            }
        }

        /// <summary>
        /// Indicates whether the price has exceeded the (arbitrary) limit specified by the user/software.
        /// </summary>
        public bool IsPriceTooHigh
        {
            get
            {
                return TotalPrice > Settings.Default.MaxPrice;
            }
        }

        /// <summary>
        /// Text/string of the user selected format.
        /// </summary>
        public string FormatDisplay
        {
            get
            {
                return Settings.Default.SavedFormat.ToString();
            }
        }

        /// <summary>
        /// Indicates whether the set of cards generated is legal in the format specified by the user/software.
        /// </summary>
        public bool IsFormatLegal
        {
            get
            {
                return _isFormatLegal;
            }

            set
            {
                _isFormatLegal = value;
                OnPropertyChanged(nameof(FormatDisplay));
                OnPropertyChanged(nameof(IsFormatLegal));
            }
        }

        /// <summary>
        /// Error messages generated during the card building process, stored to display to user.
        /// </summary>
        public ObservableCollection<string> DisplayErrors
        {
            get
            {
                return _errorsWhileBuildingCards ?? (_errorsWhileBuildingCards = new ObservableCollection<string>());
            }
        }

        /// <summary>
        /// A collection of all PDF files user has created since the app started.
        /// </summary>
        public ObservableCollection<string> SavedPdfFilePaths
        {
            get
            {
                return _savedPdfFilePaths ?? (_savedPdfFilePaths = new ObservableCollection<string>());
            }
        }

        #endregion Properties

        #region Commands

        #region ImportDeck

        private AsyncCommand _importDeck;

        /// <summary>
        /// Command which imports a deck from a remote source (web url, .dec file, etc).
        /// </summary>
        public AsyncCommand ImportDeck
        {
            get
            {
                return _importDeck ?? (_importDeck = new AsyncCommand(ImportDeck_ExecuteAsync));
            }
        }

        private async Task ImportDeck_ExecuteAsync()
        {
            if (string.IsNullOrWhiteSpace(ImportDeckUri))
            {
                return;
            }

            DisplayErrors.Clear();
            Reporter.StartBusy();
            Reporter.StatusReported += BuildingCardsErrors;
            var imported = string.Empty;

            try
            {
                imported = await DeckImport.ImportFromUrl(ImportDeckUri, Reporter);
            }
            catch (Exception exc)
            {
                DisplayError(exc, $"Could not import deck from {ImportDeckUri}\r\n");
            }

            DecklistText += "\r\n" + imported;

            Reporter.StatusReported -= BuildingCardsErrors;
            Reporter.StopBusy();
        }

        #endregion ImportDeck

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

        private async Task SearchSubmit_ExecuteAsync()
        {
            DisplayErrors.Clear();
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
            await Task.Delay(100);
            Reporter.StartBusy();
            Reporter.StartProgress();
            Reporter.Report("Deciphering old one's poem");
            Reporter.StatusReported += BuildingCardsErrors;

            List<SearchLine> lines = DecklistText
                .Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(l => new SearchLine(l))
                .ToList();

            for (var i = 0; i < lines.Count; i++)
            {
                List<Card> cards = OracleCatalog.FindExactCard(lines[i].SearchTerm);

                if (!cards.Any())
                {
                    cards.Add(await ScryfallService.GetFuzzyCardAsync(lines[i].SearchTerm, Reporter));
                }

                if (!cards.Any() || cards.Any(c => c == null))
                {
                    Reporter.Report($"[{lines[i].SearchTerm}] returned no results", true);
                    continue;
                }

                Card preferredCard;

                if (cards.Count > 1)
                {
                    var cardChooser = new ChooseCardDialogViewModel(cards, Reporter);
                    await cardChooser.LoadImagesFromCards();

                    if (!(DialogService.ShowDialog(cardChooser) ?? false))
                    {
                        continue;
                    }

                    preferredCard = ArtPreferences.GetPreferredCard(cardChooser.ChosenCard);
                }
                else
                {
                    preferredCard = ArtPreferences.GetPreferredCard(cards.Single());
                }

                var cardVm = new CardViewModel(Reporter, ArtPreferences, preferredCard, lines[i].Quantity, ZoomPercent);
                DisplayedCards.Add(cardVm);
                await Task.Delay(1);
                Reporter.Progress(i, 0, lines.Count - 1);
            }

            Reporter.StatusReported -= BuildingCardsErrors;
            Reporter.StopBusy();
            Reporter.StopProgress();
        }

        #endregion SearchSubmit

        #region BuildPdf

        private AsyncCommand _buildPdf;

        public AsyncCommand BuildPdf
        {
            get
            {
                return _buildPdf ?? (_buildPdf = new AsyncCommand(BuildPdf_ExecuteAsync));
            }
        }

        private async Task BuildPdf_ExecuteAsync()
        {
            if (DisplayedCards.Any(q => q.IsPopulatingPrints || q.IsLoadingImage))
            {
                DialogService.ShowDialog(new MessageDialogViewModel("Please wait for all images to finish loading before creating the card PDF.", "Wait before continuing..."));
                return;
            }

            Reporter.StartBusy();
            var pdfBuilder = new CardPdfBuilder(Settings.Default.PdfPageSize, Settings.Default.PdfScalingPercent, Settings.Default.PdfHasCutLines, Settings.Default.CutLineSize, Settings.Default.CutLineColor);
            PdfDocument pdfDoc;

            if (Settings.Default.PrintTwoSided)
            {
                pdfDoc = await pdfBuilder.BuildPdfTwoSided(DisplayedCards.ToList(), Reporter);
            }
            else
            {
                pdfDoc = await pdfBuilder.BuildPdfSingleSided(DisplayedCards.ToList(), Reporter);
            }

            string directory = Environment.ExpandEnvironmentVariables(Settings.Default.PdfSaveFolder);
            const string fileName = "BoxyProxies";
            const string ext = ".pdf";
            string fullPath = Path.Combine(directory, fileName + ext);
            var tries = 1;

            while (File.Exists(fullPath))
            {
                fullPath = Path.Combine(directory, fileName + $" ({tries})" + ext);
                tries += 1;
            }

            Reporter.Report($"Summoning the Unknowable One, {fileName}");
            var message = string.Empty;

            Reporter.StopBusy();
            Reporter.StopProgress();

            if (!pdfDoc.CanSave(ref message))
            {
                Reporter.Report(message, true);
                Reporter.StopBusy();
                Reporter.StopProgress();
                return;
            }

            try
            {
                pdfDoc.Save(fullPath);
            }
            catch (Exception e)
            {
                Reporter.Report(e.Message, true);
                DisplayError(e, "Could not save PDF to file.");
                return;
            }

            Reporter.Report("Ritual complete");
            SavedPdfFilePaths.Add(fullPath);

            if (!Settings.Default.PdfOpenWhenSaveDone)
            {
                return;
            }

            try
            {
                Process.Start(fullPath);
            }
            catch (Exception e)
            {
                Reporter.Report(e.Message, true);
                DisplayError(e, "Could not open PDF. Do you have a PDF viewer installed?");
            }
        }

        #endregion BuildPdf

        #region UpdateCatalog

        private AsyncCommand _updateCatalog;

        /// <summary>
        /// Command which updates the locally stored card catalog from Scryfall.
        /// </summary>
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
                Reporter.Report("Cards not found or empty", true);
                Reporter.StopBusy();
                return;
            }

            Reporter.Report("Transcribing secrets of the fish men");
            var catalog = new CardCatalog(oracleBulkData, cards, DateTime.Now);

            try
            {
                catalog.SaveToFile();
            }
            catch (Exception e)
            {
                DisplayError(e, "Could not save card catalog to local disk.");
                Reporter.Report(e.Message, true);
                Reporter.StopBusy();
                return;
            }

            OracleCatalog = catalog;
            Reporter.Report("Secrets hidden in a safe place");
            Reporter.StopBusy();
        }

        #endregion UpdateCatalog
        
        #region OpenSettings

        private RelayCommand _openSettings;

        /// <summary>
        /// Command which opens a dialog for the user to view app settings and change/save them.
        /// </summary>
        public RelayCommand OpenSettings
        {
            get
            {
                return _openSettings ?? (_openSettings = new RelayCommand(OpenSettings_ExecuteAsync));
            }
        }

        private void OpenSettings_ExecuteAsync()
        {
            var settingsVm = new SettingsDialogViewModel();

            if (!(DialogService.ShowDialog(settingsVm) ?? false))
            {
                return;
            }

            Settings.Default.PdfPageSize = settingsVm.PdfPageSize;
            Settings.Default.PdfSaveFolder = settingsVm.PdfSaveFolder;
            Settings.Default.PrintTwoSided = settingsVm.PrintTwoSided;
            Settings.Default.PdfHasCutLines = settingsVm.PdfHasCutLines;
            Settings.Default.CutLineColor = settingsVm.CutLineColor;
            Settings.Default.CutLineSize = settingsVm.CutLineSize;
            Settings.Default.PdfScalingPercent = settingsVm.PdfScalingPercent;
            Settings.Default.PdfOpenWhenSaveDone = settingsVm.PdfOpenWhenSaveDone;
            Settings.Default.MaxPrice = settingsVm.MaxPrice;
            Settings.Default.SavedFormat = settingsVm.SelectedFormat;
            Settings.Default.Save();

            TotalPrice = TotalPrice;
        }

        #endregion OpenSettings

        #region OpenSinglePdf

        private RelayCommand _openSinglePdf;

        public RelayCommand OpenSinglePdf
        {
            get
            {
                return _openSinglePdf ?? (_openSinglePdf = new RelayCommand(OpenSinglePdf_ExecuteAsync));
            }
        }

        private void OpenSinglePdf_ExecuteAsync(object parameter)
        {
            if (!(parameter is string paramAsString))
            {
                return;
            }

            try
            {
                Process.Start(paramAsString);
            }
            catch (Exception exc)
            {
                DisplayError(exc, "Could not open file. It's possible that the file no longer exists, " +
                                  "or there is no PDF reader installed on this computer.");
            }
        }

        #endregion OpenSinglePdf

        #endregion Commands

        #region Methods

        private void BuildingCardsErrors(object sender, CardMimicStatusEventArgs e)
        {
            if (!e.IsError)
            {
                return;
            }

            DisplayErrors.Add(e.Message);
        }

        private void DisplayError(Exception exc, string additionalInfo)
        {
            var message = new StringBuilder($"{additionalInfo}\r\n\r\n");

            while (exc != null)
            {
                message.AppendLine(exc.Message);
                exc = exc.InnerException;
            }

            DialogService.ShowDialog(new MessageDialogViewModel(message.ToString(), "Error Encountered"));
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
