using LambdaConverters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

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
        /// Adds two double values in string format.
        /// </summary>
        public static readonly IValueConverter Addition =
            ValueConverter.Create<object, double, object>(e =>
            {
                if (!double.TryParse(e.Value.ToString(), out double x))
                {
                    return 0;
                }

                if (!double.TryParse(e.Parameter.ToString(), out double y))
                {
                    return x;
                }

                return x + y;
            });

        /// <summary>
        /// Multiplies two double values in string format.
        /// </summary>
        public static readonly IValueConverter Multiply =
            ValueConverter.Create<object, double, object>(e =>
            {
                if (!double.TryParse(e.Value.ToString(), out double x))
                {
                    return 0;
                }

                if (!double.TryParse(e.Parameter.ToString(), out double y))
                {
                    return x;
                }

                return x * y;
            });

        /// <summary>
        /// returns the boolean not value.
        /// </summary>
        public static readonly IValueConverter BooleanNot = ValueConverter.Create<bool, bool>(e => !e.Value, e => !e.Value);

        /// <summary>
        /// Converts a boolean value to a color.
        /// </summary>
        public static readonly IValueConverter BoolToGreenRed =
            ValueConverter.Create<bool, Brush>(e =>
                (e.Value
                    ? Application.Current.Resources["LedGreen"]
                    : Application.Current.Resources["LedRed"])
                as Brush);

        /// <summary>
        /// Converts true to 0 and false to 1.
        /// </summary>
        public static readonly IValueConverter TrueToIndex0 =
            ValueConverter.Create<bool, int>(e => e.Value ? 0 : 1, e => e.Value == 0);

        /// <summary>
        /// Converts true to 'On' and false to 'Off'.
        /// </summary>
        public static readonly IValueConverter BoolToOnOff =
            ValueConverter.Create<bool, string>(e => e.Value ? "On" : "Off");

        /// <summary>
        /// Converts true to 'Yes' and false to 'No'.
        /// </summary>
        public static readonly IValueConverter BoolToYesNo =
            ValueConverter.Create<bool, string>(e => e.Value ? "Yes" : "No");

        /// <summary>
        /// Converts true to 'Pass' and false to 'Fail'.
        /// </summary>
        public static readonly IValueConverter BoolToPassFail =
            ValueConverter.Create<bool, string>(e => e.Value ? "Pass" : "Fail");

        /// <summary>
        /// Converts to true if the first value is greater than the second.
        /// </summary>
        public static readonly IValueConverter GreaterThan =
            ValueConverter.Create<object, bool, object>(e =>
            {
                if (!double.TryParse(e.Value.ToString(), out double x))
                {
                    return false;
                }

                if (!double.TryParse(e.Parameter.ToString(), out double y))
                {
                    return false;
                }

                return x > y;
            });

        /// <summary>
        /// Gives a color string based on whether the date has passed or not.
        /// </summary>
        public static readonly IValueConverter DatePassedToColor =
            ValueConverter.Create<DateTime, Brush>(e =>
                (e.Value < DateTime.Now
                    ? Application.Current.Resources["GoPop"]
                    : Application.Current.Resources["StopPop"])
                as Brush);

        /// <summary>
        /// Converts encoding to its code page integer value.
        /// </summary>
        public static readonly IValueConverter EncodingToCodePage =
            ValueConverter.Create<int, Encoding>(a => Encoding.GetEncoding(a.Value), e => e.Value.CodePage);

        /// <summary>
        /// Converts an enum to its description attribute for display.
        /// </summary>
        public static readonly IValueConverter EnumToDescription =
            ValueConverter.Create<Enum, string>(e => e.Value.ToDescription());

        /// <summary>
        /// Converts an escape character to a displayable form.
        /// </summary>
        public static readonly IValueConverter EscapeCharacterToView =
            ValueConverter.Create<string, string>(e => Regex.Escape(e.Value));

        /// <summary>
        /// True if string is null, string.Empty, or whitespace.
        /// </summary>
        public static readonly IValueConverter IsNullOrWhitespace =
            ValueConverter.Create<object, bool>(e => string.IsNullOrWhiteSpace(e.Value as string));

        /// <summary>
        /// 1 if true, 0 if false.
        /// </summary>
        public static readonly IValueConverter NullToInt0 =
            ValueConverter.Create<object, int>(e => e.Value != null ? 1 : 0);

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
        /// Converts an infix formatted string to LaTeX.
        /// </summary>
        public static readonly IValueConverter FormulaToLaTeX =
            ValueConverter.Create<string, string>(e =>
            {
                Expression expr = Expression.Undefined;

                try
                {
                    expr = Infix.ParseOrUndefined(e.Value ?? string.Empty);
                }
                catch
                {
                    // Ignore exceptions, just display the Undefined expression.
                }

                return LaTeX.Format(expr);
            });

        /// <summary>
        /// Returns the object containing a list of the 
        /// </summary>
        public static readonly IMultiValueConverter
            MultiCommandParameter = MultiValueConverter.Create<object, object[]>(e =>
            {
                var result = new object[e.Values.Count];
                var count = 0;
                foreach (object argValue in e.Values)
                {
                    result[count] = argValue;
                    count++;
                }

                return result;
            });

        /// <summary>
        /// Converts FlowControlModes to a boolean. True indicates in automatic mode.
        /// </summary>
        public static readonly IValueConverter IsInAutoMode =
            ValueConverter.Create<FlowControlModes, bool>(q => q.Value == FlowControlModes.Auto,
                p => p.Value ? FlowControlModes.Auto : FlowControlModes.Manual);

        /// <summary>
        /// Converts the given <see cref="FlowStatusTypes"/> to false if in development true if not.
        /// </summary>
        public static readonly IValueConverter FlowStatusDevelopmentToFalse =
            ValueConverter.Create<FlowStatusTypes, bool>(q => q.Value != FlowStatusTypes.Development);

        /// <summary>
        /// Converts the given <see cref="FlowStatusTypes"/> to true if in development false if not.
        /// </summary>
        public static readonly IValueConverter FlowStatusDevelopmentToTrue =
            ValueConverter.Create<FlowStatusTypes, bool>(q => q.Value == FlowStatusTypes.Development);

        /// <summary>
        /// Converts the given <see cref="IModelObject"/> to Visible if it is also <see cref="ICalibrated"/> and Collapsed if not.
        /// </summary>
        public static readonly IValueConverter HasCalibrationCoefficientsToVisible =
            ValueConverter.Create<IModelObject, Visibility>(q => q.Value is ICalibrated ? Visibility.Visible : Visibility.Collapsed);

        /// <summary>
        /// Converts the given <see cref="IModelObject"/> to Visible if it is also <see cref="Nozzle"/> and Collapsed if not.
        /// </summary>
        public static readonly IValueConverter HasChokeCoefficientsToVisible =
            ValueConverter.Create<IModelObject, Visibility>(q => q.Value is Nozzle ? Visibility.Visible : Visibility.Collapsed);

        /// <summary>
        /// Deserializes coefficients and constructs a string for displaying.
        /// </summary>
        public static readonly IValueConverter CoefficientsToDetails = ValueConverter.Create<ICalibrated, string>(q =>
        {
            const char subscript0 = '\u2080';

            if (q.Value is null || !SensorCoefficients.TryParse(q.Value.CalibrationCoefficients, out double[] coefficients))
            {
                return $"a{subscript0} = 0";
            }

            if (coefficients.Length == 0)
            {
                return $"a{subscript0} = 0";
            }

            var result = string.Empty;

            for (var i = 0; i < coefficients.Length; i++)
            {
                var subscript = string.Empty;

                if (i > 9)
                {
                    var intString = i.ToString();
                    IEnumerable<int> array = intString.ToCharArray().Select(strNum => int.Parse(strNum.ToString()));

                    subscript = array.Aggregate(subscript, (current, val) => current + (char)(subscript0 + val));
                }
                else
                {
                    subscript = ((char)(subscript0 + i)).ToString();
                }
                
                result = $"a{subscript} = {coefficients[i]:0.####E00}\r\n" + result;
            }

            return result;
        });

        /// <summary>
        /// changes the coefficient number to a_i =.
        /// </summary>
        public static readonly IValueConverter ExponentToCoefficientNumber = ValueConverter.Create<int, string>(q =>
        {
            const char subscript0 = '\u2080';

            if (q.Value <= 9)
            {
                return $"a{((char)(subscript0 + q.Value))} = ";
            }

            var intString = q.Value.ToString();
            IEnumerable<int> array = intString.ToCharArray().Select(strNum => int.Parse(strNum.ToString()));
            return $"a{array.Aggregate(string.Empty, (current, val) => current + (char)(subscript0 + val))} = " ;

        });

        /// <summary>
        /// Deserializes coefficients and constructs a string for displaying.
        /// </summary>
        public static readonly IValueConverter ChokeCoefficientsToDetails = ValueConverter.Create<Nozzle, string>(q =>
        {
            const char subscript0 = '\u2080';

            if (q.Value is null || !SensorCoefficients.TryParse(q.Value.ChokeCoefficients, out double[] coefficients))
            {
                return $"a{subscript0} = 0";
            }

            if (coefficients.Length == 0)
            {
                return $"a{subscript0} = 0";
            }

            var result = string.Empty;

            for (var i = 0; i < coefficients.Length; i++)
            {
                var subscript = string.Empty;

                if (i > 9)
                {
                    var intString = i.ToString();
                    IEnumerable<int> array = intString.ToCharArray().Select(strNum => int.Parse(strNum.ToString()));

                    subscript = array.Aggregate(subscript, (current, val) => current + (char)(subscript0 + val));
                }
                else
                {
                    subscript = ((char)(subscript0 + i)).ToString();
                }
                
                result = $"a{subscript} = {coefficients[i]:0.####E00}\r\n" + result;
            }

            return result;
        });

        /// <summary>
        /// Converts to First letter caps, converts back all caps.
        /// </summary>
        public static readonly IValueConverter FirstToUpper =
            ValueConverter.Create<string, string>(q => q.Value.FirstToUpper(), q => q.Value.ToUpper(CultureInfo.CurrentCulture));

        /// <summary>
        /// Converts an airflow user object to a boolean indicating whether that user's password is reset to the default or not.
        /// Only works with OneWay bindings.
        /// </summary>
        public static readonly IValueConverter IsPasswordDefault =
            ValueConverter.Create<object, bool>(args =>
            {
                if (!(args.Value is AirflowUser user))
                {
                    return false;
                }

                return user.FirstName?.ToLower(CultureInfo.CurrentCulture).Hash() == user.Password;
            });

        /// <summary>
        /// Checks that a function depends on the specific symbol <see cref="AirflowSymbols"/>.
        /// </summary>
        public static readonly IValueConverter FunctionDependsOnSymbol =
            ValueConverter.Create<string, bool, string>(q => q.Value.ContainsSymbols(q.Parameter));

        /// <summary>
        /// Rounds the target to the specified number of significant digits.  Round to even on ties.
        /// </summary>
        public static readonly IValueConverter RoundSignificantDigits =
            ValueConverter.Create<object, double, object>(q =>
            {
                if (!int.TryParse(q.Parameter.ToString(), out int parsedDigits))
                {
                    return double.NaN;
                }

                if (!double.TryParse(q.Value.ToString(), out double parsedInNumber))
                {
                    return double.NaN;
                }

                return parsedInNumber.Round(RoundingOptions.SignificantDigit, parsedDigits);
            });

        /// <summary>
        /// Rounds the target to the specified number of significant decimal places. Round to even on ties.
        /// </summary>
        public static readonly IValueConverter RoundDecimalPlaces =
            ValueConverter.Create<object, double, object>(q =>
            {
                if (!int.TryParse(q.Parameter.ToString(), out int parsedDigits))
                {
                    return double.NaN;
                }

                if (!double.TryParse(q.Value.ToString(), out double parsedInNumber))
                {
                    return double.NaN;
                }

                return parsedInNumber.Round(RoundingOptions.SignificantDigit, parsedDigits);
            });

        /// <summary>
        /// Indicates whether the stand type is an automated stand or not.
        /// </summary>
        public static readonly IValueConverter IsAutomaticStandToVisible =
            ValueConverter.Create<AirflowStandTypes, Visibility>(q => q.Value == AirflowStandTypes.Automtated ? Visibility.Visible : Visibility.Collapsed);

        /// <summary>
        /// Converts a CamelCase word to Camel Case;
        /// </summary>
        public static readonly IValueConverter CamelCaseToSpaces = ValueConverter.Create<string, string>(q => string.IsNullOrWhiteSpace(q.Value)
            ? string.Empty
            : Regex.Replace(Regex.Replace(q.Value, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"),
                @"(\p{Ll})(\P{Ll})",
                "$1 $2"));

        /// <summary>
        /// Returns true if the element's current aspect ratio is greater than 4/3;
        /// </summary>
        public static readonly IValueConverter FourThreeAspectRatio = ValueConverter.Create<object, bool>(e =>
        {
            if (!(e.Value is FrameworkElement fe))
            {
                return false;
            }

            return fe.ActualWidth / fe.ActualHeight > 4.0 / 3.0;
        });
    }
}
