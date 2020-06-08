using CardMimic.Utilities;
using LambdaConverters;
using PdfSharp.Drawing;
using System;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;

namespace CardMimic.Views.Resources
{
    /// <summary>
    /// Contains common converters for views.
    /// </summary>
    public static class Converters
    {
        /// <summary>
        /// Negates a boolean value.
        /// </summary>
        public static readonly IValueConverter BooleanNot =
            ValueConverter.Create<bool, bool>(e => !e.Value);

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

        /// <summary>
        /// Visible if not null, otherwise hidden.
        /// </summary>
        public static readonly IValueConverter Multiply =
            ValueConverter.Create<object, double, object>(args =>
            {
                double valueAsDouble = double.Parse(args.Value.ToString());
                double paramAsDouble = double.Parse(args.Parameter.ToString());

                return valueAsDouble * paramAsDouble;
            });

        /// <summary>
        /// Converts a bool true/false to Legal/Not Legal.
        /// </summary>
        public static readonly IValueConverter BoolToLegal =
            ValueConverter.Create<bool, string>(e => e.Value ? "Legal" : "Not Legal");

        /// <summary>
        /// Converts a full path to just the file name (no extension).
        /// </summary>
        public static readonly IValueConverter PathToFileName =
            ValueConverter.Create<string, string>(e => Path.GetFileNameWithoutExtension(e.Value));

        /// <summary>
        /// Converts a XKnownColor to the corresponding windows brush.
        /// </summary>
        public static readonly IValueConverter ColorNameToBrush =
            ValueConverter.Create<object, Brush>(e =>
            {
                if (!(e.Value is XKnownColor xKnownColor))
                {
                    return new SolidColorBrush(Colors.Transparent);
                }

                XColor xColor = XColor.FromKnownColor(xKnownColor);
                Color color = Color.FromArgb((byte) (xColor.A * 255), xColor.R, xColor.G, xColor.B);
                return new SolidColorBrush(color);
            });

        /// <summary>
        /// Converts a CutLineSizes to the corresponding double size.
        /// </summary>
        public static readonly IValueConverter CutSizeToVisible =
            ValueConverter.Create<object, double>(e =>
            {
                if (!(e.Value is CutLineSizes cutLineSize))
                {
                    return 2.0;
                }

                return cutLineSize.ToPointSize() * 2.0;
            });
    }
}
