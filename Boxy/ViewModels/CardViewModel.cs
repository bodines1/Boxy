using CardMimic.Model;
using CardMimic.Model.ScryfallData;
using CardMimic.Model.SerializedData;
using CardMimic.Mvvm;
using CardMimic.Properties;
using CardMimic.Reporting;
using CardMimic.Utilities;
using PdfSharp.Drawing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Media.Imaging;

namespace CardMimic.ViewModels
{
    public class CardViewModel : ViewModelBase
    {
        #region Constructors

        public CardViewModel(IReporter reporter, ArtworkPreferences artPreferences, Card card, int quantity, double zoomPercent)
        {
            Reporter = reporter;
            ArtPreferences = artPreferences;
            Quantity = quantity;

            if (card.IsToken)
            {
                IsLegal = true;
            }
            else
            {
                PropertyInfo specificFormatPropInfo = card.Legalities.GetType().GetProperty(Settings.Default.SavedFormat.ToString()) ?? throw new ArgumentOutOfRangeException(nameof(Settings.Default.SavedFormat));
                IsLegal = specificFormatPropInfo.GetValue(card.Legalities).ToString() == "legal";
            }

            UpdateImageTimer = new Timer(100) { AutoReset = false };
            UpdateImageTimer.Elapsed += UpdateImageTimerOnElapsed;

            ScaleToPercent(zoomPercent);
            LoadPrints(card);

            Settings.Default.SettingsSaving += DefaultOnSettingsSaving;
        }

        #endregion Constructors

        #region Fields

        private const double DefaultImageWidth = 240;
        private const double DefaultImageHeight = 340;
        private ObservableCollection<Card> _allPrintings;
        private Card _selectedPrinting;
        private int _selectedPrintingIndex;
        private int _quantity;
        private BitmapSource _frontImage;
        private BitmapSource _backImage;
        private double _imageWidth;
        private double _imageHeight;
        private bool _isPopulatingPrints;
        private double _lowestPrice;
        private double _totalPrice;
        private bool _isLegal;
        private bool _isLoadingImage;

        #endregion Fields

        #region Properties

        private IReporter Reporter { get; }

        private ArtworkPreferences ArtPreferences { get; }

        private Timer UpdateImageTimer { get; }

        /// <summary>
        /// All printings of the card.
        /// </summary>
        public ObservableCollection<Card> AllPrintings
        {
            get
            {
                return _allPrintings ?? (_allPrintings = new ObservableCollection<Card>());
            }
        }

        /// <summary>
        /// The printing of the card selected by the user, should result in an image specific to that printing being displayed.
        /// </summary>
        public Card SelectedPrinting
        {
            get
            {
                return _selectedPrinting;
            }

            set
            {
                if (IsPopulatingPrints)
                {
                    return;
                }

                _selectedPrinting = value;
                UpdateImageTimer.Stop();
                UpdateImageTimer.Start();
                ArtPreferences.UpdatePreferredCard(_selectedPrinting);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The index of which item the user has selected, makes sure the UI reflects their selected item when it is selected by code.
        /// </summary>
        public int SelectedPrintingIndex
        {
            get
            {
                return _selectedPrintingIndex;
            }

            private set
            {
                _selectedPrintingIndex = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Bindable bitmap source for UI.
        /// </summary>
        public BitmapSource FrontImage
        {
            get
            {
                return _frontImage;
            }

            set
            {
                _frontImage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Bindable bitmap source for UI.
        /// </summary>
        public BitmapSource BackImage
        {
            get
            {
                return _backImage;
            }

            set
            {
                _backImage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// User created quantity, used later to create a printable PDF.
        /// </summary>
        public int Quantity
        {
            get
            {
                return _quantity;
            }

            set
            {
                if (value < 0)
                {
                    _quantity = 0;
                }
                else if (value > 99)
                {
                    _quantity = 99;
                }
                else
                {
                    _quantity = value;
                }

                TotalPrice = LowestPrice * _quantity;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The current display width of the image.
        /// </summary>
        public double ImageWidth
        {
            get
            {
                return _imageWidth;
            }

            set
            {
                _imageWidth = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The current display height of the image.
        /// </summary>
        public double ImageHeight
        {
            get
            {
                return _imageHeight;
            }

            set
            {
                _imageHeight = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Indicates that the view model is busy populating the list of prints from scryfall.
        /// </summary>
        public bool IsPopulatingPrints
        {
            get
            {
                return _isPopulatingPrints;
            }

            set
            {
                _isPopulatingPrints = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Lowest price found from all the available printings.
        /// </summary>
        public double LowestPrice
        {
            get
            {
                return _lowestPrice;
            }

            set
            {
                _lowestPrice = Math.Round(value, 2);
                TotalPrice = _lowestPrice * Quantity;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Total price, LowestPrice * Quantity.
        /// </summary>
        public double TotalPrice
        {
            get
            {
                return _totalPrice;
            }

            set
            {
                _totalPrice = Math.Round(value, 2);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Indicates whether this card/set of cards is legal in the required format.
        /// </summary>
        public bool IsLegal
        {
            get
            {
                return _isLegal;
            }

            set
            {
                _isLegal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Indicates when a background task is busy loading an image from the services.
        /// </summary>
        public bool IsLoadingImage
        {
            get
            {
                return _isLoadingImage;
            }

            private set
            {
                isLoadingImage = value;
                OnPropertyChanged();
            }
        }

        #endregion Properties

        #region Methods

        private async void UpdateImageTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            UpdateImageTimer.Stop();
            await DispatcherHelper.UiDispatcher.InvokeAsync(UpdateCardImage);
        }

        private void DefaultOnSettingsSaving(object sender, CancelEventArgs e)
        {
            Card card = SelectedPrinting ?? AllPrintings.FirstOrDefault();

            if (card == null)
            {
                return;
            }

            if (card.IsToken)
            {
                IsLegal = true;
            }
            else
            {
                PropertyInfo specificFormatPropInfo = card.Legalities.GetType().GetProperty(Settings.Default.SavedFormat.ToString()) ?? throw new ArgumentOutOfRangeException(nameof(Settings.Default.SavedFormat));
                IsLegal = specificFormatPropInfo.GetValue(card.Legalities).ToString() == "legal";
            }
        }

        /// <summary>
        /// Select the "preferred" print using <see cref="ArtworkPreferences"/>.
        /// </summary>
        private async void LoadPrints(Card card)
        {
            IsPopulatingPrints = true;

            List<Card> prints = await ScryfallService.GetAllPrintingsAsync(card, Reporter);

            var prices = new List<double>();
            foreach (Card print in prints)
            {
                if (double.TryParse(print.Prices?.Usd, out double valAsDouble))
                {
                    prices.Add(valAsDouble);
                }
            }

            LowestPrice = prices.Any() ? prices.Min() : 0;

            var indexCounter = 0;

            foreach (Card print in prints)
            {
                if (card.Id == print.Id)
                {
                    SelectedPrintingIndex = indexCounter;
                }

                AllPrintings.Add(print);
                indexCounter++;
            }

            if (!AllPrintings.Any())
            {
                AllPrintings.Add(card);
            }

            IsPopulatingPrints = false;
            SelectedPrinting = AllPrintings[SelectedPrintingIndex];
            await UpdateCardImage();
        }

        private async Task UpdateCardImage()
        {
            IsLoadingImage = true;

            if (SelectedPrinting.IsDoubleFaced)
            {
                FrontImage = ImageHelper.LoadBitmap(await ImageCaching.GetImageAsync(SelectedPrinting.CardFaces[0].ImageUris.BorderCrop, Reporter));
                BackImage = ImageHelper.LoadBitmap(await ImageCaching.GetImageAsync(SelectedPrinting.CardFaces[1].ImageUris.BorderCrop, Reporter));
            }
            else
            {
                FrontImage = ImageHelper.LoadBitmap(await ImageCaching.GetImageAsync(SelectedPrinting.ImageUris.BorderCrop, Reporter));
                BackImage = null;
            }

            IsLoadingImage = false;
        }

        public CardPageImage GetFullCardPageImage()
        {
            if (FrontImage is null)
            {
                throw new InvalidOperationException($"Called {nameof(GetFullCardPageImage)} while {nameof(FrontImage)} is null.");
            }

            var result = new CardPageImage();

            var frontStream = new MemoryStream();
            var frontEnc = new JpegBitmapEncoder { QualityLevel = 97 };
            frontEnc.Frames.Add(BitmapFrame.Create(FrontImage));
            frontEnc.Save(frontStream);
            result.FrontImage = XImage.FromStream(frontStream);

            if (!SelectedPrinting.IsDoubleFaced)
            {
                return result;
            }

            if (BackImage is null)
            {
                throw new InvalidOperationException($"Called {nameof(GetFullCardPageImage)} on a double faced card while {nameof(BackImage)} is null.");
            }

            var backStream = new MemoryStream();
            var backEnc = new JpegBitmapEncoder { QualityLevel = 97 };
            backEnc.Frames.Add(BitmapFrame.Create(BackImage));
            backEnc.Save(backStream);
            result.BackImage = XImage.FromStream(backStream);

            return result;
        }

        public CardPageImage GetFrontCardPageImage()
        {
            if (FrontImage is null)
            {
                throw new InvalidOperationException($"Called {nameof(GetFrontCardPageImage)} while {nameof(FrontImage)} is null.");
            }

            var result = new CardPageImage();

            var stream = new MemoryStream();
            var enc = new JpegBitmapEncoder { QualityLevel = 97 };
            enc.Frames.Add(BitmapFrame.Create(FrontImage));
            enc.Save(stream);
            result.FrontImage = XImage.FromStream(stream);

            return result;
        }

        public CardPageImage GetBackCardPageImage()
        {
            if (BackImage is null)
            {
                throw new InvalidOperationException($"Called {nameof(GetBackCardPageImage)} while {nameof(BackImage)} is null.");
            }

            var result = new CardPageImage();

            var stream = new MemoryStream();
            var enc = new JpegBitmapEncoder { QualityLevel = 97 };
            enc.Frames.Add(BitmapFrame.Create(BackImage));
            enc.Save(stream);
            result.FrontImage = XImage.FromStream(stream);

            return result;
        }

        /// <summary>
        /// Scales the image from the default (240 x 340) to a percent of that default.
        /// </summary>
        /// <param name="percent">The percent to scale.</param>
        public void ScaleToPercent(double percent)
        {
            ImageWidth = DefaultImageWidth * percent / 100;
            ImageHeight = DefaultImageHeight * percent / 100;
        }

        #endregion Methods
    }
}
