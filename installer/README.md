# Kargo Raf Kurulum Paketi

## Setup olusturma

```powershell
powershell -ExecutionPolicy Bypass -File "installer\build-setup.ps1"
```

Cikti: `installer\output\KargoRaf-Setup-3.0.0.exe` (~90 MB, 32-bit + 64-bit)

## GitHub Release yukleme

Setup dosyasi repoya commit edilmez (buyuk dosya). Tag `v3.0.0` push edildikten sonra:

1. https://github.com/sergenpoyraz/kargoraf/releases/new?tag=v3.0.0
2. Baslik: `Kargo Raf v3.0.0`
3. `KargoRaf-Setup-3.0.0.exe` dosyasini surukle-birak
4. **Publish release**

## Bakkalda kurulum

1. Releases sayfasindan setup exe indir
2. Calistir, kur
3. Baslat menusunden **Kargo Raf** ac

Veriler: `%AppData%\KargoRaf\`
