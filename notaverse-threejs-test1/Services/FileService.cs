using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace notaverse_threejs_test1.Services
{
    public class FileService
    {
        private readonly string _storageFolder;

        public FileService()
        {
            _storageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AttachedFiles");
            if (!Directory.Exists(_storageFolder))
                Directory.CreateDirectory(_storageFolder);
        }

        /// <summary>
        /// Сохранение файла из потока в локальное хранилище.
        /// Возвращает ОТНОСИТЕЛЬНЫЙ путь (например, "AttachedFiles/abc123_photo.jpg").
        /// </summary>
        public async Task<string> SaveFileAsync(Stream fileStream, string originalFileName, string objectId)
        {
            var extension = Path.GetExtension(originalFileName);
            var uniqueFileName = $"{objectId}_{Guid.NewGuid().ToString()[..8]}{extension}";
            var filePath = Path.Combine(_storageFolder, uniqueFileName);

            using var output = File.Create(filePath);
            await fileStream.CopyToAsync(output);

            // Возвращаем относительный путь для переносимости
            return Path.Combine("AttachedFiles", uniqueFileName);
        }

        /// <summary>
        /// Получение абсолютного пути по относительному.
        /// </summary>
        public string GetAbsolutePath(string relativePath)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
        }

        /// <summary>
        /// Проверка существования файла.
        /// </summary>
        public bool FileExists(string relativePath)
        {
            return File.Exists(GetAbsolutePath(relativePath));
        }

        /// <summary>
        /// Удаление файла.
        /// </summary>
        public void DeleteFile(string relativePath)
        {
            var absPath = GetAbsolutePath(relativePath);
            if (File.Exists(absPath))
                File.Delete(absPath);
        }

        /// <summary>
        /// Открытие файла системным приложением.
        /// </summary>
        public void OpenFile(string relativePath)
        {
            var absPath = GetAbsolutePath(relativePath);
            if (!File.Exists(absPath))
                throw new FileNotFoundException("Файл не найден", absPath);

            Process.Start(new ProcessStartInfo
            {
                FileName = absPath,
                UseShellExecute = true
            });
        }

        /// <summary>
        /// Чтение файла как Base64 (для отображения изображений в WPF).
        /// </summary>
        public async Task<string> ReadFileAsBase64Async(string relativePath)
        {
            var absPath = GetAbsolutePath(relativePath);
            if (!File.Exists(absPath)) return null;
            var bytes = await File.ReadAllBytesAsync(absPath);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Сохранение 3D-модели во внутреннюю папку Models.
        /// Возвращает относительный путь (например, "Models/abc123_factory.glb").
        /// </summary>
        public async Task<string> SaveModelAsync(Stream stream, string originalFileName)
        {
            var modelsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models");
            if (!Directory.Exists(modelsFolder))
                Directory.CreateDirectory(modelsFolder);

            var extension = Path.GetExtension(originalFileName);
            var baseName = Path.GetFileNameWithoutExtension(originalFileName);
            // Уникальное имя, чтобы не перезаписывать одинаковые файлы
            var uniqueName = $"{baseName}_{Guid.NewGuid().ToString()[..8]}{extension}";
            var filePath = Path.Combine(modelsFolder, uniqueName);

            using var output = File.Create(filePath);
            await stream.CopyToAsync(output);

            return Path.Combine("Models", uniqueName);
        }

        /// <summary>
        /// Получение абсолютного пути к модели по относительному.
        /// </summary>
        public string GetModelAbsolutePath(string relativePath)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
        }
    }
}
