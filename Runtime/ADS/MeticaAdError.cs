// MeticaAd.cs

using System;

namespace Metica.ADS
{
[Serializable]
public class MeticaAdError
{
    public string message;

    public MeticaAdError(String message)
    {
        this.message = message;
    }
}
}
