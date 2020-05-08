using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyHelpers.StringExtensions;
using System.Linq.Dynamic;

namespace MyHelpers.DataTables
{

    public enum TipoDatosDataTable
    {
        tInt,
        tString,
        tnone
    }

    public class FiltrosDataTables
    {
        /// <summary>
        /// recibe un iqueryable y aplica todos los parametros de DataTablesParam
        /// Páginado, busqueda y ordenamiento
        /// </summary>
        /// <param name="param"></param>
        /// <param name="totalRecords"></param>
        /// <param name="datos"></param>
        /// <param name="totalRecordsDisplay"></param>
        /// <param name="nombreColumnas"></param>
        /// <param name="tipos"></param>
        /// <returns></returns>
        public IQueryable FiltroPagingSortingSearch(DataTablesParam param, int totalRecords, IQueryable datos, ref int totalRecordsDisplay,
            string[] nombreColumnas, TipoDatosDataTable[] tipos)
        {
            if (!param.sSearch.IsNullOrEmpty())
            {
                string searchString = "";
                bool first = true;
                for (int i = 0; i < param.iColumns; i++)
                {
                    if (param.bSearchable[i])
                    {
                        string columnName = nombreColumnas[i];
                        string sortDir = param.sSortDir[i];

                        if (!first)
                            searchString += " or ";
                        else
                            first = false;
                        if (tipos[i] == TipoDatosDataTable.tInt)
                        {
                            searchString += columnName + ".ToString().StartsWith(\"" + param.sSearch + "\")";
                        }
                        else if(tipos[i] == TipoDatosDataTable.tString)
                        {
                            searchString += columnName + ".Contains(\"" + param.sSearch + "\")";
                        }
                    }
                }
                datos = datos.Where(searchString);
            }

            string sortString = "";
            for (int i = 0; i < param.iSortingCols; i++)
            {
                int columnNumber = param.iSortCol[i];
                if (tipos[i] != TipoDatosDataTable.tnone)
                {
                    string columnName = nombreColumnas[columnNumber];
                    string sortDir = param.sSortDir[i];
                    if (i != 0)
                        sortString += ", ";
                    sortString += columnName + " " + sortDir;
                }
            }

            totalRecordsDisplay = datos.Count();

            datos = datos.OrderBy(sortString);
            datos = datos.Skip(param.iDisplayStart).Take(param.iDisplayLength);

            return datos;
        }

        public string BuildJSon(List<List<string>> datos, int totalRecords, int totalRecordsDisplay,string sEcho)
        {
            string jsonResponse = "{\"iTotalRecords\":" + totalRecords + ",\"iTotalDisplayRecords\":" + totalRecordsDisplay + ",\"sEcho\":" + sEcho +
                ",\"aaData\":[";            
            for (int j = 0;j<datos.Count;j++)
            {
                var row = datos[j];
                if(j>0)
                    jsonResponse+=",";
                jsonResponse += "[";
                for (int i = 0; i < row.Count;++i)
                {
                    if (i > 0)
                        jsonResponse += ",";
                    string temp = row[i].IsNullOrEmpty() ? "No asignado" : row[i];
                    jsonResponse += "\"" + temp + "\"";
                }
                jsonResponse += "]";
            }
            jsonResponse += "]}";

            return jsonResponse;
        }
    }
}
