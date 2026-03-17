using DevExpress.Mvvm;
using System.IO;
using System.Text.Json;
using MediaColor = System.Windows.Media.Color;
using MediaColorConverter = System.Windows.Media.ColorConverter;
using MediaFontFamily = System.Windows.Media.FontFamily;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;

namespace PriceTags.Models
{
    public class PrintSettings : BindableBase
    {
        private const double MmToDip = 96.0 / 25.4;

        public static string SettingsPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Accounting", "settings.json");

        public int TagWidthMm
        {
            get => GetProperty(() => TagWidthMm);
            set
            {
                SetProperty(() => TagWidthMm, value);
                RaisePropertyChanged(nameof(TagWidth));
                RaisePropertyChanged(nameof(InnerGridHeight));
            }
        }

        public int TagHeightMm
        {
            get => GetProperty(() => TagHeightMm);
            set
            {
                SetProperty(() => TagHeightMm, value);
                RaisePropertyChanged(nameof(TagHeight));
                RaisePropertyChanged(nameof(InnerGridHeight));
            }
        }

        // Computed DIP values — used by all existing XAML bindings and print code
        public double TagWidth => TagWidthMm * MmToDip;
        public double TagHeight => TagHeightMm * MmToDip;
        public double InnerGridHeight => TagHeight - 20;

        public string NameFontFamily
        {
            get => GetProperty(() => NameFontFamily);
            set
            {
                SetProperty(() => NameFontFamily, value);
                RaisePropertyChanged(nameof(NameFontFamilyObj));
            }
        }

        public MediaColor BackgroundColor
        {
            get => GetProperty(() => BackgroundColor);
            set
            {
                SetProperty(() => BackgroundColor, value);
                RaisePropertyChanged(nameof(BackgroundBrush));
            }
        }

        public MediaColor BorderColor
        {
            get => GetProperty(() => BorderColor);
            set
            {
                SetProperty(() => BorderColor, value);
                RaisePropertyChanged(nameof(BorderBrush));
            }
        }

        public MediaColor TextColor
        {
            get => GetProperty(() => TextColor);
            set
            {
                SetProperty(() => TextColor, value);
                RaisePropertyChanged(nameof(TextBrush));
            }
        }

        public double BorderThicknessValue
        {
            get => GetProperty(() => BorderThicknessValue);
            set
            {
                SetProperty(() => BorderThicknessValue, value);
                RaisePropertyChanged(nameof(TagBorderThickness));
            }
        }

        public bool NameFontBold
        {
            get => GetProperty(() => NameFontBold);
            set
            {
                SetProperty(() => NameFontBold, value);
                RaisePropertyChanged(nameof(NameFontWeightObj));
            }
        }

        public bool NameFontItalic
        {
            get => GetProperty(() => NameFontItalic);
            set
            {
                SetProperty(() => NameFontItalic, value);
                RaisePropertyChanged(nameof(NameFontStyleObj));
            }
        }

        public MediaColor NameColor
        {
            get => GetProperty(() => NameColor);
            set { SetProperty(() => NameColor, value); RaisePropertyChanged(nameof(NameBrush)); }
        }

        public string PriceFontFamily
        {
            get => GetProperty(() => PriceFontFamily);
            set { SetProperty(() => PriceFontFamily, value); RaisePropertyChanged(nameof(PriceFontFamilyObj)); }
        }

        public MediaColor PriceColor
        {
            get => GetProperty(() => PriceColor);
            set { SetProperty(() => PriceColor, value); RaisePropertyChanged(nameof(PriceBrush)); }
        }

        public string TextFontFamily
        {
            get => GetProperty(() => TextFontFamily);
            set { SetProperty(() => TextFontFamily, value); RaisePropertyChanged(nameof(TextFontFamilyObj)); }
        }

        public SolidColorBrush BackgroundBrush => new(BackgroundColor);
        public SolidColorBrush BorderBrush => new(BorderColor);
        public SolidColorBrush TextBrush => new(TextColor);
        public SolidColorBrush NameBrush => new(NameColor);
        public SolidColorBrush PriceBrush => new(PriceColor);
        public MediaFontFamily NameFontFamilyObj => new(NameFontFamily);
        public MediaFontFamily PriceFontFamilyObj => new(PriceFontFamily);
        public MediaFontFamily TextFontFamilyObj => new(TextFontFamily);
        public System.Windows.Thickness TagBorderThickness => new(BorderThicknessValue);
        public System.Windows.FontWeight NameFontWeightObj => NameFontBold ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal;
        public System.Windows.FontStyle NameFontStyleObj => NameFontItalic ? System.Windows.FontStyles.Italic : System.Windows.FontStyles.Normal;

        public static PrintSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    var dto = JsonSerializer.Deserialize<SettingsDto>(json);
                    if (dto != null)
                    {
                        // Migrate old DIP-based JSON to mm
                        const double dipToMm = 25.4 / 96.0;
                        if (dto.TagWidth.HasValue)
                            dto.TagWidthMm = (int)Math.Round(dto.TagWidth.Value * dipToMm);
                        if (dto.TagHeight.HasValue)
                            dto.TagHeightMm = (int)Math.Round(dto.TagHeight.Value * dipToMm);

                        return new PrintSettings
                        {
                            TagWidthMm = dto.TagWidthMm,
                            TagHeightMm = dto.TagHeightMm,
                            NameFontFamily = dto.NameFontFamily ?? "standard-block",
                            BackgroundColor = ParseColor(dto.BackgroundColor, MediaColor.FromArgb(0xFF, 0xFD, 0xFD, 0xFD)),
                            BorderColor = ParseColor(dto.BorderColor, MediaColor.FromArgb(0xFF, 0xD3, 0xD3, 0xD3)),
                            TextColor = ParseColor(dto.TextColor, MediaColor.FromArgb(0xFF, 0x00, 0x00, 0x00)),
                            BorderThicknessValue = dto.BorderThicknessValue,
                            NameFontBold = dto.NameFontBold,
                            NameFontItalic = dto.NameFontItalic,
                            NameColor = ParseColor(dto.NameColor, MediaColor.FromArgb(0xFF, 0x00, 0x00, 0x00)),
                            PriceFontFamily = dto.PriceFontFamily ?? "standard-block",
                            PriceColor = ParseColor(dto.PriceColor, MediaColor.FromArgb(0xFF, 0x00, 0x00, 0x00)),
                            TextFontFamily = dto.TextFontFamily ?? "standard-block"
                        };
                    }
                }
            }
            catch { }
            return CreateDefaults();
        }

        public void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(SettingsPath)!;
                Directory.CreateDirectory(dir);
                var dto = new SettingsDto
                {
                    TagWidthMm = TagWidthMm,
                    TagHeightMm = TagHeightMm,
                    NameFontFamily = NameFontFamily,
                    BackgroundColor = ColorToHex(BackgroundColor),
                    BorderColor = ColorToHex(BorderColor),
                    TextColor = ColorToHex(TextColor),
                    BorderThicknessValue = BorderThicknessValue,
                    NameFontBold = NameFontBold,
                    NameFontItalic = NameFontItalic,
                    NameColor = ColorToHex(NameColor),
                    PriceFontFamily = PriceFontFamily,
                    PriceColor = ColorToHex(PriceColor),
                    TextFontFamily = TextFontFamily
                };
                var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, json);
            }
            catch { }
        }

        public void Reset()
        {
            var d = CreateDefaults();
            TagWidthMm = d.TagWidthMm;
            TagHeightMm = d.TagHeightMm;
            NameFontFamily = d.NameFontFamily;
            BackgroundColor = d.BackgroundColor;
            BorderColor = d.BorderColor;
            TextColor = d.TextColor;
            BorderThicknessValue = d.BorderThicknessValue;
            NameFontBold = d.NameFontBold;
            NameFontItalic = d.NameFontItalic;
            NameColor = d.NameColor;
            PriceFontFamily = d.PriceFontFamily;
            PriceColor = d.PriceColor;
            TextFontFamily = d.TextFontFamily;
        }

        public static PrintSettings GetDefaults() => CreateDefaults();

        private static PrintSettings CreateDefaults() => new()
        {
            TagWidthMm = 90,
            TagHeightMm = 55,
            NameFontFamily = "standard-block",
            BackgroundColor = MediaColor.FromArgb(0xFF, 0xFD, 0xFD, 0xFD),
            BorderColor = MediaColor.FromArgb(0xFF, 0xD3, 0xD3, 0xD3),
            TextColor = MediaColor.FromArgb(0xFF, 0x00, 0x00, 0x00),
            BorderThicknessValue = 1.0,
            NameFontBold = false,
            NameFontItalic = false,
            NameColor = MediaColor.FromArgb(0xFF, 0x00, 0x00, 0x00),
            PriceFontFamily = "standard-block",
            PriceColor = MediaColor.FromArgb(0xFF, 0x00, 0x00, 0x00),
            TextFontFamily = "standard-block"
        };

        private static MediaColor ParseColor(string? hex, MediaColor fallback)
        {
            if (string.IsNullOrEmpty(hex)) return fallback;
            try { return (MediaColor)MediaColorConverter.ConvertFromString(hex); }
            catch { return fallback; }
        }

        private static string ColorToHex(MediaColor c) => $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}";

        private class SettingsDto
        {
            // Legacy DIP fields — present only in old-format JSON, null in new-format
            public double? TagWidth { get; set; }
            public double? TagHeight { get; set; }

            public int TagWidthMm { get; set; } = 90;
            public int TagHeightMm { get; set; } = 55;
            public string? NameFontFamily { get; set; } = "standard-block";
            public string? BackgroundColor { get; set; } = "#FFfdfdfd";
            public string? BorderColor { get; set; } = "#FFD3D3D3";
            public string? TextColor { get; set; } = "#FF000000";
            public double BorderThicknessValue { get; set; } = 1.0;
            public bool NameFontBold { get; set; } = false;
            public bool NameFontItalic { get; set; } = false;
            public string? NameColor { get; set; } = "#FF000000";
            public string? PriceFontFamily { get; set; } = "standard-block";
            public string? PriceColor { get; set; } = "#FF000000";
            public string? TextFontFamily { get; set; } = "standard-block";
        }
    }
}
