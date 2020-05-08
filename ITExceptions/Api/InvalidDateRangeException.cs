using System;

namespace InstaTransfer.ITExceptions.Api
{
    public class InvalidDateRangeException : ITException
    {
        #region Constructors

        public InvalidDateRangeException(string message, Exception ex) : base(message, ex)
        {
        }
        public InvalidDateRangeException(string message) : base(message)
        {
        }
        public InvalidDateRangeException(string message, string errorCode) : base(message, errorCode)
        {
        }
        public InvalidDateRangeException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion

    }
}
