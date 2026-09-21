using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.Json;
using notaverse_threejs_test1.Models;

namespace notaverse_threejs_test1.Services
{
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
}
