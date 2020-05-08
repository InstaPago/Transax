using System;

namespace InstaTransfer.ITExceptions.Scraper.Banesco
{
    public class AlternateLoginException : SException
    {
        #region Constructors

        public AlternateLoginException(string message, Exception ex) : base(message, ex)
        {
        }
        public AlternateLoginException(string message) : base(message)
        {
        }
        public AlternateLoginException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion

    }
}
