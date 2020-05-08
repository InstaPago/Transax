using System;

namespace InstaTransfer.ITExceptions.Scraper.Banesco
{
    public class SExceptionBanesco : SException
    {
        #region Constructors

        public SExceptionBanesco(string message, Exception ex) : base(message, ex)
        {
        }
        public SExceptionBanesco(string message) : base(message)
        {
        }
        public SExceptionBanesco(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion

    }
}
