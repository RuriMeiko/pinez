using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.UI.Common.Models;
using System;
using System.Globalization;

namespace Ryujinx.Ava.UI.Helpers
{
    internal class XCITrimmerFileSpaceSavingsConverter : IValueConverter
    {
        private const long _bytesPerMB = 1024 * 1024;
        public static XCITrimmerFileSpaceSavingsConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is UnsetValueType)
            {
                return BindingOperations.DoNothing;
            }

            if (!targetType.IsAssignableFrom(typeof(string)))
            {
                return null;
            }

            if (value is not XCITrimmerFileModel app)
            {
                return null;
            }

            if (app.CurrentSavingsB < app.PotentialSavingsB)
            {
                return LocaleManager.Instance.UpdateAndGetDynamicValue(LocaleKeys.TitleXCICanSaveLabel,
                    Math.Max((app.PotentialSavingsB - app.CurrentSavingsB) / _bytesPerMB, 0));
            }
            else
            {
                return LocaleManager.Instance.UpdateAndGetDynamicValue(LocaleKeys.TitleXCISavingLabel,
                    Math.Max(app.CurrentSavingsB / _bytesPerMB, 0));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
