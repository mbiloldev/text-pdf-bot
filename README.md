# 📄 Telegram PDF Bot (C#)

Foydalanuvchi yuborgan **matn** va **rasmni** avtomatik ravishda **PDF faylga** aylantirib qaytaruvchi Telegram bot.

---

## 📦 Kerakli paketlar (NuGet)

```
Telegram.Bot >= 21.0
itext7 >= 8.0
```

O'rnatish:
```bash
dotnet add package Telegram.Bot
dotnet add package itext7
```

---

## ⚙️ Sozlash

`Program.cs` faylida tokenni o'zgartiring:

```csharp
private static readonly string BotToken = "YOUR_TELEGRAM_BOT_TOKEN";
```

> **Token olish:** [@BotFather](https://t.me/BotFather) → `/newbot` → tokenni nusxalab oling.

---

## 🔨 Build va ishga tushirish

```bash
dotnet build
dotnet run
```

---

## 💬 Bot buyruqlari

| Buyruq | Tavsif |
|--------|--------|
| `/start` | Salomlashuv xabari |
| `/makepdf` | PDF yaratib yuboradi |
| *(matn)* | Matnni saqlaydi |
| *(rasm)* | Rasmni saqlaydi (ixtiyoriy) |

---

## 🔄 Ishlash tartibi

```
1. Foydalanuvchi matn yuboradi
2. Foydalanuvchi rasm yuboradi (ixtiyoriy)
3. /makepdf buyrug'ini yuboradi
4. Bot PDF yaratib yuboradi
```

---

## 📁 Loyiha tuzilmasi

```
TelegramPdfBot/
├── Program.cs
├── TelegramPdfBot.csproj
└── README.md
```

---

## ⚠️ Muhim eslatmalar

- Bir foydalanuvchi faqat **bitta matn** va **bitta rasm** saqlaydi — `/makepdf` dan keyin kesh tozalanadi
- Vaqtinchalik fayllar (`/tmp`) PDF yuborilgandan keyin avtomatik o'chiriladi
- O'zbek/kirill harflari uchun tizimda unicode font bo'lishi kerak
