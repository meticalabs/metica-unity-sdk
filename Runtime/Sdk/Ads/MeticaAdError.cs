// MeticaAd.cs

using System;

namespace Metica.Ads
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
