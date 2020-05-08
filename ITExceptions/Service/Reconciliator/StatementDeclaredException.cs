using System;

namespace InstaTransfer.ITExceptions.Service.Reconciliator
{
    public class StatementDeclaredException : ITException
    {
        #region Constructors

        public StatementDeclaredException() : base()
        {
        }
        public StatementDeclaredException(string message, Exception ex) : base(message, ex)
        {
        }
        public StatementDeclaredException(string message) : base(message)
        {
        }
        public StatementDeclaredException(string message, string errorCode) : base(message, errorCode)
        {
        }
        public StatementDeclaredException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion

    }
}
