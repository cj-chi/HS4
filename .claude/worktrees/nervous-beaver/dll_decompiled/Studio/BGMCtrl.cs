using System;
using System.IO;
using Manager;
using UnityEngine;

namespace Studio;

public class BGMCtrl
{
	public enum Repeat
	{
		None,
		All
	}

	private Repeat m_Repeat = Repeat.All;

	private int m_No;

	private bool m_Play;

	private AudioSource audioSource;

	public Repeat repeat
	{
		get
		{
			return m_Repeat;
		}
		set
		{
			if (Utility.SetStruct(ref m_Repeat, value) && (bool)audioSource)
			{
				audioSource.loop = repeat == Repeat.All;
			}
		}
	}

	public int no
	{
		get
		{
			return m_No;
		}
		set
		{
			if (m_No != value)
			{
				Play(value);
			}
		}
	}

	public bool play
	{
		get
		{
			return m_Play;
		}
		set
		{
			if (Utility.SetStruct(ref m_Play, value))
			{
				if (m_Play)
				{
					Play();
				}
				else
				{
					Stop();
				}
			}
		}
	}

	public bool isPause { get; private set; }

	public void Play()
	{
		if (isPause)
		{
			isPause = false;
			Manager.Sound.PlayBGM();
		}
		else
		{
			m_Play = true;
			Play(m_No);
		}
	}

	public void Play(int _no)
	{
		m_No = _no;
		if (!m_Play)
		{
			return;
		}
		Info.LoadCommonInfo value = null;
		if (Singleton<Info>.Instance.dicBGMLoadInfo.TryGetValue(m_No, out value))
		{
			if (Singleton<Studio>.Instance.outsideSoundCtrl.play)
			{
				Singleton<Studio>.Instance.outsideSoundCtrl.Stop();
			}
			Manager.Sound.Loader loader = new Manager.Sound.Loader
			{
				type = Manager.Sound.Type.BGM,
				bundle = value.bundlePath,
				asset = value.fileName
			};
			audioSource = Manager.Sound.Play(loader);
			if (!(audioSource == null))
			{
				audioSource.loop = repeat == Repeat.All;
				isPause = false;
			}
		}
	}

	public void Stop()
	{
		m_Play = false;
		isPause = false;
		Manager.Sound.StopBGM();
	}

	public void Pause()
	{
		if (m_Play)
		{
			isPause = true;
			Manager.Sound.PauseBGM();
		}
	}

	public void Save(BinaryWriter _writer, Version _version)
	{
		_writer.Write((int)m_Repeat);
		_writer.Write(m_No);
		_writer.Write(m_Play);
	}

	public void Load(BinaryReader _reader, Version _version)
	{
		m_Repeat = (Repeat)_reader.ReadInt32();
		m_No = _reader.ReadInt32();
		m_Play = _reader.ReadBoolean();
	}
}
