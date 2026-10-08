using System.Collections.Generic;
using System.Text.Json.Serialization;
using Core.Ipc;
using Core.Models;

namespace Core.Serialization
{
    // Source generated so serialization also works in the Native AOT compiled hook.
    // Output must stay on a single line, since pipe messages are newline delimited.
    [JsonSerializable(typeof(List<ModelInfo>))]
    [JsonSerializable(typeof(PipeMessage))]
    public partial class CoreJsonContext : JsonSerializerContext
    {
    }
}
