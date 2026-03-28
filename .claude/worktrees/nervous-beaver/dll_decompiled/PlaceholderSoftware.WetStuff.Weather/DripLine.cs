using UnityEngine;

namespace PlaceholderSoftware.WetStuff.Weather;

[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(ParticleWetSplatter))]
[HelpURL("https://placeholder-software.co.uk/wetstuff/docs/Reference/DripLine/")]
public class DripLine : MonoBehaviour
{
	[SerializeField]
	private bool _endpointEditorSectionExpanded;

	[SerializeField]
	private bool _autoconfigureEditorSectionExpanded;

	[SerializeField]
	private bool _autoconfigureSplatterEditorSectionExpanded;

	[SerializeField]
	private bool _otherSettingsEditorSectionExpanded;

	[SerializeField]
	public BaseExternalWetnessSource WetnessSource;

	[SerializeField]
	public Transform StartPoint;

	[SerializeField]
	public Transform EndPoint;

	[SerializeField]
	[Range(0f, 10f)]
	public float EmissionRateMultiplier = 1f;

	[SerializeField]
	[Range(0f, 3f)]
	public float RainTimeScale = 0.1f;

	[SerializeField]
	[Range(0f, 3f)]
	public float DryTimeScale = 0.01f;

	[SerializeField]
	public bool LiveUpdateTransform;

	private Vector3 _lastStartPosition;

	private Vector3 _lastEndPosition;

	private float _emissionRateFromLength;

	private float _intensity;

	private ParticleSystem _particles;

	public void Awake()
	{
		_particles = GetComponent<ParticleSystem>();
		UpdatePosition(force: true);
	}

	public void Update()
	{
		ParticleSystem.EmissionModule emission = _particles.emission;
		float num = ((_intensity < WetnessSource.RainIntensity) ? RainTimeScale : DryTimeScale);
		_intensity = Mathf.Lerp(_intensity, Mathf.Clamp01(WetnessSource.RainIntensity), Time.deltaTime * num);
		emission.rateOverTimeMultiplier = EmissionRateMultiplier * _emissionRateFromLength * _intensity;
		if (LiveUpdateTransform)
		{
			UpdatePosition();
		}
	}

	private void UpdatePosition(bool force = false)
	{
		Vector3 position = StartPoint.position;
		Vector3 position2 = EndPoint.position;
		if (!force)
		{
			Vector3 vector = _lastStartPosition - position;
			Vector3 vector2 = _lastEndPosition - position2;
			if (vector.x < float.Epsilon && vector.y < float.Epsilon && vector.z < float.Epsilon && vector2.x < float.Epsilon && vector2.y < float.Epsilon && vector2.z < float.Epsilon)
			{
				return;
			}
		}
		CalculateLineTransform(position, position2, out var position3, out var rotation, out var scale);
		ParticleSystem.ShapeModule shape = _particles.shape;
		shape.position = position3;
		shape.rotation = rotation;
		shape.scale = scale;
		_emissionRateFromLength = Vector3.Distance(position, position2) / 2f * 10f;
		_lastStartPosition = position;
		_lastEndPosition = position2;
	}

	public static void CalculateLineTransform(Vector3 a, Vector3 b, out Vector3 position, out Vector3 rotation, out Vector3 scale)
	{
		position = a * 0.5f + b * 0.5f;
		Vector3 vector = b - a;
		float magnitude = vector.magnitude;
		scale = new Vector3(magnitude * 0.5f, 0f, 0f);
		Vector3 forward = vector / magnitude;
		rotation = (Quaternion.LookRotation(forward, Vector3.up) * Quaternion.AngleAxis(90f, Vector3.up)).eulerAngles;
	}
}
