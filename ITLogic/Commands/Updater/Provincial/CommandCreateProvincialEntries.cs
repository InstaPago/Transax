using InstaTransfer.DataAccess;
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

namespace InstaTransfer.ITLogic.Commands.Updater.Provincial
{
    public class CommandCreateProvincialEntries : Command
    {

        public CommandCreateProvincialEntries(Object receiver) : base(receiver) { }

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
                    //Creamos un UBSE con los datos extraidos del .csv para la fila actual
                    entryList.Add(new UBankStatementEntry
                    {
                        Date = DateTime.ParseExact(UpdaterHelper.CleanValues(values[0], UpdaterResources.CleanValueDate), UpdaterResources.ShortDateFormat, CultureInfo.InvariantCulture),
                        Ref = UpdaterHelper.CleanValues(values[1], UpdaterResources.CleanValueQuotes),
                        Description = UpdaterHelper.CleanValues(values[2], UpdaterResources.CleanValueQuotes),
                        Amount = decimal.Parse(UpdaterHelper.CleanValues(values[3], UpdaterResources.CleanValueQuotes)),
                        Balance = decimal.Parse(UpdaterHelper.CleanValues(values[4], UpdaterResources.CleanValueQuotes), CultureInfo.InvariantCulture),
                        IdUBankStatement = bankStatement.Id
                    });
                    //Saltamos al siguiente registro
                    row++;
                }
                //Cerramos el archivo
                reader.Close();

                //Asociamos la lista de movimientos al banco emisor respectivo
                Console.WriteLine("Asignando banco emisor...");

                var _bankList = UpdaterHelper.GetBankIdStringArray();

                foreach (var rawEntry in entryList)
                {
                    foreach (var _bank in _bankList)
                    {
                        if (rawEntry.Description.Contains(_bank) && rawEntry.IdUBank_Issuer == null)
                        {
                            rawEntry.IdUBank_Issuer = _bank;
                        }
                    }
                }

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
                        typeof(CommandCreateEntries).Name,
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
                        typeof(CommandCreateEntries).Name,
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
