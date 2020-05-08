using System;

namespace InstaTransfer.ITExceptions.Scraper
{
    /// <summary>
    /// Representa los errores que se generan durante
    /// el funcionamiento general del Scraper
    /// </summary>
    public class SException : Exception
    {
        #region Fields

        private string _id;
        private string _className;
        private string _method;
        private string _messageException;
        private Exception _ex;

        #endregion

        #region Constructors

        /// <summary>
        /// Inicializa una nueva instancia del objeto <see cref="SException"/>
        /// con el valor indicado por un mensaje de error y la excepcion asociada
        /// </summary>
        /// <param name="message">Mensaje personalizado del error</param>
        /// <param name="ex">Excepcion arrojada</param>
        public SException(string message, Exception ex)
        {
            this._messageException = message;
            this._ex = ex;
        }
        /// <summary>
        /// Inicializa una nueva instancia del objeto <see cref="SException"/>
        /// con el valor indicado por un mensaje de error y la excepcion asociada
        /// </summary>
        /// <param name="message">Mensaje personalizado del error</param>
        /// <param name="ex">Excepcion arrojada</param>
        public SException()
        {
        }
        /// <summary>
        /// Inicializa una nueva instancia del objeto <see cref="SException"/>
        /// con el valor indicado por un mensaje de error.
        /// </summary>
        /// <param name="message">Mensaje personalizado del error</param>
        public SException(string message)
        {
            this._messageException = message;
        }

        /// <summary>
        /// Inicializa una nueva instancia del objeto <see cref="SException"/>
        /// con el valor indicado por un identificador, clase y metodo que originan el error,
        /// mensaje de error, y la excepcion asociada.
        /// </summary>
        /// <param name="id">Codigo identificador del error</param>
        /// <param name="classname">Clase que arroja el error</param>
        /// <param name="method">Metodo que arroja el error</param>
        /// <param name="message">Mensaje personalizado del error</param>
        /// <param name="ex">Excepcion arrojada</param>
        public SException(string id, string classname, string method, string message, Exception ex)
        {
            this._id = id;
            this._className = classname;
            this._method = method;
            this._messageException = message;
            this._ex = ex;
        }

        #endregion

        #region Properties

        public string Id
        {
            get { return _id; }
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
