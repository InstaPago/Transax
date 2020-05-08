using System;

namespace InstaTransfer.ITExceptions.Api
{
    public class InvalidUserException : ITException
    {
        #region Constructors

        public InvalidUserException(string message, Exception ex) : base(message, ex)
        {
        }
        public InvalidUserException(string message) : base(message)
        {
        }
        public InvalidUserException(string message, string errorCode) : base(message, errorCode)
        {
        }
        public InvalidUserException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion

    }
}
