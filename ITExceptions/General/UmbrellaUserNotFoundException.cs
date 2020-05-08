using System;

namespace InstaTransfer.ITExceptions.General
{
    public class UmbrellaUserNotFoundException : ITException
    {
        #region Constructors

        public UmbrellaUserNotFoundException(string message, Exception ex) : base(message, ex)
        {
        }
        public UmbrellaUserNotFoundException(string message) : base(message)
        {
        }
        public UmbrellaUserNotFoundException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion

    }
}
