using Boxy.Model;
using Boxy.Model.ScryfallData;
using Boxy.Mvvm;
using Boxy.Reporting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace Boxy.ViewModels
{
    public class CardViewModel : ViewModelBase
    {
        #region Constructors

        public CardViewModel(IReporter reporter, Card card, List<Card> allPrintings, int quantity)
        {
            Reporter = reporter;
            allPrintings.ForEach(AllPrintings.Add);
            SelectedPrinting = card;
            Quantity = quantity;
        }

        #endregion Constructors

        #region Fields

        private ObservableCollection<Card> _allPrintings;
        private Card _selectedPrinting;
        private int _quantity;
        private BitmapSource _cardImage;

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

        #endregion Properties

        #region Commands

        private RelayCommand _changeQuantity;

        public RelayCommand ChangeQuantity
        {
            get
            {
                return _changeQuantity ?? (_changeQuantity = new RelayCommand(ChangeQuantity_Execute));
            }
        }

        private void ChangeQuantity_Execute(object parameter)
        {
            if (!int.TryParse(parameter.ToString(), out int paramAsInt))
            {
                Reporter.Report($"Couldn't change QTY using param of {parameter}");
                return;
            }

            Quantity += paramAsInt;
        }

        #endregion Commands

        #region Methods

        private async void UpdateCardImage()
        {
            Reporter.Report(this, $"Getting '{SelectedPrinting.Name}' artwork");
            Bitmap bitmap = await ImageCaching.GetImageAsync(SelectedPrinting);
            CardImage = ImageHelper.LoadBitmap(bitmap);
        }

        #endregion Methods
    }
}
