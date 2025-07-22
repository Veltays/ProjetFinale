using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetFinale.Utils
{
    public interface IExportService
    {
        void ExportJson<T>(IEnumerable<T> data, string filePath);
        void ExportXml<T>(IEnumerable<T> data, string filePath);
        void ExportCsv<T>(IEnumerable<T> data, string filePath);
        void ExportTxt<T>(IEnumerable<T> data, string filePath);
    }

}
