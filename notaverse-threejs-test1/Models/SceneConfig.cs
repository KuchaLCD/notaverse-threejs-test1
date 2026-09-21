using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace notaverse_threejs_test1.Models
{
    public class SceneConfig
    {
        [JsonPropertyName("camera")]
        public CameraConfig Camera { get; set; } = new();

        [JsonPropertyName("objects")]
        public List<SceneObjectData> Objects { get; set; } = new();

        [JsonPropertyName("modelFilePath")]
        public string ModelFilePath { get; set; } = string.Empty;
    }

    public class CameraConfig
    {
        [JsonPropertyName("positionX")]
        public double PositionX { get; set; } = 5;

        [JsonPropertyName("positionY")]
        public double PositionY { get; set; } = 5;

        [JsonPropertyName("positionZ")]
        public double PositionZ { get; set; } = 5;

        [JsonPropertyName("targetX")]
        public double TargetX { get; set; } = 0;

        [JsonPropertyName("targetY")]
        public double TargetY { get; set; } = 0;

        [JsonPropertyName("targetZ")]
        public double TargetZ { get; set; } = 0;
    }
}
