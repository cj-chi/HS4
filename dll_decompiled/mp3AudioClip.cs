using System;
using System.IO;
using NLayer;
using UnityEngine;
using uAudio.uAudio_backend;
using uAudioDemo.Mp3StreamingDemo;

internal class mp3AudioClip
{
	private static AudioClip.PCMReaderCallback SongReadLoop;

	private static ReadFullyStream readFullyStream;

	private static MpegFile playbackDevice;

	public static uAudio.uAudio_backend.uAudio _uAudio;

	public static bool SongDone;

	public static bool flare_SongEnd;

	public static string szErrorMs { get; set; }

	public static AudioClip LoadMp3(string targetFile, long SongLength)
	{
		SongReadLoop = Song_Stream_Loop;
		FileStream sourceStream = File.OpenRead(targetFile);
		if (readFullyStream != null)
		{
			readFullyStream.Dispose();
		}
		readFullyStream = new ReadFullyStream(sourceStream);
		readFullyStream.stream_CanSeek = true;
		byte[] array = new byte[1024];
		MpegFile mpegFile = new MpegFile(readFullyStream, canSeek: true);
		mpegFile.ReadSamples(array, 0, array.Length);
		playbackDevice = mpegFile;
		int lengthSamples;
		if (SongLength > int.MaxValue)
		{
			UnityEngine.Debug.LogWarning("uAudioPlayer - Song size over size on int #4sgh54h45h45");
			lengthSamples = int.MaxValue;
		}
		else
		{
			lengthSamples = (int)SongLength;
		}
		return AudioClip.Create("uAudio_song", lengthSamples, mpegFile.WaveFormat.Channels, mpegFile.WaveFormat.SampleRate, stream: true, SongReadLoop);
	}

	private static void Song_Stream_Loop(float[] data)
	{
		try
		{
			if (!SongDone)
			{
				if (_uAudio.uwa.audioPlayback.inputStream.Read(data, 0, data.Length) <= 0)
				{
					SongDone = true;
				}
			}
			else
			{
				flare_SongEnd = true;
			}
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError("#trg56hgtyhty" + ex.Message);
			UnityEngine.Debug.LogError("Decode Error #8f76s8dsvsd");
			szErrorMs = "Failed Play Sound\0";
		}
	}
}
