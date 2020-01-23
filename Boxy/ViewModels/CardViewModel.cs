using Boxy.Model.ScryfallData;
using Boxy.Model.SerializedData;
using Boxy.Mvvm;
using Boxy.Reporting;
using Boxy.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Media.Imaging;

namespace Boxy.ViewModels
{
    public class CardViewModel : ViewModelBase
    {
        #region Constructors

        public CardViewModel(IReporter reporter, ArtworkPreferences artPreferences, List<Card> allPrintings, int quantity)
        {
            Reporter = reporter;
            ArtPreferences = artPreferences;
            allPrintings.ForEach(AllPrintings.Add);
            Quantity = quantity;
            ImageWidth = DefaultImageWidth;
            ImageHeight = DefaultImageHeight;
        }

        #endregion Constructors

        #region Fields

        private const double DefaultImageWidth = 240;
        private const double DefaultImageHeight = 340;
        private ObservableCollection<Card> _allPrintings;
        private Card _selectedPrinting;
        private int _quantity;
        private BitmapSource _cardImage;
        private double _imageWidth;
        private double _imageHeight;

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
                _selectedPrinting = value;
                UpdateCardImage();
                ArtPreferences.UpdatePreferredCard(_selectedPrinting);
                OnPropertyChanged(nameof(SelectedPrinting));
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
                
                OnPropertyChanged(nameof(Quantity));
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
            Bitmap bitmap = await ImageCaching.GetImageAsync(SelectedPrinting, Reporter);
            CardImage = ImageHelper.LoadBitmap(bitmap);
        }

        /// <summary>
        /// Make the <see cref="SelectedPrinting"/> the "preferred" print using <see cref="ArtworkPreferences"/>.
        /// </summary>
        public void SelectPreferredPrinting()
        {
            SelectedPrinting = ArtPreferences.GetPreferredCard(AllPrintings.ToList());
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
