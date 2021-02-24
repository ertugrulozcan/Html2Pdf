using System.Collections.Generic;

namespace Html2Pdf.HtmlToPdfConverter
{
	public class PdfDocument
	{
		#region Properties

		public string DocumentId { get; set; }
		
		public PaperTypes PaperType { get; set; }
		
		public string Html { get; set; }
		
		public Dictionary<string, string> Cookies { get; set; }
		
		public Dictionary<string, string> ExtraParams { get; set; }
		
		#endregion
	}
}