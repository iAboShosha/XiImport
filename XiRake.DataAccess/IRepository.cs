using System;

namespace XiRake.DataAccess
{
    public interface IRepository<out T> where T:IEntity
    {
        int? SearchBy(Type type, string entityColumnName, string value);
        void Add<T>(T entity);
        void Save();
        T ById(int id);
        void ReNewSession();
        
    }
}