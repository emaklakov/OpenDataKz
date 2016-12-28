using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenDataKzBot.Models;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace OpenDataKzBot.Controllers
{
    public class FindCompany
    {
        private static List<Company> Find(string query)
        {
            return JsonConvert.DeserializeObject<List<Company>>(Helper.GetJSON("https://data.egov.kz/api/v2/kazakstanyn____zandy_tulgalard/v1", query));
        }

        #region Поиск компании по БИН

        public static Company FindForBIN(string bin)
        {
            List<Company> resultList = Find("{\"size\":1, \"query\": {\"bool\":{\"must\":[{\"match\":{\"bin\": \"" + bin + "\"}}]}}}");

            if (resultList != null && resultList.Count > 0)
            {
                Company tempCompany = resultList[0];
                tempCompany.id = "1";
                return tempCompany;
            }

            return null;
        }

        public static async void FindForBINStr(Message ReceivedMessage, string bin)
        {
            try
            {
                string result = "Вы не верно указали БИН компании.";
                bin = bin.Replace(" ", "");

                if (CheckBIN(bin))
                {
                    result = "Извините, но по Вашему запросу компания не найдена.";
                    Company foundCompany = FindForBIN(bin);

                    if (foundCompany != null)
                    {
                        result = GetStringCompany(foundCompany);
                    }
                }
                else
                {
                    LogService.SendInfo("Message: БИН -|" + bin + "|", Helper.GetUserName(ReceivedMessage.From), "CheckBIN - Wrong");
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
                LogService.SendException(error, Comment: "FindForBINStr");

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

        #endregion  Поиск компании по БИН

        #region Поиск компании по наименованию

        public static List<Company> FindForName(string name)
        {
            name = CleaningRequest(name);

            List <Company> resultList = Find("{\"size\":5, \"query\": {\"fuzzy_like_this\" : {\"fields\" : [\"namerus\"], \"like_text\" : \"" + name + "\" }}}");

            if (resultList != null && resultList.Count > 0)
            {
                return resultList;
            }

            return null;
        }

        public static async void FindForNameStr(Message ReceivedMessage, string name)
        {
            try
            {
                string result = "Извините, но по Вашему запросу компания не найдена.";
                List<Company> foundCompanis = FindForName(name);

                if (foundCompanis != null && foundCompanis.Count > 0)
                {
                    result = "Вот что я нашел по вашему запросу:\r\n"; 
                    int count = 0;
                    foreach (Company foundCompany in foundCompanis)
                    {
                        if (count > 0)
                        {
                            result += "\r\n\r\n<b>---------------</b>\r\n\r\n";
                        }

                        foundCompany.id = (count + 1).ToString();

                        result += GetStringCompany(foundCompany);
                        count++;
                    } 
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
                LogService.SendException(error, Comment: "FindForNameStr");

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

        #endregion Поиск компании по наименованию

        #region Поиск компании по всем полям

        public static List<Company> FindForAll(string text)
        {
            text = CleaningRequest(text);

            List <Company> resultList = Find("{\"size\":5, \"query\": {\"fuzzy_like_this\" : {\"fields\" : [\"_all\"], \"like_text\" : \"" + text + "\" }}}");

            if (resultList != null && resultList.Count > 0)
            {
                return resultList;
            }

            return null;
        }

        public static async void FindForAllStr(Update Update, string text)
        {
            Message ReceivedMessage = Update.Message != null ? Update.Message : Update.EditedMessage;

            try
            {
                string result = "Извините, но по Вашему запросу компания не найдена.";
                List<Company> foundCompanis = FindForAll(text);

                if (foundCompanis != null && foundCompanis.Count > 0)
                {
                    result = "Вот что я нашел по вашему запросу:\r\n";
                    int count = 0;
                    foreach (Company foundCompany in foundCompanis)
                    {
                        if (count > 0)
                        {
                            result += "\r\n\r\n<b>---------------</b>\r\n\r\n";
                        }

                        foundCompany.id = (count + 1).ToString();

                        result += GetStringCompany(foundCompany);
                        count++;
                    }
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
                LogService.SendException(error, Comment: "FindForAllStr");

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

        #endregion Поиск компании по всем полям

        #region Дополнительные функции

        private static bool CheckBIN(string bin)
        {
            bool result = false;

            if (!String.IsNullOrWhiteSpace(bin) && bin.Length == 12)
            {
                decimal tempBin = 0;
                if (Decimal.TryParse(bin, out tempBin))
                {
                    int years = int.Parse(bin.Substring(0, 2));
                    if ((years > 90 && years < 99) || (2000 + years) <= DateTime.Now.Year)
                    {
                        int months = int.Parse(bin.Substring(2, 2));
                        if (months > 0 && months <= 12)
                        {
                            int typeUl = int.Parse(bin.Substring(4, 1));
                            if (typeUl > 3)
                            {
                                int typeUlDopInfo = int.Parse(bin.Substring(5, 1));
                                if (typeUlDopInfo <= 4)
                                {
                                    //Проверяем контрольный разряд
                                    var b1 = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
                                    var b2 = new[] { 3, 4, 5, 6, 7, 8, 9, 10, 11, 1, 2 };
                                    var a = new int[12];
                                    var controll = 0;
                                    for (var i = 0; i < 12; i++)
                                    {
                                        a[i] = int.Parse(bin.Substring(i, 1));
                                        if (i < 11)
                                            controll += a[i] * b1[i];
                                    }
                                    controll = controll % 11;
                                    if (controll == 10)
                                    {
                                        controll = 0;
                                        for (var i = 0; i < 11; i++)
                                            controll += a[i] * b2[i];
                                        controll = controll % 11;
                                    }
                                    if (controll == a[11])
                                    {
                                        result = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        private static string CleaningRequest(string request)
        {
            request = request.ToLower();
            request = request.Replace("\"", "");
            request = request.Replace("тоо", "товарищество с ограниченной ответственностью");

            return request;
        }

        private static string GetStringCompany(Company company)
        {
            return String.Format(
                "<b>" + company.id + ".Информация о компании</b>\r\n" +
                "<b>БИН:\r\n</b>{0}\r\n" +
                "<b>Наименование (ru):\r\n</b>{1}\r\n" +
                "<b>Наименование (kz):\r\n</b>{2}\r\n" +
                "<b>ФИО руководителя:\r\n</b>{3}\r\n" +
                "<b>Местонахождения:\r\n</b>{4}\r\n" +
                "<b>Вид деятельности (ru):\r\n</b>{5}\r\n" +
                "<b>Вид деятельности (kz):\r\n</b>{6}\r\n" +
                "<b>Дата регистрации:\r\n</b>{7}",
                String.IsNullOrWhiteSpace(company.bin) ? "---" : company.bin,
                String.IsNullOrWhiteSpace(company.namerus) ? "---" : company.namerus,
                String.IsNullOrWhiteSpace(company.namekaz) ? "---" : company.namekaz,
                String.IsNullOrWhiteSpace(company.manager) ? "---" : company.manager,
                String.IsNullOrWhiteSpace(company.location) ? "---" : company.location,
                String.IsNullOrWhiteSpace(company.activrus) ? "---" : company.activrus,
                String.IsNullOrWhiteSpace(company.activkaz) ? "---" : company.activkaz,
                String.IsNullOrWhiteSpace(company.regdate) ? "--.--.----" : DateTime.Parse(company.regdate).ToString("dd.MM.yyyy"));
        }

        #endregion  Дополнительные функции
    }
}
