using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Web;
using MyHelpers.Files;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net;
using System.Text.RegularExpressions;

namespace MyHelpers.Email
{   /// <summary>
    /// Summary description for Email
    /// </summary>
    public class Email
    {
        #region Atributos

        private String _cuerpo;
        /// <summary>
        /// Cuerpo del correo
        /// </summary>
        public String Cuerpo
        {
            get { return _cuerpo; }
            set { _cuerpo = value; }
        }

        private List<String> _to;

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

        private List<String> _attachments;
        public List<String> Attachments
        {
            get { return _attachments; }
            set { _attachments = value; }
        }

        private String _subject;
        public String Subject
        {
            get { return _subject; }
            set { _subject = value; }
        }

        //private List<ByteFile> _fileAttachments;
        public List<ByteFile> FileAttachments { get; set; }


        #endregion

        #region Send
        public void SendEmailMessage(String From, List<String> To, String Subject, String Message)
        {
            MailMessage mailmessage;
            foreach (String to in To)
            {
                mailmessage = new MailMessage(new MailAddress(From.Trim()), new MailAddress(to.Trim()));
                mailmessage.BodyEncoding = Encoding.Default;
                mailmessage.Subject = Subject.Trim();
                mailmessage.Body = Message;
                mailmessage.IsBodyHtml = true;

                SmtpClient smtpMail = new SmtpClient();

                smtpMail.Send(mailmessage);
            }/*
            try
            {
             
            }
            catch (Exception e)
            {
                throw e;
            }*/
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
                    mailmessage.Headers.Add("Content-Type", "multipart/mixed");
                    foreach (String _attFile in AttFiles)
                        mailmessage.Attachments.Add(new System.Net.Mail.Attachment(_attFile));

                    SmtpClient smtpMail = new SmtpClient();

                    ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

                    smtpMail.Send(mailmessage);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void SendEmailMessageWithAttachmentFiles(String From, List<String> To, String Subject, String Message, List<ByteFile> AttFiles)
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
                    mailmessage.Headers.Add("Content-Type", "multipart/mixed");
                    foreach (var _attFile in AttFiles)
                    {
                        MemoryStream ms = new MemoryStream(_attFile.archivo);
                        mailmessage.Attachments.Add(new Attachment(ms, _attFile.nombre, _attFile.extencion));
                        //      ms.Dispose();
                    }

                    ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

                    SmtpClient smtpMail = new SmtpClient();
                    smtpMail.Send(mailmessage);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

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
                    mailmessage.Headers.Add("Disposition-Notification-To", ReceiptAddress);
                    mailmessage.Headers.Add("Return-Receipt-To", ReceiptAddress);
                    mailmessage.Headers.Add("Content-Type", "multipart/mixed");
                    mailmessage.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure | DeliveryNotificationOptions.OnSuccess;

                    SmtpClient smtpMail = new SmtpClient();

                    ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

                    smtpMail.Send(mailmessage);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void SendEmailMessageWithAtachmentsMVC(String From, List<String> To, String Subject, String Message,
            IEnumerable<HttpPostedFileBase> files)
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
                    mailmessage.Headers.Add("Content-Type", "multipart/mixed");
                    foreach (HttpPostedFileBase file in files)
                    {
                        mailmessage.Attachments.Add(new Attachment(file.InputStream, file.FileName, file.ContentType));
                    }

                    SmtpClient smtpMail = new SmtpClient();

                    ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

                    smtpMail.Send(mailmessage);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Envia el correo con el ThreadPool
        /// </summary>
        /// <param name="state"></param>
        public static void SendAlertaAsync(Object state)
        {
            MyHelpers.Email.Email _mail = state as MyHelpers.Email.Email;
            try
            {
                if (_mail.FileAttachments != null)
                    _mail.SendEmailMessageWithAttachmentFiles(_mail.From, _mail.To, _mail.Subject, _mail.Cuerpo, _mail.FileAttachments);
                else
                    _mail.SendEmailMessage(_mail.From, _mail.To, _mail.Subject, _mail.Cuerpo);
            }
            catch (Exception)
            {
                //throw e;
            }
        }

        private static bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {

            return true;

        }
        #endregion

        public void SendEmailMessage(String From, String FromName, List<String> To, String Subject, String Message)
        {
    //        ServicePointManager.ServerCertificateValidationCallback =
    //delegate (object s, X509Certificate certificate,
    //         X509Chain chain, SslPolicyErrors sslPolicyErrors)
    //{ return true; };

            MailMessage mailmessage;
            try
            {
                foreach (String to in To)
                {
                    mailmessage = new MailMessage(new MailAddress(From.Trim(), FromName), new MailAddress(to.Trim()));
                    mailmessage.BodyEncoding = Encoding.Default;
                    mailmessage.Subject = Subject.Trim();
                    mailmessage.Body = Message;
                    mailmessage.IsBodyHtml = true;

                    SmtpClient smtpMail = new SmtpClient();

                    smtpMail.Send(mailmessage);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #region Constructores
        public Email()
        {
            String ret = "";

            ret += "<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>";
            ret += "<html xmlns='http://www.w3.org/1999/xhtml'>";
            ret += "<head>";
            ret += "<meta http-equiv='Content-Type' content='text/html; charset=UTF-8' />";
            ret += "<title>..:::PESA:::..</title>";
            ret += "<link href='<URL>textstyles.css' rel='stylesheet' type='text/css' />";
            ret += "<link href='<URL>div_styles.css' rel='stylesheet' type='text/css' />";
            ret += "<style type='text/css'>";
            ret += "<!--";
            ret += ".style1 {";
            ret += "font-family: Arial, Helvetica, sans-serif";
            ret += "};";
            ret += "h6{font-size: 10px};";
            ret += "-->";
            ret += "</style>";
            ret += "</head>";
            ret += "<body>";
            ret += "</body>";
            ret += "</html>";

            this.Cuerpo = ret;
        }
        #endregion

    }


}
