using Boxy.Model;
using Boxy.Model.ScryfallData;
using Boxy.Model.SerializedData;
using Boxy.Mvvm;
using Boxy.Reporting;
using Boxy.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Boxy.ViewModels
{
    public class CardViewModel : ViewModelBase
    {
        #region Constructors

        public CardViewModel(IReporter reporter, ArtworkPreferences artPreferences, Card card, BitmapSource cardImage, int quantity, double zoomPercent, bool isFront = true)
        {
            Reporter = reporter;
            ArtPreferences = artPreferences;
            CardImage = cardImage;
            Quantity = quantity;
            IsLegal = card.Legalities.Pioneer == "legal";
            _isFront = isFront;

            ScaleToPercent(zoomPercent);
            LoadPrints(card);
        }

        #endregion Constructors

        #region Fields

        private const double DefaultImageWidth = 240;
        private const double DefaultImageHeight = 340;
        private ObservableCollection<Card> _allPrintings;
        private Card _selectedPrinting;
        private int _selectedPrintingIndex;
        private int _quantity;
        private BitmapSource _cardImage;
        private double _imageWidth;
        private double _imageHeight;
        private bool _isPopulatingPrints;
        private bool _isFront;
        private double _lowestPrice;
        private double _totalPrice;
        private bool _isLegal;

        #endregion Fields

        #region Properties

        private IReporter Reporter { get; }

        private ArtworkPreferences ArtPreferences { get; }

        /// <summary>
        /// Indicates that an image update has been triggered, throttles the amount of triggers possible to 10/second.
        /// </summary>
        private bool IsUpdatingImage { get; set; }

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
                UpdateCardImage();
                ArtPreferences.UpdatePreferredCard(_selectedPrinting);
                OnPropertyChanged(nameof(SelectedPrinting));
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

            set
            {
                _selectedPrintingIndex = value;
                OnPropertyChanged(nameof(SelectedPrintingIndex));
            }
        }

        /// <summary>
        /// Bindable bitmap source for UI.
        /// </summary>
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
                    _quantity = value > 99 ? 99 : value;
                }
                
                
                TotalPrice = LowestPrice * _quantity;
                OnPropertyChanged(nameof(Quantity));
            }
        }

        /// <summary>
        /// Indicates whether this is the front of a card. Only relevant for double faced cards.
        /// </summary>
        public bool IsFront
        {
            get
            {
                return _isFront;
            }

            set
            {
                _isFront = value;
                UpdateCardImage();
                OnPropertyChanged(nameof(IsFront));
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
                OnPropertyChanged(nameof(ImageWidth));
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
                OnPropertyChanged(nameof(ImageHeight));
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
                OnPropertyChanged(nameof(IsPopulatingPrints));
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
                _lowestPrice = value;
                TotalPrice = _lowestPrice * Quantity;
                OnPropertyChanged(nameof(LowestPrice));
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
                _totalPrice = value;
                OnPropertyChanged(nameof(TotalPrice));
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
                OnPropertyChanged(nameof(IsLegal));
            }
        }

        #endregion Properties

        #region Methods

        private async void UpdateCardImage()
        {
            if (IsUpdatingImage)
            {
                return;
            }

            IsUpdatingImage = true;
            await Task.Delay(100);

            CardImage = SelectedPrinting.IsDoubleFaced
                ? ImageHelper.LoadBitmap(await ImageCaching.GetImageAsync(SelectedPrinting.CardFaces[IsFront ? 0 : 1].ImageUris.BorderCrop, Reporter))
                : ImageHelper.LoadBitmap(await ImageCaching.GetImageAsync(SelectedPrinting.ImageUris.BorderCrop, Reporter));

            IsUpdatingImage = false;
        }

        /// <summary>
        /// Select the "preferred" print using <see cref="ArtworkPreferences"/>.
        /// </summary>
        private async void LoadPrints(Card card)
        {
            IsPopulatingPrints = true;

            List<Card> prints = await ScryfallService.GetAllPrintingsAsync(card, Reporter);

            if (IsFront)
            {
                IEnumerable<double> prices = prints.Select(c =>
                {
                    bool success = double.TryParse(c.Prices.Usd, out double valAsDouble);
                    return success ? valAsDouble : double.MaxValue;
                });

                LowestPrice = prices.Min();
            }
            else
            {
                LowestPrice = 0;
            }

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

            IsPopulatingPrints = false;
            SelectedPrinting = AllPrintings[SelectedPrintingIndex];
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
