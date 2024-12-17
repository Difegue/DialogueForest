using DialogueForest.Core.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DialogueForest.Core
{
    /// <summary>
    /// Source Generation support for <see cref="DialogueDatabase"/> to work in AOT.
    /// https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation
    /// https://learn.microsoft.com/en-us/dotnet/core/compatibility/serialization/8.0/publishtrimmed
    /// </summary>
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(DialogueDatabase))]
    [JsonSerializable(typeof(bool))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(JsonElement))]
    internal partial class JsonSourceGenerationContext : JsonSerializerContext
    {
    }
}
