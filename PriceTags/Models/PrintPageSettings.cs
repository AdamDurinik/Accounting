using Microsoft.Win32;
using System.Globalization;
using System.Printing;

namespace PriceTags.Models
{
    public class PrintPageSettings
    {
        private const string RegKey = @"Software\Accounting\PriceTags\PrintSettings";
        private const double MmToDip = 96.0 / 25.4;

        public double MarginTopMm    { get; set; } = 10;
        public double MarginBottomMm { get; set; } = 10;
        public double MarginLeftMm   { get; set; } = 10;
        public double MarginRightMm  { get; set; } = 10;
        public string PaperSizeName  { get; set; } = "A4";
        public bool   IsLandscape    { get; set; } = false;
        public int    Copies         { get; set; } = 1;

        public double MarginTopDip    => MarginTopMm    * MmToDip;
        public double MarginBottomDip => MarginBottomMm * MmToDip;
        public double MarginLeftDip   => MarginLeftMm   * MmToDip;
        public double MarginRightDip  => MarginRightMm  * MmToDip;

        public PageMediaSizeName PageMediaSizeName => PaperSizeName switch
        {
            "A3"     => PageMediaSizeName.ISOA3,
            "A5"     => PageMediaSizeName.ISOA5,
            "Letter" => PageMediaSizeName.NorthAmericaLetter,
            "Legal"  => PageMediaSizeName.NorthAmericaLegal,
            _        => PageMediaSizeName.ISOA4
        };

        public PageOrientation PageOrientation =>
            IsLandscape ? PageOrientation.Landscape : PageOrientation.Portrait;

        public static PrintPageSettings Load()
        {
            var s = new PrintPageSettings();
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegKey);
                if (key != null)
                {
                    s.MarginTopMm    = GetDouble(key, nameof(MarginTopMm),    10);
                    s.MarginBottomMm = GetDouble(key, nameof(MarginBottomMm), 10);
                    s.MarginLeftMm   = GetDouble(key, nameof(MarginLeftMm),   10);
                    s.MarginRightMm  = GetDouble(key, nameof(MarginRightMm),  10);
                    s.PaperSizeName  = key.GetValue(nameof(PaperSizeName))  as string ?? "A4";
                    s.IsLandscape    = ((int)(key.GetValue(nameof(IsLandscape)) ?? 0)) != 0;
                    s.Copies         = (int)(key.GetValue(nameof(Copies)) ?? 1);
                }
            }
            catch { }
            return s;
        }

        public void Save()
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(RegKey);
                key.SetValue(nameof(MarginTopMm),    MarginTopMm   .ToString("R", CultureInfo.InvariantCulture));
                key.SetValue(nameof(MarginBottomMm), MarginBottomMm.ToString("R", CultureInfo.InvariantCulture));
                key.SetValue(nameof(MarginLeftMm),   MarginLeftMm  .ToString("R", CultureInfo.InvariantCulture));
                key.SetValue(nameof(MarginRightMm),  MarginRightMm .ToString("R", CultureInfo.InvariantCulture));
                key.SetValue(nameof(PaperSizeName),  PaperSizeName);
                key.SetValue(nameof(IsLandscape),    IsLandscape ? 1 : 0, RegistryValueKind.DWord);
                key.SetValue(nameof(Copies),         Copies,              RegistryValueKind.DWord);
            }
            catch { }
        }

        private static double GetDouble(RegistryKey key, string name, double fallback)
        {
            var val = key.GetValue(name) as string;
            return double.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : fallback;
        }
    }
}
