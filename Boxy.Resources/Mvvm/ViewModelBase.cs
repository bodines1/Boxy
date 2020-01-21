using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CellManager.Resources.MvvmUtilities
{
    /// <summary>
    /// Base implementation of <see cref="INotifyPropertyChanged"/> for view models.
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the property with the given name.
        /// </summary>
        /// <param name="propertyName">Name of the property which has changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// A place to put cleanup code for when a view model is no longer needed.
        /// </summary>
        public virtual void Cleanup()
        {
        }
    }
}
