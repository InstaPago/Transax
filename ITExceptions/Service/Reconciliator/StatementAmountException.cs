using System;

namespace InstaTransfer.ITExceptions.Service.Reconciliator
{
    public class StatementAmountException : ITException
    {
        #region Constructors

        public StatementAmountException(string message, Exception ex) : base(message, ex)
        {
        }
        public StatementAmountException(string message) : base(message)
        {
        }
        public StatementAmountException(string message, string errorCode) : base(message, errorCode)
        {
        }
        public StatementAmountException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion

    }
}
