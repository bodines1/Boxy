﻿using Boxy.DialogService;
using Boxy.Model;
using Boxy.Model.ScryfallData;
using Boxy.Model.SerializedData;
using Boxy.Mvvm;
using Boxy.Properties;
using Boxy.Reporting;
using Boxy.Utilities;
using Boxy.ViewModels.Dialogs;
using PdfSharp;
using PdfSharp.Drawing;
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
using System.Windows.Media.Imaging;

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
            ZoomPercent = Settings.Default.ZoomPercent;

            Reporter.StatusReported += (sender, args) => LastStatus = args;
            Reporter.ProgressReported += (sender, args) => LastProgress = args;
        }

        #endregion Constructors

        #region Fields

        private string _importDeckUri;
        private string _decklistText;
        private string _softwareVersion;
        private BoxyStatusEventArgs _lastStatus;
        private BoxyProgressEventArgs _lastProgress;
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
        public BoxyStatusEventArgs LastStatus
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
        public BoxyProgressEventArgs LastProgress
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

        #region SearchSubmit

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

        #endregion SearchSubmit

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

            List<SearchLine> lines = DecklistText.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).Select(l => new SearchLine(l)).ToList();

            for (var i = 0; i < lines.Count; i++)
            {
                Card card = OracleCatalog.FindExactCard(lines[i].SearchTerm) ?? await ScryfallService.GetFuzzyCardAsync(lines[i].SearchTerm, Reporter);

                if (card == null)
                {
                    Reporter.Report(this, $"[{lines[i].SearchTerm}] returned no results", true);
                    continue;
                }

                Card preferredCard = ArtPreferences.GetPreferredCard(card);

                if (preferredCard.IsDoubleFaced)
                {
                    BitmapSource frontImage = ImageHelper.LoadBitmap(await ImageCaching.GetImageAsync(preferredCard.CardFaces[0].ImageUris.BorderCrop, Reporter));
                    var frontVm = new CardViewModel(Reporter, ArtPreferences, preferredCard, frontImage, lines[i].Quantity, ZoomPercent);

                    BitmapSource backImage = ImageHelper.LoadBitmap(await ImageCaching.GetImageAsync(preferredCard.CardFaces[1].ImageUris.BorderCrop, Reporter));
                    var backVm = new CardViewModel(Reporter, ArtPreferences, preferredCard, backImage, lines[i].Quantity, ZoomPercent, false);

                    DisplayedCards.Add(frontVm);
                    await Task.Delay(10);
                    DisplayedCards.Add(backVm);
                    await Task.Delay(10);

                    Reporter.Progress(this, i, 0, lines.Count - 1);
                }
                else
                {
                    BitmapSource preferredImage = ImageHelper.LoadBitmap(await ImageCaching.GetImageAsync(preferredCard.ImageUris.BorderCrop, Reporter));
                    var cardVm = new CardViewModel(Reporter, ArtPreferences, preferredCard, preferredImage, lines[i].Quantity, ZoomPercent);
                    DisplayedCards.Add(cardVm);
                    await Task.Delay(10);
                    Reporter.Progress(this, i, 0, lines.Count - 1);
                }
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
            Reporter.StartBusy();
            Reporter.StartProgress();
            var pdfBuilder = new CardPdfBuilder(PageSize.Letter, 1, true);

            var images = new List<XImage>();
            int totalCount = DisplayedCards.Aggregate(0, (a, b) => a + b.Quantity);
            var count = 0;

            foreach (CardViewModel t in DisplayedCards)
            {
                for (var j = 0; j < t.Quantity; j++)
                {
                    await Task.Delay(1);
                    Reporter.Progress(this, count, 0, totalCount);
                    Reporter.Report($"Performing ancient ritual {count}/{totalCount}");

                    var enc = new JpegBitmapEncoder { QualityLevel = Settings.Default.PdfJpegQuality };
                    var stream = new MemoryStream();
                    enc.Frames.Add(BitmapFrame.Create(t.CardImage));
                    enc.Save(stream);
                    images.Add(XImage.FromStream(stream));

                    count += 1;
                }
            }

            Reporter.StopProgress();
            await pdfBuilder.DrawImages(images, Reporter);

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

            Reporter.Report($"Summoning the ancient one, {fileName}");
            var message = string.Empty;

            Reporter.StopBusy();
            Reporter.StopProgress();

            if (!pdfBuilder.Document.CanSave(ref message))
            {
                Reporter.Report(pdfBuilder, message, true);
                Reporter.StopBusy();
                Reporter.StopProgress();
                return;
            }

            try
            {
                pdfBuilder.Document.Save(fullPath);
            }
            catch (Exception e)
            {
                Reporter.Report(this, e.Message, true);
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
                Reporter.Report(this, e.Message, true);
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
                Reporter.Report(this, "Cards not found or empty", true);
                Reporter.StopBusy();
                return;
            }

            Reporter.Report(this, "Transcribing secrets of the fish men");
            var catalog = new CardCatalog(oracleBulkData, cards, DateTime.Now);

            try
            {
                catalog.SaveToFile();
            }
            catch (Exception e)
            {
                DisplayError(e, "Could not save card catalog to local disk.");
                Reporter.Report(this, e.Message, true);
                Reporter.StopBusy();
                return;
            }

            OracleCatalog = catalog;
            Reporter.Report(this, "Secrets hidden in a safe place");
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
            Settings.Default.PdfJpegQuality = settingsVm.PdfJpegQuality;
            Settings.Default.PdfHasCutLines = settingsVm.PdfHasCutLines;
            Settings.Default.PdfScaling = settingsVm.PdfScaling;
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

        private void BuildingCardsErrors(object sender, BoxyStatusEventArgs e)
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
