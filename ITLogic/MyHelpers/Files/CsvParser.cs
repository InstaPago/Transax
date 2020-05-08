using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MyHelpers.Files
{
    public static class CsvParser
    {
        /// <summary>
        /// parse a CSV file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<string[]> ParseCsv(string path)
        {
            List<string[]> _parsedData = new List<string[]>();
            try
            {
                using (StreamReader _readFile = new StreamReader(path))
                {
                    string _line;
                    string[] _row;
                    while ((_line = _readFile.ReadLine()) != null)
                    {
                        _row = _line.Split(',');
                        _parsedData.Add(_row);
                    }
                }
            }
            catch
            {
                throw;
            }

            return _parsedData;
        }
    }
}
