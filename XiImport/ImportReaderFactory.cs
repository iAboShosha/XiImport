using System;
using System.Collections.Generic;
using System.IO;
using XiImport.Interface;

namespace XiImport
{
    public class ImportReaderFactory
    {
        static readonly Dictionary<string, Type> ImportReaderTypes = new Dictionary<string, Type>
            {
                {"csv",typeof(ImportCsv)},
                {"xls",typeof(ImportExcel)},
                {"xlsx",typeof(ImportExcel)}
            };

        public static void AddReader<T>(string extention) where T : IImportReader, new()
        {
            if (string.IsNullOrEmpty(extention)) throw new ArgumentNullException("extention");
            extention = extention.ToLower();
            if (ImportReaderTypes.ContainsKey(extention))
                ImportReaderTypes[extention] = typeof(T);
            else
                ImportReaderTypes.Add(extention, typeof(T));

        }

        public static IImportReader Create(string fileName)
        {
            CheckIfFileNameIsNull(fileName);

            var ext = GetFileExtention(fileName);
            if (IsFileExtentionInReaderList(ext))
            {
                var t = Activator.CreateInstance(ImportReaderTypes[ext]) as IImportReader;
                if (t != null)
                {
                    t.SetFileName(fileName);
                    return t;
                }
            }

            throw new ApplicationException("No reader found for this file type");
        }

        private static bool IsFileExtentionInReaderList(string ext)
        {
            return ImportReaderTypes.ContainsKey(ext);
        }

        private static string GetFileExtention(string fileName)
        {
            var ext = new FileInfo(fileName).Extension.Remove(0, 1).ToLower();
            return ext;
        }

        private static void CheckIfFileNameIsNull(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");
        }
    }
}