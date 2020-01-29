using Boxy.DialogService;
using Boxy.Model;
using Boxy.Model.ScryfallData;
using Boxy.Mvvm;
using Boxy.Reporting;
using Boxy.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Boxy.ViewModels.Dialogs
{
    /// <summary>
    /// View model for interacting with a message dialog window.
    /// </summary>
    public class ChooseCardDialogViewModel : DialogViewModelBase
    {
        public struct ImageWithIndex
        {
            public int Index { get; set; }

            public BitmapSource Image { get; set; }
        }

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="ChooseCardDialogViewModel"/>.
        /// </summary>
        public ChooseCardDialogViewModel(List<Card> cards, IReporter reporter)
        {
            if (cards == null || !cards.Any())
            {
                throw new ArgumentNullException(nameof(cards), @"Must pass valid cards in to use this view model.");
            }

            Cards = cards;
            Reporter = reporter;
            CardName = cards[0].Name;
        }

        #endregion Constructors

        #region Fields

        private ObservableCollection<ImageWithIndex> _images;

        #endregion Fields

        #region Properties

        private List<Card> Cards { get; }

        private IReporter Reporter { get; }

        public string CardName { get; }

        /// <summary>
        /// The images representing the possible card options to the user.
        /// </summary>
        public ObservableCollection<ImageWithIndex> Images
        {
            get
            {
                return _images ?? (_images = new ObservableCollection<ImageWithIndex>());
            }
        }

        /// <summary>
        /// The card chosen by the user.
        /// </summary>
        public Card ChosenCard { get; private set; }

        #endregion Properties

        #region Commands

        private RelayCommand _chooseCard;

        /// <summary>
        /// Gets a command which chooses a card from the options based on user input.
        /// </summary>
        public RelayCommand ChooseCard
        {
            get
            {
                return _chooseCard ?? (_chooseCard = new RelayCommand(ChooseCard_Execute));
            }
        }

        private void ChooseCard_Execute(object parameter)
        {
            if (!(parameter is ImageWithIndex image))
            {
                return;
            }

            ChosenCard = Cards[image.Index];
            RequestClose(new DialogCloseRequestedEventArgs(true));
        }

        #endregion Commands

        #region Methods

        public async Task LoadImagesFromCards()
        {
            for (var i = 0; i < Cards.Count; i++)
            {
                Card card = Cards[i];
                if (string.IsNullOrWhiteSpace(card?.ImageUris?.Small))
                {
                    continue;
                }

                BitmapSource image = ImageHelper.LoadBitmap(await ScryfallService.GetImageAsync(card.ImageUris.BorderCrop, Reporter));

                if (image != null)
                {
                    Images.Add(new ImageWithIndex { Image = image, Index = i });
                }
            }
        }

        #endregion Methods
    }
}
