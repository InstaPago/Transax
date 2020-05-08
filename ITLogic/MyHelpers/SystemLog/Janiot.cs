using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHelpers.SystemLog
{
    public class Janiot
    {
#if DEBUG
        const string strLogName = "Janiot-Play";

#else
        const string strLogName = "Janiot InstaPago";
#endif
        const string strSource = "Janiot";
        public static bool CreateLog(string strLogName)
        {
            var Result = false;

            try
            {
                EventLog.CreateEventSource(strSource, strLogName);
                var SQLEventLog = new EventLog();
                SQLEventLog.Source = strLogName;
                SQLEventLog.Log = strLogName;

                SQLEventLog.Source = strLogName;
                SQLEventLog.WriteEntry("El log para " + strLogName + " ha sido creado.", EventLogEntryType.Information);

                Result = true;
            }
            catch
            {
                Result = false;
            }

            return Result;
        }
        public static void Write(string strErrDetail, EventLogEntryType type)
        {
            var SQLEventLog = new EventLog();

            try
            {
                if (!EventLog.SourceExists(strLogName))
                    CreateLog(strLogName);


                SQLEventLog.Log = strLogName;
                SQLEventLog.Source = strLogName;
                SQLEventLog.WriteEntry(strErrDetail, type);

            }
            catch (Exception ex)
            {
                SQLEventLog.Source = strLogName;
                SQLEventLog.WriteEntry(Convert.ToString("WARNING: ") +
                                        Convert.ToString(ex.Message),
                                        EventLogEntryType.Error);
            }
            finally
            {
                SQLEventLog.Dispose();
                SQLEventLog = null;
            }
        }
        public static void MailErrors(string __message, string __subject)
        {
            //            try
            //            {
            //                SystemLog.Write("Enviando correo de alerta: " + __message, EventLogEntryType.Information);
            //#if DEBUG
            //                var mailmessage = new MailMessage(new MailAddress("playground@instapago.com", "PLAYGROUND Janiot"), new MailAddress("notificacion@instapago.com", "PLAYGROUND Alerta Janiot"));

            //#else
            //                var mailmessage = new MailMessage(new MailAddress("Janiot@instapago.com", "Janiot"), new MailAddress("notificacion@instapago.com", "Alerta Janiot"));
            //#endif

            //                mailmessage.BodyEncoding = Encoding.Default;
            //                mailmessage.Subject = __subject;
            //                mailmessage.Body = __message;
            //                mailmessage.IsBodyHtml = true;

            //                SmtpClient smtpMail = new SmtpClient(Properties.Settings.Default.SMTP, 25);
            //                smtpMail.Timeout = 60000;

            //                smtpMail.Send(mailmessage);
            //            }
            //            catch (Exception ex)
            //            {
            //                SystemLog.Write("El servicio falló al momento de mandar un correo: " + ex.Message + "\n\n" + ex.StackTrace, EventLogEntryType.Error);
            //            }

        }
    }
}

