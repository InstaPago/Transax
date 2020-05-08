using InstaTransfer.ITExceptions.Scraper;
using System;

namespace InstaTransfer.ScraperPresenter.Exceptions.Provincial
{
    public class SExceptionProvincial : SException
    {
        #region Constructors

        public SExceptionProvincial(string message, Exception ex) : base(message, ex)
        {
        }
        public SExceptionProvincial(string message) : base(message)
        {
        }
        public SExceptionProvincial(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion
    }
}
