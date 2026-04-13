using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using schedule_automation_app_client.Models;

namespace schedule_automation_app_client.Services;

public class JsonStorageService : IStorageService
{
    private readonly string _filePath;

    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    public JsonStorageService()
    {
        string appFolder = AppDomain.CurrentDomain.BaseDirectory;
        _filePath = Path.Combine(appFolder, "subjects.json");
    }

    public async Task SaveAsync(ObservableCollection<Subject> subjects)
    {
        try
        {
            string json = JsonSerializer.Serialize(subjects, _jsonOptions);
            await File.WriteAllTextAsync(_filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при сохранении данных: {ex.Message}");
        }
    }

    public async Task<ObservableCollection<Subject>> LoadAsync()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return new ObservableCollection<Subject>();
            }

            string json = await File.ReadAllTextAsync(_filePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new ObservableCollection<Subject>();
            }

            ObservableCollection<Subject>? subjects = JsonSerializer.Deserialize<ObservableCollection<Subject>>(json, _jsonOptions);

            if (subjects == null)
            {
                return new ObservableCollection<Subject>();
            }

            return subjects;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке данных: {ex.Message}");
            return new ObservableCollection<Subject>();
        }
    }
}