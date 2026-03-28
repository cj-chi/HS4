using UnityEngine;

namespace PlaceholderSoftware.WetStuff;

[RequireComponent(typeof(ParticleWetSplatter))]
[HelpURL("https://placeholder-software.co.uk/wetstuff/docs/Reference/ParticleWetSplatterTemplate/")]
public class ParticleWetSplatterTemplate : MonoBehaviour
{
	[SerializeField]
	[UsedImplicitly]
	[Range(0f, 1f)]
	[Tooltip("Chance of this template being used")]
	private float _probability;

	[SerializeField]
	[UsedImplicitly]
	private DecalSettings _settings;

	public DecalSettings Settings => _settings;

	public float Probability => _probability;

	protected virtual void OnValidate()
	{
		if (Settings != null)
		{
			Settings.OnChanged(requiresRebuild: true);
		}
	}
}
