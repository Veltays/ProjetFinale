using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
using ProjetFinale.Models;

namespace ProjetFinale.Utils
{
    public class ExportService : IExportService
    {
        // ===== JSON =====
        public void ExportJson<T>(T data, string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        // ===== XML =====
        public void ExportXml<T>(T data, string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            var serializer = new XmlSerializer(typeof(T));
            using var stream = new FileStream(filePath, FileMode.Create);
            serializer.Serialize(stream, data!);
        }

        // ===== CSV (copie 1:1 de l’ancienne ExportCSVToFile) =====
        public void ExportCsv<T>(T data, string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            if (data is not Utilisateur u)
            {
                // si jamais tu l'appelles sur autre chose, on écrit juste ToString()
                File.WriteAllText(filePath, data?.ToString() ?? string.Empty);
                return;
            }

            var sb = new StringBuilder();

            // ==== Utilisateur ====
            sb.AppendLine("==== Utilisateur ====");
            sb.AppendLine($"Pseudo,{u.Pseudo}");
            sb.AppendLine($"Nom,{u.Nom}");
            sb.AppendLine($"Prenom,{u.Prenom}");
            sb.AppendLine($"Email,{u.Email}");
            sb.AppendLine($"Age,{u.Age}");
            sb.AppendLine($"Taille,{u.Taille}");
            sb.AppendLine($"Poids,{u.Poids}");
            sb.AppendLine($"ObjectifPoids,{u.ObjectifPoids}");
            sb.AppendLine($"DateInscription,{u.DateInscription:yyyy-MM-dd}");
            sb.AppendLine($"DateObjectif,{u.DateObjectif:yyyy-MM-dd}");
            sb.AppendLine();

            // ==== Tâches ====
            sb.AppendLine("==== Tâches ====");
            sb.AppendLine("Description,EstTerminee");
            foreach (var t in u.ListeTaches)
                sb.AppendLine($"{t.Description},{t.EstTerminee}");
            sb.AppendLine();

            // ==== Activités ====
            sb.AppendLine("==== Activités ====");
            sb.AppendLine("Titre,Duree,ImagePath,CaloriesBrulees");
            foreach (var a in u.ListeActivites)
                sb.AppendLine($"{a.Titre},{a.Duree},{a.ImagePath},{a.CaloriesBrulees}");
            sb.AppendLine();

            // ==== Agenda ====
            sb.AppendLine("==== Agenda ====");
            sb.AppendLine("Titre,HeureDebut,HeureFin,Date,ActiviteTitre,Couleur,Description");
            foreach (var a in u.ListeAgenda)
                sb.AppendLine($"  {a.Titre},{a.HeureDebut},{a.HeureFin},{a.Date:yyyy-MM-dd},{a.Activite?.Titre},{a.Couleur},{a.Description}");
            sb.AppendLine();

            File.WriteAllText(filePath, sb.ToString());
        }
    }
}