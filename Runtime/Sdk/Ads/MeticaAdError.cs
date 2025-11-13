// MeticaAd.cs

using System;
using UnityEngine;

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

    public static MeticaAdError FromJson(string json)
    {
        return JsonUtility.FromJson<MeticaAdError>(json);
    }
}
}
