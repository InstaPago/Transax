using InstaTransfer.ITResources.General;
using System;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;

namespace InstaTransfer.BLL.Concrete
{
    public class Repository<T> : IDisposable 
        where T : class
    {
        public static string _connectionString = ConfigurationManager.ConnectionStrings[GeneralResources.ConnectionStringKey].ConnectionString;
        public DataAccess.DataAccessDataContext db= new DataAccess.DataAccessDataContext(_connectionString);

        /// <summary>
        /// obtiene todos los registros de la tabla T
        /// </summary>
        /// <returns></returns>
        public IQueryable<T> GetAllRecords()
        {
            return db.GetTable<T>();
        }

        /// <summary>
        /// obtiene todos los registros de la tabla T que cumplan una condicion
        /// </summary>
        /// <param name="predicate">predicado a cumplir</param>
        /// <returns></returns>
        public IQueryable<T> GetAllRecords(Expression<Func<T, bool>> predicate)
        {
            return db.GetTable<T>().Where(predicate);
        }

        /// <summary>
        /// obtiene un registro de la tabla T
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T GetEntity(int id)
        {
            var table = GetAllRecords();
            var mapping = db.Mapping.GetTable(typeof(T));
            var pkfield = mapping.RowType.DataMembers.SingleOrDefault(d => d.IsPrimaryKey);
            if (pkfield == null)
                throw new Exception(String.Format("Table {0} does not contain entity Primary Key field", mapping.TableName));
            var param = Expression.Parameter(typeof(T), "e");
            var predicate = Expression.Lambda<Func<T, bool>>(Expression.Equal(Expression.Property(param, pkfield.Name), Expression.Constant(id)), param);
            return table.SingleOrDefault(predicate);
        }

        /// <summary>
        /// obtiene un registro de la tabla T
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T GetEntity(Guid id)
        {
            var table = GetAllRecords();
            var mapping = db.Mapping.GetTable(typeof(T));
            var pkfield = mapping.RowType.DataMembers.SingleOrDefault(d => d.IsPrimaryKey);
            if (pkfield == null)
                throw new Exception(String.Format("Table {0} does not contain entity Primary Key field", mapping.TableName));
            var param = Expression.Parameter(typeof(T), "e");
            var predicate = Expression.Lambda<Func<T, bool>>(Expression.Equal(Expression.Property(param, pkfield.Name), Expression.Constant(id)), param);
            return table.SingleOrDefault(predicate);
        }

        /// <summary>
        /// elimina los registros de la tabla T que cumplan una condicion
        /// </summary>
        /// <param name="predicate"></param>
        public void DeleteGroup(Expression<Func<T, bool>> predicate)
        {
            var __entidades = GetAllRecords().Where(predicate);
            db.GetTable<T>().DeleteAllOnSubmit(__entidades);
        }

        /// <summary>
        /// Submit
        /// </summary>
        public void SaveChanges()
        {
            db.SubmitChanges();
        }

        /// <summary>
        /// agrega un registro a la tabla T
        /// </summary>
        /// <param name="entidad"></param>
        public void AddEntity(T entidad)
        {
            db.GetTable<T>().InsertOnSubmit(entidad);
        }

        /// <summary>
        /// elimina un registro de la tabla T
        /// </summary>
        /// <param name="id"></param>
        public void DeleteT(T entidad)
        {
            db.GetTable<T>().DeleteOnSubmit(entidad);
        }

        public void DeleteEntity(int id)
        {
            var entidad = this.GetEntity(id);
            db.GetTable<T>().DeleteOnSubmit(entidad);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Repository() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion



    }
}
