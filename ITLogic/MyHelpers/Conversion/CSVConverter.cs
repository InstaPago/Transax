using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHelpers.Conversion
{
    public class CSVConverter
    {
        public static string ConvertToCSVNoInfoColumns(DataTable objDataSet, string Delimiter)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            var columnNames = objDataSet.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray();
            //sb.AppendLine(string.Join(Delimiter, columnNames));
            foreach (DataRow row in objDataSet.Rows)
            {
                var fields = row.ItemArray.Select(field => field.ToString()).ToArray();
                sb.AppendLine(string.Join(Delimiter, fields));
            }
            return sb.ToString();
        }
        public static string ConvertToCSV(DataTable objDataSet, string Delimiter)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            var columnNames = objDataSet.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray();
            sb.AppendLine(string.Join(Delimiter, columnNames));
            foreach (DataRow row in objDataSet.Rows)
            {
                var fields = row.ItemArray.Select(field => field.ToString()).ToArray();
                sb.AppendLine(string.Join(Delimiter, fields));
            }
            return sb.ToString();
        }

    }
}
