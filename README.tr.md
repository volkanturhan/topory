# Topory

**[English](README.md) | Türkçe**

Hafif bir Windows "her zaman üstte" yöneticisi.

Topory sistem tepsisinde sessizce durur. Bir kısayola bas, kullandığın pencere
diğer her şeyin üstüne sabitlensin — çalışırken bir videoyu, notları, bir
referansı ya da hesap makinesini görünür tutmak için birebir. Tekrar bas,
sabitleme kalksın. Küçük bir pencere sabitlediklerini listeler, tek tek bırakabilirsin.

<p align="center">
  <img src="docs/screenshot.png" alt="Topory sabitlenen pencereler yöneticisi" width="520" />
</p>

## Özellikler

- **Üstte sabitle** — global kısayol (`Ctrl + Shift + T`) odaktaki pencereyi her
  şeyin üstünde tutar; tekrar bas, kalksın.
- **Her pencerede çalışır** — pencerenin kendi "en üstte" bayrağını değiştirir,
  böylece Topory kapansa bile üstte kalır (Topory çıkışta hepsini serbest bırakır).
- **Sabitlenenler listesi** — bir pencere o an sabitli olanları gösterir; tek tek
  ya da tümünü birden kaldır.
- **Koyu ya da açık** — menüden **Sistem**, **Koyu** ya da **Açık** temasını seç.
  Varsayılan **Sistem**, yani Windows ayarını takip eder.
- **Windows ile başla** — isteğe bağlı, menüden aç/kapa.
- **İngilizce & Türkçe** — arayüz dilini menüden değiştir.
- **Tasarımı gereği gizli** — her şey senin makinende kalır, hiçbir şey yüklenmez.

## Çalıştır

Topory henüz hazır bir indirme olarak yayınlanmadı, bu yüzden şimdilik kaynaktan
çalıştırıyorsun. Windows'ta [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
(sadece runtime değil, SDK) kurulu olmalı.

```bash
git clone https://github.com/volkanturhan/Topory.git
cd Topory
dotnet run --project Topory/Topory.csproj
```

Topory sessizce sistem tepsisinde başlar — **hiçbir pencere açılmaz**. Bu
normaldir; kısayolu kullan ya da sabitlediklerini görmek için tepsi ikonuna çift tıkla.

## Nasıl kullanılır

1. Topory'i başlat — sessizce sistem tepsisine yerleşir.
2. Görünür tutmak istediğin pencereye tıkla, sonra **`Ctrl + Shift + T`**'ye bas
   (ya da tepsiden **Geçerli pencereyi sabitle**). Artık her şeyin üstünde.
3. Aynı pencerenin üzerindeyken **`Ctrl + Shift + T`**'ye tekrar bas, kalksın.
4. Tepsi ikonuna çift tıkla (ya da **Sabitlenen pencereler**): sabitlediklerini
   gör — birini **Sabitlemeyi kaldır** ya da **Tümünü kaldır**.

Tepsi ikonuna sağ tık: **Geçerli pencereyi sabitle**, **Sabitlenen pencereler**,
**Windows ile başlat**, dil ve **Çıkış**. Çıkış, sabitlenen tüm pencereleri serbest bırakır.

## Paylaşılabilir exe oluştur

SDK olmadan birine verebileceğin bağımsız bir `.exe` mi istiyorsun? Kendin
derle — çıktı repoya dahil edilmez:

```bash
# dist/ içine derler (self-contained Topory.exe + lite sürüm)
pwsh tools/publish.ps1
```

## Teknoloji

- C# / WPF, .NET 8 (Windows)
- Üçüncü parti bağımlılık yok

## Lisans

MIT — bkz. [LICENSE](LICENSE).
