
using InstaTransfer.ITExceptions.Updater;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITLogic.Factory;
using InstaTransfer.ITLogic.Helpers;
using InstaTransfer.ITResources.General;
using InstaTransfer.ITResources.Updater;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Configuration;
using InstaTransfer.DataAccess;

namespace InstaTransfer.ITLogic.Commands.Updater.Banesco
{
    public class CommandCreateBanescoEntries : Command
    {

        public CommandCreateBanescoEntries(Object receiver) : base(receiver) { }

        public override void Execute()
        {
            Command commandSaveEntries;

            try
            {
                Console.WriteLine("Leyendo movimientos bancarios...");

                //Definimos las variables a utilizar
                Dictionary<string, object> parameter = (Dictionary<string, object>)Parameter;
                Bank bankID = (Bank)Receiver;
                string path = (string)parameter[UpdaterResources.DictionaryKeyFullPath];
                UBankStatement bankStatement = (UBankStatement)parameter[UpdaterResources.DictionaryKeyBankStatement];

                var reader = new StreamReader(File.OpenRead(path));
                var entryList = new List<UBankStatementEntry>();
                var row = 1;
                var line = reader.ReadLine();

                Console.WriteLine("Llenando campos del registro en base de datos...");

                //Leemos el archivo hasta el final
                while (!reader.EndOfStream && (line != null))
                {
                    line = reader.ReadLine();
                    //Obtenemos los valores separados por el delimitador del .csv
                    var values = line.Split(GeneralResources.SemiColon.ToCharArray());

                    #region Filtros
                    // Procesamos los datos del estado de cuenta
                    // Risk(CreateBanescoEntries): Estamos sujetos al orden de los campos del edo de cuenta de Banesco
                    var _date = DateTime.ParseExact(UpdaterHelper.CleanValues(values[0], UpdaterResources.CleanValueDate), UpdaterResources.ShortDateFormat, CultureInfo.InvariantCulture);
                    var _ref = UpdaterHelper.CleanValues(values[1], UpdaterResources.CleanValueQuotes);
                    var _description = UpdaterHelper.CleanValues(values[2], UpdaterResources.CleanValueQuotes);
                    var _amount = decimal.Parse(UpdaterHelper.CleanValues(values[3], UpdaterResources.CleanValueQuotes));
                    var _balance = decimal.Parse(UpdaterHelper.CleanValues(values[4], UpdaterResources.CleanValueQuotes));

                    // Obtenemos los filtros de exclusion de movimientos
                    string[] filtersExclude = ConfigurationManager.AppSettings[UpdaterResources.AppSettingsKeyUpdaterFiltersExclude].Split(GeneralResources.SemiColon.ToCharArray());

                    // Filtramos los movimientos por palabras claves en la descripcion
                    if (!_description.Contains(filtersExclude))
                    {
                        //Creamos un UBSE con los datos extraidos del .csv para la fila actual
                        entryList.Add(new UBankStatementEntry
                        {
                            Date = _date,
                            Ref = _ref,
                            Description = _description,
                            Amount = _amount,
                            Balance = _balance,
                            IdUBankStatement = bankStatement.Id
                        });
                    }

                    #endregion

                    //Saltamos al siguiente registro
                    row++;
                }
                //Cerramos el archivo
                reader.Close();

                // Verificamos si se creo algun movimiento
                if (entryList.Count == 0)
                {
                    throw new EmptyStatementException(UpdaterErrors.EmptyStatementExceptionMessage, UpdaterErrors.EmptyStatementExceptionCode);
                }
                
                #region PreClasificacion
                //Asociamos la lista de movimientos al banco emisor respectivo
                Console.WriteLine("Asignando banco emisor...");

                var _bankList = UpdaterHelper.GetBankIdStringArray();

                //Todo (AddIssuingBank): Mejorar rendimiento. Esto se coloca aparte para separar la preclasificacion
                foreach (var rawEntry in entryList)
                {
                    foreach (var _bank in _bankList)
                    {
                        if (rawEntry.Description.Contains(" " + _bank) && rawEntry.IdUBank_Issuer == null)
                        {
                            rawEntry.IdUBank_Issuer = _bank;
                            break;
                        }
                    }
                    // Obtenemos los filtros de inclusion de movimientos a clasificar
                    string[] filtersBanescoInclude = ConfigurationManager.AppSettings[UpdaterResources.AppSettingsKeyUpdaterBanescoFiltersInclude].Split(GeneralResources.SemiColon.ToCharArray());
                    // Filtramos los movimientos a asociar a banesco por palabras claves en la descripcion
                    if (rawEntry.Description.Contains(filtersBanescoInclude) && rawEntry.IdUBank_Issuer == null)
                    {
                        rawEntry.IdUBank_Issuer = UpdaterHelper.GetBankIdString(Bank.Banesco);
                    }
                }
                #endregion

                //Guardamos la lista de UBSE en la BD
                commandSaveEntries = CommandFactory.GetCommandSaveEntries(entryList);
                commandSaveEntries.Execute();
                Receiver = commandSaveEntries.Receiver;
            }
            catch (IOException e)
            {
                throw new UException
                    (
                        GeneralErrors.IOExceptionCode,
                        typeof(CommandCreateBanescoEntries).Name,
                        MethodBase.GetCurrentMethod().Name,
                        e.Message,
                        e
                    );
            }
            catch (FormatException e)
            {
                throw new UException
                    (
                        GeneralErrors.IOExceptionCode,
                        typeof(CommandCreateBanescoEntries).Name,
                        MethodBase.GetCurrentMethod().Name,
                        e.Message,
                        e
                    );
            }
            catch (UnauthorizedAccessException e)
            {
                throw new UException
                    (
                        GeneralErrors.UnauthorizedAccessExceptionCode,
                        this.GetType().Name,
                        MethodBase.GetCurrentMethod().Name,
                        e.Message,
                        e
                    );
            }
            catch (Exception)
            {
                throw;
            }


        }
    }
}
