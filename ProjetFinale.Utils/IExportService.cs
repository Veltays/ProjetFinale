using System;

namespace ProjetFinale.Utils
{
    public interface IExportService
    {
        void ExportJson<T>(T data, string filePath);
        void ExportXml<T>(T data, string filePath);
        void ExportCsv<T>(T data, string filePath);
    }
}