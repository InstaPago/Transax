using InstaTransfer.DataAccess;
using InstaTransfer.ITExceptions.Updater;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITLogic.Factory;
using InstaTransfer.ITLogic.Helpers;
using InstaTransfer.ITLogic.Log;
using InstaTransfer.ITResources.General;
using InstaTransfer.ITResources.Updater;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace InstaTransfer.ITLogic.Commands.Updater
{
    public class CommandUpdateFiles : Command
    {

        /// <summary>
        /// Procesa los archivos nuevos
        /// </summary>
        /// <param name="receiver">Ruta base de los archivos a procesar</param>
        public CommandUpdateFiles(Object receiver) : base(receiver) { }

        public override void Execute()
        {
            UBankStatement _uBankStatement;
            Command commandCreateBankStatement;
            Command commandCreateEntries;
            int fileCount = 0;
            string newFile = string.Empty;

            bool fileUpdated;

            try
            {
                Dictionary<string, object> parameter = (Dictionary<string, object>)Parameter;
                string basePath = (string)Receiver;
                string backupPath = (string)parameter[UpdaterResources.DictionaryKeyBackupPath];

                Console.WriteLine("Verificando archivos...");

                //Obtenemos todos los archivos en todos los subdirectorios de Umbrella
                var fileNameList = Directory.GetFiles(basePath, "*", searchOption: SearchOption.AllDirectories);

                //Verificamos si la lista de nombres esta vacia
                if (fileNameList.Any())
                {
                    //Recorremos cada nombre de archivo en la lista
                    foreach (var file in fileNameList)
                    {
                        // Guardamos la ruta del archivo a procesar
                        newFile = file;
                        //Separamos la informacion de la ruta en un arreglo
                        string[] userData = file.Substring(basePath.Length + 1).Split('\\');
                        //Guardamos los elementos de la ruta en su variable respectiva
                        string socialReason = userData[0];
                        Bank bank = GeneralHelper.GetBankEnum(userData[1]);
                        string fileName = userData[2];
                        //Obtenemos el nombre del archivo sin su extensión. Será usado como la fecha del UBS.
                        string simpleFileName = Path.GetFileNameWithoutExtension(file);
                        //Construimos la ruta de respaldo del archivo a traves de la ruta de origen
                        string destPath = GeneralHelper.BuildFilePathString(backupPath, userData);

                        Console.WriteLine("Iniciando procesos...");

                        //Creamos y almacenamos el UBS en base de datos
                        commandCreateBankStatement = CommandFactory.GetCommandCreateBankStatement(bank);
                        commandCreateBankStatement.Parameter = new Dictionary<string, object>
                            {
                                { UpdaterResources.DictionaryKeyStatementDate, simpleFileName },
                                { GeneralResources.DictionaryKeySocialReason,  socialReason   }
                            };
                        commandCreateBankStatement.Execute();
                        //Asignamos el UBS creado a una variable
                        _uBankStatement = (UBankStatement)commandCreateBankStatement.Receiver;

                        //Creamos y almacenamos la lista de UBSE asociado al UBS generado
                        commandCreateEntries = CommandFactory.GetCommandCreateEntries(bank);
                        commandCreateEntries.Parameter = new Dictionary<string, object>
                            {
                                { UpdaterResources.DictionaryKeyBankStatement, _uBankStatement },
                                { UpdaterResources.DictionaryKeyFullPath, file }
                            };
                        commandCreateEntries.Execute();
                        fileUpdated = (bool)commandCreateEntries.Receiver;

                        //Movemos el archivo si fue procesado exitosamente
                        if (fileUpdated)
                        {
                            Console.WriteLine("Respaldando archivos...");
                            Console.WriteLine();
                            if (File.Exists(destPath))
                            {
                                File.Delete(destPath);
                            }
                            File.Move(file, destPath);
                            fileCount++;
                        }
                    }
                    Logger.WriteSuccessLog("Actualización finalizada. " + fileCount + " archivo(s) procesado(s).", MethodBase.GetCurrentMethod().DeclaringType.Name);
                }
                else
                {
                    //Console.WriteLine("Actualización finalizada");
                    Logger.WriteSuccessLog("Actualización finalizada. No hay archivos pendientes por procesar.", MethodBase.GetCurrentMethod().DeclaringType.Name);
                }
            }
            catch (EmptyStatementException e)
            {
                // Borramos el archivo vacio para que no lo vuelva a procesar
                File.Delete(newFile);

                throw new UException
                    (
                        GeneralErrors.ArgumentNullExceptionCode,
                        this.GetType().Name,
                        MethodBase.GetCurrentMethod().Name,
                        e.MessageException,
                        e
                    );
            }
            catch (ArgumentNullException e)
            {
                throw new UException
                    (
                        GeneralErrors.ArgumentNullExceptionCode,
                        this.GetType().Name,
                        MethodBase.GetCurrentMethod().Name,
                        e.Message,
                        e
                    );
            }
            catch (NotSupportedException e)
            {
                throw new UException
                    (
                        GeneralErrors.NotSupportedExceptionCode,
                        this.GetType().Name,
                        MethodBase.GetCurrentMethod().Name,
                        e.Message,
                        e
                    );
            }
            catch (IOException e)
            {
                throw new UException
                    (
                        GeneralErrors.IOExceptionCode,
                        this.GetType().Name,
                        MethodBase.GetCurrentMethod().Name,
                        e.Message,
                        e
                    );
            }
            catch (Exception e)
            {
                Logger.WriteErrorLog(e, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }
    }
}
