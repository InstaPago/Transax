using System;

namespace InstaTransfer.ITExceptions.Api
{
    public class InvalidUserRoleException : ITException
    {
        #region Constructors

        public InvalidUserRoleException(string message, Exception ex) : base(message, ex)
        {
        }
        public InvalidUserRoleException(string message) : base(message)
        {
        }
        public InvalidUserRoleException(string message, string errorCode) : base(message, errorCode)
        {
        }
        public InvalidUserRoleException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion

    }
}
