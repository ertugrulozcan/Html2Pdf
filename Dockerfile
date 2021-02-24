FROM debian:10

LABEL maintainer="Ertuğrul Özcan ertugrul.ozcan@bil.omu.edu.tr"

RUN apt update --fix-missing
RUN dpkg --configure -a
RUN apt install -f xfonts-75dpi xfonts-base gvfs colord glew-utils libvisual-0.4-plugins gstreamer1.0-tools opus-tools qt5-image-formats-plugins qtwayland5 qt5-qmltooling-plugins librsvg2-bin lm-sensors -y
RUN apt install wget -y
RUN wget https://github.com/wkhtmltopdf/wkhtmltopdf/releases/download/0.12.5/wkhtmltox_0.12.5-1.stretch_amd64.deb
RUN dpkg -i wkhtmltox_0.12.5-1.stretch_amd64.deb

ENTRYPOINT ["wkhtmltopdf"]

CMD ["-H"]