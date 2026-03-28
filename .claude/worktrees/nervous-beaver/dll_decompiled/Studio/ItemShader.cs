using UnityEngine;

namespace Studio;

[DefaultExecutionOrder(-5)]
public static class ItemShader
{
	public static int _Color { get; private set; }

	public static int _Color2 { get; private set; }

	public static int _Color3 { get; private set; }

	public static int _Color4 { get; private set; }

	public static int _PatternMask1 { get; private set; }

	public static int _PatternMask2 { get; private set; }

	public static int _PatternMask3 { get; private set; }

	public static int _Color1_2 { get; private set; }

	public static int _Color2_2 { get; private set; }

	public static int _Color3_2 { get; private set; }

	public static int _patternuv1 { get; private set; }

	public static int _patternuv2 { get; private set; }

	public static int _patternuv3 { get; private set; }

	public static int _patternuv1Rotator { get; private set; }

	public static int _patternuv2Rotator { get; private set; }

	public static int _patternuv3Rotator { get; private set; }

	public static int _patternclamp1 { get; private set; }

	public static int _patternclamp2 { get; private set; }

	public static int _patternclamp3 { get; private set; }

	public static int _alpha { get; private set; }

	public static int _EmissionColor { get; private set; }

	public static int _EmissionStrength { get; private set; }

	public static int _LightCancel { get; private set; }

	public static int _MainTex { get; private set; }

	public static int _Metallic { get; private set; }

	public static int _Metallic2 { get; private set; }

	public static int _Metallic3 { get; private set; }

	public static int _Metallic4 { get; private set; }

	public static int _Glossiness { get; private set; }

	public static int _Glossiness2 { get; private set; }

	public static int _Glossiness3 { get; private set; }

	public static int _Glossiness4 { get; private set; }

	public static int _UsesWaterVolume { get; private set; }

	static ItemShader()
	{
		_Color = Shader.PropertyToID("_Color");
		_Color2 = Shader.PropertyToID("_Color2");
		_Color3 = Shader.PropertyToID("_Color3");
		_Color4 = Shader.PropertyToID("_Color4");
		_Color1_2 = Shader.PropertyToID("_Color1_2");
		_Color2_2 = Shader.PropertyToID("_Color2_2");
		_Color3_2 = Shader.PropertyToID("_Color3_2");
		_PatternMask1 = Shader.PropertyToID("_PatternMask1");
		_PatternMask2 = Shader.PropertyToID("_PatternMask2");
		_PatternMask3 = Shader.PropertyToID("_PatternMask3");
		_patternuv1 = Shader.PropertyToID("_patternuv1");
		_patternuv2 = Shader.PropertyToID("_patternuv2");
		_patternuv3 = Shader.PropertyToID("_patternuv3");
		_patternuv1Rotator = Shader.PropertyToID("_patternuv1Rotator");
		_patternuv2Rotator = Shader.PropertyToID("_patternuv2Rotator");
		_patternuv3Rotator = Shader.PropertyToID("_patternuv3Rotator");
		_patternclamp1 = Shader.PropertyToID("_patternclamp1");
		_patternclamp2 = Shader.PropertyToID("_patternclamp2");
		_patternclamp3 = Shader.PropertyToID("_patternclamp3");
		_alpha = Shader.PropertyToID("_alpha");
		_EmissionColor = Shader.PropertyToID("_EmissionColor");
		_EmissionStrength = Shader.PropertyToID("_EmissionStrength");
		_LightCancel = Shader.PropertyToID("_LightCancel");
		_MainTex = Shader.PropertyToID("_MainTex");
		_Metallic = Shader.PropertyToID("_Metallic");
		_Metallic2 = Shader.PropertyToID("_Metallic2");
		_Metallic3 = Shader.PropertyToID("_Metallic3");
		_Metallic4 = Shader.PropertyToID("_Metallic4");
		_Glossiness = Shader.PropertyToID("_Glossiness");
		_Glossiness2 = Shader.PropertyToID("_Glossiness2");
		_Glossiness3 = Shader.PropertyToID("_Glossiness3");
		_Glossiness4 = Shader.PropertyToID("_Glossiness4");
		_UsesWaterVolume = Shader.PropertyToID("_UsesWaterVolume");
	}
}
