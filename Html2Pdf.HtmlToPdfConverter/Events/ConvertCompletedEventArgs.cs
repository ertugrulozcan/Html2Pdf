using System;

namespace Html2Pdf.HtmlToPdfConverter.Events
{
	public class ConvertCompletedEventArgs : EventArgs
	{
		#region Properties

		public string DocumentId { get; }
		
		public byte[] Document { get; }

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="documentId"></param>
		/// <param name="document"></param>
		public ConvertCompletedEventArgs(string documentId, byte[] document)
		{
			this.DocumentId = documentId;
			this.Document = document;
		}

		#endregion
	}
}