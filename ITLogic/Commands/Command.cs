using System;

namespace InstaTransfer.ITLogic
{
    /// <summary>
    /// Comando Base para el patron comando
    /// </summary>
    public abstract class Command : ICommand
    {

        #region Receiver
        private Object receiver;

        public Object Receiver
        {
            get { return receiver; }
            protected set { receiver = value; }
        }

        private object parameter;
        public object Parameter
        {
            get { return parameter; }
            set { parameter = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor de la clase comando
        /// </summary>
        /// <param name="receiver">recibe un objeto</param>
        public Command(Object receiver)
        {
            this.receiver = receiver;
        }

        /// <summary>
        /// Constructor vacio de la clase comando
        /// </summary>
        public Command() { }
        #endregion

        /// <summary>
        /// Metodo para ejecutar el comando
        /// </summary>
        public abstract void Execute();

    }
}
