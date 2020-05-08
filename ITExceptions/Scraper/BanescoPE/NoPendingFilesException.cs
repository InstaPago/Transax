using System;

namespace InstaTransfer.ITExceptions.Scraper.ScraperPEBanesco
{ 
    public class NoPendingFilesException : ITException
    {
        #region Constructors

        public NoPendingFilesException(string message, Exception ex) : base(message, ex)
        {
        }
        public NoPendingFilesException(string message) : base(message)
        {
        }
        public NoPendingFilesException(string message, string errorCode) : base(message, errorCode)
        {
        }
        public NoPendingFilesException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion

    }
}
