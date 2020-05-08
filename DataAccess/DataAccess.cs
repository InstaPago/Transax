using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITResources.General;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace InstaTransfer.DataAccess
{
    partial class CPurchaseOrder
    {
        /// <summary>
        /// Constructor de la orden de compra a partir de un PurchaseOrderModel
        /// </summary>
        /// <param name="model">PurchaseOrderModel recibido desde el api</param>
        public CPurchaseOrder(object model):this()
        {
            Type type = model.GetType();
            IList<PropertyInfo> props = new List<PropertyInfo>(type.GetProperties());

            foreach (PropertyInfo prop in props)
            {
                switch (prop.Name)
                {
                    case "amount":
                        {
                            Amount = (decimal)prop.GetValue(model);
                            break;
                        }
                    case "paymentuser":
                        {
                            EndUserCI = (int)prop.PropertyType.GetProperty("userci").GetValue(prop.GetValue(model));
                            EndUserEmail = (string)prop.PropertyType.GetProperty("useremail").GetValue(prop.GetValue(model));
                            break;
                        }
                    case "ordernumber":
                        {
                            OrderNumber = (string)prop.GetValue(model);
                            break;
                        }
                    case "id":
                        {
                            Id = (Guid)prop.GetValue(model);
                            break;
                        }
                    default:
                        break;
                }
            }
            CreateDate = DateTime.Now;
            StatusChangeDate = DateTime.Now;
            IdCPurchaseOrderStatus = (int)PurchaseOrderStatus.DeclarationPending;
        }
    }


    partial class UDeclaration
    {
        /// <summary>
        /// Constructor de la declaracion a partir de un DeclarationModel
        /// </summary>
        /// <param name="model">DeclarationModel recibido desde el api</param>
        public UDeclaration(object model)
        {
            Type type = model.GetType();
            IList<PropertyInfo> props = new List<PropertyInfo>(type.GetProperties());

            foreach (PropertyInfo prop in props)
            {
                switch (prop.Name)
                {
                    case "idissuingbank":
                        {
                            IdUBank = (string)prop.GetValue(model);
                            break;
                        }
                    case "transactiondate":
                        {
                            TransactionDate = DateTime.ParseExact((string)prop.GetValue(model), GeneralResources.ShortDateFormat, CultureInfo.GetCultureInfo("es-VE"));
                            break;
                        }
                    case "idoperationtype":
                        {
                            IdUOperationType = (int)prop.GetValue(model);
                            break;
                        }
                    case "referencenumber":
                        {
                            Reference = (string)prop.GetValue(model);
                            break;
                        }
                    case "amount":
                        {
                            Amount = (decimal)prop.GetValue(model);
                            break;
                        }
                    case "paymentuser":
                        {
                            EndUserCI = (int)prop.PropertyType.GetProperty("userci").GetValue(prop.GetValue(model));
                            EndUserEmail = (string)prop.PropertyType.GetProperty("useremail").GetValue(prop.GetValue(model));
                            break;
                        }
                    case "id":
                        {
                            Id = (Guid)prop.GetValue(model);
                            break;
                        }
                    default:
                        break;
                }
            }
            CreateDate = DateTime.Now;
            StatusChangeDate = DateTime.Now;
            IdUDeclarationStatus = (int)DeclarationStatus.ReconciliationPending;
        }
    }

    partial class DataAccessDataContext
    {

    }
}