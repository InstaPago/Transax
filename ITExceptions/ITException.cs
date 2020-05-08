using System;

namespace InstaTransfer.ITExceptions
{
    /// <summary>
    /// Representa los errores que se generan durante
    /// el funcionamiento general de la aplicacion
    /// </summary>
    public class ITException : Exception
    {
        #region Fields

        private string _errorCode;
        private string _className;
        private string _method;
        private string _messageException;
        private Exception _ex;

        #endregion

        #region Constructors

        /// <summary>
        /// Inicializa una nueva instancia vacia del objeto <see cref="ITException"/>
        /// </summary>
        public ITException()
        {
        }
        /// <summary>
        /// Inicializa una nueva instancia del objeto <see cref="ITException"/>
        /// con el valor indicado por un mensaje de error y la excepcion asociada
        /// </summary>
        /// <param name="message">Mensaje personalizado del error</param>
        /// <param name="ex">Excepcion arrojada</param>
        public ITException(string message, Exception ex)
        {
            this._messageException = message;
            this._ex = ex;
        }
        /// <summary>
        /// Inicializa una nueva instancia del objeto <see cref="ITException"/>
        /// con el valor indicado por un mensaje de error.
        /// </summary>
        /// <param name="message">Mensaje personalizado del error</param>
        public ITException(string message)
        {
            this._messageException = message;
        }
        /// <summary>
        /// Inicializa una nueva instancia del objeto <see cref="ITException"/>
        /// con el valor indicado por un mensaje y codigo de error.
        /// </summary>
        /// <param name="message">Mensaje personalizado del error</param>
        /// <param name="errorCode">Codigo personalizado del error</param>
        public ITException(string message, string errorCode)
        {
            this._messageException = message;
            this._errorCode = errorCode;
        }
        /// <summary>
        /// Inicializa una nueva instancia del objeto <see cref="ITException"/>
        /// con el valor indicado por un identificador, clase y metodo que originan el error,
        /// mensaje de error, y la excepcion asociada.
        /// </summary>
        /// <param name="id">Codigo identificador del error</param>
        /// <param name="classname">Clase que arroja el error</param>
        /// <param name="method">Metodo que arroja el error</param>
        /// <param name="message">Mensaje personalizado del error</param>
        /// <param name="ex">Excepcion arrojada</param>
        public ITException(string errorCode, string classname, string method, string message, Exception ex)
        {
            this._errorCode = errorCode;
            this._className = classname;
            this._method = method;
            this._messageException = message;
            this._ex = ex;
        }

        #endregion

        #region Properties

        public string ErrorCode
        {
            get { return _errorCode; }
        }

        public string ClassName
        {
            get { return _className; }
        }

        public string Method
        {
            get { return _method; }
        }

        public string MessageException
        {
            get { return _messageException; }
        }

        public Exception Ex
        {
            get { return _ex; }
        }

       
        #endregion
    }
}
