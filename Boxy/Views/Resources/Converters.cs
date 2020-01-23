using Boxy.Utilities;
using LambdaConverters;
using System;
using System.Windows;
using System.Windows.Data;

namespace Boxy.Views.Resources
{
    /// <summary>
    /// Contains common converters for views.
    /// </summary>
    public static class Converters
    {
        /// <summary>
        /// Converts a boolean value to visibility.  Visible if true, hidden if false.
        /// </summary>
        public static readonly IValueConverter FalseToHidden =
            ValueConverter.Create<bool, Visibility>(e => e.Value ? Visibility.Visible : Visibility.Hidden);

        /// <summary>
        /// Converts a boolean value to visibility.  Visible if true, collapsed if false.
        /// </summary>
        public static readonly IValueConverter FalseToCollapsed =
            ValueConverter.Create<bool, Visibility>(e => e.Value ? Visibility.Visible : Visibility.Collapsed);

        /// <summary>
        /// Converts a boolean value to visibility.  Hidden if true, visible if false.
        /// </summary>
        public static readonly IValueConverter TrueToHidden =
            ValueConverter.Create<bool, Visibility>(e => e.Value ? Visibility.Hidden : Visibility.Visible);

        /// <summary>
        /// Converts a boolean value to visibility.  Collapsed if true, visible if false.
        /// </summary>
        public static readonly IValueConverter TrueToCollapsed =
            ValueConverter.Create<bool, Visibility>(e => e.Value ? Visibility.Collapsed : Visibility.Visible);

        /// <summary>
        /// Converts a null value to false and a not null value to true.
        /// </summary>
        public static readonly IValueConverter NullToFalse = ValueConverter.Create<object, bool>(e => e.Value != null);

        /// <summary>
        /// Converts an enum to its description attribute for display.
        /// </summary>
        public static readonly IValueConverter EnumToDescription =
            ValueConverter.Create<Enum, string>(e => e.Value.ToDescription());

        /// <summary>
        /// True if string is null, string.Empty, or whitespace.
        /// </summary>
        public static readonly IValueConverter IsNullOrWhitespace =
            ValueConverter.Create<object, bool>(e => string.IsNullOrWhiteSpace(e.Value as string));

        /// <summary>
        /// Visible if not null, otherwise collapsed.
        /// </summary>
        public static readonly IValueConverter NullToCollapsed =
            ValueConverter.Create<object, Visibility>(e => e.Value == null ? Visibility.Collapsed : Visibility.Visible);

        /// <summary>
        /// Collapsed if not null, otherwise visible.
        /// </summary>
        public static readonly IValueConverter NullToVisible =
            ValueConverter.Create<object, Visibility>(e => e.Value == null ? Visibility.Visible : Visibility.Collapsed);

        /// <summary>
        /// Visible if not null, otherwise hidden.
        /// </summary>
        public static readonly IValueConverter NullToHidden =
            ValueConverter.Create<object, Visibility>(e => e.Value == null ? Visibility.Hidden : Visibility.Visible);
    }
}
