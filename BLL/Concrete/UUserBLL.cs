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
    /// BLL de los movimientos bancarios del sistema
    /// </summary>
    public class UUserBLL : Repository<UUser>
    {
        /// <summary>
        /// Cambia el estado del usuario
        /// </summary>
        /// <param name="currentUser">Usuario actual</param>
        /// <param name="status">Estado a cambiar</param>
        /// <returns>Usuario actualizado</returns>
        public UUser ChangeUserStatus(UUser currentUser, UmbrellaUserStatus status)
        {
            // Obtenemos el usuario
            UUser updatedUser = GetUser(currentUser.Username, currentUser.IdUBank);

            // Cambiamos el estado
            updatedUser.IdUserStatus = (int)status;
            updatedUser.StatusChangeDate = DateTime.Now;

            //Guardamos los cambios
            SaveChanges();

            // Devolvemos el usuario actualizado
            return updatedUser;
        }

        /// <summary>
        /// Obtiene el usuario de la banca en linea a partir del username y id del banco
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="idBank">Id del banco asociado</param>
        /// <returns>Usuario de banca en linea</returns>
        public UUser GetUser(string username, string idBank)
        {
            return db.GetTable<UUser>().Where(t => t.Username == username && t.IdUBank == idBank).FirstOrDefault();
        }

        /// <summary>
        /// Retorna la lista de usuarios por estado
        /// </summary>
        /// <param name="status">Estado especifico</param>
        /// <returns>Lista de usuarios</returns>
        public List<UUser> GetUsers(UmbrellaUserStatus status)
        {
            return db.GetTable<UUser>().Where(u => u.IdUserStatus.Equals((int)status)).ToList();
        }

        /// <summary>
        /// Retorna la lista de usuarios por estado y tipo
        /// </summary>
        /// <param name="status">Estado especifico</param>
        /// <param name="type">Tipo de usuario</param>
        /// <returns>Lista de usuarios</returns>
        public List<UUser> GetUsers(UmbrellaUserStatus status, UserType type)
        {
            return db.GetTable<UUser>().Where(u => u.IdUserStatus.Equals((int)status) && u.IdUUserType.Equals((int)type)).ToList();
        }


        /// <summary>
        /// Retorna el numero de cuenta bancaria asociada a un usuario
        /// </summary>
        /// <param name="currentUser">Usuario actual</param>
        /// <param name="bankId">Id del banco</param>
        /// <returns>Numero de cuenta</returns>
        public string GetBankAccount(UUser currentUser, string bankId)
        {
            return currentUser.USocialReason.UBankAccounts.Where(ba => ba.IdUBank_Receiver == bankId).FirstOrDefault().AccountNumber;
        }

        /// <summary>
        /// Escribe el error en el inicio de sesion de la plataforma bancaria
        /// </summary>
        /// <param name="request">Solicitud de estado de cuenta</param>
        /// <param name="error">Error en el inicio de sesion</param>
        /// <returns>Resultado de la operacion</returns>
        public bool WriteError(UUser user, string errorMessage, string errorCode)
        {
            UUser currentUser = GetUser(user.Username, user.IdUBank);
            try
            {
                // Cambiamos el estatus segun el codigo de error
                switch (errorCode)
                {
                    case BanescOnlineErrorConstant.UserBlocked:
                        {
                            currentUser.IdUserStatus = (int)UmbrellaUserStatus.Blocked;
                            break;
                        }
                    case BanescOnlineErrorConstant.UserJustBlocked:
                        {
                            currentUser.IdUserStatus = (int)UmbrellaUserStatus.Blocked;
                            break;
                        }
                    case BanescOnlineErrorConstant.UserPasswordExpired:
                        {
                            currentUser.IdUserStatus = (int)UmbrellaUserStatus.PasswordExpired;
                            break;
                        }
                    case BanescOnlineErrorConstant.UserInUse:
                        {
                            currentUser.IdUserStatus = (int)UmbrellaUserStatus.InUse;
                            break;
                        }
                    case BanescOnlineErrorConstant.NavigationCanceled:
                        {
                            currentUser.IdUserStatus = (int)UmbrellaUserStatus.Active;
                            break;
                        }
                    case BanescOnlineErrorConstant.GeneralError:
                        {
                            currentUser.IdUserStatus = (int)UmbrellaUserStatus.PlatformError;
                            break;
                        }
                    case BanescOnlineErrorConstant.UnexpectedError:
                        {
                            currentUser.IdUserStatus = (int)UmbrellaUserStatus.PlatformError;
                            break;
                        }
                    default:
                        {
                            currentUser.IdUserStatus = (int)UmbrellaUserStatus.PlatformError;
                        };
                        break;
                }
                // Guardamos los cambios
                SaveChanges();

                return true;
            }
            // Error
            catch (Exception)
            {
                return false;
            }

        }
    }
}

