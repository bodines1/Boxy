using Boxy.Model;
using Boxy.Model.ScryfallData;
using Boxy.Resources;
using Boxy.Resources.Mvvm;
using Boxy.Resources.Reporting;
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

        public CardViewModel(IReporter reporter, List<Card> allPrintings, int quantity)
        {
            Reporter = reporter;
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

        public ObservableCollection<Card> AllPrintings
        {
            get
            {
                return _allPrintings ?? (_allPrintings = new ObservableCollection<Card>());
            }
        }

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
                ArtworkPreferences.UpdatePreferredCard(_selectedPrinting);
                OnPropertyChanged(nameof(SelectedPrinting));
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

        public void SelectPreferredPrinting()
        {
            SelectedPrinting = ArtworkPreferences.GetPreferredCard(AllPrintings.ToList());
        }

        public void ScaleToPercent(double percent)
        {
            ImageWidth = DefaultImageWidth * percent / 100;
            ImageHeight = DefaultImageHeight * percent / 100;
        }

        #endregion Methods
    }
}
