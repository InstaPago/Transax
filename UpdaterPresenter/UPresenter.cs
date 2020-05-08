using InstaTransfer.BLL.Concrete;
using InstaTransfer.DataAccess;
using InstaTransfer.ITExceptions.Updater;
using InstaTransfer.ITLogic;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITLogic.Factory;
using InstaTransfer.ITLogic.Helpers;
using InstaTransfer.ITLogic.Log;
using InstaTransfer.ITResources.General;
using InstaTransfer.ITResources.Updater;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace InstaTransfer.UpdaterPresenter
{
    /// <summary>
    /// Contiene todos los metodos del updater
    /// </summary>
    public class UPresenter
    {
        #region Constructor

        public UPresenter()
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// Actualiza la base de datos con el estado de cuenta descargado
        /// para un banco específico.
        /// </summary>
        /// <param name="path">Ruta del estado de cuenta descargado.</param>
        /// <param name="bankId">Id del banco asociado al estado de cuenta.</param>
        public void UpdateStatement(string path, string bankId)
        {
            UBankStatement _uBankStatement;
            Command commandCreateBankStatement;
            Command commandCreateEntries;
            Bank _bank = GeneralHelper.GetBankEnum(bankId);

            try
            {
                Console.WriteLine("Iniciando procesos...");

                //Creamos y almacenamos el UBS en base de datos
                commandCreateBankStatement = CommandFactory.GetCommandCreateBankStatement(_bank);
                commandCreateBankStatement.Execute();
                //Asignamos el UBS creado a una variable
                _uBankStatement = (UBankStatement)commandCreateBankStatement.Receiver;

                //Creamos y almacenamos la lista de UBSE asociado al UBS generado
                commandCreateEntries = CommandFactory.GetCommandCreateEntries(_bank);
                commandCreateEntries.Parameter = new Dictionary<string, object>
                    {
                        { UpdaterResources.DictionaryKeyBankStatement, _uBankStatement },
                        { UpdaterResources.DictionaryKeyPath, path }
                    };
                commandCreateEntries.Execute();
            }
            catch (UException e)
            {
                Console.WriteLine("Error: {0}", e.MessageException);
                Logger.WriteErrorLog(e, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }

        /// <summary>
        /// Actualiza la base de datos de existir estados de cuenta sin procesar.
        /// </summary>
        /// <param name="path">Ruta base de los archivos a procesar</param>
        public void UpdateStatement()
        {
            string basePath = ConfigurationManager.AppSettings[GeneralResources.AppSettingsKeyBasePath];
            string backupPath = ConfigurationManager.AppSettings[GeneralResources.AppSettingsKeyBackupPath];

            Command commandUpdateFiles;
            //string basePath = @"C:\UmbrellaOrigen";
            //string basePath = ConfigurationManager.AppSettings[GeneralResources.AppSettingsKeyBasePath];
            //string backupPath = ConfigurationManager.AppSettings[GeneralResources.AppSettingsKeyBackupPath];
            //string backupPath = @"C:\UmbrellaRespaldo";
            try
            {
                //Verificamos si hay archivos nuevos para procesar

                commandUpdateFiles = CommandFactory.GetCommandUpdateFiles(basePath);
                commandUpdateFiles.Parameter = new Dictionary<string, object>
                            {
                                { UpdaterResources.DictionaryKeyBackupPath, backupPath }
                            };
                commandUpdateFiles.Execute();
            }
            catch (UException e)
            {
                Console.WriteLine("Error: {0}", e.MessageException);
                Logger.WriteErrorLog(e, MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }

        #endregion

        #region Obsolete

        /// <summary>
        /// Crea un archivo excel a partir del estado de cuenta procesado
        /// </summary>
        /// <param name="statement">Lista de entradas procesadas desde un EDC</param>
        [Obsolete("Se debe almacenar en base de datos en lugar de generar un archivo excel")]
        static _Workbook CreateExcelFile(UBankStatement statement)
        {
            Console.WriteLine("Creando excel a partir del estado de cuenta...");
            var excelApp = new Application();

            excelApp.DisplayAlerts = false;
            // Crea un nuevo workbook
            excelApp.Workbooks.Add();

            // This example uses a single workSheet. The explicit type casting is
            // removed in a later procedure.
            _Worksheet workSheet = (Worksheet)excelApp.ActiveSheet;

            // Establish column headings in cells A1 and B1.
            workSheet.Cells[1, "A"] = "Fecha";
            workSheet.Cells[1, "B"] = "Referencia";
            workSheet.Cells[1, "C"] = "Descripcion";
            workSheet.Cells[1, "D"] = "Monto";
            workSheet.Cells[1, "E"] = "Saldo";

            //Llena el contenido de las celdas
            var row = 1;
            foreach (var entry in statement.UBankStatementEntries)
            {
                row++;
                workSheet.Cells[row, "A"] = entry.Date.ToString(UpdaterResources.ShortDateFormat);
                workSheet.Cells[row, "B"] = entry.Ref;
                workSheet.Cells[row, "C"] = entry.Description;
                workSheet.Cells[row, "D"] = entry.Amount;
                workSheet.Cells[row, "E"] = entry.Balance;
            }

            //Formatea las columnas
            workSheet.Columns[1].AutoFit();
            workSheet.Columns[2].AutoFit();
            workSheet.Columns[3].AutoFit();
            workSheet.Columns[4].AutoFit();
            workSheet.Columns[5].AutoFit();
            workSheet.Cells[1, "A"].EntireRow.Font.Bold = true;

            return excelApp.Workbooks[1];
        }

        /// <summary>
        /// Guarda un Workbook de excel generado a un directorio especificado
        /// </summary>
        /// <param name="workBook">Archivo excel generado</param>
        /// <param name="path">Rita donde se va a almacenar el archivo</param>
        /// <param name="fileName">Nombre del archivo a almacenar</param>
        [Obsolete("Se debe almacenar en base de datos en lugar de generar un archivo excel")]
        static void SaveFile(_Workbook workBook, string path, string fileName)
        {
            //Guarda el excel limpio en el servidor
            workBook.SaveAs(path + "U-" + fileName, XlFileFormat.xlCSV, Local: true);
            //Cerramos el archivo en memoria
            workBook.Close();
        }

        /// <summary>
        /// Asocia un UBankStatement con un UBank
        /// </summary>
        /// <param name="bankStatement">UBankStatement a asociar</param>
        [Obsolete("Al saberse el id del banco, no se necesita este metodo")]
        static void AddStatementToBank(DataAccess.UBankStatement bankStatement)
        {
            Repository<DataAccess.UBank> UBRepo = new Repository<DataAccess.UBank>();

            var bank = new DataAccess.UBank();

            bank = UBRepo.GetAllRecords().Where(t => t.Id == "0134").Single();

            bank.UBankStatements.Add(bankStatement);

            UBRepo.SaveChanges();
        }

        #endregion

    }
}
