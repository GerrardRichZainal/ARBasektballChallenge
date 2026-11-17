# 🏀 AR Basketball Challenge

AR Basketball Challenge adalah permainan basket berbasis Augmented Reality (AR) yang dibuat menggunakan Unity dan Vuforia Engine untuk perangkat Android. Permainan ini memungkinkan pemain untuk mencoba memasukkan bola basket ke dalam ring virtual melalui kamera smartphone. Pemain dapat mengarahkan bola dan mengatur kekuatan lemparan untuk mendapatkan skor.

Project ini menggunakan dinding bertuliskan **INFORMATIKA di lantai 5 (Didepan Lift) Universitas Ciputra Makassar** sebagai Image Target. Ketika kamera smartphone diarahkan ke marker tersebut, objek ring basket virtual akan muncul di layar.

## 🎮 Tentang Project

AR Basketball Challenge dibuat untuk memberikan pengalaman bermain basket yang berbeda dengan memanfaatkan teknologi Augmented Reality. Pemain tidak perlu menggunakan ring basket sungguhan karena objek permainan ditampilkan secara virtual melalui kamera smartphone. Selain permainan utama, terdapat beberapa fitur tambahan seperti tutorial, timer, leaderboard, efek suara, dan halaman hasil permainan.

## ✨ Fitur Permainan

- **Augmented Reality (AR):** Menampilkan ring dan bola basket virtual ketika kamera berhasil mengenali marker.
- **Lempar Bola:** Pemain dapat mengarahkan dan melempar bola menggunakan interaksi pada layar.
- **Sistem Skor:** Poin dihitung berdasarkan jarak lemparan bola.
- **Timer:** Menghitung waktu selama permainan berlangsung.
- **Leaderboard:** Menampilkan daftar skor pemain.
- **Tutorial:** Memberikan panduan mengenai cara bermain.
- **Sound Effect:** Menambahkan suara ketika pemain mendapatkan skor dan saat permainan berakhir.
- **Game Over:** Menampilkan hasil akhir setelah permainan selesai.
- **Play Again:** Memberikan pilihan untuk mengulang permainan.

## 🛠️ Teknologi yang Digunakan

| Teknologi | Kegunaan |
|---|---|
| Unity 6 | Membuat dan mengembangkan permainan |
| C# | Membuat logika dan sistem permainan |
| Vuforia Engine 11.4.4 | Menjalankan fitur Augmented Reality |
| Android | Platform untuk menjalankan permainan |
| TextMesh Pro | Membuat tampilan teks pada game |
| Unity Input System | Mengatur interaksi pemain |

## ⚙️ Kebutuhan untuk Menjalankan Project

Sebelum membuka project, pastikan perangkat sudah memiliki:

- Unity Hub.
- Unity Editor sesuai versi yang digunakan pada project.
- Android Build Support.
- Vuforia Engine versi 11.4.4.
- Smartphone Android yang memiliki kamera.

## 🚀 Cara Menggunakan Project

### 1. Mengambil Project dari GitHub

Clone repository menggunakan perintah berikut:

```bash
git clone LINK_REPOSITORY_GITHUB
```

Ganti `LINK_REPOSITORY_GITHUB` dengan link repository project ini.

### 2. Memasang Vuforia Engine

File Vuforia Engine tidak disertakan di repository karena ukurannya terlalu besar untuk diunggah langsung ke GitHub.

Untuk memasangnya kembali:

1. Download Vuforia Engine versi 11.4.4 melalui website resmi Vuforia.
2. Siapkan file package `com.ptc.vuforia.engine-11.4.4.tgz`.
3. Letakkan file tersebut pada lokasi yang sesuai dengan konfigurasi dependency di `Packages/manifest.json`.
4. Buka kembali project melalui Unity Hub.
5. Tunggu sampai Unity selesai memuat package yang dibutuhkan.

Website resmi: https://developer.vuforia.com/

### 3. Mengatur Vuforia

1. Buka project melalui Unity.
2. Masukkan Vuforia License Key pada pengaturan Vuforia.
3. Pastikan database Image Target sudah tersedia.
4. Periksa kembali marker INFORMATIKA yang digunakan pada permainan.

Jangan membagikan License Key pribadi ke repository publik.

### 4. Menjalankan Project

1. Buka Unity Hub.
2. Pilih Add Project from Disk.
3. Pilih folder ARBasketball.
4. Buka project menggunakan versi Unity yang sesuai.
5. Buka scene permainan.
6. Jalankan project untuk mencoba fitur permainan.

### 5. Membuat APK Android

1. Buka Build Profiles di Unity.
2. Pilih platform Android.
3. Periksa pengaturan Android yang dibutuhkan.
4. Jalankan proses Build.
5. Setelah selesai, install APK pada smartphone Android untuk melakukan pengujian.

## 🏀 Cara Bermain

Berikut langkah-langkah untuk memainkan AR Basketball Challenge:

1. Buka aplikasi AR Basketball Challenge pada smartphone.
2. Pilih menu Play pada halaman utama.
3. Arahkan kamera ke dinding bertuliskan INFORMATIKA di lantai 5.
4. Tunggu sampai ring dan bola basket virtual muncul.
5. Arahkan bola sesuai posisi yang diinginkan.
6. Lakukan lemparan untuk memasukkan bola ke dalam ring.
7. Dapatkan skor berdasarkan jarak lemparan.
8. Lihat hasil permainan dan skor pada halaman akhir.
9. Gunakan fitur Play Again untuk mencoba bermain kembali.

Beberapa folder hasil generate Unity, file sementara, hasil Build, APK, dan package Vuforia berukuran besar tidak disertakan dalam repository agar ukuran project lebih ringan. Vuforia Engine perlu dipasang secara terpisah sebelum project dapat dijalankan dengan normal.
