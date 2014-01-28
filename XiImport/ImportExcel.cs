using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TakeIo.Spreadsheet;
using XiImport.Interface;

namespace XiImport
{
    public class ImportExcel : IImportReader
    {
        IEnumerator<IList<string>> IEnumerable<IList<string>>.GetEnumerator()
        {
            RowIndex = 0;
            foreach (var row in Data.Skip(1))
            {
                RowIndex++;
                yield return row;
            }
            RowIndex = 0;
        }

        public IEnumerator GetEnumerator()
        {
            RowIndex = 0;
            foreach (var row in Data.Skip(1))
            {
                RowIndex++;
                yield return row;
            }
            RowIndex = 0;
        }

        internal IList<IList<string>> Data;
        internal int RowIndex;
        internal string FileName;
        public virtual void Open()
        {
            Data = Spreadsheet.Read(new FileInfo(FileName));
            Data.Normalize();
            Data.RemoveEmptyRows();
        }

        public IEnumerable<string> GetFields()
        {
            return Data[0];
        }

        public int GetRowsCount()
        {
            return Data.Count - 1;
        }

        public void SetFileName(string fileName)
        {
            FileName = fileName;
        }

        public int? GetInt(int index)
        {
            return int.Parse(Data[RowIndex][index]);
        }

        public decimal? GetDecimal(int index)
        {
            return decimal.Parse(Data[RowIndex][index]);
        }

        public string GetString(int index)
        {
            return Data[RowIndex][index];
        }

        public bool? GetBool(int index)
        {
            return bool.Parse(Data[RowIndex][index].ToLower());
        }

        public DateTime? GetDate(int index)
        {
            return DateTime.Parse(Data[RowIndex][index]);
        }

        public float? GetFloat(int index)
        {
            return float.Parse(Data[RowIndex][index]);
        }

        public double? GetDouble(int index)
        {
            return double.Parse(Data[RowIndex][index]);
        }

        public char? GetChar(int index)
        {
            return char.Parse(Data[RowIndex][index]);
        }
    }
}