using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenDataKzBot.Models;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace OpenDataKzBot.Controllers
{

    /// <summary>
    /// Получить обменный курс
    /// </summary>
    public class GetExchangeRate
    {
        private static List<Currency> Get(string query)
        {
            return JsonConvert.DeserializeObject<List<Currency>>(Helper.GetJSON("https://data.egov.kz/api/v2/valutalar_bagamdary3/v1", query));
        }

        #region Получение курса по коду

        public static Currency GetForKod(string kod)
        {
            List<Currency> resultList = Get("{\"size\":1, \"query\": {\"bool\":{\"must\":[{\"match\":{\"kod\": \"" + kod + "\"}}]}}}");

            if (resultList != null && resultList.Count > 0)
            {
                Currency tempCurrency = resultList[0];
                tempCurrency.id = "1";
                return tempCurrency;
            }

            return null;
        }

        public static async void GetForKodStr(Message ReceivedMessage, string kod)
        {
            try
            {
                string result = "Извините. Сервис временно недоступен. Повторите попытку позднее.";
                kod = kod.Replace(" ", "");
                kod = kod.Substring(kod.Length - 4, 3);

                Currency foundCurrency = GetForKod(kod);

                if (foundCurrency != null)
                {
                    result = GetStringCurrency(foundCurrency);
                }

                result = Helper.CheckingLengthMessage(result);

                Message message =
                                await
                                    Helper.Bot.SendTextMessageAsync(ReceivedMessage.Chat.Id,
                                        result,
                                        replyMarkup: new ReplyKeyboardHide(), parseMode: ParseMode.Html);

                if (message != null)
                {
                    Helper.LogMessage(null, ReceivedMessage.From, message.Date, message.Text);
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(String.Format("{0:dd.MM.yyyy HH:mm:ss}: {1}\r\n",
                    Helper.GetLocaDateTime(DateTime.Now), error.Message));
                LogService.SendException(error, Comment: "GetForKodStr");

                Message message =
                    await
                        Helper.Bot.SendTextMessageAsync(ReceivedMessage.Chat.Id,
                            "Извините. Сервис временно недоступен. Повторите попытку позднее.",
                            replyMarkup: new ReplyKeyboardHide(), parseMode: ParseMode.Html);

                if (message != null)
                {
                    Helper.LogMessage(null, ReceivedMessage.From, message.Date, message.Text);
                }
            }
        }

        #endregion Получение курса по коду

        private static string GetStringCurrency(Currency currency)
        {
            return String.Format(
                "<b>" + currency.id + ".Курс валюты</b>\r\n" +
                "<b>Код:\r\n</b>{0}\r\n" +
                "<b>Наименование (ru):\r\n</b>{1}\r\n" +
                "<b>Наименование (kz):\r\n</b>{2}\r\n" +
                "<b>Курс:\r\n</b>{3}\r\n",
                String.IsNullOrWhiteSpace(currency.kod) ? "---" : currency.kod,
                String.IsNullOrWhiteSpace(currency.name_rus) ? "---" : currency.name_rus,
                String.IsNullOrWhiteSpace(currency.name_kaz) ? "---" : currency.name_kaz,
                String.Format("за {0} ЕД. - {1} {2}", currency.sootnowenie, currency.kurs, currency.edinica_izmerenia));
        }

        public static ReplyKeyboardMarkup GetKeyboardMarkup()
        {
            KeyboardButton[][] tempKeyboardButtons = new KeyboardButton[][]
            {
                new KeyboardButton[] {new KeyboardButton("ДОЛЛАР США (USD)")},
                new KeyboardButton[] {new KeyboardButton("ЕВРО (EUR)")},
                new KeyboardButton[] {new KeyboardButton("РОССИЙСКИЙ РУБЛЬ (RUB)")},
                new KeyboardButton[] {new KeyboardButton("Отмена")}
            };

            List<Currency> Currences = Get("{\"size\":50}");

            if (Currences.Count > 0)
            {
                Currences = Currences.OrderBy(n => n.name_rus).ToList();

                tempKeyboardButtons = new KeyboardButton[Currences.Count + 1][];

                for (int i = 0; i < Currences.Count; i++)
                {
                    tempKeyboardButtons[i] = new KeyboardButton[] {new KeyboardButton(String.Format("{0} ({1})", Currences[i].name_rus, Currences[i].kod))};
                }

                tempKeyboardButtons[Currences.Count] = new KeyboardButton[] { new KeyboardButton("Отмена") };
            }

            ReplyKeyboardMarkup replyKeyboardMarkupCurrences = new ReplyKeyboardMarkup()
            {
                OneTimeKeyboard = true,
                Keyboard = tempKeyboardButtons
            };

            return replyKeyboardMarkupCurrences;
        }
    }
}
