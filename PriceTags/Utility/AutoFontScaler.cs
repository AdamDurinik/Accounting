using PriceTags.Models;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace PriceTags.Utility
{
    public static class AutoFontScaler
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(AutoFontScaler),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static void SetIsEnabled(DependencyObject element, bool value) =>
            element.SetValue(IsEnabledProperty, value);

        public static bool GetIsEnabled(DependencyObject element) =>
            (bool)element.GetValue(IsEnabledProperty);

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBlock tb) return;
            if ((bool)e.NewValue)
            {
                tb.Loaded += (_, __) => AdjustFontSize(tb);
                tb.DataContextChanged += (_,__) => AdjustFontSize(tb);
                tb.SourceUpdated += (_, __) => AdjustFontSize(tb);
                tb.SizeChanged += (_, __) => AdjustFontSize(tb);
                var desc = DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock));
                if (desc != null) desc.AddValueChanged(tb, (_, __) => AdjustFontSize(tb));
                var be = BindingOperations.GetBindingExpression(tb, TextBlock.TextProperty);
                try
                {
                    //just because it's used in the template for export
                    if (be?.ParentBinding != null) be.ParentBinding.NotifyOnTargetUpdated = true;
                }
                catch
                {
                    AdjustFontSize(tb);
                }
            }
        }

        private static void AdjustFontSize(TextBlock tb)
        {
            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                return;
            }

            const double minFontSize = 12;
            const double maxFontSize = 36;
            double newSize = GetAdjustedFontSize(tb.Text, tb.FontFamily, tb.FontStyle, tb.FontWeight,
                315, 95, maxFontSize, minFontSize, true);
            tb.FontSize = newSize;
        }

        private static double GetAdjustedFontSize(string text, System.Windows.Media.FontFamily fontFamily,
            System.Windows.FontStyle fontStyle, FontWeight fontWeight, double containerWidth,
            double containerHeight, double maxFontSize, double minFontSize, bool smallestOnFail)
        {
            for (double size = maxFontSize; size >= minFontSize; size--)
            {
                var ft = new FormattedText(
                    text,
                    CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    new Typeface(fontFamily, fontStyle, fontWeight, FontStretches.Normal),
                    size,
                    System.Windows.Media.Brushes.Black,
                    VisualTreeHelper.GetDpi(new DrawingVisual()).PixelsPerDip);
                ft.MaxTextWidth = containerWidth-10;

                if (ft.Width <= containerWidth && ft.Height <= containerHeight) return size;
            }
            return smallestOnFail ? minFontSize : maxFontSize;
        }
    }
}
