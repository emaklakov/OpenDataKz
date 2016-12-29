using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using OpenDataKzBot.Controllers;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace OpenDataKzBot
{
    public class BodyBot
    {
        private static void UpdateReceived(Update update)
        {
            switch (update.Type)
            {
                case UpdateType.MessageUpdate:
                    MessageReceived(update.Message);
                    break;

                case UpdateType.InlineQueryUpdate:
                    break;

                case UpdateType.ChosenInlineResultUpdate:
                    break;

                case UpdateType.CallbackQueryUpdate:            
                    break;

                case UpdateType.EditedMessage:
                    MessageReceived(update.EditedMessage);
                    break;
            }
        }

        private static void MessageReceived(Message ReceivedMessage)
        {
            if (ReceivedMessage == null || ReceivedMessage.Type != MessageType.TextMessage)
                return;

            DateTime MessageSendDate = Helper.GetLocaDateTime(ReceivedMessage.Date);

            if ((DateTime.Now - MessageSendDate).TotalMinutes <= 30)
            {
                string textMessage = ReceivedMessage.Text.Trim().ToLower();

                Helper.LogMessage(ReceivedMessage.From, null,
                    Helper.GetLocaDateTime(ReceivedMessage.Date), ReceivedMessage.Text);

                Helper.UpdateLastActivityDate(ReceivedMessage.From.Id);

                if (!String.IsNullOrWhiteSpace(textMessage) && textMessage.StartsWith("/"))
                {
                    Helper.HistoryComand.Remove(ReceivedMessage.From.Id);

                    if (textMessage.StartsWith("/start"))
                    {
                        StartCommand(ReceivedMessage, false);
                    }
                    else if (textMessage.StartsWith("/help"))
                    {
                        HelpCommand(ReceivedMessage, false);
                    }
                    else if (textMessage.StartsWith("/findcompanyforbin"))
                    {
                        FindCompanyForBINCommand(ReceivedMessage, false, textMessage);
                    }
                    else if (textMessage.StartsWith("/findcompanyforname"))
                    {
                        FindCompanyForNameCommand(ReceivedMessage, false, textMessage);
                    }
                    else if (textMessage.StartsWith("/findcompanyforall"))
                    {
                        FindCompanyForAllCommand(ReceivedMessage, false, textMessage);
                    }
                    else if (textMessage.StartsWith("/getexchangerate"))
                    {
                        GetExchangeRateCommand(ReceivedMessage, false, textMessage);
                    }
                    else if (textMessage.StartsWith("/feedback"))
                    {
                        FeedbackCommand(ReceivedMessage, false, textMessage);
                    }
                    else
                    {
                        CommandNotFound(ReceivedMessage, textMessage, "Message 1");
                    }
                }
                else if (!String.IsNullOrWhiteSpace(textMessage) && textMessage.Trim().ToLower() == "отмена")
                {
                    // Имитация набора текста
                    Helper.Bot.SendChatActionAsync(ReceivedMessage.Chat.Id, ChatAction.Typing);

                    Task<Message> messageTask =
                        Helper.Bot.SendTextMessageAsync(ReceivedMessage.Chat.Id,
                            "Все Ваши действия отменены.", replyMarkup: new ReplyKeyboardHide());

                    if (messageTask.Result != null)
                    {
                        Helper.HistoryComand.Remove(ReceivedMessage.From.Id);
                        Helper.LogMessage(null, ReceivedMessage.From, messageTask.Result.Date,
                            messageTask.Result.Text);
                    }
                }
                else if (Helper.HistoryComand.ContainsKey(ReceivedMessage.From.Id))
                {
                    #region Обработка с учетом истории

                    if (Helper.HistoryComand[ReceivedMessage.From.Id] == "/findcompanyforbin")
                    {
                        FindCompanyForBINCommand(ReceivedMessage, true, textMessage);
                    }
                    else if (Helper.HistoryComand[ReceivedMessage.From.Id] == "/findcompanyforname")
                    {
                        FindCompanyForNameCommand(ReceivedMessage, true, textMessage);
                    }
                    else if (Helper.HistoryComand[ReceivedMessage.From.Id] == "/findcompanyforall")
                    {
                        FindCompanyForAllCommand(ReceivedMessage, true, textMessage);
                    }
                    else if (Helper.HistoryComand[ReceivedMessage.From.Id] == "/getexchangerate")
                    {
                        GetExchangeRateCommand(ReceivedMessage, true, textMessage);
                    }
                    else if (Helper.HistoryComand[ReceivedMessage.From.Id] == "/feedback")
                    {
                        FeedbackCommand(ReceivedMessage, true, textMessage);
                    }
                    else
                    {
                        Helper.HistoryComand.Remove(ReceivedMessage.From.Id);
                    }

                    #endregion Обработка с учетом истории
                }
                else
                {
                    CommandNotFound(ReceivedMessage, textMessage, "Message 2");
                }
            }
            else
            {
                Helper.HistoryComand.Remove(ReceivedMessage.From.Id);
            }
        }

        #region Обработка команд

        /// <summary>
        /// start - Подружиться со мой
        /// </summary>
        /// <param name="ReceivedMessage"></param>
        /// <param name="IsHistory">Расматривается ли история</param>
        private static void StartCommand(Message ReceivedMessage, bool IsConsideredHistory)
        {
            // Имитация набора текста
            Helper.Bot.SendChatActionAsync(ReceivedMessage.Chat.Id, ChatAction.Typing);

            if (!IsConsideredHistory)
            {
                Task<Message> messageTask = Helper.Bot.SendTextMessageAsync(
                            ReceivedMessage.Chat.Id,
                            "Здравствуйте.\r\nВоспользуйтесь командой /help, чтобы узнать мои возможности.",
                            replyMarkup: new ReplyKeyboardHide());

                if (messageTask.Result != null)
                {
                    Helper.LogMessage(null, ReceivedMessage.From, messageTask.Result.Date,
                        messageTask.Result.Text);
                    Helper.RegisterUser(ReceivedMessage.From);
                }
            }
        }

        /// <summary>
        /// help - Узнать мои возможности
        /// </summary>
        /// <param name="ReceivedMessage"></param>
        /// <param name="IsConsideredHistory">Расматривается ли история</param>
        private static void HelpCommand(Message ReceivedMessage, bool IsConsideredHistory)
        {
            // Имитация набора текста
            Helper.Bot.SendChatActionAsync(ReceivedMessage.Chat.Id, ChatAction.Typing);

            if (!IsConsideredHistory)
            {
                Task<Message> messageTask = Helper.Bot.SendTextMessageAsync(
                            ReceivedMessage.Chat.Id,
                            "<b>Вот список моих возможностей:</b>\r\n\r\n" +
                            "/findcompanyforbin - Поиск компании по БИН\r\n" +
                            "/findcompanyforname - Поиск компании по наименованию\r\n" +
                            "/findcompanyforall - Поиск компании по всей информации\r\n" +
                            "/getexchangerate - Получить обменный курс\r\n" +
                            "/feedback - Оставить отзыв"
                            , replyMarkup: new ReplyKeyboardHide(), parseMode: ParseMode.Html,
                            disableWebPagePreview: true);

                if (messageTask != null)
                {
                    Helper.LogMessage(null, ReceivedMessage.From, messageTask.Result.Date,
                        messageTask.Result.Text);
                }
            }
        }

        /// <summary>
        /// feedback - Оставить отзыв
        /// </summary>
        /// <param name="ReceivedMessage"></param>
        /// <param name="IsConsideredHistory"></param>
        /// <param name="textMessage"></param>
        private static void FeedbackCommand(Message ReceivedMessage, bool IsConsideredHistory, string textMessage)
        {
            // Имитация набора текста
            Helper.Bot.SendChatActionAsync(ReceivedMessage.Chat.Id, ChatAction.Typing);

            if (!IsConsideredHistory)
            {
                Task<Message> messageTask = Helper.Bot.SendTextMessageAsync(
                            ReceivedMessage.Chat.Id,
                            "Поделитесь впечатлениями о моей работе или предложите идею, и я перешлю ваше сообщение моим разработчикам.",
                            replyMarkup: new ReplyKeyboardHide());

                if (messageTask.Result != null)
                {
                    Helper.LogMessage(null, ReceivedMessage.From, messageTask.Result.Date,
                        messageTask.Result.Text);
                    Helper.HistoryComand.Add(ReceivedMessage.From.Id, "/feedback");
                }
            }
            else
            {
                Helper.HistoryComand.Remove(ReceivedMessage.From.Id);

                Task<Message> messageTask = Helper.Bot.SendTextMessageAsync(
                    ReceivedMessage.Chat.Id,
                    "Спасибо за отклик!",
                    replyMarkup: new ReplyKeyboardHide(), disableWebPagePreview: true);

                if (messageTask.Result != null)
                {
                    Helper.LogMessage(null, ReceivedMessage.From, messageTask.Result.Date,
                        messageTask.Result.Text);
                    LogService.SendInfo(textMessage,
                        !String.IsNullOrWhiteSpace(ReceivedMessage.From.Username)
                            ? ReceivedMessage.From.Username
                            : ReceivedMessage.From.Id.ToString(), "Feedback");
                }
            }
        }

        /// <summary>
        /// Сообщение, если команда не известна
        /// </summary>
        /// <param name="ReceivedMessage"></param>
        /// <param name="textMessage"></param>
        /// <param name="Comment"></param>
        private static void CommandNotFound(Message ReceivedMessage, string textMessage, string Comment)
        {
            // Имитация набора текста
            Helper.Bot.SendChatActionAsync(ReceivedMessage.Chat.Id, ChatAction.Typing);

            Task<Message> messageTask = Helper.Bot.SendTextMessageAsync(ReceivedMessage.Chat.Id,
                "Извините, но я не знаю такую команду.\r\nВоспользуйтесь командой /help, чтобы узнать мои возможности.",
                replyMarkup: new ReplyKeyboardHide());

            if (messageTask.Result != null)
            {
                Helper.LogMessage(null, ReceivedMessage.From, messageTask.Result.Date,
                    messageTask.Result.Text);

                LogService.SendInfo("Message: |" + textMessage + "|",
                    Helper.GetUserName(ReceivedMessage.From), Comment);
            }
        }

        /// <summary>
        /// findcompanyforbin - Поиск компании по БИН
        /// </summary>
        /// <param name="ReceivedMessage"></param>
        /// <param name="IsConsideredHistory"></param>
        /// <param name="textMessage"></param>
        private static void FindCompanyForBINCommand(Message ReceivedMessage, bool IsConsideredHistory, string textMessage)
        {
            // Имитация набора текста
            Helper.Bot.SendChatActionAsync(ReceivedMessage.Chat.Id, ChatAction.Typing);

            if (!IsConsideredHistory)
            {
                if (textMessage == "/findcompanyforbin")
                {
                    Task<Message> messageTask =
                        Helper.Bot.SendTextMessageAsync(ReceivedMessage.Chat.Id,
                            "Пожалуйста, напишите БИН компании, по которому будет происходить поиск.",
                            replyMarkup: new ReplyKeyboardHide());

                    if (messageTask.Result != null)
                    {
                        Helper.LogMessage(null, ReceivedMessage.From, messageTask.Result.Date,
                            messageTask.Result.Text);
                        Helper.HistoryComand.Add(ReceivedMessage.From.Id, "/findcompanyforbin");
                    }
                }
                else
                {
                    string BIN = textMessage.Replace("/findcompanyforbin", "").Trim();

                    Task<Message> messageTask =
                        Helper.Bot.SendTextMessageAsync(ReceivedMessage.Chat.Id,
                            "Пожалуйста, подождите. Это может занять некоторое время.",
                            replyMarkup: new ReplyKeyboardHide());

                    if (messageTask.Result != null)
                    {
                        Helper.LogMessage(null, ReceivedMessage.From, messageTask.Result.Date,
                            messageTask.Result.Text);
                        FindCompany.FindForBINStr(ReceivedMessage, BIN);
                    }
                }
            }
            else
            {
                Helper.HistoryComand.Remove(ReceivedMessage.From.Id);
                string BIN = textMessage.Replace("/findcompanyforbin", "").Trim();

                Task<Message> messageTask = Helper.Bot.SendTextMessageAsync(ReceivedMessage.Chat.Id,
                    "Пожалуйста, подождите. Это может занять некоторое время.",
                    replyMarkup: new ReplyKeyboardHide());

                if (messageTask.Result != null)
                {
                    Helper.LogMessage(null, ReceivedMessage.From, messageTask.Result.Date,
                        messageTask.Result.Text);
                    FindCompany.FindForBINStr(ReceivedMessage, BIN);
                }
            }
        }

        /// <summary>
        /// findcompanyforname - Поиск компании по наименованию
        /// </summary>
        /// <param name="ReceivedMessage"></param>
        /// <param name="IsConsideredHistory"></param>
        /// <param name="textMessage"></param>
        private static void FindCompanyForNameCommand(Message ReceivedMessage, bool IsConsideredHistory, string textMessage)
        {
            // Имитация набора текста
            Helper.Bot.SendChatActionAsync(ReceivedMessage.Chat.Id, ChatAction.Typing);

            if (!IsConsideredHistory)
            {
                Task<Message> messageTask =
                                Helper.Bot.SendTextMessageAsync(ReceivedMessage.Chat.Id,
                                    "Пожалуйста, напишите наименование компании, которую Вы хотите найти.",
                                    replyMarkup: new ReplyKeyboardHide());

                if (messageTask.Result != null)
                {
                    Helper.LogMessage(null, ReceivedMessage.From, messageTask.Result.Date,
                        messageTask.Result.Text);
                    Helper.HistoryComand.Add(ReceivedMessage.From.Id, "/findcompanyforname");
                }
            }
            else
            {
                Helper.HistoryComand.Remove(ReceivedMessage.From.Id);
                string Name = textMessage.Replace("/findcompanyforname", "").Trim();

                Task<Message> messageTask = Helper.Bot.SendTextMessageAsync(ReceivedMessage.Chat.Id,
                    "Пожалуйста, подождите. Это может занять некоторое время.",
                    replyMarkup: new ReplyKeyboardHide());

                if (messageTask.Result != null)
                {
                    Helper.LogMessage(null, ReceivedMessage.From, messageTask.Result.Date,
                        messageTask.Result.Text);
                    FindCompany.FindForNameStr(ReceivedMessage, Name);
                }
            }
        }

        /// <summary>
        /// findcompanyforall - Поиск компании по всей информации
        /// </summary>
        /// <param name="ReceivedMessage"></param>
        /// <param name="IsConsideredHistory"></param>
        /// <param name="textMessage"></param>
        private static void FindCompanyForAllCommand(Message ReceivedMessage, bool IsConsideredHistory, string textMessage)
        {
            // Имитация набора текста
            Helper.Bot.SendChatActionAsync(ReceivedMessage.Chat.Id, ChatAction.Typing);

            if (!IsConsideredHistory)
            {
                Task<Message> messageTask =
                                Helper.Bot.SendTextMessageAsync(ReceivedMessage.Chat.Id,
                                    "Пожалуйста, напишите любую информацию, которую Вы знаете о компании.",
                                    replyMarkup: new ReplyKeyboardHide());

                if (messageTask.Result != null)
                {
                    Helper.LogMessage(null, ReceivedMessage.From, messageTask.Result.Date,
                        messageTask.Result.Text);
                    Helper.HistoryComand.Add(ReceivedMessage.From.Id, "/findcompanyforall");
                }
            }
            else
            {
                Helper.HistoryComand.Remove(ReceivedMessage.From.Id);
                string Name = textMessage.Replace("/findcompanyforall", "").Trim();

                Task<Message> messageTask = Helper.Bot.SendTextMessageAsync(ReceivedMessage.Chat.Id,
                    "Пожалуйста, подождите. Это может занять некоторое время.",
                    replyMarkup: new ReplyKeyboardHide());

                if (messageTask.Result != null)
                {
                    Helper.LogMessage(null, ReceivedMessage.From, messageTask.Result.Date,
                        messageTask.Result.Text);
                    FindCompany.FindForNameStr(ReceivedMessage, Name);
                }
            }
        }

        /// <summary>
        /// getexchangerate - Получить обменный курс
        /// </summary>
        /// <param name="ReceivedMessage"></param>
        /// <param name="IsConsideredHistory"></param>
        /// <param name="textMessage"></param>
        private static void GetExchangeRateCommand(Message ReceivedMessage, bool IsConsideredHistory, string textMessage)
        {
            // Имитация набора текста
            Helper.Bot.SendChatActionAsync(ReceivedMessage.Chat.Id, ChatAction.Typing);

            if (!IsConsideredHistory)
            {
                Task<Message> messageTask =
                                Helper.Bot.SendTextMessageAsync(ReceivedMessage.Chat.Id,
                                    "Пожалуйста, выберите валюту.",
                                    replyMarkup: GetExchangeRate.GetKeyboardMarkup());

                if (messageTask.Result != null)
                {
                    Helper.LogMessage(null, ReceivedMessage.From, messageTask.Result.Date,
                        messageTask.Result.Text);
                    Helper.HistoryComand.Add(ReceivedMessage.From.Id, "/getexchangerate");
                }
            }
            else
            {
                Helper.HistoryComand.Remove(ReceivedMessage.From.Id);
                string Kod = textMessage.Replace("/getexchangerate", "").Trim();

                Task<Message> messageTask = Helper.Bot.SendTextMessageAsync(ReceivedMessage.Chat.Id,
                    "Пожалуйста, подождите. Это может занять некоторое время.",
                    replyMarkup: new ReplyKeyboardHide());

                if (messageTask.Result != null)
                {
                    Helper.LogMessage(null, ReceivedMessage.From, messageTask.Result.Date,
                        messageTask.Result.Text);
                    GetExchangeRate.GetForKodStr(ReceivedMessage, Kod);
                }
            }
        }

        #endregion Обработка команд

        public static async Task StartBotAsync()
        {
            Helper.Bot = new Telegram.Bot.TelegramBotClient(Helper.Token);

            var me = Helper.Bot.GetMeAsync().Result;
            Console.Title = me.Username;
            Helper.BotName = me.Username;

            int offSet = 0;

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }

                try
                {
                    var updates = await Helper.Bot.GetUpdatesAsync(offSet);

                    #region Создаем очередь сообщений с разделением по пользователям

                    foreach (var update in updates)
                    {
                        int FromUserId = Helper.GetFromUserId(update);

                        if (FromUserId > 0)
                        {
                            if (!Helper.MessageQueuing.ContainsKey(FromUserId))
                            {
                                Helper.MessageQueuing.Add(FromUserId, new List<Update>());
                            }

                            Helper.MessageQueuing[FromUserId].Add(update);
                        }

                        offSet = update.Id + 1;
                    }

                    #endregion Создаем очередь сообщений с разделением по пользователям 

                    #region Обрабатываем сформированную очередь

                    foreach (var item in Helper.MessageQueuing)
                    {
                        var task = Task.Factory.StartNew(() =>
                        {
                            foreach (var update in item.Value)
                            {
                                try
                                {
                                    UpdateReceived(update);
                                }
                                catch (Exception error)
                                {
                                    if (error.Source != "System.Net.Http.Formatting")
                                    {
                                        LogService.SendException(error, Comment: "UpdateReceived - Exception");
                                    }
                                    else
                                    {
                                        Console.WriteLine(String.Format("{0:dd.MM.yyyy HH:mm:ss}: {1}\r\n",
                                            Helper.GetLocaDateTime(DateTime.Now), error.Message));
                                    }
                                }
                            }
                        });
                    }

                    Helper.MessageQueuing.Clear();

                    #endregion

                    await Task.Delay(500);
                }
                catch (HttpRequestException)
                {
                    Console.WriteLine(
                        String.Format(
                            "{0:dd.MM.yyyy HH:mm:ss}: Потеряно соединение. Следующее подключение будет через 30 секунд.\r\n",
                            Helper.GetLocaDateTime(DateTime.Now)));
                    await Task.Delay(30000);
                }
                catch (TaskCanceledException)
                {
                    await Task.Delay(2000);
                }
                catch (Exception error)
                {
                    if (error.Source != "System.Net.Http.Formatting")
                    {
                        LogService.SendException(error, Comment: "StartBotAsync - Exception");
                    }
                    else
                    {
                        Console.WriteLine(String.Format("{0:dd.MM.yyyy HH:mm:ss}: {1}\r\n",
                            Helper.GetLocaDateTime(DateTime.Now), error.Message));
                    }

                    await Task.Delay(15000);
                }
            }
        }
    }
}
