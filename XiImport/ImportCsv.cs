using System.IO;

namespace XiImport
{
    public class ImportCsv : ImportExcel
    {

        public override void Open()
        {
            Data = TakeIo.Spreadsheet.Spreadsheet.ReadCsv(new FileInfo(FileName));
        }


    }
}