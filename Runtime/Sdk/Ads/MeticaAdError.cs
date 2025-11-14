// MeticaAdError.cs

#nullable enable

using System;
using Newtonsoft.Json;

namespace Metica.Ads
{
    public record MeticaAdError(
      string message,
      string? adUnitId
    );
  
    public static class MeticaAdErrorJson
    {
        public static MeticaAdError FromJson(string json) =>
            JsonConvert.DeserializeObject<MeticaAdError>(json)!;
    }
}
