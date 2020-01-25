using Boxy.Model;
using Boxy.Model.ScryfallData;
using Boxy.Model.SerializedData;
using Boxy.Mvvm;
using Boxy.Reporting;
using Boxy.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private bool _isUpdatingImage;
        private bool _isFront;

        #endregion Fields

        #region Properties

        private IReporter Reporter { get; }

        private ArtworkPreferences ArtPreferences { get; }

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
        /// Indicates that an image update has been triggered, throttles the amount of triggers possible to 10/second.
        /// </summary>
        public bool IsUpdatingImage
        {
            get
            {
                return _isUpdatingImage;
            }

            set
            {
                _isUpdatingImage = value;
                OnPropertyChanged(nameof(IsUpdatingImage));
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
                
                OnPropertyChanged(nameof(Quantity));
            }
        }

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
