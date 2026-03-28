using System;
using UnityEngine;

public static class LuxWaterUtils
{
	public struct GersterWavesDescription
	{
		public Vector3 intensity;

		public Vector4 steepness;

		public Vector4 amp;

		public Vector4 freq;

		public Vector4 speed;

		public Vector4 dirAB;

		public Vector4 dirCD;

		public Vector4 secondaryWaveParams;
	}

	public static void GetGersterWavesDescription(ref GersterWavesDescription Description, Material WaterMaterial)
	{
		Description.intensity = WaterMaterial.GetVector("_GerstnerVertexIntensity");
		Description.steepness = WaterMaterial.GetVector("_GSteepness");
		Description.amp = WaterMaterial.GetVector("_GAmplitude");
		Description.freq = WaterMaterial.GetVector("_GFinalFrequency");
		Description.speed = WaterMaterial.GetVector("_GFinalSpeed");
		Description.dirAB = WaterMaterial.GetVector("_GDirectionAB");
		Description.dirCD = WaterMaterial.GetVector("_GDirectionCD");
		Description.secondaryWaveParams = WaterMaterial.GetVector("_GerstnerSecondaryWaves");
	}

	public static Vector3 InternalGetGestnerDisplacement(Vector2 xzVtx, Vector4 intensity, Vector4 steepness, Vector4 amp, Vector4 freq, Vector4 speed, Vector4 dirAB, Vector4 dirCD, float TimeOffset)
	{
		Vector4 vector = default(Vector4);
		vector.x = steepness.x * amp.x * dirAB.x;
		vector.y = steepness.x * amp.x * dirAB.y;
		vector.z = steepness.y * amp.y * dirAB.z;
		vector.w = steepness.y * amp.y * dirAB.w;
		Vector4 vector2 = default(Vector4);
		vector2.x = steepness.z * amp.z * dirCD.x;
		vector2.y = steepness.z * amp.z * dirCD.y;
		vector2.z = steepness.w * amp.w * dirCD.z;
		vector2.w = steepness.w * amp.w * dirCD.w;
		Vector4 vector3 = default(Vector4);
		vector3.x = freq.x * (dirAB.x * xzVtx.x + dirAB.y * xzVtx.y);
		vector3.y = freq.y * (dirAB.z * xzVtx.x + dirAB.w * xzVtx.y);
		vector3.z = freq.z * (dirCD.x * xzVtx.x + dirCD.y * xzVtx.y);
		vector3.w = freq.w * (dirCD.z * xzVtx.x + dirCD.w * xzVtx.y);
		float num = Time.timeSinceLevelLoad + TimeOffset;
		Vector4 vector4 = default(Vector4);
		vector4.x = num * speed.x;
		vector4.y = num * speed.y;
		vector4.z = num * speed.z;
		vector4.w = num * speed.w;
		vector3.x += vector4.x;
		vector3.y += vector4.y;
		vector3.z += vector4.z;
		vector3.w += vector4.w;
		Vector4 vector5 = default(Vector4);
		vector5.x = (float)Math.Cos(vector3.x);
		vector5.y = (float)Math.Cos(vector3.y);
		vector5.z = (float)Math.Cos(vector3.z);
		vector5.w = (float)Math.Cos(vector3.w);
		Vector4 vector6 = default(Vector4);
		vector6.x = (float)Math.Sin(vector3.x);
		vector6.y = (float)Math.Sin(vector3.y);
		vector6.z = (float)Math.Sin(vector3.z);
		vector6.w = (float)Math.Sin(vector3.w);
		Vector3 result = default(Vector3);
		result.x = (vector5.x * vector.x + vector5.y * vector.z + vector5.z * vector2.x + vector5.w * vector2.z) * intensity.x;
		result.z = (vector5.x * vector.y + vector5.y * vector.w + vector5.z * vector2.y + vector5.w * vector2.w) * intensity.z;
		result.y = (vector6.x * amp.x + vector6.y * amp.y + vector6.z * amp.z + vector6.w * amp.w) * intensity.y;
		return result;
	}

	public static Vector3 GetGestnerDisplacement(Vector3 WorldPosition, GersterWavesDescription Description, float TimeOffset)
	{
		Vector2 xzVtx = default(Vector2);
		xzVtx.x = WorldPosition.x;
		xzVtx.y = WorldPosition.z;
		Vector3 result = InternalGetGestnerDisplacement(xzVtx, Description.intensity, Description.steepness, Description.amp, Description.freq, Description.speed, Description.dirAB, Description.dirCD, TimeOffset);
		if (Description.secondaryWaveParams.x > 0f)
		{
			xzVtx.x += result.x;
			xzVtx.y += result.z;
			result += InternalGetGestnerDisplacement(xzVtx, Description.intensity, Description.steepness * Description.secondaryWaveParams.z, Description.amp * Description.secondaryWaveParams.x, Description.freq * Description.secondaryWaveParams.y, Description.speed * Description.secondaryWaveParams.w, new Vector4(Description.dirAB.z, Description.dirAB.w, Description.dirAB.x, Description.dirAB.y), new Vector4(Description.dirCD.z, Description.dirCD.w, Description.dirCD.x, Description.dirCD.y), TimeOffset);
		}
		return result;
	}

	public static Vector3 GetGestnerDisplacementSingle(Vector3 WorldPosition, GersterWavesDescription Description, float TimeOffset)
	{
		Vector2 vector = default(Vector2);
		vector.x = WorldPosition.x;
		vector.y = WorldPosition.z;
		Vector4 vector2 = default(Vector4);
		vector2.x = Description.steepness.x * Description.amp.x * Description.dirAB.x;
		vector2.y = Description.steepness.x * Description.amp.x * Description.dirAB.y;
		vector2.z = Description.steepness.y * Description.amp.y * Description.dirAB.z;
		vector2.w = Description.steepness.y * Description.amp.y * Description.dirAB.w;
		Vector4 vector3 = default(Vector4);
		vector3.x = Description.steepness.z * Description.amp.z * Description.dirCD.x;
		vector3.y = Description.steepness.z * Description.amp.z * Description.dirCD.y;
		vector3.z = Description.steepness.w * Description.amp.w * Description.dirCD.z;
		vector3.w = Description.steepness.w * Description.amp.w * Description.dirCD.w;
		Vector4 vector4 = default(Vector4);
		vector4.x = Description.freq.x * (Description.dirAB.x * vector.x + Description.dirAB.y * vector.y);
		vector4.y = Description.freq.y * (Description.dirAB.z * vector.x + Description.dirAB.w * vector.y);
		vector4.z = Description.freq.z * (Description.dirCD.x * vector.x + Description.dirCD.y * vector.y);
		vector4.w = Description.freq.w * (Description.dirCD.z * vector.x + Description.dirCD.w * vector.y);
		float num = Time.timeSinceLevelLoad + TimeOffset;
		Vector4 vector5 = default(Vector4);
		vector5.x = num * Description.speed.x;
		vector5.y = num * Description.speed.y;
		vector5.z = num * Description.speed.z;
		vector5.w = num * Description.speed.w;
		vector4.x += vector5.x;
		vector4.y += vector5.y;
		vector4.z += vector5.z;
		vector4.w += vector5.w;
		Vector4 vector6 = default(Vector4);
		vector6.x = (float)Math.Cos(vector4.x);
		vector6.y = (float)Math.Cos(vector4.y);
		vector6.z = (float)Math.Cos(vector4.z);
		vector6.w = (float)Math.Cos(vector4.w);
		Vector4 vector7 = default(Vector4);
		vector7.x = (float)Math.Sin(vector4.x);
		vector7.y = (float)Math.Sin(vector4.y);
		vector7.z = (float)Math.Sin(vector4.z);
		vector7.w = (float)Math.Sin(vector4.w);
		Vector3 result = default(Vector3);
		result.x = (vector6.x * vector2.x + vector6.y * vector2.z + vector6.z * vector3.x + vector6.w * vector3.z) * Description.intensity.x;
		result.z = (vector6.x * vector2.y + vector6.y * vector2.w + vector6.z * vector3.y + vector6.w * vector3.w) * Description.intensity.z;
		result.y = (vector7.x * Description.amp.x + vector7.y * Description.amp.y + vector7.z * Description.amp.z + vector7.w * Description.amp.w) * Description.intensity.y;
		return result;
	}
}
