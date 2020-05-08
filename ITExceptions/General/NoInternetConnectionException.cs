using System;

namespace InstaTransfer.ITExceptions.General
{
    public class NoInternetConnectionException : ITException
    {
        #region Constructors

        public NoInternetConnectionException(string message, Exception ex) : base(message, ex)
        {
        }
        public NoInternetConnectionException(string message) : base(message)
        {
        }
        public NoInternetConnectionException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion

    }
}
