using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NovaStaff.Shared.Serialization;

public static class SystemJson
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

        DefaultIgnoreCondition =
            JsonIgnoreCondition.WhenWritingNull,

        Encoder =
            JavaScriptEncoder.UnsafeRelaxedJsonEscaping,

        ReferenceHandler =
            ReferenceHandler.IgnoreCycles,

        WriteIndented = false
    };
}