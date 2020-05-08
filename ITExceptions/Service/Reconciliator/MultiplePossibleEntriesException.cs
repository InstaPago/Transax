using System;

namespace InstaTransfer.ITExceptions.Service.Reconciliator
{
    public class MultiplePossibleEntriesException : ITException
    {
        #region Constructors

        public MultiplePossibleEntriesException(string message, Exception ex) : base(message, ex)
        {
        }
        public MultiplePossibleEntriesException(string message) : base(message)
        {
        }
        public MultiplePossibleEntriesException(string message, string errorCode) : base(message, errorCode)
        {
        }
        public MultiplePossibleEntriesException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion

    }
}
