using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Data.SqlClient;
using System.Data;
using System.Data.Linq.Mapping;
using InstaTransfer.DataAccess;
using System.Configuration;
using InstaTransfer.ITResources.General;
using System.Data.Linq;
using InstaTransfer.ITResources.Enums;

namespace InstaTransfer.BLL.Concrete
{
    public class URepository<T> : Repository<T>
        where T : class
    {
        #region Constructor

        /// <summary>
        /// Constructor para configurar la conexion de la base de datos desde un archivo por parametro.
        /// </summary>
        /// <param name="isAppConfigFile">True - Lectura externa del archivo de configuracion.</param>
        public URepository(bool isAppConfigFile)
        {
            ConfigurationFileMap fileMap;
            Configuration configuration;

            switch (isAppConfigFile)
            {
                case true:
                    {
                        fileMap = new ConfigurationFileMap(GeneralResources.AppConfigPath);
                        configuration = ConfigurationManager.OpenMappedMachineConfiguration(fileMap);
                        _connectionString = ConfigurationManager.ConnectionStrings[GeneralResources.ConnectionStringKey].ConnectionString;
                    }
                    break;
                case false:
                    break;
            }
            db = new DataAccess.DataAccessDataContext(_connectionString);
        }

        /// <summary>
        /// Constructor vacio para la configuracion de la conexion a la base de datos.
        /// </summary>
        public URepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings[GeneralResources.ConnectionStringKey].ConnectionString;
            db = new DataAccess.DataAccessDataContext(_connectionString);
        }

        #endregion

        #region Methods

        #region General

        /// <summary>
        /// Inserta una lista de entidades a la base de datos usando el metodo <see cref="SqlBulkCopy"/>
        /// </summary>
        /// <typeparam name="T">Entidad generica</typeparam>
        /// <param name="entities">Lista de entidades a insertar</param>
        public void BulkInsertAll<T>(IEnumerable<T> entities)
        {
            // Convertimos la lista de entidades a un arreglo
            entities = entities.ToArray();
            // Obtenemos el tipo de dato del parametro generico T
            Type t = typeof(T);
            // Obtenemos el tipo de atributo de la entidad
            var tableAttribute = (TableAttribute)t.GetCustomAttributes(typeof(TableAttribute), false).Single();
            // Creamos una nueva instancia de la conexion sql
            using (SqlConnection targetConnection = new SqlConnection(_connectionString))
            {
                // Abrimos la conexion sql
                targetConnection.Open();
                // Instanciamos la clase SqlBulkCopy
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(targetConnection) { DestinationTableName = tableAttribute.Name })
                {
                    // Obtenemos las propiedadas de la entidad
                    var properties = t.GetProperties().Where(EventTypeFilter).ToArray();
                    // Instanciamos un DataTable donde almacenar la data y la estructura
                    var table = new DataTable();
                    // Limpiamos el mapeo
                    bulkCopy.ColumnMappings.Clear();
                    // Recorremos cada propiedad de la entidad
                    foreach (var property in properties)
                    {
                        // Obtenemos el tipo de la propiedad
                        Type propertyType = property.PropertyType;
                        // Verificamos si el tipo de la propiedad es generica y nulleable
                        if (propertyType.IsGenericType &&
                            propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            // Obtenemos el tipo de dato que es nulleable
                            propertyType = Nullable.GetUnderlyingType(propertyType);
                        }
                        // Verificamos si el tipo de propiedad es generica y EntitySet
                        if (propertyType.IsGenericType &&
                            propertyType.GetGenericTypeDefinition() == typeof(EntitySet<>))
                        {
                            // Si es un EntitySet saltamos el valor
                            continue;
                        }
                        // Agregamos la propiedad a una nueva columna del Datatable
                        table.Columns.Add(new DataColumn(property.Name, propertyType));
                        // Agregamos el mapeo de la propiedad
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(property.Name, property.Name));
                    }
                    // Recorremos cada entidad en la lista pasada por parametros
                    foreach (var entity in entities)
                    {
                        var e = entity;
                        // Obtenemos el set de valores para las propiedades de la entidad
                        var values = properties.Select(property => GetPropertyValue(property.GetValue(e, null)));
                        // Creamos una lista donde se almacenaran los nuevos valores
                        var newValues = new List<object>(values);
                        // Recorremos cada valor de la lista de valores
                        foreach (var set in values)
                        {
                            // Si el valor es de tipo generico y EntitySet, removemos el valor de la lista
                            if (set.GetType().IsGenericType && set.GetType().GetGenericTypeDefinition() == typeof(EntitySet<>))
                            {
                                newValues.Remove(set);
                            }
                        }
                        // Añadimos la lista de valores a una nueva fila del DataTable
                        table.Rows.Add(newValues.ToArray());
                    }
                    // Copiamos el DataTable a la tabla de base de datos
                    bulkCopy.WriteToServer(table);
                }
            }
        }

        /// <summary>
        /// Filtra las llaves foraneas para una propiedad especifica
        /// </summary>
        /// <param name="p">Propiedad a filtrar</param>
        /// <returns>True - No es llave foranea. False - Es llave foranea.</returns>
        private bool EventTypeFilter(PropertyInfo p)
        {
            var attribute = Attribute.GetCustomAttribute(p, typeof(AssociationAttribute)) as AssociationAttribute;

            if (attribute == null) return true;
            if (attribute.IsForeignKey == false) return true;

            return false;
        }

        /// <summary>
        /// Verifica el valor de la propiedad. Si es null devuelve DBNull
        /// </summary>
        /// <param name="o">Valor de la propiedad</param>
        /// <returns>Valor verificado.</returns>
        private object GetPropertyValue(object o)
        {
            if (o == null)
            {
                return DBNull.Value;
            }
            return o;
        }

        #endregion

        #region InstaTransfer

        #region Commerce

        /// <summary>
        /// Retorna el comercio desde un rif especifico
        /// </summary>
        /// <param name="rif">Rif del comercio especifico</param>
        /// <returns><see cref="Commerce"/> especifico</returns>
        public Commerce GetCommerce(string rif)
        {
            return db.GetTable<Commerce>().Where(c => c.Rif.Equals(rif)).FirstOrDefault();
        }

        /// <summary>
        /// Retorna los comercios en la base de datos
        /// </summary>
        /// <returns>Lista de <see cref="Commerce"/></returns>
        public IQueryable<Commerce> GetCommerces()
        {
            return db.GetTable<Commerce>();
        }

        #endregion

        #region CUser
        /// <summary>
        /// Retorna el usuario del comercio asociado al aspnet user logeado actualmente
        /// </summary>
        /// <param name="userID">Id del usuario asociado al usuario del comercio</param>
        /// <returns>El <see cref="CUser"/> del comercio asociado</returns>
        public CUser GetCUser(string userID)
        {
            return db.GetTable<CUser>().Where(cu => cu.IdAspNetUser == userID).FirstOrDefault();
        }

        /// <summary>
        /// Retorna el usuario del comercio asociado a un id especifico
        /// </summary>
        /// <param name="cUserID">Id del usuario especifico</param>
        /// <returns>El <see cref="CUser"/> especifico</returns>
        public CUser GetCUser(Guid cUserID)
        {
            return db.GetTable<CUser>().Where(cu => cu.Id == cUserID).FirstOrDefault();
        }



        #endregion

        #region UUser
        /// <summary>
        /// Retorna todos los usuarios activos del sistema
        /// </summary>
        /// <returns>Lista de <see cref="UUser"/> activos.</returns>
        public IQueryable<UUser> GetUniqueUmbrellaUsers()
        {
            return db.GetTable<UUser>().Where(u => u.IdUserStatus.Equals(1)).Distinct();
        }

        /// <summary>
        /// Retorna los usuarios asociados a un banco y razon social especifico
        /// </summary>
        /// <param name="bankId">Id del banco asociado al usuario</param>
        /// <param name="socialReasonId">Id de la razon social asociada al usuario</param>
        /// <returns>Lista de <see cref="UUser"/> asociados a los parametros especificados.</returns>
        public UUser ChangeUmbrellaUserStatus(string username, string bankId, int status)
        {
            UUser updatedUser = db.GetTable<UUser>().Where(t => t.Username == username && t.IdUBank == bankId).FirstOrDefault();
            updatedUser.IdUserStatus = status;
            updatedUser.StatusChangeDate = DateTime.Now;
            SaveChanges();
            return updatedUser;
        }

        /// <summary>
        /// Retorna los usuarios asociados a un banco y razon social especifico
        /// </summary>
        /// <param name="bankId">Id del banco asociado al usuario</param>
        /// <param name="socialReasonId">Id de la razon social asociada al usuario</param>
        /// <returns>Lista de <see cref="UUser"/> asociados a los parametros especificados.</returns>
        public IQueryable<UUser> GetAllActiveUmbrellaUsers(string bankId, string socialReasonId)
        {
            return db.GetTable<UUser>().Where(u => u.IdUBank.Equals(bankId) &&
                                              u.IdUSocialReason.Equals(socialReasonId) &&
                                              u.IdUserStatus.Equals(1));
        }

        /// <summary>
        /// Retorna todos los usuarios activos del sistema
        /// </summary>
        /// <returns>Lista de <see cref="UUser"/> activos.</returns>
        public IQueryable<UUser> GetAllActiveUmbrellaUsers()
        {
            return db.GetTable<UUser>().Where(u => u.IdUserStatus.Equals(1));
        }

        #endregion

        #region UBankStatementEntry
        /// <summary>
        /// Retorna todos movimientos positivos
        /// </summary>
        /// <returns>Lista de <see cref="UBankStatementEntry"/> positivos.</returns>
        public IQueryable<UBankStatementEntry> GetCredits()
        {
            return db.GetTable<UBankStatementEntry>().Where(u => u.Amount > 0);
        }

        /// <summary>
        /// Retorna todos movimientos positivos de un comercio especifico
        /// </summary>
        /// <param name="rif">Rif del comercio</param>
        /// <returns>Lista de <see cref="UBankStatementEntry"/> positivos.</returns>
        public IQueryable<UBankStatementEntry> GetCredits(string rif)
        {
            return db.GetTable<UBankStatementEntry>().Where(ubse => ubse.Amount > 0 && ubse.UBankStatement.USocialReason.Id == rif);
        }

        /// <summary>
        /// Retorna todos los movimientos negativos
        /// </summary>
        /// <returns>Lista de <see cref="UBankStatementEntry"/> negativos.</returns>
        public IQueryable<UBankStatementEntry> GetDebits()
        {
            return db.GetTable<UBankStatementEntry>().Where(ubse => ubse.Amount < 0);
        }

        #endregion

        #endregion

        #endregion
    }
}
