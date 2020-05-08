using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstaTransfer.ITResources.Global
{
    /// <summary>
    /// Variables globales del BackEnd
    /// </summary>
    public static class BackEndGlobals
    {
        private static DateTime _lastDashboardUpdate = DateTime.Now;
        /// <summary>
        /// Fecha en que se actualizo la pagina de inicio del BackEnd por última vez
        /// </summary>
        public static DateTime LastDashboardUpdate
        {
            get { return _lastDashboardUpdate; }

            set { _lastDashboardUpdate = value; }
        }

        private static int _realTimeDeclarationsCount = 0;
        /// <summary>
        /// Numero de declaraciones actualizadas en tiempo real
        /// </summary>
        public static int RealTimeDeclarationsCount
        {
            get { return _realTimeDeclarationsCount; }

            set { _realTimeDeclarationsCount = value; }
        }


        private static int _realTimePurchaseOrdersCount = 0;
        /// <summary>
        /// Numero de ordenes de compra actualizadas en tiempo real
        /// </summary>
        public static int RealTimePurchaseOrdersCount
        {
            get { return _realTimePurchaseOrdersCount; }

            set { _realTimePurchaseOrdersCount = value; }
        }

    }
}
