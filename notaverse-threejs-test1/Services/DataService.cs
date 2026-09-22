using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.Json;
using notaverse_threejs_test1.Models;

namespace notaverse_threejs_test1.Services
{
    /*
    public class DataService
    {
        private readonly string _dataFilePath;

        public DataService()
        {
            // Папка рядом с .exe — для переносимости
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var dataFolder = Path.Combine(baseDir, "Data");
            if (!Directory.Exists(dataFolder))
                Directory.CreateDirectory(dataFolder);

            _dataFilePath = Path.Combine(dataFolder, "scene_config.json");
        }

        public string DataFilePath => _dataFilePath;

        /// <summary>
        /// Загрузка всей конфигурации сцены (камера + объекты).
        /// </summary>
        public async Task<SceneConfig> LoadSceneConfigAsync()
        {
            if (!File.Exists(_dataFilePath))
                return new SceneConfig();

            var json = await File.ReadAllTextAsync(_dataFilePath);
            return JsonSerializer.Deserialize<SceneConfig>(json) ?? new SceneConfig();
        }

        /// <summary>
        /// Сохранение всей конфигурации сцены.
        /// </summary>
        public async Task SaveSceneConfigAsync(SceneConfig config)
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(_dataFilePath, json);
        }

        /// <summary>
        /// Загрузка только списка объектов (для передачи в JS).
        /// </summary>
        public async Task<List<SceneObjectData>> LoadObjectsAsync()
        {
            var config = await LoadSceneConfigAsync();
            return config.Objects;
        }

        /// <summary>
        /// Сохранение только списка объектов.
        /// </summary>
        public async Task SaveObjectsAsync(List<SceneObjectData> objects)
        {
            var config = await LoadSceneConfigAsync();
            config.Objects = objects;
            await SaveSceneConfigAsync(config);
        }
    }
    */
    public class ProjectInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string ModelFilePath { get; set; }

        public string DisplayName =>
            $"{Name} — {ModifiedAt:dd.MM.yyyy HH:mm}";

        public override string ToString() => DisplayName;
    }

    public class DataService
    {
        private readonly string _projectsFolder;
        private readonly string _currentProjectPointer;

        public DataService()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _projectsFolder = Path.Combine(baseDir, "Projects");
            Directory.CreateDirectory(_projectsFolder);

            var dataDir = Path.Combine(baseDir, "Data");
            Directory.CreateDirectory(dataDir);
            _currentProjectPointer = Path.Combine(dataDir, "current_project.txt");
        }

        public string ProjectsFolder => _projectsFolder;

        // ---------- Проекты ----------

        /// <summary>Загрузка проекта по ID.</summary>
        public async Task<SceneConfig> LoadProjectAsync(string projectId)
        {
            var path = Path.Combine(_projectsFolder, $"{projectId}.json");
            if (!File.Exists(path))
                return new SceneConfig { ProjectId = projectId };

            var json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<SceneConfig>(json)
                   ?? new SceneConfig { ProjectId = projectId };
        }

        /// <summary>Сохранение проекта (создаёт или перезаписывает).</summary>
        public async Task SaveProjectAsync(SceneConfig config)
        {
            config.ModifiedAt = DateTime.Now;

            var path = Path.Combine(_projectsFolder, $"{config.ProjectId}.json");
            var json = JsonSerializer.Serialize(config,
                new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(path, json);
        }

        /// <summary>Список всех сохранённых проектов.</summary>
        public async Task<List<ProjectInfo>> ListProjectsAsync()
        {
            var result = new List<ProjectInfo>();
            if (!Directory.Exists(_projectsFolder)) return result;

            foreach (var file in Directory.GetFiles(_projectsFolder, "*.json"))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var config = JsonSerializer.Deserialize<SceneConfig>(json);
                    if (config != null)
                    {
                        result.Add(new ProjectInfo
                        {
                            Id = config.ProjectId,
                            Name = string.IsNullOrWhiteSpace(config.ProjectName)
                                ? "Без имени"
                                : config.ProjectName,
                            ModifiedAt = config.ModifiedAt,
                            ModelFilePath = config.ModelFilePath
                        });
                    }
                }
                catch { /* битый файл — пропускаем */ }
            }

            return result.OrderByDescending(p => p.ModifiedAt).ToList();
        }

        /// <summary>Удаление проекта по ID.</summary>
        public void DeleteProject(string projectId)
        {
            var path = Path.Combine(_projectsFolder, $"{projectId}.json");
            if (File.Exists(path)) File.Delete(path);
        }

        // ---------- Указатель на текущий проект ----------

        public async Task SaveCurrentProjectPointerAsync(string projectId)
        {
            await File.WriteAllTextAsync(_currentProjectPointer, projectId);
        }

        public async Task<string> GetCurrentProjectPointerAsync()
        {
            if (!File.Exists(_currentProjectPointer)) return null;
            var id = await File.ReadAllTextAsync(_currentProjectPointer);
            return string.IsNullOrWhiteSpace(id) ? null : id.Trim();
        }
    }
}
