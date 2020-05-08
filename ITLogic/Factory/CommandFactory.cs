using InstaTransfer.DataAccess;
using InstaTransfer.ITLogic.Commands.Api;
using InstaTransfer.ITLogic.Commands.General;
using InstaTransfer.ITLogic.Commands.Scraper.Banesco;
using InstaTransfer.ITLogic.Commands.Scraper.Provincial;
using InstaTransfer.ITLogic.Commands.Updater;
using InstaTransfer.ITLogic.Commands.Updater.Banesco;
using InstaTransfer.ITLogic.Commands.Updater.Provincial;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITLogic.Helpers;
using InstaTransfer.ITLogic.Commands.Scraper.Banesco2;

namespace InstaTransfer.ITLogic.Factory
{
    /// <summary>
    /// Fabrica que genera los comandos del sistema
    /// </summary>
    public class CommandFactory
    {
        #region Api

        /// <summary>
        /// Metodo de la fabrica para el comando <see cref="CommandCreateDeclaration"/>
        /// </summary>
        /// <param name="receiver">Declaracion</param>
        /// <returns>Comando <see cref="CommandCreateDeclaration"/></returns>
        public static Command GetCommandCreateDeclaration(object receiver)
        {
            return new CommandCreateDeclaration(receiver);
        }

        /// <summary>
        /// Metodo de la fabrica para el comando <see cref="CommandGetAllBankAccounts"/>
        /// </summary>
        /// <returns>Comando <see cref="CommandGetAllBankAccounts"/></returns>
        public static Command GetCommandGetAllBankAccounts()
        {
            return new CommandGetAllBankAccounts();
        }

        /// <summary>
        /// Metodo de la fabrica para el comando <see cref="CommandGetBankAccounts"/>
        /// </summary>
        /// <returns>Comando <see cref="CommandGetBankAccounts"/></returns>
        public static Command GetCommandGetBankAccounts()
        {
            return new CommandGetBankAccounts();
        }

        #endregion

        #region General

        /// <summary>
        /// Metodo de la fabrica para el comando <see cref="CommandChangeUserStatus"/>
        /// </summary>
        /// <param name="receiver">Usuario a modificar</param>
        /// <returns>Comando <see cref="CommandChangeUserStatus"/></returns>
        public static Command GetCommandChangeUserStatus(object receiver)
        {
            return new CommandChangeUserStatus(receiver);
        }

        /// <summary>
        /// Metodo de la fabrica para el comando <see cref="CommandGetUser"/>
        /// </summary>
        /// <returns>Comando <see cref="CommandGetUser"/></returns>
        public static Command GetCommandGetUser()
        {
            return new CommandGetUser();
        }

        /// <summary>
        /// Metodo de la fabrica para el comando <see cref="CommandGetAllUsers"/>
        /// </summary>
        /// <returns>Comando <see cref="CommandGetAllUser"/></returns>
        public static Command GetCommandGetAllUsers()
        {
            return new CommandGetAllUsers();
        }

        /// <summary>
        /// Metodo de la fabrica para el comando <see cref="CommandGetAllEntries"/>
        /// </summary>
        /// <returns>Comando <see cref="CommandGetAllEntries"/></returns>
        public static Command GetCommandGetAllEntries()
        {
            return new CommandGetAllEntries();
        }

        /// <summary>
        /// Metodo de la fabrica para el comando <see cref="CommandGetAllSocialReasons"/>
        /// </summary>
        /// <returns>Comando <see cref="CommandGetAllSocialReasons"/></returns>
        public static Command GetCommandGetAllSocialReasons()
        {
            return new CommandGetAllSocialReasons();
        }

        #endregion

        #region Scraper

        /// <summary>
        /// Metodo de la fabrica para el comando <see cref="CommandScraperLogin"/>
        /// </summary>
        /// <param name="receiver"><see cref="UUser"/> de la plataforma bancaria</param>
        /// <returns>Comando <see cref="CommandScraperLogin"/></returns>
        public static Command GetCommandScraperLogin(object receiver)
        {
            Bank bank = GeneralHelper.GetBankEnum(((UUser)receiver).IdUBank);
            switch (bank)
            {
                case Bank.Banesco:
                    {
                        /// BanescOnline antiguo.
                        /// return new CommandBanescoScraperLogin(receiver);
                        return new CommandBanesco2ScraperLogin(receiver);
                    }
                case Bank.Provincial:
                    {
                        return new CommandProvincialScraperLogin(receiver);
                    }
                default:
                    {
                        throw new System.Exception();
                    }
            }
        }

        /// <summary>
        /// Metodo de la fabrica para el comando <see cref="CommandScraperLogout"/>
        /// </summary>
        /// <param name="receiver"><see cref="UUser"/> de la plataforma bancaria</param>
        /// <returns>Comando <see cref="CommandScraperLogout"/></returns>
        public static Command GetCommandScraperLogout(object receiver)
        {
            Bank bank = GeneralHelper.GetBankEnum(((UUser)receiver).IdUBank);
            switch (bank)
            {
                case Bank.Banesco:
                    {
                        /// BanescOnline antiguo.
                        /// return new CommandBanescoScraperLogout(receiver);
                        return new CommandBanesco2ScraperLogout(receiver);
                    }
                case Bank.Provincial:
                    {
                        return new CommandProvincialScraperLogout(receiver);
                    }
                default:
                    {
                        throw new System.Exception();
                    }
            }
        }

        #endregion

        #region Updater

        /// <summary>
        /// Metodo de la fabrica para el comando <see cref="CommandCreateBankStatement"/>
        /// </summary>
        /// <param name="receiver">Id del banco</param>
        /// <returns>Comando <see cref="CommandCreateBankStatement"/></returns>
        public static Command GetCommandCreateBankStatement(object receiver)
        {
            return new CommandCreateBankStatement(receiver);
        }

        /// <summary>
        /// Metodo de la fabrica para el comando <see cref="CommandCreateBankStatementEntries"/>
        /// </summary>
        /// <param name="receiver"><see cref="UBankStatement"/> a asociar con la lista de entradas</param>
        /// <returns>Comando <see cref="CommandCreateBankStatementEntries"/></returns>
        public static Command GetCommandCreateEntries(object receiver)
        {
            switch ((Bank)receiver)
            {
                case Bank.Banesco:
                    {
                        return new CommandCreateBanescoEntries(receiver);
                    }
                case Bank.Provincial:
                    {
                        return new CommandCreateProvincialEntries(receiver);
                    }
                default:
                    {
                        return new CommandCreateEntries(receiver);
                    }
            }
        }

        /// <summary>
        /// Metodo de la fabrica para el comando <see cref="CommandSaveBankStatement"/>
        /// </summary>
        /// <param name="receiver"><see cref="UBankStatement"/> a guardar en base de datos</param>
        /// <returns>Comando <see cref="CommandSaveBankStatement"/></returns>
        public static Command GetCommandSaveBankStatement(object receiver)
        {
            return new CommandSaveBankStatement(receiver);
        }

        /// <summary>
        /// Metodo de la fabrica para el comando <see cref="CommandSaveEntries"/>
        /// </summary>
        /// <param name="receiver">Lista de <see cref="UBankStatementEntry"/> a guardar en base de datos</param>
        /// <returns>Comando <see cref="CommandSaveEntries"/></returns>
        public static Command GetCommandSaveEntries(object receiver)
        {
            return new CommandSaveEntries(receiver);
        }

        /// <summary>
        /// Metodo de la fabrica para el comando <see cref="CommandUpdateFiles"/>
        /// </summary>
        /// <param name="receiver">Directorio base para la verificacion de archivos</param>
        /// <returns>Lista de rutas de los archivos a procesar</returns>
        public static Command GetCommandUpdateFiles(object receiver)
        {
            return new CommandUpdateFiles(receiver);
        }
        #endregion

    }
}
