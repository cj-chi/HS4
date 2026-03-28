using UnityEngine;

namespace PlaceholderSoftware.WetStuff.Weather;

[HelpURL("https://placeholder-software.co.uk/wetstuff/docs/Reference/BaseExternalWetnessSource/")]
public abstract class BaseExternalWetnessSource : MonoBehaviour
{
	public abstract float RainIntensity { get; }
}
