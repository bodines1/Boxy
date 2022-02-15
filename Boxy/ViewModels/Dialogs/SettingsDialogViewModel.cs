using CardMimic.Model.ScryfallData;
using CardMimic.Mvvm;
using CardMimic.Properties;
using CardMimic.Utilities;
using PdfSharp;
using PdfSharp.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CardMimic.ViewModels.Dialogs
{
    /// <summary>
    /// View model for interacting with a message dialog window.
    /// </summary>
    public class SettingsDialogViewModel : DialogViewModelBase
    {
        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="SettingsDialogViewModel"/>.
        /// </summary>
        public SettingsDialogViewModel()
        {
            FormatOptions = Enum.GetValues(typeof(FormatTypes)).Cast<FormatTypes>().ToList();
            PageSizeOptions = Enum.GetValues(typeof(PageSize)).Cast<PageSize>().ToList();
            PageSizeOptions.Remove(PageSize.Undefined);
            ColorOptions = Enum.GetValues(typeof(XKnownColor)).Cast<XKnownColor>().ToList();
            LineSizeOptions = Enum.GetValues(typeof(CutLineSizes)).Cast<CutLineSizes>().ToList();

            PdfSaveFolder = Settings.Default.PdfSaveFolder;
            PdfJpegQuality = Settings.Default.PdfJpegQuality;
            PdfHasCutLines = Settings.Default.PdfHasCutLines;
            PrintTwoSided = Settings.Default.PrintTwoSided;
            CutLineColor = Settings.Default.CutLineColor;
            CutLineSize = Settings.Default.CutLineSize;
            PdfScalingPercent = Settings.Default.PdfScalingPercent;
            PdfOpenWhenSaveDone = Settings.Default.PdfOpenWhenSaveDone;
            PdfPageSize = Settings.Default.PdfPageSize;
            SelectedFormat = Settings.Default.SavedFormat;
            MaxPrice = Settings.Default.MaxPrice;
        }

        #endregion Constructors

        #region Fields

        private string _pdfSaveFolder;
        private PageSize _pdfPageSize;
        private FormatTypes _selectedFormat;
        private double _pdfScalingPercent;
        private bool _pdfHasCutLines;
        private bool _printTwoSided;
        private XKnownColor _cutLineColor;
        private CutLineSizes _cutLineSize;
        private int _pdfJpegQuality;
        private bool _pdfOpenWhenSaveDone;
        private double _maxPrice;

        #endregion Fields

        #region Properties

        /// <summary>
        /// List to populate the options for user to select from.
        /// </summary>
        public List<PageSize> PageSizeOptions { get; }

        /// <summary>
        /// List to populate the options for user to select from.
        /// </summary>
        public List<XKnownColor> ColorOptions { get; }

        /// <summary>
        /// List to populate the options for user to select from.
        /// </summary>
        public List<CutLineSizes> LineSizeOptions { get; }

        /// <summary>
        /// List to populate the options for user to select from.
        /// </summary>
        public List<FormatTypes> FormatOptions { get; }

        /// <summary>
        /// Way to display to user what the expected cards per page with their settings will be.
        /// </summary>
        // ReSharper disable once MemberCanBeMadeStatic.Global <- Not possible, binding in XAML
        public int CardsPerPage
        {
            get
            {
                var temp = new CardPdfBuilder(PdfPageSize, PdfScalingPercent, PdfHasCutLines, CutLineSize, CutLineColor);
                return temp.ExampleImageDrawer.ImagesPerPage;
            }
        }

        /// <summary>
        /// PdfSaveFolder setting.
        /// </summary>
        public string PdfSaveFolder
        {
            get
            {
                return _pdfSaveFolder;
            }

            set
            {
                _pdfSaveFolder = value;
                OnPropertyChanged(nameof(PdfSaveFolder));
            }
        }

        /// <summary>
        /// PdfJpegQuality setting.
        /// </summary>
        public int PdfJpegQuality
        {
            get
            {
                return _pdfJpegQuality;
            }

            set
            {
                if (value < 60)
                {
                    _pdfJpegQuality = 60;
                }
                else if (value > 100)
                {
                    _pdfJpegQuality = 100;
                }
                else
                {
                    _pdfJpegQuality = value;
                }
                
                OnPropertyChanged(nameof(PdfJpegQuality));
            }
        }

        /// <summary>
        /// PdfHasCutLines setting.
        /// </summary>
        public bool PdfHasCutLines
        {
            get
            {
                return _pdfHasCutLines;
            }

            set
            {
                _pdfHasCutLines = value;
                OnPropertyChanged(nameof(PdfHasCutLines));
                OnPropertyChanged(nameof(CardsPerPage));
            }
        }

        /// <summary>
        /// PrintTwoSided setting.
        /// </summary>
        public bool PrintTwoSided
        {
            get
            {
                return _printTwoSided;
            }

            set
            {
                _printTwoSided = value;
                OnPropertyChanged(nameof(PrintTwoSided));
            }
        }

        /// <summary>
        /// CutLineColor setting.
        /// </summary>
        public XKnownColor CutLineColor
        {
            get
            {
                return _cutLineColor;
            }

            set
            {
                _cutLineColor = value;
                OnPropertyChanged(nameof(CutLineColor));
            }
        }

        /// <summary>
        /// CutLineSize setting.
        /// </summary>
        public CutLineSizes CutLineSize
        {
            get
            {
                return _cutLineSize;
            }

            set
            {
                _cutLineSize = value;
                OnPropertyChanged(nameof(CutLineSize));
                OnPropertyChanged(nameof(CardsPerPage));
            }
        }

        /// <summary>
        /// PdfScaling setting.
        /// </summary>
        public double PdfScalingPercent
        {
            get
            {
                return _pdfScalingPercent;
            }

            set
            {
                if (value < 90)
                {
                    _pdfScalingPercent = 90;
                }
                else if (value > 110)
                {
                    _pdfScalingPercent = 110;
                }
                else
                {
                    _pdfScalingPercent = value;
                }
                
                OnPropertyChanged(nameof(PdfScalingPercent));
                OnPropertyChanged(nameof(CardsPerPage));
            }
        }

        /// <summary>
        /// PdfOpenWhenSaveDone setting.
        /// </summary>
        public bool PdfOpenWhenSaveDone
        {
            get
            {
                return _pdfOpenWhenSaveDone;
            }

            set
            {
                _pdfOpenWhenSaveDone = value;
                OnPropertyChanged(nameof(PdfOpenWhenSaveDone));
            }
        }

        /// <summary>
        /// PdfPageSize setting.
        /// </summary>
        public PageSize PdfPageSize
        {
            get
            {
                return _pdfPageSize;
            }

            set
            {
                _pdfPageSize = value;
                OnPropertyChanged(nameof(PdfPageSize));
                OnPropertyChanged(nameof(CardsPerPage));
            }
        }

        /// <summary>
        /// SavedFormat setting.
        /// </summary>
        public FormatTypes SelectedFormat
        {
            get
            {
                return _selectedFormat;
            }

            set
            {
                _selectedFormat = value;
                OnPropertyChanged(nameof(SelectedFormat));
            }
        }

        /// <summary>
        /// MaxPrice setting.
        /// </summary>
        public double MaxPrice
        {
            get
            {
                return _maxPrice;
            }

            set
            {
                _maxPrice = value;
                OnPropertyChanged(nameof(MaxPrice));
            }
        }

        #endregion Properties
    }
}
