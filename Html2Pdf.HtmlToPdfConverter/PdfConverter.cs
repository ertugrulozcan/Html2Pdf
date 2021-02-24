using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Html2Pdf.HtmlToPdfConverter.Events;

namespace Html2Pdf.HtmlToPdfConverter
{
	public interface IPdfConverter
    {
        #region Events

        event EventHandler<ConvertCompletedEventArgs> Completed;
        event EventHandler<ConvertFailedEventArgs> Failed;

        #endregion
        
        #region Methods

        Task ConvertAsync(PdfDocument document, TimeSpan? timeout_ = null);

        #endregion
    }
    
	public class PdfConverter : IPdfConverter
    {
        #region Properties

        private string WkHtmlToPdfApplicationPath { get; }
        
        private TimeSpan DefaultTimeOut { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="wkHtmlToPdfApplicationPath"></param>
        public PdfConverter(string wkHtmlToPdfApplicationPath)
        {
            this.WkHtmlToPdfApplicationPath = wkHtmlToPdfApplicationPath;
            this.DefaultTimeOut = TimeSpan.FromSeconds(60);
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options"></param>
        public PdfConverter(IPdfConverterOptions options)
        {
            this.WkHtmlToPdfApplicationPath = Path.GetFullPath(options.WkHtmlToPdfPath);
            this.DefaultTimeOut = TimeSpan.FromMilliseconds(options.TimeOut);
        }

        #endregion

        #region Events

        public event EventHandler<ConvertCompletedEventArgs> Completed;
        public event EventHandler<ConvertFailedEventArgs> Failed;

        #endregion
        
        #region Methods

        public async Task ConvertAsync(PdfDocument document, TimeSpan? timeout_ = null)
        {
            if (!File.Exists(this.WkHtmlToPdfApplicationPath))
            {
                this.Failed?.Invoke(this, new ConvertFailedEventArgs(document.DocumentId, new Exception($"File '{this.WkHtmlToPdfApplicationPath}' not found. Check if wkhtmltopdf application is installed.")));
            }

            TimeSpan timeout;
            if (timeout_ == null)
            {
                timeout = this.DefaultTimeOut;
            }
            else
            {
                timeout = timeout_.Value;
            }
            
            string outputFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");
            
            var args = new Dictionary<string, string>
            {
                { "page-size", document.PaperType.ToString() }
            };
            
            if (document.ExtraParams != null)
                foreach (var (key, value) in document.ExtraParams)
                    args.Add(key, value);

            if (document.Cookies != null)
                foreach (var (key, value) in document.Cookies)
                    args.Add($"cookie {key}", value);

            EventHandler<byte[]> convertCompleteEventHandler = (sender, bytes) =>
            {
                this.Completed?.Invoke(this, new ConvertCompletedEventArgs(document.DocumentId, bytes));
            };
            
            using (var process = new PdfConvertProcess(this.WkHtmlToPdfApplicationPath, outputFilePath, args))
            {
                try
                {
                    process.Completed += convertCompleteEventHandler;
                    await process.StartAsync(document.Html, outputFilePath, (int) timeout.TotalMilliseconds);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    this.Failed?.Invoke(this, new ConvertFailedEventArgs(document.DocumentId, ex));
                }
                finally
                {
                    process.Completed -= convertCompleteEventHandler;
                }
            }
        }

        #endregion
    }
}