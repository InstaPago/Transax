using System;

namespace InstaTransfer.ITExceptions.Updater
{
    public class EmptyStatementException : ITException
    {
        #region Constructors

        public EmptyStatementException() : base()
        {
        }
        public EmptyStatementException(string message, Exception ex) : base(message, ex)
        {
        }
        public EmptyStatementException(string message) : base(message)
        {
        }
        public EmptyStatementException(string message, string errorCode) : base(message, errorCode)
        {
        }
        public EmptyStatementException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion

    }
}
