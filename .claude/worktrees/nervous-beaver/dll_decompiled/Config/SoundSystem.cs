using System;
using System.Diagnostics;
using System.Xml;
using UniRx;
using UnityEngine.Audio;

namespace Config;

public class SoundSystem : BaseSystem, IDisposable
{
	public (string Name, SoundData sd)[] Sounds
	{
		get
		{
			(string, SoundData)[] array = _Sounds;
			if (array == null)
			{
				(string, SoundData)[] obj = new(string, SoundData)[6]
				{
					("Master", Master),
					("BGM", BGM),
					("ENV", ENV),
					("SystemSE", SystemSE),
					("GameSE", GameSE),
					("AutoBGM", AutoBGM)
				};
				(string, SoundData)[] array2 = obj;
				_Sounds = obj;
				array = array2;
			}
			return array;
		}
	}

	private (string, SoundData)[] _Sounds { get; set; }

	public SoundData Master { get; } = new SoundData();

	public SoundData BGM { get; } = new SoundData();

	public SoundData ENV { get; } = new SoundData();

	public SoundData SystemSE { get; } = new SoundData();

	public SoundData GameSE { get; } = new SoundData();

	public SoundData AutoBGM { get; } = new SoundData();

	private CompositeDisposable disposables { get; } = new CompositeDisposable();

	public void Dispose()
	{
		disposables.Clear();
	}

	public SoundSystem(string elementName)
		: base(elementName)
	{
	}

	public void AddEvent(AudioMixer mixer)
	{
		Master.observer.Subscribe(delegate(SoundData sd)
		{
			MixerVolume.Set(mixer, MixerVolume.Names.MasterVolume, sd.GetVolume());
		}).AddTo(disposables);
		BGM.observer.Subscribe(delegate(SoundData sd)
		{
			MixerVolume.Set(mixer, MixerVolume.Names.BGMVolume, sd.GetVolume());
		}).AddTo(disposables);
		ENV.observer.Subscribe(delegate(SoundData sd)
		{
			MixerVolume.Set(mixer, MixerVolume.Names.ENVVolume, sd.GetVolume());
		}).AddTo(disposables);
		SystemSE.observer.Subscribe(delegate(SoundData sd)
		{
			MixerVolume.Set(mixer, MixerVolume.Names.SystemSEVolume, sd.GetVolume());
		}).AddTo(disposables);
		GameSE.observer.Subscribe(delegate(SoundData sd)
		{
			MixerVolume.Set(mixer, MixerVolume.Names.GameSEVolume, sd.GetVolume());
		}).AddTo(disposables);
	}

	public override void Init()
	{
		(string, SoundData)[] sounds = Sounds;
		for (int i = 0; i < sounds.Length; i++)
		{
			sounds[i].Item2.Switch = true;
		}
		Master.Volume = 100;
		BGM.Volume = 40;
		ENV.Volume = 80;
		SystemSE.Volume = 50;
		GameSE.Volume = 70;
		AutoBGM.Volume = 40;
	}

	public override void Read(string rootName, XmlDocument xml)
	{
		try
		{
			string text = rootName + "/" + base.elementName + "/";
			(string, SoundData)[] sounds = Sounds;
			for (int i = 0; i < sounds.Length; i++)
			{
				(string, SoundData) tuple = sounds[i];
				if (xml.SelectNodes(text + tuple.Item1).Item(0) is XmlElement xmlElement)
				{
					tuple.Item2.Parse(xmlElement.InnerText);
				}
			}
		}
		catch (Exception)
		{
		}
	}

	public override void Write(XmlWriter writer)
	{
		writer.WriteStartElement(base.elementName);
		(string, SoundData)[] sounds = Sounds;
		for (int i = 0; i < sounds.Length; i++)
		{
			(string, SoundData) tuple = sounds[i];
			writer.WriteStartElement(tuple.Item1);
			writer.WriteValue(tuple.Item2);
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	[Conditional("OUTPUT_LOG")]
	private void Log(string name, string output)
	{
	}
}
