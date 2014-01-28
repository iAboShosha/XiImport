using System;
using System.Collections.Generic;
using XiRake.DataAccess;

namespace XiImport.Interface
{
    public interface IImport<T> where T : class, IEntity, new()
    {
        int SuccessfulEntries { get; }
        int FailureEntries { get; }
        void SetFileName(string fileName);
        void SetColumnMapping(ICollection<Mapping> mappings);
        void AddCustomObjectInitializer(Type t, Func<string, object> create);
        void AddCustomFindMethod(Type t, Func<string, int?> create);
        string[] GetColumnName();
        string[] GetSampleData();
        void Run();
    }
}