using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace OpenDataKzBot
{
    public class LogService
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static void SendException(Exception exception, string User = "Application", string Comment = "")
        {
            if (exception != null)
            {
                logger.Error(String.Format("OpenDataKz | Source: {0}\r\n\r\nMessage: {1}\r\n\r\nUser: {2}\r\n\r\nStackTrace: {3}\r\n\r\nComment: {4}", exception.Source, exception.Message, User, exception.StackTrace, Comment));
            }
        }

        public static void SendInfo(string Info, string User, string Comment = "")
        {
            logger.Info(String.Format("OpenDataKz | Info: {0}\r\n\r\nUser: {1}\r\n\r\nComment: {2}", Info, User, Comment));
        }
    }
}
