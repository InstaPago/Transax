using System;

namespace InstaTransfer.ITExceptions.Scraper
{
    public class UserLoginException : ITException
    {
        #region Constructors

        public UserLoginException(string message, Exception ex) : base(message, ex)
        {
        }
        public UserLoginException(string message) : base(message)
        {
        }
        public UserLoginException(string message, string errorCode) : base(message, errorCode)
        {
        }
        public UserLoginException() : base()
        {
        }
        public UserLoginException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion

    }
}
