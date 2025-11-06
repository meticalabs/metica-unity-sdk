// MeticaAdError.cs

#nullable enable

using System;

namespace Metica.Ads
{
public record MeticaAdError(
    string message,
    string? adUnitId
);
}
