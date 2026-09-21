using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace notaverse_threejs_test1.Models
{
    public class SceneObjectData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("name")]
        public string Name { get; set; } = "Без имени";

        [JsonPropertyName("parameters")]
        public List<ObjectParameter> Parameters { get; set; } = new();
    }

    public class ObjectParameter
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// "text", "link", "image", "file"
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = "text";

        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Пути к файлам (относительно папки AttachedFiles).
        /// </summary>
        [JsonPropertyName("filePaths")]
        public List<string> FilePaths { get; set; } = new();
    }
}
