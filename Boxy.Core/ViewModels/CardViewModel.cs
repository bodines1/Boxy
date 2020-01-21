using Boxy.Model.ScryfallData;
using Boxy.Mvvm;
using System.Collections.ObjectModel;
using System.Drawing;

namespace Boxy.ViewModels
{
    public class CardViewModel : ViewModelBase
    {
        #region Constructors

        public CardViewModel(Card card)
        {
            Card = card;
            SelectedPrinting = card.Set;
        }

        #endregion Constructors

        #region Fields

        private ObservableCollection<Card> _allPrintings;
        private int _quantity;
        private Bitmap _cardImage;
        private ObservableCollection<string> _availablePrintings;
        private string _selectedPrinting;

        #endregion Fields

        #region Properties

        public Card Card { get; }


        public ObservableCollection<Card> AllPrintings
        {
            get
            {
                return _allPrintings ?? (_allPrintings = new ObservableCollection<Card>());
            }
        }

        public Bitmap CardImage
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
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
            }
        }


        public ObservableCollection<string> AvailablePrintings
        {
            get
            {
                return _availablePrintings ?? (_availablePrintings = new ObservableCollection<string>());
            }
        }


        public string SelectedPrinting
        {
            get
            {
                return _selectedPrinting;
            }

            set
            {
                _selectedPrinting = value;



                OnPropertyChanged(nameof(SelectedPrinting));
            }
        }

        #endregion Properties
    }
}
