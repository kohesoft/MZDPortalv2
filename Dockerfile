# Temel imaj olarak mono kullanın
FROM mono:latest

# Çalışma dizinini ayarlayın
WORKDIR /app

# Proje dosyalarını kopyalayın
COPY . .

# Projeyi derleyin
RUN msbuild MZDNETWORK.sln

# Uygulamayı çalıştırın
CMD ["mono", "bin/Debug/MZDNETWORK.exe"]