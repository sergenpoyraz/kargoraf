# Kargo Raf

Bakkal ve küçük işletmeler için hızlı kargo takip uygulaması. Müşteri kargolarını fiziksel bölümlere (raf, kutu, dolap vb.) kaydeder, arar ve teslim edildiğinde geçmişe taşır.

## Gereksinimler

- Windows 10 / 11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Çalıştırma

```powershell
cd "KargoRaf"
dotnet restore
dotnet run
```

## Publish (kurulum gerektirmeden)

Tek klasörde taşınabilir sürüm:

```powershell
cd "KargoRaf"
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```

Çıktı:

`KargoRaf\bin\Release\net8.0-windows\win-x64\publish\KargoRaf.exe`

`Assets` klasörü publish çıktısına otomatik kopyalanır.

## Arayüz özellikleri

- Hızlı ekleme alanı, bölüm seçimi ve opsiyonel not girişi
- Notlu kayıtlarda sarı badge, preview ve tooltip
- Mini widget: bölüm sayaçları, arama ve tıklanarak ana pencerede vurgulama
- Klavye: Ctrl+1..5 hızlı ekle, Enter ekle, Ctrl+F ara, Esc temizle, Ctrl+W widget

## Veritabanı konumu

- `%AppData%\KargoRaf\kargoraf.db`
- Log: `%AppData%\KargoRaf\logs\app.log`
- Yedekler: `%AppData%\KargoRaf\backups\`

## Yedek alma

1. Sistem tepsisi menüsü → **Yedek Al**
2. Ayarlar ekranı → **Veritabanı Yedeği Al**
3. CSV dışa aktarım: Ayarlar → **Aktif CSV** / **Geçmiş CSV**

Yedek dosyası: `kargoraf_YYYYMMDD_HHMMSS.backup.db`

## Klavye kısayolları

| Kısayol | İşlem |
|---------|--------|
| `Ctrl + F` | Arama kutusuna odaklan |
| `Ctrl + 1..9` | Yazılan ismi ilgili bölüme ekle |
| `Enter` | Seçili bölüme ekle |
| `Ctrl + W` | Mini widget aç/kapat |
| `Esc` | Aramayı temizle |
| `Delete` | Seçili kaydı teslim edildi yap |

## Özellikler

- 5 bölümlü ana ekran (bölüm adları ayarlardan değiştirilebilir)
- Ayarlardan yeni bölüm ekleme (en fazla 12) ve boş bölüm kaldırma
- Hızlı isim ekleme ve arama
- Teslim edildi + 5 sn geri al
- Geçmiş ekranı (bugün / 7 gün / bu ay / tümü)
- Mini widget (always-on-top)
- Sistem tepsisine küçültme
- SQLite yerel veritabanı
- CSV ve veritabanı yedekleme

## Proje yapısı

```
KargoRaf/
├── Models/
├── ViewModels/
├── Views/
├── Services/
├── Data/
├── Converters/
├── Commands/
├── Helpers/
└── Assets/
```

## Notlar

- Kapat (X) tuşu uygulamayı kapatmaz; sistem tepsisine küçültür.
- Gerçek çıkış: tepsi menüsü → **Çıkış**
- Aynı isim tekrar eklenebilir; uyarı gösterilir.
- Çift tıklama ile kayıt düzenlenebilir; değişiklikler otomatik kaydedilir.
- Kullanım bilgisi: uygulama içi **Yardım** menüsü.

## Teslim / geçmiş doğrulama senaryosu

1. Ana ekranda `Ahmet Yılmaz` adını yazıp Bölüm 2'ye notlu veya notsuz ekleyin.
2. Kaydı **Teslim** yapın; kayıt ana aktif listeden kalkmalı.
3. **Geçmiş > Bugün** filtresinde aynı kayıt, eklenme ve teslim zamanı ile görünmeli.
4. **Tekrar Aktif Et** ile kayıt aynı bölümde yeniden aktif listeye dönmeli.
5. Uygulamayı kapatıp açınca aktif ve geçmiş kayıtları korunmalı.
6. Widget açıldığında bölüm numarası/chip göstermeden sadece aktif alıcı isimleri akmalı; notlu kayıtlarda sarı not simgesi görünmeli.
