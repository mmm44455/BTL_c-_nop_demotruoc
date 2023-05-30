using Microsoft.Data.SqlClient;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace BTL_c_
{
    public partial class Form1 : Form
    {
        TelegramBotClient botClient;

        void AddLog(string msg)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.BeginInvoke((MethodInvoker)delegate ()
                {
                    AddLog(msg);
                });
            }
            else
            {
                txtLog.AppendText(msg + "\r\n");
            }
        }
        public Form1()
        {
            InitializeComponent();
            string token = "6238418467:AAGGw8RVz_JTiMeXa1i90gZa46wdW-KO1R4";

            //Console.WriteLine("my token=" + token);

            botClient = new TelegramBotClient(token);

            using CancellationTokenSource cts = new();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
            };

            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,  //hàm xử lý khi có người chát đến
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            Task<User> me = botClient.GetMeAsync();

            AddLog($"Bot begin working: @{me.Result.Username}");

            //async lập trình bất đồng bộ
            async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            {
                // Only process Message updates: https://core.telegram.org/bots/api#message
                if (update.Message is not { } message)
                    return;
                // Only process text messages
                if (message.Text is not { } messageText)
                    return;

                var chatId = message.Chat.Id;

                AddLog($"{chatId}: {messageText}");
                string reply = "";
                if (messageText.StartsWith("/cmd dir ") || messageText.StartsWith("/cmd type "))
                {
                    string cmd = messageText.Substring(5);
                    string st = "Data Source=duycao;Initial Catalog=thongtincanhan;Encrypt = False ;Integrated Security=True";
                    using SqlConnection sql = new SqlConnection(st);
                    try
                    {
                        sql.Open();
                        string thongtin = " select * from thongtincanhan";
                        using SqlCommand xuat = new SqlCommand(thongtin, sql);
                        using SqlDataReader reader = xuat.ExecuteReader();
                        while (reader.Read())
                        {
                            reply += reader.GetString(0);
                        }
                       
                    }
                    catch (Exception ex)
                    {
                        reply = "Error executing SQL query: " + ex.Message;
                    }
                    finally
                    {
                        // Đóng kết nối cơ sở dữ liệu
                        sql.Close();
                    }
                }
                else
                {
                    reply = "You said: " + messageText;
                }

                AddLog(reply);
                // Echo received message text
                //Telegram.Bot.Types.Message sentMessage = await botClient.SendTextMessageAsync(
                //    chatId: chatId,
                //    text: reply,
                //    cancellationToken: cancellationToken);
            }

            Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
            {
                var ErrorMessage = exception switch
                {
                    ApiRequestException apiRequestException
                        => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                    _ => exception.ToString()
                };

                Console.WriteLine(ErrorMessage);
                return Task.CompletedTask;
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void txtLog_TextChanged(object sender, EventArgs e)
        {

        }
    }
}