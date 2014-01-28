using System;
using System.Collections.Generic;
using System.Linq;
using XiImport.Interface;
using XiRake.DataAccess;

namespace XiImport
{
    public class Import<T> : IImport<T> where T : class,IEntity, new()
    {
        private string _fileName;
        private ICollection<Mapping> _map;
        private IImportReader _reader;
        private readonly IRepository<T> _repository;
        readonly Dictionary<Type, Func<string, object>> _customObjectInitializers = new Dictionary<Type, Func<string, object>>();
        readonly Dictionary<Type, Func<string, int?>> _objectsFindMethos = new Dictionary<Type, Func<string, int?>>();

        public int SuccessfulEntries { get; private set; }
        public int FailureEntries { get; private set; }


        public Import(IRepository<T> repository)
        {
            _repository = repository;
        }

        public void SetFileName(string fileName)
        {
            _fileName = fileName;
        }

        public void SetColumnMapping(ICollection<Mapping> mappings)
        {
            _map = mappings;

        }
        public void AddCustomObjectInitializer(Type t, Func<string, object> create)
        {
            if (_customObjectInitializers.ContainsKey(t))
                _customObjectInitializers[t] = create;
            else
                _customObjectInitializers.Add(t, create);
        }
        public void AddCustomFindMethod(Type t, Func<string, int?> create)
        {
            if (_objectsFindMethos.ContainsKey(t))
                _objectsFindMethos[t] = create;
            else
                _objectsFindMethos.Add(t, create);
        }

        private T Create()
        {
            if (_customObjectInitializers.ContainsKey(typeof(T)))
                return _customObjectInitializers[typeof(T)].Invoke("") as T;
            return new T();
        }

        private int? CheckElementExsite(Mapping map, string value)
        {
            if (map != null)
            {
                return _repository.SearchBy(typeof(T), map.EntityColumnName, value);
            }

            
            
            if (typeof(T).GetType().GetInterfaces().Any(x => x == typeof(INamedEntity)))
            {
                return _repository.SearchBy(typeof(T),  "Name", value);
            }
            
            if (_objectsFindMethos.ContainsKey(typeof(T)))
                return _objectsFindMethos[typeof(T)].Invoke(value);

            throw new Exception(string.Format("no check method found for {0} type", typeof(T).Name));
        }

        public string[] GetColumnName()
        {
            OpenFile();
            return _reader.GetFields().ToArray();
        }

        public string[] GetSampleData()
        {
            OpenFile();
            return _reader.First().ToArray();
        }

        public void Run()
        {
            OpenFile();
            _map = GetActiveMappingEntries();

            var uniqueRole = _map.FirstOrDefault(x => x.IsUnique);
            Func<T> findElementOrCreateIt = Create;

            if (uniqueRole != null)
            {
                findElementOrCreateIt = () =>
                    {
                        string v = _reader.GetString(uniqueRole.FileColumnIndex.Value);
                        var i = CheckElementExsite(uniqueRole, v);
                        if (i == null)
                            return Create();

                        return _repository.ById(i.Value);

                    };
            }
            foreach (var row in _reader)
            {

                T newEntry = findElementOrCreateIt();
                foreach (var mapping in _map) //fill values
                {
                    var propertyName = mapping.EntityColumnName;
                    int fileColumnIndex = mapping.FileColumnIndex ?? -1;
                    object value = _reader.GetString(fileColumnIndex);

                    var propertytype = GetPropertyType(propertyName);
                    if (value != null)
                    {
                        if (IsIEntity(propertytype))
                        {
                            propertyName += "Id";
                            value = GetValueForIEntityOrCreateIt(propertytype, value);
                        }
                        else
                            value = GetNormalValue(propertytype, fileColumnIndex);
                    }

                    if (value != null)
                    {
                        typeof(T).GetProperty(propertyName).SetValue(newEntry, value, null);
                    }
                }

                if (newEntry.Id == 0)
                    _repository.Add(newEntry);

                try
                {
                    _repository.Save();
                    SuccessfulEntries++;
                }
                catch
                {
                    FailureEntries++;
                    _repository.ReNewSession();
                }
            }
        }

        private void OpenFile()
        {
            if (_reader == null)
            {
                _reader = ImportReaderFactory.Create(_fileName);
                _reader.Open();
            }
        }

        private object GetValueForIEntityOrCreateIt(Type pType, object value)
        {
            try
            {
                value = GetIntegerFromValue(value);
            }
            catch (FormatException) //to handle value if it not integer
            {
                value = GetDbEntityId(pType, value) ?? CreateNewObject(pType, value);
            }
            return value;
        }

        private object GetNormalValue(Type propertytype, int fileColumnIndex)
        {
            if (typeof(int) == propertytype || typeof(int?) == propertytype)
                return _reader.GetInt(fileColumnIndex);

            if (typeof(decimal) == propertytype || typeof(decimal?) == propertytype)
                return _reader.GetDecimal(fileColumnIndex);

            if (typeof(float) == propertytype || typeof(float?) == propertytype)
                return _reader.GetFloat(fileColumnIndex);

            if (typeof(bool) == propertytype || typeof(bool?) == propertytype)
                return _reader.GetBool(fileColumnIndex);

            if (typeof(DateTime) == propertytype || typeof(DateTime?) == propertytype)
                return _reader.GetDate(fileColumnIndex);

            if (typeof(char) == propertytype || typeof(char?) == propertytype)
                return _reader.GetChar(fileColumnIndex);

            return _reader.GetString(fileColumnIndex);
        }


        private object CreateNewObject(Type propertytype, object value)
        {
            return _customObjectInitializers.ContainsKey(propertytype) ?
                CreateObjectByCustomObjectInitializer(propertytype, value) :
                CreateObjectByDefaultInitializer(propertytype, value);
        }

        private object CreateObjectByDefaultInitializer(Type propertytype, object value)
        {
            //var obj = Activator.CreateInstance(propertytype) as IEntity;
            //var named = obj as INamedEntity;
            //if (named != null)
            //{
            //    named.NameA = value.ToString();
            //    named.NameL = value.ToString();
            //    _repository.Add(obj);
            //    _repository.Save();
            //    value = obj.Id;
            //}
            return value;
        }

        private object CreateObjectByCustomObjectInitializer(Type propertytype, object value)
        {
            var obj = _customObjectInitializers[propertytype].Invoke(value.ToString()) as IEntity;
            _repository.Add(obj);
            _repository.Save();
            value = obj.Id;
            return value;
        }

        private int? GetDbEntityId(Type propertytype, object value)
        {
            //if (propertytype.GetInterfaces().Contains(typeof(ICodedEntity)))
            //    return _repository.SearchBy(propertytype, new[] { "NameA", "NameL", "Code" }, value.ToString());

            //if (propertytype.GetInterfaces().Contains(typeof(INamedEntity)))
            //    return _repository.SearchBy(propertytype, new[] { "NameA", "NameL" }, value.ToString());

            if (_objectsFindMethos.ContainsKey(propertytype))
                return _objectsFindMethos[propertytype].Invoke(value.ToString());

            return null;
        }

        private static object GetIntegerFromValue(object value)
        {
            return (string)value != "" ? (object)Convert.ToInt32(value) : null;
        }

        private static bool IsIEntity(Type propertytype)
        {
            return propertytype.GetInterfaces().Contains(typeof(IEntity));
        }

        private static Type GetPropertyType(string propertyName)
        {
            return typeof(T).GetProperty(propertyName).PropertyType;
        }

        private List<Mapping> GetActiveMappingEntries()
        {
            return _map.Where(x => x.FileColumnIndex != null).ToList();
        }
    }
}