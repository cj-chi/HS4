namespace PlaceholderSoftware.WetStuff;

public interface IDecalSettings
{
	WetDecalMode Mode { get; }

	float Saturation { get; }

	float SampleJitter { get; }

	bool EnableJitter { get; }

	float FaceSharpness { get; }

	LayerMode LayerMode { get; }

	ProjectionMode LayerProjection { get; }

	[NotNull]
	DecalLayer XLayer { get; }

	[NotNull]
	DecalLayer YLayer { get; }

	[NotNull]
	DecalLayer ZLayer { get; }

	DecalShape Shape { get; }

	float EdgeFadeoff { get; }
}
