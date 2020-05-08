using InstaTransfer.BLL.Concrete;
using InstaTransfer.BLL.Models.Declaration;
using InstaTransfer.DataAccess;
using InstaTransfer.ITExceptions.Api;
using InstaTransfer.ITExceptions.BackEnd;
using InstaTransfer.ITExceptions.Service.Reconciliator;
using InstaTransfer.ITResources.Api;
using InstaTransfer.ITResources.BackEnd;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITResources.General;
using InstaTransfer.ITResources.Service.Reconciliator;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Concrete
{
    /// <summary>
    /// Lógica de negocio de las declaraciones
    /// </summary>
    public class DeclarationBLL : Repository<UDeclaration>
    {
        #region Create

        /// <summary>
        /// Declara el pago a partir de la solicitud de un usuario nuevo
        /// </summary>
        /// <param name="declarationModel">Modelo de la declaracion</param>
        /// <param name="idPaymentRequest">Id de la solicitud</param>
        public void DeclareRequest(DeclarationRequestModel declarationModel, Guid idPaymentRequest)
        {
            // Variables
            var paymentRequest = new PaymentRequest();
            var DBLL = new DeclarationBLL();
            var _uDeclaration = new UDeclaration();
            // Obtenemos la solicitud desde la base de datos
            using (PaymentRequestBLL PRBLL = new PaymentRequestBLL())
            {
                paymentRequest = PRBLL.GetEntity(idPaymentRequest);
            }
            // Obtenemos el comercio y la orden de compra asociada a la solicitud
            var commerce = paymentRequest.Commerce;
            var _cPurchaseOrder = paymentRequest.CPurchaseOrder;

            #region Validations
            // Verificamos que el numero de referencia no este duplicado
            if (DBLL.GetDeclarations(declarationModel.referencenumber, declarationModel.idissuingbank).ToList().Count > 0)
            {
                throw new DuplicateReferenceException(BackEndErrors.DuplicateReferenceExceptionMessage, BackEndErrors.DuplicateReferenceExceptionCode);
            }
            // Verificamos que la orden de compra exista
            if (_cPurchaseOrder == null)
            {
                throw new PurchaseOrderNotFoundException(BackEndErrors.PurchaseOrderNotFoundExceptionMessage, BackEndErrors.PurchaseOrderNotFoundExceptionCode);
            }
            // Verificamos si la orden de compra ya tiene una declaracion asociada no anulada
            if (_cPurchaseOrder.UDeclarations.Count > 0 && _cPurchaseOrder.UDeclarations.Any(c => c.IdUDeclarationStatus != (int)DeclarationStatus.Annulled))
            {
                throw new PurchaseOrderDeclaredException(BackEndErrors.PurchaseOrderDeclaredExceptionMessage, BackEndErrors.PurchaseOrderDeclaredExceptionCode);
            }
            // Verificamos si los montos coinciden
            if (_cPurchaseOrder.Amount != declarationModel.amount)
            {
                throw new DeclarationAmountException(BackEndErrors.DeclarationAmountExceptionMessage, BackEndErrors.DeclarationAmountExceptionCode);
            }
            // Verificamos que las credenciales del pagador coincidan
            if (_cPurchaseOrder.EndUserCI != declarationModel.declarationuser.userci || _cPurchaseOrder.EndUserEmail != declarationModel.declarationuser.useremail)
            {
                throw new DeclarationPaymentUserException(BackEndErrors.DeclarationPaymentUserExceptionMessage, BackEndErrors.DeclarationPaymentUserExceptionCode);
            }
            // Verificamos el estado de la orden de compra
            if (_cPurchaseOrder.IdCPurchaseOrderStatus.Equals((int)PurchaseOrderStatus.Annulled))
            {
                throw new InstaTransfer.ITExceptions.BackEnd.PurchaseOrderAnnulledException(BackEndErrors.PurchaseOrderAnnulledExceptionMessage, BackEndErrors.PurchaseOrderAnnulledExceptionCode);
            }
            #endregion

            // Creamos la declaracion en la base de datos
            CreateDeclaration(declarationModel, paymentRequest);
        }

        /// <summary>
        /// Declara el pago a partir de la solicitud de un usuario existente
        /// </summary>
        /// <param name="declarationModel">Modelo de la declaración</param>
        /// <param name="idPaymentRequest">Id de la solicitud</param>
        public void DeclareExistingUserRequest(DeclarationRequestViewModel declarationModel, Guid idPaymentRequest)
        {
            // Variables
            var paymentRequest = new PaymentRequest();
            var DBLL = new DeclarationBLL();
            var _uDeclaration = new UDeclaration();
            // Obtenemos la solicitud desde la base de datos
            using (PaymentRequestBLL PRBLL = new PaymentRequestBLL())
            {
                paymentRequest = PRBLL.GetEntity(idPaymentRequest);
            }
            // Obtenemos el comercio y la orden de compra asociada a la solicitud
            var commerce = paymentRequest.Commerce;
            var _cPurchaseOrder = paymentRequest.CPurchaseOrder;

            #region Validations
            // Verificamos que el numero de referencia no este duplicado
            if (DBLL.GetDeclarations(declarationModel.referencenumber, declarationModel.idissuingbank).ToList().Count > 0)
            {
                throw new DuplicateReferenceException(BackEndErrors.DuplicateReferenceExceptionMessage, BackEndErrors.DuplicateReferenceExceptionCode);
            }
            // Verificamos que la orden de compra exista
            if (_cPurchaseOrder == null)
            {
                throw new PurchaseOrderNotFoundException(BackEndErrors.PurchaseOrderNotFoundExceptionMessage, BackEndErrors.PurchaseOrderNotFoundExceptionCode);
            }
            // Verificamos si la orden de compra ya tiene una declaracion asociada no anulada
            if (_cPurchaseOrder.UDeclarations.Count > 0 && _cPurchaseOrder.UDeclarations.Any(c => c.IdUDeclarationStatus != (int)DeclarationStatus.Annulled))
            {
                throw new PurchaseOrderDeclaredException(BackEndErrors.PurchaseOrderDeclaredExceptionMessage, BackEndErrors.PurchaseOrderDeclaredExceptionCode);
            }
            // Verificamos si los montos coinciden
            if (_cPurchaseOrder.Amount != declarationModel.amount)
            {
                throw new DeclarationAmountException(BackEndErrors.DeclarationAmountExceptionMessage, BackEndErrors.DeclarationAmountExceptionCode);
            }
            // Verificamos que las credenciales del pagador coincidan
            if (_cPurchaseOrder.EndUserCI != declarationModel.declarationuser.userci || _cPurchaseOrder.EndUserEmail != declarationModel.declarationuser.useremail)
            {
                throw new DeclarationPaymentUserException(BackEndErrors.DeclarationPaymentUserExceptionMessage, BackEndErrors.DeclarationPaymentUserExceptionCode);
            }
            // Verificamos el estado de la orden de compra
            if (_cPurchaseOrder.IdCPurchaseOrderStatus.Equals((int)PurchaseOrderStatus.Annulled))
            {
                throw new InstaTransfer.ITExceptions.BackEnd.PurchaseOrderAnnulledException(BackEndErrors.PurchaseOrderAnnulledExceptionMessage, BackEndErrors.PurchaseOrderAnnulledExceptionCode);
            }
            #endregion

            // Creamos la declaracion en la base de datos
            CreateExistingUserDeclaration(declarationModel, paymentRequest);
        }

        /// <summary>
        /// Crea una declaracion a partir de una solicitud de pago
        /// </summary>
        /// <param name="requestModel">Modelo de datos de la declaracion</param>
        /// <param name="paymentRequest">Solicitud de pago</param>
        public void CreateDeclaration(DeclarationRequestModel declarationModel, PaymentRequest paymentRequest)
        {
            // Variables
            var order = new CPurchaseOrder();

            // Creamos la declaracion
            var declaration = new UDeclaration
            {
                IdUBank = declarationModel.idissuingbank,
                TransactionDate = DateTime.ParseExact(declarationModel.transactiondate, GeneralResources.ShortDateFormat, CultureInfo.GetCultureInfo("es-VE")),
                IdUOperationType = declarationModel.idoperationtype,
                Reference = declarationModel.referencenumber,
                Amount = declarationModel.amount,
                EndUserCI = declarationModel.declarationuser.userci,
                EndUserEmail = declarationModel.declarationuser.useremail,
                Id = declarationModel.id,
                CreateDate = DateTime.Now,
                IdUDeclarationStatus = (int)DeclarationStatus.ReconciliationPending,
                RifCommerce = paymentRequest.RifCommerce,
                IdCPurchaseOrder = declarationModel.idpurchaseorder,
                IdCUser = Guid.Parse(ConfigurationManager.AppSettings["TransaxApiUserID"]),
                StatusChangeDate = DateTime.Now
            };

            // Agregamos la tabla y guardamos los cambios en base de datos
            AddEntity(declaration);
            SaveChanges();

            // Cambiamos el estado de la orden de compra
            using (CPurchaseOrderBLL CPOBLL = new CPurchaseOrderBLL())
            {
                order = CPOBLL.GetEntity(declarationModel.idpurchaseorder);
                order.IdCPurchaseOrderStatus = (int)PurchaseOrderStatus.Declared;
                order.StatusChangeDate = DateTime.Now;
                CPOBLL.SaveChanges();
            }

            // Cambiamos el estado de la solicitud
            using (PaymentRequestBLL PRBLL = new PaymentRequestBLL())
            {
                paymentRequest = PRBLL.GetEntity(paymentRequest.Id);
                paymentRequest.IdPaymentRequestStatus = (int)InstaTransfer.ITResources.Enums.PaymentRequestStatus.Declared;
                paymentRequest.ChangeDate = DateTime.Now;
                PRBLL.SaveChanges();
            }
        }

        /// <summary>
        /// Crea una declaracion a partir de una solicitud de pago
        /// </summary>
        /// <param name="requestModel">Modelo de datos de la declaracion</param>
        /// <param name="paymentRequest">Solicitud de pago</param>
        public void CreateExistingUserDeclaration(DeclarationRequestViewModel declarationModel, PaymentRequest paymentRequest)
        {
            // Variables
            var order = new CPurchaseOrder();

            // Creamos la declaracion
            var declaration = new UDeclaration
            {
                IdUBank = declarationModel.idissuingbank,
                TransactionDate = DateTime.ParseExact(declarationModel.transactiondate, GeneralResources.ShortDateFormat, CultureInfo.GetCultureInfo("es-VE")),
                IdUOperationType = declarationModel.idoperationtype,
                Reference = declarationModel.referencenumber,
                Amount = declarationModel.amount,
                EndUserCI = declarationModel.declarationuser.userci,
                EndUserEmail = declarationModel.declarationuser.useremail,
                Id = declarationModel.id,
                CreateDate = DateTime.Now,
                IdUDeclarationStatus = (int)DeclarationStatus.ReconciliationPending,
                RifCommerce = paymentRequest.RifCommerce,
                IdCPurchaseOrder = declarationModel.idpurchaseorder,
                IdCUser = Guid.Parse(ConfigurationManager.AppSettings["TransaxApiUserID"]),
                StatusChangeDate = DateTime.Now
            };

            // Agregamos la tabla y guardamos los cambios en base de datos
            AddEntity(declaration);
            SaveChanges();

            // Cambiamos el estado de la orden de compra
            using (CPurchaseOrderBLL CPOBLL = new CPurchaseOrderBLL())
            {
                order = CPOBLL.GetEntity(declarationModel.idpurchaseorder);
                order.IdCPurchaseOrderStatus = (int)PurchaseOrderStatus.Declared;
                order.StatusChangeDate = DateTime.Now;
                CPOBLL.SaveChanges();
            }

            // Cambiamos el estado de la solicitud
            using (PaymentRequestBLL PRBLL = new PaymentRequestBLL())
            {
                paymentRequest = PRBLL.GetEntity(paymentRequest.Id);
                paymentRequest.IdPaymentRequestStatus = (int)InstaTransfer.ITResources.Enums.PaymentRequestStatus.Declared;
                paymentRequest.ChangeDate = DateTime.Now;
                PRBLL.SaveChanges();
            }
        }

        #endregion

        #region Read

        /// <summary>
        /// Retorna la declaracion por id especifico
        /// </summary>
        /// <param name="idDeclaration">Id de la declaracion</param>
        /// <returns>Declaracion especifica</returns>
        public UDeclaration GetDeclaration(Guid idDeclaration)
        {
            return GetEntity(idDeclaration);
        }

        /// <summary>
        /// Retorna la declaracion con el id y el comercio asociado especifico.
        /// </summary>
        /// <param name="rif">Rif del comercio especificado</param>
        /// <param name="id">Id de la declaracion</param>
        /// <returns>Lista de <see cref="UDeclaration"/> del comercio</returns>
        public UDeclaration GetDeclaration(string rif, Guid id)
        {
            return db.GetTable<UDeclaration>().Where(po => po.RifCommerce.Equals(rif) && po.Id.Equals(id)).FirstOrDefault();
        }

        /// <summary>
        /// Retorna todas las declaraciones del sistema
        /// </summary>
        /// <returns>Lista de declaraciones</returns>
        public List<UDeclaration> GetDeclarations()
        {
            return GetAllRecords().ToList();
        }

        /// <summary>
        /// Retorna todas las declaraciones asignadas a un comercio especifico
        /// </summary>
        /// <param name="rif">Rif del comercio especificado</param>
        /// <returns>Lista de <see cref="UDeclaration"/> del comercio</returns>
        public List<UDeclaration> GetDeclarations(string rif)
        {
            return db.GetTable<UDeclaration>().Where(d => d.RifCommerce.Equals(rif)).ToList();
        }

        /// <summary>
        /// Encuentra la declaracion a partir de un banco y numero de referencia
        /// </summary>
        /// <param name="refNum">Numero de referencia de la declaracion a buscar</param>
        /// <param name="idBank">Id del banco emisor para la declaracion</param>
        /// <returns>Lista de <see cref="UDeclaration"/> encontradas</returns>
        public List<UDeclaration> GetDeclarations(string refNum, string idBank)
        {
            return db.GetTable<UDeclaration>().Where(d => d.Reference == refNum && d.IdUBank == idBank).ToList();
        }

        /// <summary>
        /// Retorna las declaraciones asociadas a una orden de compra de un comercio especifico
        /// </summary>
        /// <param name="idPurchaseOrder">Id de la orden de compra asociada</param>
        /// <param name="rif">Rif del comercio especificado</param>
        /// <returns>Lista de <see cref="UDeclaration"/> del comercio</returns>
        public List<UDeclaration> GetDeclarations(Guid idPurchaseOrder, string rif)
        {
            return db.GetTable<UDeclaration>().Where(d => d.RifCommerce.Equals(rif) &&
                                                       d.IdCPurchaseOrder.Equals(idPurchaseOrder)).ToList();
        }

        /// <summary>
        /// Retorna las declaraciones por fecha de un comercio especifico
        /// </summary>
        /// <param name="rif">Rif del comercio</param>
        /// <param name="date">Fecha de la declaracion</param>
        /// <param name="dateType">Tipo de fecha entre dia y mes</param>
        /// <returns>Declaraciones por la fecha y comercio especifico</returns>
        public List<UDeclaration> GetDeclarationsByDate(string rif, DateTime date, DateType dateType)
        {
            switch (dateType)
            {
                case DateType.Day:
                    {
                        return db.GetTable<UDeclaration>().Where(ud => ud.RifCommerce.Equals(rif) &&
                            ud.StatusChangeDate.Day == date.Day &&
                            ud.StatusChangeDate.Month == date.Month &&
                            ud.StatusChangeDate.Year == date.Year).ToList();
                    }
                case DateType.Month:
                    {
                        return db.GetTable<UDeclaration>().Where(ud => ud.RifCommerce.Equals(rif) &&
                            ud.StatusChangeDate.Month == date.Month &&
                            ud.StatusChangeDate.Year == date.Year).ToList();
                    }
                case DateType.RealTime:
                    {
                        return db.GetTable<UDeclaration>().Where(ud => ud.RifCommerce.Equals(rif) &&
                            ud.StatusChangeDate.Date >= date).ToList();
                    }
                default:
                    {
                        return db.GetTable<UDeclaration>().Where(ud => ud.RifCommerce.Equals(rif) &&
                            ud.StatusChangeDate == date).ToList();
                    }
            }

        }

        /// <summary>
        /// Retorna las declaraciones por fecha
        /// </summary>
        /// <param name="date">Fecha de la declaracion</param>
        /// <param name="dateType">Tipo de fecha entre dia y mes</param>
        /// <returns>Declaraciones por la fecha y comercio especifico</returns>
        public List<UDeclaration> GetDeclarationsByDate(DateTime date, DateType dateType)
        {
            switch (dateType)
            {
                case DateType.Day:
                    {
                        return db.GetTable<UDeclaration>().Where(ud => ud.StatusChangeDate.Day == date.Day &&
                        ud.StatusChangeDate.Month == date.Month &&
                        ud.StatusChangeDate.Year == date.Year).ToList();
                    }
                case DateType.Month:
                    {
                        return db.GetTable<UDeclaration>().Where(ud => ud.StatusChangeDate.Month == date.Month &&
                        ud.StatusChangeDate.Year == date.Year).ToList();
                    }
                case DateType.RealTime:
                    {
                        return db.GetTable<UDeclaration>().Where(ud => ud.StatusChangeDate.Date >= date).ToList();
                    }
                default:
                    {
                        return db.GetTable<UDeclaration>().Where(ud => ud.StatusChangeDate == date).ToList();
                    }
            }
        }

        /// <summary>
        /// Retorna las declaraciones por status de un comercio especifico
        /// </summary>
        /// <param name="rif">Rif del comercio</param>
        /// <param name="status">Status de la declaracion</param>
        /// <returns>Declaraciones por status y comercio especifico</returns>
        public List<UDeclaration> GetDeclarationsByStatus(string rif, DeclarationStatus status)
        {
            return db.GetTable<UDeclaration>().Where(ud => ud.RifCommerce.Equals(rif) &&
                ud.IdUDeclarationStatus == (int)status).ToList();
        }

        /// <summary>
        /// Retorna todas las declaraciones de un comercio especifico
        /// </summary>
        /// <param name="rif">Rif del comercio</param>
        /// <param name="status">Status de la declaracion</param>
        /// <returns>Declaraciones por status y comercio especifico</returns>
        public List<UDeclaration> GetDeclarationsByStatus(DeclarationStatus status)
        {
            return db.GetTable<UDeclaration>().Where(ud => ud.IdUDeclarationStatus == (int)status).ToList();
        }

        /// <summary>
        /// Retorna las declaraciones por fecha y status de un comercio especifico
        /// </summary>
        /// <param name="rif">Rif del comercio</param>
        /// <param name="date">Fecha de la declaracion</param>
        /// <param name="dateType">Tipo de fecha entre dia y mes</param>
        /// <param name="status">Status de la declaracion</param>
        /// <returns>Declaraciones por la fecha, status y comercio especifico</returns>
        public List<UDeclaration> GetDeclarationsByDateAndStatus(string rif, DateTime date, DateType dateType, DeclarationStatus status)
        {
            switch (dateType)
            {
                case DateType.Day:
                    {
                        return db.GetTable<UDeclaration>().Where(ud => ud.RifCommerce.Equals(rif) &&
                            ud.StatusChangeDate.Day == date.Day &&
                            ud.StatusChangeDate.Month == date.Month &&
                            ud.StatusChangeDate.Year == date.Year &&
                            ud.IdUDeclarationStatus == (int)status).ToList();
                    }
                case DateType.Month:
                    {
                        return db.GetTable<UDeclaration>().Where(ud => ud.RifCommerce.Equals(rif) &&
                            ud.StatusChangeDate.Month == date.Month &&
                            ud.StatusChangeDate.Year == date.Year &&
                            ud.IdUDeclarationStatus == (int)status).ToList();
                    }
                default:
                    {
                        return db.GetTable<UDeclaration>().Where(ud => ud.RifCommerce.Equals(rif) &&
                            ud.StatusChangeDate == date &&
                            ud.IdUDeclarationStatus == (int)status).ToList();
                    }
            }
        }

        /// <summary>
        /// Retorna las declaraciones creadas en un rango de fechas para un comercio especifico
        /// </summary>
        /// <param name="rif">Rif del comercio</param>
        /// <param name="startDate">Fecha inicio</param>
        /// <param name="endDate">Fecha fin</param>
        /// <param name="status">Estado de la declaracion</param>
        /// <returns>Lista de declaraciones para el rango de fechas y comercio especifico</returns>
        public List<UDeclaration> GetDeclarationsByDateRangeAndStatus(string rif, DateTime startDate, DateTime endDate, DeclarationStatus status)
        {
            return db.GetTable<UDeclaration>().Where(ud => ud.RifCommerce.Equals(rif) &&
                (ud.StatusChangeDate >= startDate && ud.StatusChangeDate <= endDate) &&
                ud.IdUDeclarationStatus == (int)status).ToList();
        }

        /// <summary>
        /// Retorna las declaraciones creadas en un rango de fechas especifico
        /// </summary>
        /// <param name="startDate">Fecha inicio</param>
        /// <param name="endDate">Fecha fin</param>
        /// <param name="status">Estado de la declaracion</param>
        /// <returns>Lista de declaraciones para el rango de fechas especifico</returns>
        public List<UDeclaration> GetDeclarationsByDateRangeAndStatus(DateTime startDate, DateTime endDate, DeclarationStatus status)
        {
            return db.GetTable<UDeclaration>().Where(ud => (ud.StatusChangeDate.Day >= startDate.Day && ud.StatusChangeDate.Day <= endDate.Day) &&
                ud.IdUDeclarationStatus == (int)status).ToList();
        }

        /// <summary>
        /// Retorna todas las declaraciones por fecha y status
        /// </summary>
        /// <param name="date">Fecha de la declaracion</param>
        /// <param name="dateType">Tipo de fecha entre dia y mes</param>
        /// <param name="status">Status de la declaracion</param>
        /// <returns>Declaraciones por la fecha, y status</returns>
        public List<UDeclaration> GetDeclarationsByDateAndStatus(DateTime date, DateType dateType, DeclarationStatus status)
        {
            switch (dateType)
            {
                case DateType.Day:
                    {
                        return db.GetTable<UDeclaration>().Where(ud => ud.StatusChangeDate.Day == date.Day &&
                            ud.StatusChangeDate.Month == date.Month &&
                            ud.StatusChangeDate.Year == date.Year &&
                            ud.IdUDeclarationStatus == (int)status).ToList();
                    }
                case DateType.Month:
                    {
                        return db.GetTable<UDeclaration>().Where(ud => ud.StatusChangeDate.Month == date.Month &&
                            ud.StatusChangeDate.Year == date.Year &&
                            ud.IdUDeclarationStatus == (int)status).ToList();
                    }
                default:
                    {
                        return db.GetTable<UDeclaration>().Where(ud => ud.StatusChangeDate == date &&
                            ud.IdUDeclarationStatus == (int)status).ToList();
                    }
            }
        }

        #endregion

        #region Update

        /// <summary>
        /// Anula una declaración específica y devuelve el resultado de la operación
        /// </summary>
        /// <param name="declarationId">Id de la declaración a anular</param>
        /// <param name="rif">Rif del comercio asociado</param>
        /// <param name="IsAnulled">Indica si la declaración pudo ser anulada</param>
        /// <returns>Resultado de la operacion</returns>
        public bool TryAnnulDeclaration(string rif, Guid declarationId, bool? annulOrder)
        {
            // Inicializamos las variables y obtenemos la data
            UDeclaration declaration = GetDeclaration(declarationId);
            int dStatus = declaration.IdUDeclarationStatus;

            // Cambiamos los estados de la declaracion segun el estado actual
            switch (dStatus)
            {
                // Validamos si la declaracion ya esta anulada
                case (int)DeclarationStatus.Annulled:
                    {
                        throw new DeclarationAnnulledException(BackEndErrors.DeclarationAnnulledExceptionMessage, BackEndErrors.DeclarationAnnulledExceptionCode);
                    }
                // Validamos si la declaracion esta conciliada
                case (int)DeclarationStatus.Reconciled:
                    {
                        throw new DeclarationReconciledException(BackEndErrors.DeclarationReconciledExceptionMessage, BackEndErrors.DeclarationReconciledExceptionCode);
                    }
                // Solo anulamos si la declaracion esta por conciliar
                case (int)DeclarationStatus.ReconciliationPending:
                    {
                        // Cambiamos la fecha y id del estado de la declaracion 
                        declaration.IdUDeclarationStatus = (int)DeclarationStatus.Annulled;
                        declaration.StatusChangeDate = DateTime.Now;
                        // Cambiamos la fecha y id del estado de la orden si no esta pendiente
                        if (declaration.CPurchaseOrder.IdCPurchaseOrderStatus != (int)PurchaseOrderStatus.DeclarationPending)
                        {
                            declaration.CPurchaseOrder.IdCPurchaseOrderStatus = (int)PurchaseOrderStatus.DeclarationPending;
                            declaration.CPurchaseOrder.StatusChangeDate = DateTime.Now;
                        }
                        if (annulOrder.Value)
                        {
                            declaration.CPurchaseOrder.IdCPurchaseOrderStatus = (int)PurchaseOrderStatus.Annulled;
                            declaration.CPurchaseOrder.StatusChangeDate = DateTime.Now;
                        }
                        break;
                    }
                default:
                    break;
            }

            // Guardamos los cambios en la base de datos
            SaveChanges();
            // Success
            return true;
        }

        /// <summary>
        /// Intenta conciliar una declaracion
        /// </summary>
        /// <param name="IdDeclaration">Id de la declaracion</param>
        /// <returns>Resultado de la operacion</returns>
        public bool TryReconcileDeclaration(Guid IdDeclaration, ReconciliationType type)
        {
            // Declaramos las variables            
            UBankStatementEntry entry = new UBankStatementEntry();
            CPurchaseOrder order;
            UDeclaration declaration = GetDeclaration(IdDeclaration);
            var entryList = new List<UBankStatementEntry>();
            UBankStatementEntryBLL UBSEBLL = new UBankStatementEntryBLL();
            string description;

            // Generamos la descripcion de la conciliacion
            switch (type)
            {
                case ReconciliationType.Automatic:
                    description = "Conciliación Automática ID: ";
                    break;
                case ReconciliationType.Manual:
                    description = "Conciliación Manual ID: ";
                    break;
                case ReconciliationType.RealTime:
                    description = "Conciliación Tiempo Real ID: ";
                    break;
                case ReconciliationType.Api:
                    description = "Conciliación Api ID: ";
                    break;
                default:
                    description = "Conciliación ID: ";
                    break;
            }


            try
            {
                if (declaration != null)
                {
                    // Verifico que la declaracion no este conciliada
                    if (declaration.IdUBankStatementEntry.HasValue)
                    {
                        throw new DeclarationReconciledException(BackEndErrors.DeclarationReconciledExceptionMessage, BackEndErrors.DeclarationReconciledExceptionMessage);
                    }
                    // Obtengo todos los movimientos que coincidan con el numero de referencia y banco declarado
                    entryList = UBSEBLL.GetAllRecords(ubse => (ubse.Description.Contains(declaration.Reference) || ubse.Ref.Contains(declaration.Reference)) &&
                                                                ubse.UBankStatement.IdUBank_Receiver == declaration.IdUBank).ToList();

                    // Verifico que existan movimientos
                    if (entryList.Count > 0)
                    {
                        // Obtengo los movimientos que coincidan con el monto declarado
                        entryList = entryList.Where(ubse => ubse.Amount == declaration.Amount).ToList();
                        // Verifico si existe mas de un movimiento
                        if (entryList.Count > 1)
                        {
                            throw new MultiplePossibleEntriesException(ReconciliatorServiceErrors.MultiplePossibleEntriesExceptionMessage, ReconciliatorServiceErrors.MultiplePossibleEntriesExceptionCode);
                        }
                        // Si no coincide el monto declarado con el monto en cuenta entonces se notificara al comercio que debe modificar la orden de compra
                        else if (entryList.Count == 0)
                        {
                            throw new StatementAmountException(ReconciliatorServiceErrors.StatementAmountExceptionMessage, ReconciliatorServiceErrors.StatementAmountExceptionCode);
                        }
                    }
                    // Si no hay movimientos que coincidan, retorno falso
                    else
                    {
                        return false;
                    }
                    // Validamos el numero de movimientos posibles
                    if (entryList.Count == 1)
                    {
                        // Agarramos el unico elemento de la lista
                        entry = entryList.FirstOrDefault();

                        // Verifico que el movimiento no tenga declaraciones asociadas previamente
                        if (entry.UDeclarations.Count == 0)
                        {
                            // Le asignamos el entry a la declaracion
                            declaration.IdUBankStatementEntry = entry.Id;
                            // Cambiamos el estado de la declaracion a concilaida
                            declaration.IdUDeclarationStatus = (int)DeclarationStatus.Reconciled;
                            // Obtenemos la orden de compra asociada a la declaracion
                            order = declaration.CPurchaseOrder;
                            // Cambiamos el estado de la orden de compra
                            order.IdCPurchaseOrderStatus = (int)PurchaseOrderStatus.DeclaredReconciled;
                            // Guardamos los cambios en la orden de compra en la base de datos
                            SaveChanges();

                            // Registramos la transaccion en el balance del comercio
                            using (var CBLL = new CommerceBLL())
                            {
                                // Obtenemos el comercio asociado a la declaracion
                                var commerce = CBLL.GetCommerce(declaration.Id);
                                // Agregamos el registro de la conciliacion al balance del comercio
                                using (var CBBLL = new CommerceBalanceBLL())
                                {
                                    CBBLL.AddBalance(description + declaration.Id + "Declara: " + declaration.EndUserCI, commerce.Rif, declaration.Amount, declaration.Id, CommerceBalanceType.Declaration);
                                }
                            }

                            // Verificamos si hay una solicitud asociada
                            if (order.PaymentRequests.Count > 0)
                            {
                                var request = order.PaymentRequests.FirstOrDefault();
                                // Cambiamos el estado de la solicitud
                                using (PaymentRequestBLL PRBLL = new PaymentRequestBLL())
                                {
                                    var paymentRequest = PRBLL.GetEntity(request.Id);
                                    paymentRequest.IdPaymentRequestStatus = (int)InstaTransfer.ITResources.Enums.PaymentRequestStatus.DeclaredReconciled;
                                    paymentRequest.ChangeDate = DateTime.Now;
                                    PRBLL.SaveChanges();
                                }
                            }
                            // Success
                            return true;
                        }
                        // Error - El movimiento ya tiene una declaracion asociada
                        else
                        {
                            throw new StatementDeclaredException(ReconciliatorServiceErrors.StatementDeclaredExceptionMessage, ReconciliatorServiceErrors.StatementDeclaredExceptionCode);
                        }
                    }
                    // Error - Multiples movimientos asociados
                    else if (entryList.Count > 1)
                    {
                        throw new MultiplePossibleEntriesException(ReconciliatorServiceErrors.MultiplePossibleEntriesExceptionMessage, ReconciliatorServiceErrors.MultiplePossibleEntriesExceptionCode);
                    }
                    // Error - No hay movimientos posibles
                    else if (entryList.Count == 0 || entryList.Equals(null))
                    {
                        return false;
                    }
                }
                else
                {
                    throw new DeclarationNotFoundException(BackEndErrors.DeclarationNotFoundExceptionMessage, BackEndErrors.DeclarationNotFoundExceptionCode);
                }
            }
            // Error - Exceptiones
            catch (DeclarationNotFoundException e)
            {
                return false;
            }
            catch (StatementDeclaredException e)
            {
                return false;
            }
            catch (MultiplePossibleEntriesException e)
            {
                return false;
            }
            catch (StatementAmountException e)
            {
                return false;
            }
            catch (Exception e)
            {
                return false;
            }
            // Error - llegamos a este punto
            return false;
        }

        #endregion
    }
}
