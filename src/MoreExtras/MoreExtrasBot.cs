using System;

using System.Diagnostics;

using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputMessageContents;
using Telegram.Bot.Types.ReplyMarkups;

using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SQS;

using MongoDB;
using MongoDB.Driver;

using MoreExtras.Util;

namespace MoreExtras
{
    public class MoreExtrasBot
    {
        TelegramBotClient BotClient;
        User BotUser;
        RegionEndpoint region = RegionEndpoint.USEast1;
        AmazonS3Client s3 = null;
        AmazonSQSClient sqs = null;
        IMongoClient mng = null;
        public string CodeBaseFileName()
        {
            return Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().CodeBase);
        }
        public string ReadFromUserProfile(string prefix)
        {
            var pathName = $"{prefix}{CodeBaseFileName()}.bot";
            var filePath = $"{Environment.ExpandEnvironmentVariables($"%HOME%/{pathName}")}";
            return System.IO.File.ReadAllText(filePath).Trim();
        }
        public void Run()
        {
            var codeBaseFileName = CodeBaseFileName();
            var telegramBotToken = ReadFromUserProfile(".telegram/");
            var awsCredentials = new StoredProfileAWSCredentials($"{codeBaseFileName}.bot").GetCredentials();
#if DEBUG
            Console.WriteLine($"Token: {telegramBotToken}");
            Console.WriteLine($"AWS credentials: {awsCredentials.AccessKey}:{awsCredentials.SecretKey}");
#endif

            mng = new MongoClient();
            var mdb = mng.GetDatabase($"{codeBaseFileName}");
            Console.WriteLine($"Mongo: {mdb.DatabaseNamespace} @ {mng}");
            //s3 = new AmazonS3Client(awsCredentials.AccessKey, awsCredentials.SecretKey, region);
            //sqs = new AmazonSQSClient(awsCredentials.AccessKey, awsCredentials.SecretKey, region);
            //Console.WriteLine($"s3: {s3.ToString()}");

            BotClient = new TelegramBotClient(telegramBotToken);

            BotClient.OnInlineQuery += BotOnInlineQueryReceived;
            BotClient.OnMessage += BotOnMessageReceived;
            //Bot.OnMessageEdited += BotOnMessageReceived;
            //Bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            //Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            BotClient.OnReceiveError += (sndr, err) => { Debug.Fail($"{err}"); };

            BotUser = BotClient.GetMeAsync().Result;

            //BotClient.PollingTimeout = new TimeSpan(0, 0, 20);
            BotClient.StartReceiving();
            Console.WriteLine($"Your bot {BotUser.Username}({BotUser.Id}) is running");
            Console.Write($"Press Enter to stop it and exit...");
            Console.ReadLine();
            BotClient.StopReceiving();
        }
        // an easy way of initiating private chat
        void BotOnInlineQueryReceived(object sender, InlineQueryEventArgs inlineQueryEventArgs)
        {
            // for now, just a way of creating/switching to private chat
            InlineQueryHandlerForPrivateChat(sender, inlineQueryEventArgs);
        }
        async void InlineQueryHandlerForPrivateChat(object sender, InlineQueryEventArgs inlineQueryEventArgs)
        {
            await BotClient.AnswerInlineQueryAsync(inlineQueryEventArgs.InlineQuery.Id,
                new InlineQueryResult[] { },
                switchPmText: $"Go chat with {BotUser?.FirstName ?? "the bot"}"
            );
        }
        void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            if (message != null)
                switch (message.Type)
                {
                    case MessageType.TextMessage:
                        if (message.Text.IsNotNullOrEmpty()) BotOnTextMessageReceived(message);
                        break;
                }
        }
        async void BotOnTextMessageReceived(Message message)
        {
            //Debug.Assert(0 < message.Text.Length);
            switch (message.Text[0])
            {
                case '/':
                    if(1 < message.Text.Length) BotOnCommand(message.Text.Substring(1));
                    break;
            }
            await BotClient.SendTextMessageAsync(message.Chat.Id, $"Why did you say '{message.Text}'?");
        }

        async void BotOnCommand(string cmd)
        {

        }
    }
}