using System;
using System.IO;
using System.Linq;
using UnityEngine;
using uAudio.uAudio_backend;

public class ExternalAudioClip
{
	public static readonly float RangeValue8Bit = 1f / Mathf.Pow(2f, 7f);

	public static readonly float RangeValue16Bit = 1f / Mathf.Pow(2f, 15f);

	public static readonly float RangeValue24Bit = 1f / Mathf.Pow(2f, 23f);

	public static readonly float RangeValue32Bit = 1f / Mathf.Pow(2f, 31f);

	public const int BaseConvertSamples = 20480;

	public static AudioClip Load(string path, long SongLength, global::uAudio.uAudio_backend.uAudio uAudio, ref string szErrorMs)
	{
		switch (Path.GetExtension(path))
		{
		case ".wav":
		case ".WAV":
		{
			WaveHeader waveHeader = WaveHeader.ReadWaveHeader(path);
			float[] array = CreateRangedRawData(path, waveHeader.TrueWavBufIndex, waveHeader.TrueSamples, waveHeader.Channels, waveHeader.BitPerSample);
			if (array.Length == 0)
			{
				return null;
			}
			return CreateClip(Path.GetFileNameWithoutExtension(path), array, waveHeader.TrueSamples, waveHeader.Channels, waveHeader.Frequency);
		}
		case ".mp3":
		case ".MP3":
		{
			mp3AudioClip._uAudio = uAudio;
			AudioClip result = mp3AudioClip.LoadMp3(path, SongLength);
			szErrorMs = mp3AudioClip.szErrorMs;
			return result;
		}
		default:
			return null;
		}
	}

	public static AudioClip CreateClip(string name, float[] rawData, int lengthSamples, int channels, int frequency)
	{
		AudioClip audioClip = AudioClip.Create(name, lengthSamples, channels, frequency, stream: false);
		audioClip.SetData(rawData, 0);
		return audioClip;
	}

	public static float[] CreateRangedRawData(string path, int wavBufIndex, int samples, int channels, int bitPerSample)
	{
		byte[] array = File.ReadAllBytes(path);
		if (array.Length == 0)
		{
			return null;
		}
		return CreateRangedRawData(array, wavBufIndex, samples, channels, bitPerSample);
	}

	public static float[] CreateRangedRawData(byte[] data, int wavBufIndex, int samples, int channels, int bitPerSample)
	{
		float[] array = new float[samples * channels];
		int num = bitPerSample / 8;
		int num2 = wavBufIndex;
		try
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = ByteToFloat(data, num2, bitPerSample);
				num2 += num;
			}
			return array;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString() + ": 対応してない音声ファイル");
			return Enumerable.Empty<float>().ToArray();
		}
	}

	private static float ByteToFloat(byte[] data, int index, int bitPerSample)
	{
		float num = 0f;
		return bitPerSample switch
		{
			8 => (float)(data[index] - 128) * RangeValue8Bit, 
			16 => (float)BitConverter.ToInt16(data, index) * RangeValue16Bit, 
			_ => throw new Exception(bitPerSample + "bit is not supported."), 
		};
	}

	public static void LoadFile(string targetFileIN, string targetFile, ref bool _loadedTarget, global::uAudio.uAudio_backend.uAudio uAudio)
	{
		if (!_loadedTarget || uAudio.targetFile != targetFile)
		{
			_loadedTarget = true;
			uAudio.LoadFile(targetFileIN);
		}
	}
}
