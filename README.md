# Html2PDF Converter (wkhtmltopdf .Net Core Wrapper)

A wkhtmltopdf wrapper for HTML to Pdf converting on .NET Core.

https://wkhtmltopdf.org/

## Installation

You need to wkhtmltopdf application, installation packages available on https://wkhtmltopdf.org/downloads.html

### Nuget

```
Install-Package ertis.html2pdf
```

## Usage

```C#
var options = new PdfConverterOptions() 
{
    WkHtmlToPdfPath = "/usr/local/bin/wkhtmltopdf",
    TimeOut = 60000
};

var pdfConverter = new PdfConverter(options);
pdfConverter.Completed += this.PdfConverterOnCompleted;
pdfConverter.Failed += this.PdfConverterOnFailed;

await pdfConverter.ConvertAsync(new PdfDocument
{
    DocumentId = documentId,
    Html = html,
    PaperType = PaperTypes.A4
});

// ...

#region Event Handlers

private void PdfConverterOnCompleted(object sender, ConvertCompletedEventArgs eventArgs)
{
    var bytes = eventArgs.Document;
    File.WriteAllBytes("Foo.pdf", bytes);
}

private void PdfConverterOnFailed(object sender, ConvertFailedEventArgs eventArgs)
{
    Console.WriteLine(eventArgs.Exception);
}

#endregion
```

### Dependency Injection

appsettings.json

```json
...

"PdfConverter": {
		"WkHtmlToPdfPath": "wkhtmltopdf",
		"TimeOut": 60000
	}

...
```


```C#
services.Configure<PdfConverterOptions>(this.Configuration.GetSection("PdfConverter"));
services.AddSingleton<IPdfConverterOptions>(serviceProvider => serviceProvider.GetRequiredService<IOptions<PdfConverterOptions>>().Value);

services.AddSingleton<IPdfConverter, PdfConverter>();
```

## Using with a dockerized application

Dockerfile of host application (tested with container with Debian 10 amd64)

```Dockerfile

...

RUN apt update --fix-missing
RUN dpkg --configure -a
RUN apt install -f xfonts-75dpi xfonts-base gvfs colord glew-utils libvisual-0.4-plugins gstreamer1.0-tools opus-tools qt5-image-formats-plugins qtwayland5 qt5-qmltooling-plugins librsvg2-bin lm-sensors -y
RUN apt install wget -y
RUN wget https://github.com/wkhtmltopdf/wkhtmltopdf/releases/download/0.12.5/wkhtmltox_0.12.5-1.stretch_amd64.deb
RUN dpkg -i wkhtmltox_0.12.5-1.stretch_amd64.deb

...

```

## Licence

This repository is under GPL3 Licence.
