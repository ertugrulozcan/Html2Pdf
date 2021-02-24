using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Html2Pdf.HtmlToPdfConverter
{
	internal sealed class PdfConvertProcess : IDisposable
	{
		#region Fields

		private readonly Process process;

		#endregion

		#region Properties

		private AutoResetEvent OutputWaitHandler { get; set; }
		
		private AutoResetEvent ErrorWaitHandler { get; set; }
		
		private List<string> OutputList { get; }
		
		private List<string> ErrorList { get; }

		#endregion
		
		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="webKitApplicationPath"></param>
		/// <param name="outputFilePath"></param>
		/// <param name="parameters"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public PdfConvertProcess(string webKitApplicationPath, string outputFilePath, IDictionary<string, string> parameters)
		{
			if (string.IsNullOrEmpty(webKitApplicationPath))
			{
				throw new ArgumentNullException(nameof(webKitApplicationPath), "wkhtmltopdf application path could not null or empty");
			}

			if (!File.Exists(webKitApplicationPath))
			{
				throw new Exception($"File '{webKitApplicationPath}' not found. Check if wkhtmltopdf application is installed.");
			}
			
			var args = parameters != null && parameters.Any() ? 
				string.Join(" ", parameters.Select(x => $"--{x.Key} {x.Value}")) : 
				string.Empty;

			args += $" - {outputFilePath}";
			
			this.process = new Process
			{
				StartInfo =
				{
					FileName = webKitApplicationPath,
					Arguments = args,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					RedirectStandardInput = true
				}
			};
			
			this.process.OutputDataReceived += ProcessOnOutputDataReceived;
			this.process.ErrorDataReceived += ProcessOnErrorDataReceived;
			
			this.OutputWaitHandler = new AutoResetEvent(false);
			this.ErrorWaitHandler = new AutoResetEvent(false);

			this.OutputList = new List<string>();
			this.ErrorList = new List<string>();
		}

		#endregion
		
		#region Events

		public event EventHandler<string> OutputDataReceived;
		public event EventHandler<string> ErrorDataReceived;
		public event EventHandler<byte[]> Completed;

		#endregion

		#region Event Handlers

		private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (e.Data == null)
			{
				this.OutputWaitHandler.Set();
			}
			else
			{
				this.OutputList.Add(e.Data);
				this.OutputDataReceived?.Invoke(this, e.Data);
			}
		}
		
		private void ProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (e.Data == null)
			{
				this.ErrorWaitHandler.Set();
			}
			else
			{
				this.ErrorList.Add(e.Data);
				this.ErrorDataReceived?.Invoke(this, e.Data);
			}
		}

		#endregion
		
		#region Methods

		public string GetOutput()
		{
			return string.Join("\r\n", this.OutputList);
		}

		public async Task StartAsync(string input, string outputFilePath, int timeout = 60000)
		{
			try
			{
				process.Start();

				process.BeginOutputReadLine();
				process.BeginErrorReadLine();

				if (!string.IsNullOrEmpty(input))
				{
					await using (var stream = process.StandardInput)
					{
						byte[] buffer = Encoding.UTF8.GetBytes(input);
						await stream.BaseStream.WriteAsync(buffer, 0, buffer.Length);
						await stream.WriteLineAsync();
					}
				}

				if (process.WaitForExit(timeout) && this.OutputWaitHandler.WaitOne(timeout) && this.ErrorWaitHandler.WaitOne(timeout))
				{
					if (process.ExitCode != 0)
					{
						throw new Exception($"Html to PDF conversion was failed. Wkhtmltopdf output: \r\n{string.Join("\r\n", this.ErrorList)}");
					}
				}
				else
				{
					if (!process.HasExited)
						process.Kill();

					throw new Exception($"HTML to PDF conversion process has not finished in the given period. ({timeout} milliseconds)");
				}
				
				await using (Stream fileStream = new FileStream(outputFilePath, FileMode.Open))
				{
					var bytes = ConvertStreamToByteArray(fileStream);
					this.Completed?.Invoke(this, bytes);
				}
			}
			finally
			{
				this.OutputList.Clear();
				this.ErrorList.Clear();
				
				if (File.Exists(outputFilePath))
					File.Delete(outputFilePath);
			}
		}
		
		public static byte[] ConvertStreamToByteArray(Stream stream)
		{
			long originalPosition = 0;

			if (stream.CanSeek)
			{
				originalPosition = stream.Position;
				stream.Position = 0;
			}

			try
			{
				byte[] readBuffer = new byte[4096];

				int totalBytesRead = 0;
				int bytesRead;

				while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
				{
					totalBytesRead += bytesRead;

					if (totalBytesRead == readBuffer.Length)
					{
						int nextByte = stream.ReadByte();
						if (nextByte != -1)
						{
							byte[] temp = new byte[readBuffer.Length * 2];
							Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
							Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
							readBuffer = temp;
							totalBytesRead++;
						}
					}
				}

				byte[] buffer = readBuffer;
				if (readBuffer.Length != totalBytesRead)
				{
					buffer = new byte[totalBytesRead];
					Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
				}
				
				return buffer;
			}
			finally
			{
				if(stream.CanSeek)
				{
					stream.Position = originalPosition; 
				}
			}
		}

		#endregion
		
		#region Disposing

		private bool isDisposed;

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (isDisposed) 
				return;

			if (disposing)
			{
				this.OutputDataReceived = null;
				this.ErrorDataReceived = null;
				this.Completed = null;
				
				this.OutputWaitHandler.Dispose();
				this.OutputWaitHandler = null;
				
				this.ErrorWaitHandler.Dispose();
				this.ErrorWaitHandler = null;
				
				this.process.OutputDataReceived -= ProcessOnOutputDataReceived;
				this.process.ErrorDataReceived -= ProcessOnErrorDataReceived;
				this.process.Dispose();
			}

			isDisposed = true;
		}

		~PdfConvertProcess()
		{
			this.Dispose(false);
		}

		#endregion
	}
}