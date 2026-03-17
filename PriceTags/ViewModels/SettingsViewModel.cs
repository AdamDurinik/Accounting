using DevExpress.Mvvm;
using PriceTags.Enums;
using PriceTags.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MediaColor = System.Windows.Media.Color;
using MediaFontFamily = System.Windows.Media.FontFamily;
using MediaFonts = System.Windows.Media.Fonts;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;
using WpfApplication = System.Windows.Application;

namespace PriceTags.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly PrintSettings _settings;
        private const double MmToDip = 96.0 / 25.4;

        // Local working copy — _settings is only written on Save
        private int _tagWidthMm;
        private int _tagHeightMm;
        private string _nameFontFamily = string.Empty;
        private MediaColor _backgroundColor;
        private MediaColor _borderColor;
        private MediaColor _textColor;
        private double _borderThicknessValue;
        private bool _nameFontBold;
        private bool _nameFontItalic;
        private MediaColor _nameColor;
        private string _priceFontFamily = string.Empty;
        private MediaColor _priceColor;
        private string _textFontFamily = string.Empty;

        public SettingsViewModel()
        {
            _settings = (PrintSettings)WpfApplication.Current.Resources["AppSettings"];
            LoadFrom(_settings);

            SaveCommand = new DelegateCommand(() =>
            {
                ApplyTo(_settings);
                _settings.Save();
                IsDirty = false;
                CloseRequested?.Invoke();
            });

            CancelCommand = new DelegateCommand(() =>
            {
                if (!IsDirty)
                {
                    CloseRequested?.Invoke();
                    return;
                }
                var result = GetService<IMessageBoxService>()?.ShowMessage(
                    "Uložiť zmeny pred zatvorením?",
                    "Neuložené zmeny",
                    MessageButton.YesNoCancel,
                    MessageIcon.Question);

                if (result == MessageResult.Yes)
                {
                    ApplyTo(_settings);
                    _settings.Save();
                    IsDirty = false; 
                    CloseRequested?.Invoke();
                }
                else if (result == MessageResult.No)
                {
                    CloseRequested?.Invoke();
                }
                // Cancel: stay open
            });

            ResetCommand = new DelegateCommand(() =>
            {
                LoadFrom(PrintSettings.GetDefaults());
                IsDirty = true;
            });

            SystemFonts = new[] { "standard-block" }
                .Concat(MediaFonts.SystemFontFamilies.Select(f => f.Source).OrderBy(f => f))
                .ToList();

            SampleTag = new PriceTagModel
            {
                Name = "Čerstvý chlieb",
                Price = 1.29,
                Quantity = 500,
                QuantityType = QuantityType.WeightInGrams
            };
        }

        private void LoadFrom(PrintSettings source)
        {
            _tagWidthMm = source.TagWidthMm;
            _tagHeightMm = source.TagHeightMm;
            _nameFontFamily = source.NameFontFamily;
            _backgroundColor = source.BackgroundColor;
            _borderColor = source.BorderColor;
            _textColor = source.TextColor;
            _borderThicknessValue = source.BorderThicknessValue;
            _nameFontBold = source.NameFontBold;
            _nameFontItalic = source.NameFontItalic;
            _nameColor = source.NameColor;
            _priceFontFamily = source.PriceFontFamily;
            _priceColor = source.PriceColor;
            _textFontFamily = source.TextFontFamily;

            RaisePropertyChanged(nameof(TagWidthMm));
            RaisePropertyChanged(nameof(TagHeightMm));
            RaisePropertyChanged(nameof(NameFontFamily));
            RaisePropertyChanged(nameof(BackgroundColor));
            RaisePropertyChanged(nameof(BorderColor));
            RaisePropertyChanged(nameof(TextColor));
            RaisePropertyChanged(nameof(BorderThicknessValue));
            RaisePropertyChanged(nameof(NameFontBold));
            RaisePropertyChanged(nameof(NameFontItalic));
            RaisePropertyChanged(nameof(TagWidth));
            RaisePropertyChanged(nameof(TagHeight));
            RaisePropertyChanged(nameof(InnerGridHeight));
            RaisePropertyChanged(nameof(TagBorderThickness));
            RaisePropertyChanged(nameof(BackgroundBrush));
            RaisePropertyChanged(nameof(PreviewBorderBrush));
            RaisePropertyChanged(nameof(NameFontFamilyObj));
            RaisePropertyChanged(nameof(NameFontWeightObj));
            RaisePropertyChanged(nameof(NameFontStyleObj));
            RaisePropertyChanged(nameof(NameColor));
            RaisePropertyChanged(nameof(NameBrush));
            RaisePropertyChanged(nameof(PriceFontFamily));
            RaisePropertyChanged(nameof(PriceFontFamilyObj));
            RaisePropertyChanged(nameof(PriceColor));
            RaisePropertyChanged(nameof(PriceBrush));
            RaisePropertyChanged(nameof(TextFontFamily));
            RaisePropertyChanged(nameof(TextFontFamilyObj));
            RaisePropertyChanged(nameof(TextColor));
            RaisePropertyChanged(nameof(TextBrush));
        }

        private void ApplyTo(PrintSettings target)
        {
            target.TagWidthMm = _tagWidthMm;
            target.TagHeightMm = _tagHeightMm;
            target.NameFontFamily = _nameFontFamily;
            target.BackgroundColor = _backgroundColor;
            target.BorderColor = _borderColor;
            target.TextColor = _textColor;
            target.BorderThicknessValue = _borderThicknessValue;
            target.NameFontBold = _nameFontBold;
            target.NameFontItalic = _nameFontItalic;
            target.NameColor = _nameColor;
            target.PriceFontFamily = _priceFontFamily;
            target.PriceColor = _priceColor;
            target.TextFontFamily = _textFontFamily;
        }

        public bool IsDirty
        {
            get => GetProperty(() => IsDirty);
            private set => SetProperty(() => IsDirty, value);
        }

        public Action? CloseRequested { get; set; }

        public DelegateCommand SaveCommand { get; }
        public DelegateCommand CancelCommand { get; }
        public DelegateCommand ResetCommand { get; }
        public IEnumerable<string> SystemFonts { get; }
        public PriceTagModel SampleTag { get; }

        // --- Editable properties (local copy only) ---

        public decimal TagWidthMm
        {
            get => _tagWidthMm;
            set
            {
                _tagWidthMm = (int)value;
                IsDirty = true;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(TagWidth));
                RaisePropertyChanged(nameof(InnerGridHeight));
            }
        }

        public decimal TagHeightMm
        {
            get => _tagHeightMm;
            set
            {
                _tagHeightMm = (int)value;
                IsDirty = true;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(TagHeight));
                RaisePropertyChanged(nameof(InnerGridHeight));
            }
        }

        public string NameFontFamily
        {
            get => _nameFontFamily;
            set
            {
                _nameFontFamily = value;
                IsDirty = true;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(NameFontFamilyObj));
            }
        }

        public MediaColor BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                IsDirty = true;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(BackgroundBrush));
            }
        }

        public MediaColor BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                IsDirty = true;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(PreviewBorderBrush));
            }
        }

        public MediaColor TextColor
        {
            get => _textColor;
            set
            {
                _textColor = value;
                IsDirty = true;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(TextBrush));
            }
        }

        public double BorderThicknessValue
        {
            get => _borderThicknessValue;
            set
            {
                _borderThicknessValue = value;
                IsDirty = true;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(TagBorderThickness));
            }
        }

        public bool NameFontBold
        {
            get => _nameFontBold;
            set
            {
                _nameFontBold = value;
                IsDirty = true;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(NameFontWeightObj));
            }
        }

        public bool NameFontItalic
        {
            get => _nameFontItalic;
            set
            {
                _nameFontItalic = value;
                IsDirty = true;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(NameFontStyleObj));
            }
        }

        public MediaColor NameColor
        {
            get => _nameColor;
            set
            {
                _nameColor = value;
                IsDirty = true;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(NameBrush));
            }
        }

        public string PriceFontFamily
        {
            get => _priceFontFamily;
            set
            {
                _priceFontFamily = value;
                IsDirty = true;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(PriceFontFamilyObj));
            }
        }

        public MediaColor PriceColor
        {
            get => _priceColor;
            set
            {
                _priceColor = value;
                IsDirty = true;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(PriceBrush));
            }
        }

        public string TextFontFamily
        {
            get => _textFontFamily;
            set
            {
                _textFontFamily = value;
                IsDirty = true;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(TextFontFamilyObj));
            }
        }

        // --- Computed preview properties ---

        public double TagWidth => _tagWidthMm * MmToDip;
        public double TagHeight => _tagHeightMm * MmToDip;
        public double InnerGridHeight => TagHeight - 20;
        public Thickness TagBorderThickness => new(_borderThicknessValue);
        public SolidColorBrush BackgroundBrush => new(_backgroundColor);
        public SolidColorBrush PreviewBorderBrush => new(_borderColor);
        public SolidColorBrush TextBrush => new(_textColor);
        public SolidColorBrush NameBrush => new(_nameColor);
        public SolidColorBrush PriceBrush => new(_priceColor);
        public MediaFontFamily NameFontFamilyObj => new(_nameFontFamily);
        public MediaFontFamily PriceFontFamilyObj => new(_priceFontFamily);
        public MediaFontFamily TextFontFamilyObj => new(_textFontFamily);
        public FontWeight NameFontWeightObj => _nameFontBold ? FontWeights.Bold : FontWeights.Normal;
        public System.Windows.FontStyle NameFontStyleObj => _nameFontItalic ? FontStyles.Italic : FontStyles.Normal;
    }
}
