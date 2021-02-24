namespace Html2Pdf.HtmlToPdfConverter
{
	public interface IPdfConverterOptions
	{
		string WkHtmlToPdfPath { get; set; }
		
		int TimeOut { get; set; }
	}
	
	public class PdfConverterOptions : IPdfConverterOptions
	{
		public string WkHtmlToPdfPath { get; set; }
		
		public int TimeOut { get; set; }
	}
}