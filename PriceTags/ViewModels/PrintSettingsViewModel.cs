using DevExpress.Mvvm;
using PriceTags.Models;
using System;
using System.Collections.Generic;
using System.Windows;

namespace PriceTags.ViewModels
{
    public class PrintSettingsViewModel : ViewModelBase
    {
        // 1 mm = PreviewScale DIPs in the preview panel
        private const double PreviewScale = 0.5;

        private decimal _marginTopMm;
        private decimal _marginBottomMm;
        private decimal _marginLeftMm;
        private decimal _marginRightMm;
        private string  _paperSizeName = "A4";
        private bool    _isLandscape;
        private int     _copies;

        public bool    ShouldPrint    { get; private set; }
        public Action? CloseRequested { get; set; }

        public DelegateCommand PrintCommand  { get; }
        public DelegateCommand CancelCommand { get; }

        public IReadOnlyList<string> PaperSizes { get; } = new[] { "A3", "A4", "A5", "Letter", "Legal" };

        public PrintSettingsViewModel()
        {
            var saved = PrintPageSettings.Load();
            _marginTopMm    = (decimal)saved.MarginTopMm;
            _marginBottomMm = (decimal)saved.MarginBottomMm;
            _marginLeftMm   = (decimal)saved.MarginLeftMm;
            _marginRightMm  = (decimal)saved.MarginRightMm;
            _paperSizeName  = saved.PaperSizeName;
            _isLandscape    = saved.IsLandscape;
            _copies         = saved.Copies;

            PrintCommand = new DelegateCommand(() =>
            {
                BuildSettings().Save();
                ShouldPrint = true;
                CloseRequested?.Invoke();
            });

            CancelCommand = new DelegateCommand(() => CloseRequested?.Invoke());
        }

        public PrintPageSettings BuildSettings() => new()
        {
            MarginTopMm    = (double)_marginTopMm,
            MarginBottomMm = (double)_marginBottomMm,
            MarginLeftMm   = (double)_marginLeftMm,
            MarginRightMm  = (double)_marginRightMm,
            PaperSizeName  = _paperSizeName,
            IsLandscape    = _isLandscape,
            Copies         = _copies
        };

        // ── Editable properties ──────────────────────────────────────────────

        public decimal MarginTopMm
        {
            get => _marginTopMm;
            set { _marginTopMm = value; RaisePropertyChanged(); RaisePreview(); }
        }

        public decimal MarginBottomMm
        {
            get => _marginBottomMm;
            set { _marginBottomMm = value; RaisePropertyChanged(); RaisePreview(); }
        }

        public decimal MarginLeftMm
        {
            get => _marginLeftMm;
            set { _marginLeftMm = value; RaisePropertyChanged(); RaisePreview(); }
        }

        public decimal MarginRightMm
        {
            get => _marginRightMm;
            set { _marginRightMm = value; RaisePropertyChanged(); RaisePreview(); }
        }

        public string PaperSizeName
        {
            get => _paperSizeName;
            set { _paperSizeName = value; RaisePropertyChanged(); RaisePreview(); }
        }

        public bool IsLandscape
        {
            get => _isLandscape;
            set { _isLandscape = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(IsPortrait)); RaisePreview(); }
        }

        public bool IsPortrait
        {
            get => !_isLandscape;
            set { IsLandscape = !value; }
        }

        public int Copies
        {
            get => _copies;
            set { _copies = Math.Max(1, value); RaisePropertyChanged(); }
        }

        // ── Preview computed properties ──────────────────────────────────────

        private (double w, double h) PageMm => _paperSizeName switch
        {
            "A3"     => (297, 420),
            "A5"     => (148, 210),
            "Letter" => (215.9, 279.4),
            "Legal"  => (215.9, 355.6),
            _        => (210, 297)  // A4
        };

        public double PreviewPageWidth  => (_isLandscape ? PageMm.h : PageMm.w) * PreviewScale;
        public double PreviewPageHeight => (_isLandscape ? PageMm.w : PageMm.h) * PreviewScale;

        public Thickness PreviewMargin => new(
            (double)_marginLeftMm   * PreviewScale,
            (double)_marginTopMm    * PreviewScale,
            (double)_marginRightMm  * PreviewScale,
            (double)_marginBottomMm * PreviewScale);

        private void RaisePreview()
        {
            RaisePropertyChanged(nameof(PreviewPageWidth));
            RaisePropertyChanged(nameof(PreviewPageHeight));
            RaisePropertyChanged(nameof(PreviewMargin));
        }
    }
}
