using InstaTransfer.BLL.Concrete;
using InstaTransfer.BLL.Models.PaymentRequest;
using InstaTransfer.DataAccess;
using InstaTransfer.ITExceptions.BackEnd;
using InstaTransfer.ITResources.BackEnd;
using InstaTransfer.ITResources.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Concrete
{
    /// <summary>
    /// Logica de negocios de las ordenes de compra
    /// </summary>
    public class CPurchaseOrderBLL : Repository<CPurchaseOrder>
    {

        #region Create

        /// <summary>
        /// Crea una orden de compra
        /// </summary>
        /// <param name="description">Descripcion de la orden (Procedencia)</param>
        /// <param name="amount">Monto de la orden</param>
        /// <param name="cuser">Usuario del comercio</param>
        /// <param name="userCI">Cedula del pagador</param>
        /// <param name="userEmail">Correo del pagador</param>
        /// <returns></returns>
        public CPurchaseOrder CreatePurchaseOrder(string description, decimal amount, CUser cuser, string userCI, string userEmail)
        {
            // Inicializamos
            var order = new CPurchaseOrder();
            // Construimos la orden
            order.Id = Guid.NewGuid();
            order.Amount = amount;
            order.EndUserEmail = userEmail;
            order.EndUserCI = Convert.ToInt32(userCI);
            // order.OrderNumber
            order.Description = description;
            order.RifCommerce = cuser.RifCommerce;
            order.IdCUser = cuser.Id;
            order.IdCPurchaseOrderStatus = (int)PurchaseOrderStatus.DeclarationPending;
            order.CreateDate = DateTime.Now;
            order.StatusChangeDate = DateTime.Now;

            // Agregamos la tabla a la base de datos
            AddEntity(order);
            // Guardamos los cambios
            SaveChanges();

            // Retornamos la orden creada
            return order;
        }

        #endregion

        #region Read

        /// <summary>
        /// Retorna la orden de compra asociada a una declaracion de un comercio especifico
        /// </summary>
        /// <param name="idDeclaration">Id de la declaracion asociada</param>
        /// <param name="rif">Rif del comercio especificado</param>
        /// <returns>Lista de <see cref="CPurchaseOrder"/> del comercio</returns>
        public CPurchaseOrder GetPurchaseOrder(Guid idDeclaration, string rif)
        {
            var declaration = db.GetTable<UDeclaration>().Where(ud => ud.Id.Equals(idDeclaration) && ud.RifCommerce == rif).FirstOrDefault();

            var purchaseOrder = declaration.CPurchaseOrder;

            return purchaseOrder;
        }

        /// <summary>
        /// Retorna la orden de compra segun el id asignada a un comercio especifico
        /// </summary>
        /// <param name="rif">Rif del comercio especificado</param>
        /// <param name="id">Id de la orden de compra</param>
        /// <returns><see cref="CPurchaseOrder"/> del comercio</returns>
        public CPurchaseOrder GetPurchaseOrder(string rif, Guid id)
        {
            return db.GetTable<CPurchaseOrder>().Where(po => po.RifCommerce.Equals(rif) && po.Id.Equals(id)).FirstOrDefault();
        }

        /// <summary>
        /// Retorna la orden de compra segun el id de la declaracion especifica
        /// </summary>
        /// <param name="declarationId">Id de la orden de compra</param>
        /// <returns><see cref="CPurchaseOrder"/></returns>
        public CPurchaseOrder GetPurchaseOrder(Guid declarationId)
        {
            var declaration = db.GetTable<UDeclaration>().Where(ud => ud.Id.Equals(declarationId)).FirstOrDefault();

            var purchaseOrder = declaration.CPurchaseOrder;

            return purchaseOrder;
        }

        /// <summary>
        /// Retorna la orden de compra segun el numero de orden asignada a un comercio especifico
        /// </summary>
        /// <param name="rif">Rif del comercio especificado</param>
        /// <param ame="orderNumber">Numero de la orden de compra</param>
        /// <returns><see cref="CPurchaseOrder"/> del comercio</returns>
        public CPurchaseOrder GetPurchaseOrder(string rif, string orderNumber)
        {
            return db.GetTable<CPurchaseOrder>().Where(po => po.RifCommerce.Equals(rif) && po.OrderNumber.Equals(orderNumber)).FirstOrDefault();
        }

        /// <summary>
        /// Retorna todas las ordenes de compra asignadas a un comercio especifico
        /// </summary>
        /// <param name="rif">Rif del comercio especificado</param>
        /// <returns>Lista de <see cref="CPurchaseOrder"/> del comercio</returns>
        public List<CPurchaseOrder> GetPurchaseOrders(string rif)
        {
            return db.GetTable<CPurchaseOrder>().Where(po => po.RifCommerce.Equals(rif)).ToList();
        }

        /// <summary>
        /// Retorna las ordenes creadas en un rango de fechas
        /// </summary>
        /// <param name="startDate">Fecha inicio</param>
        /// <param name="endDate">Fecha fin</param>
        /// <param name="status">Estado de la declaracion</param>
        /// <returns>Lista de ordenes para el rango de fechas</returns>
        public List<CPurchaseOrder> GetPurchaseOrdersByDateRangeAndStatus(DateTime startDate, DateTime endDate, PurchaseOrderStatus status)
        {
            return db.GetTable<CPurchaseOrder>().Where(po => (po.StatusChangeDate.Day >= startDate.Day && po.StatusChangeDate.Day <= endDate.Day) &&
                po.IdCPurchaseOrderStatus == (int)status).ToList();
        }

        /// <summary>
        /// Retorna las ordenes creadas en un rango de fechas para un comercio especifico
        /// </summary>
        /// <param name="rif">Rif del comercio</param>
        /// <param name="startDate">Fecha inicio</param>
        /// <param name="endDate">Fecha fin</param>
        /// <param name="status">Estado de la declaracion</param>
        /// <returns>Lista de ordenes para el rango de fechas y comercio especifico</returns>
        public List<CPurchaseOrder> GetPurchaseOrdersByDateRangeAndStatus(string rif, DateTime startDate, DateTime endDate, PurchaseOrderStatus status)
        {
            return db.GetTable<CPurchaseOrder>().Where(po => po.RifCommerce.Equals(rif) &&
                (po.StatusChangeDate >= startDate && po.StatusChangeDate <= endDate) &&
                po.IdCPurchaseOrderStatus == (int)status).ToList();
        }

        /// <summary>
        /// Retorna las ordenes de compra por status de un comercio especifico
        /// </summary>
        /// <param name="rif">Rif del comercio</param>
        /// <param name="status">Status de la orden</param>
        /// <returns>Ordenes de compra por status y comercio especifico</returns>
        public List<CPurchaseOrder> GetPurchaseOrdersByStatus(string rif, PurchaseOrderStatus status)
        {
            return db.GetTable<CPurchaseOrder>().Where(d => d.RifCommerce.Equals(rif) &&
                d.IdCPurchaseOrderStatus == (int)status).ToList();
        }

        /// <summary>
        /// Retorna las ordenes de compra por fecha de un comercio especifico
        /// </summary>
        /// <param name="rif">Rif del comercio</param>
        /// <param name="date">Fecha de la orden de compra</param>
        /// <param name="dateType">Tipo de fecha entre dia o mes</param>
        /// <returns>Ordenes de compra del dia</returns>
        public List<CPurchaseOrder> GetPurchaseOrdersByDate(string rif, DateTime date, DateType dateType)
        {
            switch (dateType)
            {
                case DateType.Day:
                    {
                        return db.GetTable<CPurchaseOrder>().Where(po => po.RifCommerce.Equals(rif) &&
                        po.StatusChangeDate.Day == date.Day &&
                        po.StatusChangeDate.Month == date.Month &&
                        po.StatusChangeDate.Year == date.Year).ToList();
                    }
                case DateType.Month:
                    {
                        return db.GetTable<CPurchaseOrder>().Where(po => po.RifCommerce.Equals(rif) &&
                        po.StatusChangeDate.Month == date.Month &&
                        po.StatusChangeDate.Year == date.Year).ToList();
                    }
                case DateType.RealTime:
                    {
                        return db.GetTable<CPurchaseOrder>().Where(po => po.RifCommerce.Equals(rif) &&
                        po.StatusChangeDate.Date >= date).ToList();
                    }
                default:
                    {
                        return db.GetTable<CPurchaseOrder>().Where(po => po.RifCommerce.Equals(rif) &&
                        po.StatusChangeDate == date).ToList();
                    }
            }




        }

        /// <summary>
        /// Retorna las ordenes de compra por fecha y statusde un comercio especifico
        /// </summary>
        /// <param name="rif">Rif del comercio</param>
        /// <param name="date">Fecha de la orden de compra</param>
        /// <param name="dateType">Tipo de fecha entre dia o mes</param>
        /// <param name="status">Status de la orden</param>
        /// <returns>Ordenes de compra por fecha, status y comercio especifico</returns>
        public List<CPurchaseOrder> GetPurchaseOrdersByDateAndStatus(string rif, DateTime date, DateType dateType, PurchaseOrderStatus status)
        {
            switch (dateType)
            {
                case DateType.Day:
                    {
                        return db.GetTable<CPurchaseOrder>().Where(po => po.RifCommerce.Equals(rif) &&
                        po.StatusChangeDate.Day == date.Day &&
                        po.StatusChangeDate.Month == date.Month &&
                        po.StatusChangeDate.Year == date.Year &&
                        po.IdCPurchaseOrderStatus == (int)status).ToList();
                    }
                case DateType.Month:
                    {
                        return db.GetTable<CPurchaseOrder>().Where(po => po.RifCommerce.Equals(rif) &&
                        po.StatusChangeDate.Month == date.Month &&
                        po.StatusChangeDate.Year == date.Year &&
                        po.IdCPurchaseOrderStatus == (int)status).ToList();
                    }
                default:
                    {
                        return db.GetTable<CPurchaseOrder>().Where(po => po.RifCommerce.Equals(rif) &&
                        po.StatusChangeDate == date &&
                        po.IdCPurchaseOrderStatus == (int)status).ToList();
                    }
            }
        }

        /// <summary>
        /// Retorna las ordenes de compra por fecha y statusde un comercio especifico
        /// </summary>
        /// <param name="rif">Rif del comercio</param>
        /// <param name="date">Fecha de la orden de compra</param>
        /// <param name="dateType">Tipo de fecha entre dia o mes</param>
        /// <param name="status">Status de la orden</param>
        /// <returns>Ordenes de compra por fecha, status y comercio especifico</returns>
        public List<CPurchaseOrder> GetPurchaseOrdersByDateAndStatus(DateTime date, DateType dateType, PurchaseOrderStatus status)
        {
            switch (dateType)
            {
                case DateType.Day:
                    {
                        return db.GetTable<CPurchaseOrder>().Where(po => po.StatusChangeDate.Day == date.Day &&
                        po.StatusChangeDate.Month == date.Month &&
                        po.StatusChangeDate.Year == date.Year &&
                        po.IdCPurchaseOrderStatus == (int)status).ToList();
                    }
                case DateType.Month:
                    {
                        return db.GetTable<CPurchaseOrder>().Where(po => po.StatusChangeDate.Month == date.Month &&
                        po.StatusChangeDate.Year == date.Year &&
                        po.IdCPurchaseOrderStatus == (int)status).ToList();
                    }
                default:
                    {
                        return db.GetTable<CPurchaseOrder>().Where(po => po.StatusChangeDate == date &&
                        po.IdCPurchaseOrderStatus == (int)status).ToList();
                    }
            }
        }

        #endregion

        #region Update

        /// <summary>
        /// Anula una orden de compra
        /// </summary>
        /// <param name="purchaseOrderId">Id de la orden</param>
        /// <param name="annulPaymentRequest">Opcion para anular solicitud asociada</param>
        /// <returns>Resultado de la operacion</returns>
        public bool TryAnnulPurchaseOrder(Guid purchaseOrderId, bool? annulPaymentRequest)
        {
            // Inicializamos las variables y obtenemos la data
            CPurchaseOrder purchaseOrder = GetEntity(purchaseOrderId);
            List<UDeclaration> declarations = purchaseOrder.UDeclarations.ToList();
            int poStatus = purchaseOrder.IdCPurchaseOrderStatus;
            bool isAnnulled;

            // Si posee una solicitud asociada, la anulamos
            if (annulPaymentRequest.Value == true && purchaseOrder.PaymentRequests.Count > 0)
            {
                purchaseOrder.PaymentRequests.FirstOrDefault().IdPaymentRequestStatus = (int)InstaTransfer.ITResources.Enums.PaymentRequestStatus.Annulled;
                purchaseOrder.PaymentRequests.FirstOrDefault().ChangeDate = DateTime.Now;
            }

            // Cambiamos los estados de la orden de compra segun el estado actual
            switch (poStatus)
            {
                // Validamos si la orden de compra ya esta anulada
                case (int)PurchaseOrderStatus.Annulled:
                    {
                        throw new PurchaseOrderAnnulledException(BackEndErrors.PurchaseOrderAnnulledExceptionMessage, BackEndErrors.PurchaseOrderAnnulledExceptionCode);
                    }
                // Validamos si la orden de compra esta declarada
                case (int)PurchaseOrderStatus.Declared:
                    {
                        foreach (var declaration in declarations)
                        {
                            using (DeclarationBLL DBLL = new DeclarationBLL())
                            {
                                // Anulamos cada declaracion asociada
                                isAnnulled = DBLL.TryAnnulDeclaration(purchaseOrder.RifCommerce, declaration.Id, true);
                            }
                            // Verificamos el resultado de la operación
                            if (!isAnnulled)
                            {
                                throw new PurchaseOrderDeclarationException(BackEndErrors.PurchaseOrderDeclarationExceptionMessage, BackEndErrors.PurchaseOrderDeclarationExceptionCode);
                            }
                        }
                        break;
                    }
                // Validamos si no tiene declaraciones asociadas
                case (int)PurchaseOrderStatus.DeclarationPending:
                    {
                        purchaseOrder.IdCPurchaseOrderStatus = (int)PurchaseOrderStatus.Annulled;
                        purchaseOrder.StatusChangeDate = DateTime.Now;
                        break;
                    }
                // Cambiamos el estado a anulado por defecto
                default:
                    {
                        purchaseOrder.IdCPurchaseOrderStatus = (int)PurchaseOrderStatus.Annulled;
                        purchaseOrder.StatusChangeDate = DateTime.Now;
                        break;
                    }
            }

            // Guardamos los cambios en la base de datos
            SaveChanges();
            // Success
            return true;
        }

        #endregion
        
    }
}
