using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using iText.Kernel.Pdf;
using iText.Kernel.Font;
using iText.IO.Font;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Image;

class Program
{
    private static readonly string BotToken = "YOUR_TELEGRAM_BOT_TOKEN";
    private static TelegramBotClient BotClient;

    private static ConcurrentDictionary<long, UserData> UserCache = new ConcurrentDictionary<long, UserData>();

    class UserData
    {
        public string Text { get; set; } = "";
        public string ImagePath { get; set; } = null;
    }

    static async Task Main(string[] args)
    {
        BotClient = new TelegramBotClient(BotToken);

        using var cts = new CancellationTokenSource();

        BotClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandlePollingErrorAsync,
            cancellationToken: cts.Token
        );

        var me = await BotClient.GetMeAsync();
        Console.WriteLine($"@{me.Username} bot ishga tushdi!");
        Console.ReadLine();

        cts.Cancel();
    }

    private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message) return;
        long chatId = message.Chat.Id;

        UserCache.TryAdd(chatId, new UserData());

        if (message.Type == MessageType.Text)
        {
            string messageText = message.Text;

            if (messageText == "/start")
            {
                await botClient.SendTextMessageAsync(chatId, "Salom! Menga PDF-ga joylash uchun matn va rasm yuboring. Keyin esa /makepdf komandasini bosing.", cancellationToken: cancellationToken);
                return;
            }
            else if (messageText == "/makepdf")
            {
                await CreateAndSendPdfAsync(botClient, chatId, cancellationToken);
                return;
            }

            UserCache[chatId].Text = messageText;
            await botClient.SendTextMessageAsync(chatId, "Matn qabul qilindi! Endi rasm yuborishingiz mumkin yoki /makepdf buyrug'ini bering.", cancellationToken: cancellationToken);
        }

        if (message.Type == MessageType.Photo)
        {
            var photo = message.Photo[^1];
            var fileId = photo.FileId;

            string localFilePath = Path.Combine(Path.GetTempPath(), $"{fileId}.jpg");

            using (var saveFileStream = File.Create(localFilePath))
            {
                await botClient.GetInfoAndDownloadFileAsync(fileId, saveFileStream, cancellationToken);
            }

            UserCache[chatId].ImagePath = localFilePath;
            await botClient.SendTextMessageAsync(chatId, "Rasm qabul qilindi! PDF tayyorlash uchun /makepdf buyrug'ini yuboring.", cancellationToken: cancellationToken);
        }
    }

    private static async Task CreateAndSendPdfAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        if (!UserCache.TryGetValue(chatId, out var data))
        {
            await botClient.SendTextMessageAsync(chatId, "Iltimos, avval matn yoki rasm yuboring.", cancellationToken: cancellationToken);
            return;
        }

        if (string.IsNullOrEmpty(data.Text) && data.ImagePath == null)
        {
            await botClient.SendTextMessageAsync(chatId, "Hech qanday ma'lumot yubormadingiz! Iltimos, oldin rasm yoki matn yuboring.", cancellationToken: cancellationToken);
            return;
        }

        await botClient.SendTextMessageAsync(chatId, "PDF fayl tayyorlanmoqda, iltimos kuting...", cancellationToken: cancellationToken);

        string pdfPath = Path.Combine(Path.GetTempPath(), $"Document_{chatId}.pdf");

        try
        {
            using (PdfWriter writer = new PdfWriter(pdfPath))
            using (PdfDocument pdf = new PdfDocument(writer))
            using (Document document = new Document(pdf))
            {
                PdfFont font = PdfFontFactory.CreateFont("Helvetica", PdfEncodings.UTF8, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);

                if (!string.IsNullOrEmpty(data.Text))
                {
                    document.Add(new Paragraph(data.Text).SetFont(font).SetFontSize(14));
                }

                if (data.ImagePath != null && File.Exists(data.ImagePath))
                {
                    ImageData imageData = ImageDataFactory.Create(data.ImagePath);
                    Image image = new Image(imageData);
                    image.SetMaxWidth(500);
                    document.Add(image);
                }
            }

            using (var stream = File.OpenRead(pdfPath))
            {
                await botClient.SendDocumentAsync(
                    chatId: chatId,
                    document: InputFile.FromStream(stream, "Generated_Document.pdf"),
                    caption: "Sizning PDF faylingiz tayyor! 🎉",
                    cancellationToken: cancellationToken
                );
            }

            if (data.ImagePath != null && File.Exists(data.ImagePath))
                File.Delete(data.ImagePath);

            if (File.Exists(pdfPath))
                File.Delete(pdfPath);

            UserCache.TryRemove(chatId, out _);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Xatolik: {ex.Message}");
            await botClient.SendTextMessageAsync(chatId, "PDF yaratishda xatolik yuz berdi. Qaytadan urinib ko'ring.", cancellationToken: cancellationToken);
        }
    }

    private static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Telegram API Xatoligi: {exception.Message}");
        return Task.CompletedTask;
    }
}
