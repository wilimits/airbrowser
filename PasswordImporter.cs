using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Linq;

namespace SearchBrowser
{
    public class Credential
    {
        public string Url { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public static class PasswordImporter
    {
        public static List<Credential> SavedCredentials { get; set; } = new();
        private static string StoragePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AirBrowser", "passwords.json");

        public static void LoadCredentials()
        {
            try
            {
                if (File.Exists(StoragePath))
                {
                    string json = File.ReadAllText(StoragePath);
                    SavedCredentials = JsonSerializer.Deserialize<List<Credential>>(json) ?? new List<Credential>();
                }
            }
            catch { }
        }

        private static void SaveCredentials()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(StoragePath)!);
                string json = JsonSerializer.Serialize(SavedCredentials);
                File.WriteAllText(StoragePath, json);
            }
            catch { }
        }

        public static int ImportFromCsv(string filePath)
        {
            if (!File.Exists(filePath)) return 0;
            
            LoadCredentials();
            int count = 0;
            using var reader = new StreamReader(filePath);
            bool isFirstLine = true;
            
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                
                if (isFirstLine) 
                { 
                    isFirstLine = false; 
                    continue; // Skip header
                }
                
                var values = ParseCsvLine(line);
                if (values.Count >= 4)
                {
                    string url = values[1];
                    string username = values[2];
                    string password = values[3];
                    
                    if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                    {
                        if (!SavedCredentials.Any(c => c.Url == url && c.Username == username))
                        {
                            SavedCredentials.Add(new Credential { Url = url, Username = username, Password = password });
                            count++;
                        }
                    }
                }
            }
            
            if (count > 0)
                SaveCredentials();
                
            return count;
        }

        private static List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var currentField = new StringBuilder();
            
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '\"') 
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                    {
                        currentField.Append('\"');
                        i++; 
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes) 
                {
                    result.Add(currentField.ToString());
                    currentField.Clear();
                }
                else 
                {
                    currentField.Append(c);
                }
            }
            result.Add(currentField.ToString());
            return result;
        }
    }
}
