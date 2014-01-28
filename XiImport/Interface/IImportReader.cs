using System;
using System.Collections.Generic;

namespace XiImport.Interface
{
    public interface IImportReader : IEnumerable<IList<string>>
    {
        void Open();
        IEnumerable<string> GetFields();
        int GetRowsCount();
        void SetFileName(string fileName);

        int? GetInt(int index);
        decimal? GetDecimal(int index);
        string GetString(int index);
        bool? GetBool(int index);
        DateTime? GetDate(int index);
        float? GetFloat(int index);
        char? GetChar(int index);
    }


}