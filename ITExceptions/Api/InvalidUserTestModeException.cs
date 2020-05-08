using System;

namespace InstaTransfer.ITExceptions.Api
{
    public class InvalidUserTestModeException : ITException
    {
        #region Constructors

        public InvalidUserTestModeException(string message, Exception ex) : base(message, ex)
        {
        }
        public InvalidUserTestModeException(string message) : base(message)
        {
        }
        public InvalidUserTestModeException(string message, string errorCode) : base(message, errorCode)
        {
        }
        public InvalidUserTestModeException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion

    }
}
