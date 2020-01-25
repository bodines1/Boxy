using Boxy.Mvvm;
using Boxy.Properties;
using Boxy.Utilities;
using PdfSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Boxy.ViewModels.Dialogs
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
            PageSizeOptions = Enum.GetValues(typeof(PageSize)).Cast<PageSize>().ToList();
            PageSizeOptions.Remove(PageSize.Undefined);
            PdfSaveFolder = Settings.Default.PdfSaveFolder;
            PdfJpegQuality = Settings.Default.PdfJpegQuality;
            PdfHasCutLines = Settings.Default.PdfHasCutLines;
            PdfScaling = Settings.Default.PdfScaling;
            PdfOpenWhenSaveDone = Settings.Default.PdfOpenWhenSaveDone;
            PdfPageSize = Settings.Default.PdfPageSize;
        }

        #endregion Constructors

        #region Fields

        private string _pdfSaveFolder;
        private PageSize _pdfPageSize;
        private double _pdfScaling;
        private bool _pdfHasCutLines;
        private int _pdfJpegQuality;
        private bool _pdfOpenWhenSaveDone;

        #endregion Fields

        #region Properties

        public List<PageSize> PageSizeOptions { get; }

        /// <summary>
        /// Way to display to user what the expected cards per page with their settings will be.
        /// </summary>
        public int CardsPerPage
        {
            get
            {
                var sdgfsdgf = new CardPdfBuilder(PdfPageSize, PdfScaling / 100.0, PdfHasCutLines);
                return sdgfsdgf.Pages.First().CardsPerPage;
            }
        }

        public double ExpectedMegabytes
        {
            get
            {
                // y = 0.8361x2 - 2.7034x + 5.5693 <- MB where x = quality for a 60 image deck
                double beforeSize = 0.8361 * Math.Pow(PdfJpegQuality / 100.0, 2) - 2.7034 * PdfJpegQuality / 100.0 + 5.5693;
                return beforeSize * Math.Pow(PdfScaling / 100.0, 2);
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
                
                OnPropertyChanged(nameof(ExpectedMegabytes));
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
        /// PdfScaling setting.
        /// </summary>
        public double PdfScaling
        {
            get
            {
                return _pdfScaling;
            }

            set
            {
                if (value < 95)
                {
                    _pdfScaling = 95;
                }
                else if (value > 105)
                {
                    _pdfScaling = 105;
                }
                else
                {
                    _pdfScaling = value;
                }
                
                OnPropertyChanged(nameof(ExpectedMegabytes));
                OnPropertyChanged(nameof(PdfScaling));
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
        /// PdfOpenWhenSaveDone setting.
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

        #endregion Properties
    }
}
