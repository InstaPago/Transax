using InstaTransfer.BLL.Concrete;
using InstaTransfer.DataAccess;
using InstaTransfer.ITResources.Constants;
using InstaTransfer.ITResources.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Concrete
{
    /// <summary>
    /// BLL de los archivos de cargo en cuenta
    /// </summary>
    public class AE_ArchivoBLL : Repository<AE_Archivo>
    {

        /// <summary>
        /// Obtiene los archivos de cargo en cuenta segun los estados especificos
        /// </summary>
        /// <param name="statusList">La lista de estados</param>
        /// <returns>Lista de archivos de cargo en cuenta</returns>
        public List<AE_Archivo> GetFilesByStatus(List<ChargeAccountStatus> statusList)
        {
            // Variables
            List<AE_Archivo> files = new List<AE_Archivo>();

            // Recorremos la lista de estados
            foreach (var status in statusList)
            {
                // Devolvemos los archivos con el estado actual
                files.AddRange(GetFilesByStatus(status));
            }

            // Devolvemos todos los archivos encontrados
            return files;
        }

        /// <summary>
        /// Obtiene los archivos de cargo en cuenta segun el estado especifico
        /// </summary>
        /// <returns>Lista de archivos de cargo en cuenta</returns>
        public List<AE_Archivo> GetFilesByStatus(ChargeAccountStatus status)
        {
            // Variables
            List<AE_Archivo> fileList = new List<AE_Archivo>();

            // Evaluamos el estado
            switch (status)
            {
                // Evaluamos el caso especial de los documentos aceptados
                case ChargeAccountStatus.Accepted:
                    {
                        // Buscamos los archivo cuyo ultimo cambio de estado haya sido hace menos de un dia
                        fileList.AddRange(GetAllRecords(ae => ae.IdAE_ArchivosStatus == (int)status && ae.StatusChangeDate.Day >= DateTime.Now.AddDays(-1).Day).ToList());
                        // Retornamos la lista de archivos
                        return fileList;
                    }
                default:
                    {
                        // Buscamos todos los archivos con el estado especificado
                        fileList.AddRange(GetAllRecords(ae => ae.IdAE_ArchivosStatus == (int)status).ToList());
                        // Retornamos la lista de archivos
                        return fileList;
                    }
            }
        }


        /// <summary>
        /// Obtiene los archivos pendientes por uploadear
        /// </summary>
        /// <returns></returns>
        public List<AE_Archivo> GetPendingFiles()
        {
            return GetAllRecords(ae => ae.IdAE_ArchivosStatus == (int)ChargeAccountStatus.Pending).ToList();
        }

        /// <summary>
        /// Obtiene los archivos cargados o aceptados en PE
        /// </summary>
        /// <returns>Lista de archivos cargados</returns>
        public List<AE_Archivo> GetUploadedOrAcceptedFiles()
        {
            return GetAllRecords(ae => ae.IdAE_ArchivosStatus == (int)ChargeAccountStatus.Uploaded || ae.IdAE_ArchivosStatus == (int)ChargeAccountStatus.Accepted).ToList();
        }

        /// <summary>
        /// Cambia el estado del archivo
        /// </summary>
        /// <param name="currentChargeAccount">Archivo actual</param>
        /// <param name="status">Estado a cambiar</param>
        /// <returns>Archivo actualizado</returns>
        public AE_Archivo ChangeFileStatus(AE_Archivo currentChargeAccount, ChargeAccountStatus status)
        {
            // Obtenemos el archivo
            AE_Archivo updatedFile = GetEntity(currentChargeAccount.Id);

            // Verificamos si el estado sigue igual
            if (updatedFile.IdAE_ArchivosStatus == (int)status)
            {
                // Do nothing
            }
            else
            {
                // Cambiamos el estado
                currentChargeAccount.IdAE_ArchivosStatus = (int)status;
                currentChargeAccount.StatusChangeDate = DateTime.Now;

                //Guardamos los cambios
                SaveChanges();
            }
            // Devolvemos el usuario actualizado
            return updatedFile;
        }
    }
}
