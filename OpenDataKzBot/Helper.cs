using System;
using System.Collections.Generic;
using System.Globalization;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace OpenDataKzBot
{
    public class Helper
    {
        #region Переменные

        public static Telegram.Bot.TelegramBotClient Bot = null;
        public static string BotName = "OpenDataKzBot";
        private static string connectionString = "";
        public static string Token = "";

        #endregion Переменные

        public static DateTime GetLocaDateTime(DateTime MessageDate)
        {
            return MessageDate.ToLocalTime();
        }

        public static decimal DecimalParse(string DecimalVal)
        {
            DecimalVal = !String.IsNullOrWhiteSpace(DecimalVal)
                ? DecimalVal.Trim().Replace(',', '.').Replace(" ", "")
                : "0";

            decimal DecimalResult = 0m;
            Decimal.TryParse(DecimalVal, NumberStyles.Number, CultureInfo.InvariantCulture, out DecimalResult);

            return DecimalResult;
        }

        public static string CheckingLengthMessage(string message)
        {
            if (message.Length > 4096)
            {
                message = message.Remove(4090) + "...";
            }

            return message;
        }

        public static string GetUserName(User user)
        {
            return String.Format("{0} {1} ({2} {3})", user.FirstName, user.LastName, user.Username, user.Id);
        }

        public static string GetJSON(string url, string query)
        {
            string json = "";

            string urlFull = String.Format("{0}?source={1}", url, query);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            CookieContainer cook = new CookieContainer();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlFull);
            request.ProtocolVersion = HttpVersion.Version10;
            request.CookieContainer = cook;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responsestream = response.GetResponseStream();

            using (StreamReader ResponseStreamReader = new StreamReader(responsestream))
            {
                json = ResponseStreamReader.ReadToEnd();
            }

            //string json = Encoding.UTF8.GetString(response);

            return json;
        }

        public static int GetFromUserId(Update update)
        {
            switch (update.Type)
            {
                case UpdateType.MessageUpdate:
                    if (update.Message.From != null)
                    {
                        return update.Message.From.Id;
                    }
                    break;

                case UpdateType.InlineQueryUpdate:
                    break;

                case UpdateType.ChosenInlineResultUpdate:
                    break;

                case UpdateType.CallbackQueryUpdate:
                    if (update.CallbackQuery.From != null)
                    {
                        return update.CallbackQuery.From.Id;
                    }
                    break;

                case UpdateType.EditedMessage:
                    if (update.EditedMessage.From != null)
                    {
                        return update.EditedMessage.From.Id;
                    }
                    break;
            }

            return 0;
        }

        #region Логирование

        public static async void LogMessage(User FromUserName, User ToUserName, DateTime MessageDate, string Text)
        {
            await Task.Run(() => {
                if (FromUserName == null)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }

                Console.WriteLine(String.Format("[{2:dd.MM.yyyy HH:mm:ss:ff}] {0} > {1}:\r\n|{3}|\r\n", FromUserName != null ? GetUserName(FromUserName) : BotName, ToUserName != null ? GetUserName(ToUserName) : BotName, GetLocaDateTime(MessageDate), Text));
                Console.ResetColor();
            });
        }

        #endregion Логирование

        public static bool LoadSettings()
        {
            bool result = false;

            try
            {
                connectionString = ConfigurationManager.ConnectionStrings["OpenDataKzBotConnection"].ConnectionString;

#if DEBUG
                Token = ConfigurationManager.AppSettings.Get("TokenDebug");
                //Token = ConfigurationManager.AppSettings.Get("TokenRelease");
#else
                Token = ConfigurationManager.AppSettings.Get("TokenRelease");
#endif

                result = true;
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(String.Format("{0:dd.MM.yyyy HH:mm:ss}: {1}\r\n", Helper.GetLocaDateTime(DateTime.Now), error.Message));
                Console.ResetColor();
            }

            return result;
        }

        public static Dictionary<int, List<Update>> MessageQueuing = new Dictionary<int, List<Update>>();

        public static Dictionary<int, string> HistoryComand = new Dictionary<int, string>();

        #region Работа с БД

        /// <summary>
        /// Добавление в БД нового зарегестрированного пользователя
        /// </summary>
        /// <param name="user"></param>
        public static async void RegisterUser(User user)
        {
            await Task.Run(() => {
                try
                {
                    using (SqlConnection dbConnection = new SqlConnection(connectionString))
                    {
                        dbConnection.Open();

                        using (
                            SqlCommand cmdCheckSubscribe =
                                new SqlCommand(
                                    "USE WebServices SELECT Id FROM OpenDataKzBot_RegisteredUsers WHERE Id=@Id",
                                    dbConnection))
                        {
                            cmdCheckSubscribe.Parameters.AddWithValue("@Id", user.Id);

                            using (SqlDataReader reader = cmdCheckSubscribe.ExecuteReader())
                            {
                                bool IsExist = false;

                                while (reader.Read())
                                {
                                    IsExist = true;
                                }

                                reader.Close();

                                if (!IsExist)
                                {
                                    using (
                                        SqlCommand cmd =
                                            new SqlCommand(
                                                "USE WebServices INSERT INTO OpenDataKzBot_RegisteredUsers(Id, Username, FirstName, LastName) VALUES (@Id, @Username, @FirstName, @LastName)",
                                                dbConnection))
                                    {
                                        cmd.Parameters.AddWithValue("@Id", user.Id);
                                        cmd.Parameters.AddWithValue("@Username", ((object)user.Username) ?? DBNull.Value);
                                        cmd.Parameters.AddWithValue("@FirstName", ((object)user.FirstName) ?? DBNull.Value);
                                        cmd.Parameters.AddWithValue("@LastName", ((object)user.LastName) ?? DBNull.Value);

                                        int rows = cmd.ExecuteNonQuery();

                                        if (rows > 0)
                                        {
                                            LogService.SendInfo(String.Format("Зарегистрировался новый пользователь: Id - {0}.", user.Id), user.Username);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception error)
                {
                    LogService.SendException(error, GetUserName(user), "RegisterUser");
                }
            });
        }

        /// <summary>
        /// Устанавливаем, либо обновляем дату последней активности пользователя
        /// </summary>
        /// <param name="UserId"></param>
        public static async void UpdateLastActivityDate(int UserId)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (SqlConnection dbConnection = new SqlConnection(connectionString))
                    {
                        dbConnection.Open();

                        using (
                                        SqlCommand cmd =
                                            new SqlCommand(
                                                "USE WebServices UPDATE OpenDataKzBot_RegisteredUsers SET LastActivityDate=@LastActivityDate WHERE Id=@Id",
                                                dbConnection))
                        {
                            cmd.Parameters.AddWithValue("@Id", UserId);
                            cmd.Parameters.AddWithValue("@LastActivityDate", DateTime.Now);

                            int rows = cmd.ExecuteNonQuery();

                            if (rows > 0)
                            {
                                //Console.WriteLine("UpdateLastActivityDate");
                            }
                        }
                    }
                }
                catch (Exception error)
                {
                    LogService.SendException(error, String.Format("UserId: {0}", UserId), "UpdateLastActivityDate");
                }
            });
        }

        #endregion Работа с БД
    }
}
