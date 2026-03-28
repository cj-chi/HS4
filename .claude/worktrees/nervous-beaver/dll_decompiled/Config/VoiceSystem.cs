using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using Manager;
using UniRx;
using UnityEngine;

namespace Config;

public class VoiceSystem : BaseSystem, IDisposable
{
	public class Voice
	{
		public string file { get; }

		public SoundData sound { get; }

		public Voice(string file, SoundData sound)
		{
			this.file = file;
			this.sound = sound;
		}
	}

	public SoundData PCM { get; } = new SoundData();

	public IReadOnlyDictionary<int, Voice> chara { get; }

	private CompositeDisposable disposables { get; } = new CompositeDisposable();

	public void Dispose()
	{
		disposables.Clear();
	}

	public VoiceSystem(string elementName, IReadOnlyDictionary<int, string> table)
		: base(elementName)
	{
		PCM.observer.Subscribe(delegate(SoundData sd)
		{
			MixerVolume.Set(Manager.Voice.Mixer, MixerVolume.Names.PCMVolume, sd.GetVolume());
		}).AddTo(disposables);
		chara = table.ToDictionary((KeyValuePair<int, string> v) => v.Key, (KeyValuePair<int, string> v) => new Voice(v.Value, new SoundData()));
		chara.Select((KeyValuePair<int, Voice> p) => new
		{
			sd = new
			{
				p.Key,
				p.Value.sound
			}
		}).ToList().ForEach(p =>
		{
			p.sd.sound.observer.Subscribe(delegate(SoundData sd)
			{
				Manager.Voice.ToPlaying(p.sd.Key).ForEach(delegate(AudioSource playing)
				{
					playing.volume = sd.GetVolume();
				});
			}).AddTo(disposables);
		});
	}

	public override void Init()
	{
		PCM.Switch = true;
		PCM.Volume = 100;
		foreach (KeyValuePair<int, Voice> item in chara)
		{
			SoundData sound = item.Value.sound;
			sound.Switch = true;
			sound.Volume = 100;
		}
	}

	public override void Read(string rootName, XmlDocument xml)
	{
		try
		{
			XmlNodeList xmlNodeList = null;
			string text = rootName + "/" + base.elementName + "/";
			xmlNodeList = xml.SelectNodes(text + "PCM");
			if (xmlNodeList != null && xmlNodeList.Item(0) is XmlElement xmlElement)
			{
				PCM.Parse(xmlElement.InnerText);
			}
			foreach (KeyValuePair<int, Voice> item in chara)
			{
				xmlNodeList = xml.SelectNodes(text + item.Value.file);
				if (xmlNodeList.Item(0) is XmlElement xmlElement2)
				{
					item.Value.sound.Parse(xmlElement2.InnerText);
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
		writer.WriteStartElement("PCM");
		writer.WriteValue(PCM);
		writer.WriteEndElement();
		foreach (KeyValuePair<int, Voice> item in chara)
		{
			Voice value = item.Value;
			writer.WriteStartElement(value.file);
			writer.WriteValue(value.sound);
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	[Conditional("OUTPUT_LOG")]
	private void Log(string name, string output)
	{
	}
}
