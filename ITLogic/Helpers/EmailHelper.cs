using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace InstaTransfer.ITLogic.Helpers
{   /// <summary>
    /// Summary description for Email
    /// </summary>
    public class EmailHelper
    {
        #region Atributos

        private String _pais;
        public String Pais
        {
            get { return _pais; }
            set { _pais = value; }
        }

        private String _cuerpo;

        /// <summary>
        /// Cuerpo del correo
        /// </summary>
        public String Cuerpo
        {
            get { return _cuerpo; }
            set { _cuerpo = value; }
        }

        private List<String> _to = new List<string>();

        /// <summary>
        /// Lista de Destinatarios del correo
        /// </summary>
        public List<String> To
        {
            get { return _to; }
            set { _to = value; }
        }

        private String _from;

        // Emisor del correo
        public String From
        {
            get { return _from; }
            set { _from = value; }
        }

        private String _subject;

        // Emisor del correo
        public String Subject
        {
            get { return _subject; }
            set { _subject = value; }
        }

        private List<String> _attachments = new List<string>();

        public List<String> Attachments
        {
            get { return _attachments; }
            set { _attachments = value; }
        }

        #endregion Atributos

        #region Send

        public void SendEmailMessage(String From, String DisplayName, List<String> To, String Subject, String Message)
        {

            MailMessage mailmessage;
            try
            {
                foreach (String to in To)
                {
                    mailmessage = new MailMessage(new MailAddress(From.Trim()), new MailAddress(to.Trim()));
                    mailmessage.BodyEncoding = Encoding.Default;
                    mailmessage.Subject = Subject.Trim();
                    mailmessage.Body = Message.Trim();
                    mailmessage.Priority = MailPriority.High;
                    mailmessage.IsBodyHtml = true;
                    mailmessage.From = new MailAddress(From, DisplayName);
                    SmtpClient smtpMail = new SmtpClient();

                    smtpMail.Send(mailmessage);

                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SendEmailMessageWithAttach(String From, List<String> To, String Subject, String Message, List<String> AttFiles)
        {
            MailMessage mailmessage;
            try
            {
                foreach (String to in To)
                {
                    mailmessage = new MailMessage(new MailAddress(From.Trim()), new MailAddress(to.Trim()));
                    mailmessage.BodyEncoding = Encoding.Default;
                    mailmessage.Subject = Subject.Trim();
                    mailmessage.Body = Message.Trim();
                    mailmessage.Priority = MailPriority.High;
                    mailmessage.IsBodyHtml = true;
                    mailmessage.From = new MailAddress("atc@TransaX.com", "TransaX.com");
                    foreach (String _attFile in AttFiles)
                        mailmessage.Attachments.Add(new System.Net.Mail.Attachment(_attFile));

                    SmtpClient smtpMail = new SmtpClient();

                    smtpMail.Send(mailmessage);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SendEmailMessageWithAttach(String From, List<String> To, String Subject, String Message, List<EmailAttachment> AttFiles)
        {
            MailMessage mailmessage;
            try
            {
                foreach (String to in To)
                {
                    mailmessage = new MailMessage(new MailAddress(From.Trim()), new MailAddress(to.Trim()));
                    mailmessage.BodyEncoding = Encoding.Default;
                    mailmessage.Subject = Subject.Trim();
                    mailmessage.Body = Message.Trim();
                    mailmessage.Priority = MailPriority.High;
                    mailmessage.IsBodyHtml = true;
                    mailmessage.From = new MailAddress("atc@TransaX.com", "TransaX");
                    foreach (EmailAttachment _attFile in AttFiles)
                        mailmessage.Attachments.Add(new System.Net.Mail.Attachment(_attFile.FileStream, _attFile.FileName, _attFile.MimeType));

                    SmtpClient smtpMail = new SmtpClient();

                    smtpMail.Send(mailmessage);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="From"></param>
        /// <param name="To"></param>
        /// <param name="Subject"></param>
        /// <param name="Message"></param>
        /// <param name="AttFiles"></param>La lista está formada por rutas de archivos FTP
        //public void SendEmailMessageWithAttachFTP(String From, List<String> To, String Subject, String Message, List<String> AttFiles)
        //{
        //    MailMessage mailmessage;
        //    MyHelpers.Files.HelperFTP _helperFTP = new Files.HelperFTP();
        //    try
        //    {
        //        foreach (String to in To)
        //        {
        //            mailmessage = new MailMessage(new MailAddress(From.Trim()), new MailAddress(to.Trim()));
        //            mailmessage.BodyEncoding = Encoding.Default;
        //            mailmessage.Subject = Subject.Trim();
        //            mailmessage.Body = Message.Trim();
        //            mailmessage.Priority = MailPriority.High;
        //            mailmessage.IsBodyHtml = true;
        //            mailmessage.From = new MailAddress("info@LatinBoletos.com", "LatinBoletos.com");
        //            foreach (String _attFile in AttFiles)
        //            {
        //                Byte[] bytesArchivo;
        //                bytesArchivo = _helperFTP.ObtenerArchivo(_attFile);
        //                MemoryStream _stream = new MemoryStream(bytesArchivo);
        //                mailmessage.Attachments.Add(new System.Net.Mail.Attachment(_stream, _attFile.Substring(_attFile.LastIndexOf("/") + 1)));
        //                //mailmessage.Attachments.Add(new System.Net.Mail.Attachment(_attFile));
        //            }

        //            SmtpClient smtpMail = new SmtpClient();

        //            smtpMail.Send(mailmessage);
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        public void SendEmailMessageWithNotification(String From, ArrayList To, String Subject, String Message, String ReceiptAddress)
        {
            MailMessage mailmessage;
            try
            {
                foreach (String to in To)
                {
                    mailmessage = new MailMessage(new MailAddress(From.Trim()), new MailAddress(to.Trim()));
                    mailmessage.BodyEncoding = Encoding.Default;
                    mailmessage.Subject = Subject.Trim();
                    mailmessage.Body = Message.Trim();
                    mailmessage.Priority = MailPriority.High;
                    mailmessage.IsBodyHtml = true;
                    mailmessage.From = new MailAddress("info@TransaX.com", "TransaX.com");
                    mailmessage.Headers.Add("Disposition-Notification-To", ReceiptAddress);
                    mailmessage.Headers.Add("Return-Receipt-To", ReceiptAddress);
                    mailmessage.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure | DeliveryNotificationOptions.OnSuccess;

                    SmtpClient smtpMail = new SmtpClient();
                    smtpMail.Send(mailmessage);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Obsolete]
        public void SendEmailMessageAmazonSES(string toName, string toEmail, string subject,
            string body, string bodyHtml, string Attatchments)
        {
            var request = (HttpWebRequest)WebRequest.Create("https://ttmail.tuticket.com/email");
            request.Headers.Add("ttmail_user", "ttmail_user");
            request.Headers.Add("ttmail_password", "Jmpc14667301*");

            var postData = "from_name=" + "Contacto LatinBoletos.com";
            postData += "&from_email=" + "info@LatinBoletos.com";
            postData += "&to_name=" + toName;
            postData += "&to_email=" + toEmail;
            postData += "&subject=" + subject;
            postData += "&body=" + body;
            postData += "&body_html=" + bodyHtml;
            postData += "&attatchments=" + Attatchments;

            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
        }

        public class TimeoutWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
                request.Timeout = 300000; //3 minutos...
                return request;
            }
        }

        #endregion Send

        #region Constructores

        public EmailHelper()
        {
            //_pais = pais;

            //String ret = "";

            //ret += "<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>";
            //ret += "<html xmlns='http://www.w3.org/1999/xhtml'>";
            //ret += "<head>";
            //ret += "<meta http-equiv='Content-Type' content='text/html; charset=UTF-8' />";
            //ret += "<title>..:::PESA:::..</title>";
            //ret += "<link href='<URL>textstyles.css' rel='stylesheet' type='text/css' />";
            //ret += "<link href='<URL>div_styles.css' rel='stylesheet' type='text/css' />";
            //ret += "<style type='text/css'>";
            //ret += "<!--";
            //ret += ".style1 {";
            //ret += "font-family: Arial, Helvetica, sans-serif";
            //ret += "};";
            //ret += "h6{font-size: 10px};";
            //ret += "-->";
            //ret += "</style>";
            //ret += "</head>";
            //ret += "<body>";
            //ret += "</body>";
            //ret += "</html>";

            //this.Cuerpo = ret;
        }

        #endregion Constructores
    }

    public class EmailAttachment
    {
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public Stream FileStream { get; set; }

    }
}