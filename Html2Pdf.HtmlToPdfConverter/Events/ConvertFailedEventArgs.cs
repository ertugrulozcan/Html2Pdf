using System;

namespace Html2Pdf.HtmlToPdfConverter.Events
{
	public class ConvertFailedEventArgs
	{
		#region Properties

		public string DocumentId { get; }
		
		public Exception Exception { get; }

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="documentId"></param>
		/// <param name="exception"></param>
		public ConvertFailedEventArgs(string documentId, Exception exception)
		{
			this.DocumentId = documentId;
			this.Exception = exception;
		}

		#endregion
	}
}