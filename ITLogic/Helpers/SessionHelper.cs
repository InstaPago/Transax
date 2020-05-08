using InstaTransfer.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace InstaTransfer.ITLogic.Helpers
{
    public class SessionHelper
    {
        public class MySession
        {
            // private constructor
            private MySession()
            {
                CommerceUser = new CUser();
                LoggedIn = false;
            }

            // Gets the current session.
            public static MySession Current
            {
                get
                {
                    MySession session =
                      (MySession)HttpContext.Current.Session["__MySession__"];
                    if (session == null)
                    {
                        session = new MySession();
                        HttpContext.Current.Session["__MySession__"] = session;
                    }
                    return session;
                }
            }

            // **** add your session properties here ****

            /// <summary>
            /// Usuario del comercio logeado
            /// </summary>
            public CUser CommerceUser { get; set; }
            /// <summary>
            /// Estado de inicio de sesion del usuario
            /// </summary>
            public bool LoggedIn { get; set; }
        }

        public class MyPRSession
        {
            // private constructor
            private MyPRSession()
            {
                EndUser = new EndUser();
                LoggedIn = false;
            }

            // Gets the current session.
            public static MyPRSession Current
            {
                get
                {
                    MyPRSession session =
                      (MyPRSession)HttpContext.Current.Session["__MyPRSession__"];
                    if (session == null)
                    {
                        session = new MyPRSession();
                        HttpContext.Current.Session["__MyPRSession__"] = session;
                    }
                    return session;
                }
            }

            // **** add your session properties here ****

            /// <summary>
            /// Usuario de solicitud de pago logeado
            /// </summary>
            public EndUser EndUser { get; set; }
            /// <summary>
            /// Estado de inicio de sesion del usuario
            /// </summary>
            public bool LoggedIn { get; set; }
        }
    }
}
