using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Boxy.Views
{
    /// <summary>
    /// Interaction logic for CardView.xaml
    /// </summary>
    public partial class CardView
    {
        public CardView()
        {
            InitializeComponent();
        }

        private void ComboBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!(sender is ComboBox comboBox))
            {
                return;
            }

            if (comboBox.Template.FindName("PART_Popup", comboBox) is Popup pop)
            {
                pop.Placement = PlacementMode.Top;
            }
        }
    }
}
