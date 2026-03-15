using System.Collections.Generic;
using Manager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Config;

public class VoiceSetting : BaseSetting
{
	private class SetData
	{
		public SoundData sd;

		public Toggle toggle;

		public Slider slider;

		public Image image;

		public int voiceKey = -9999;
	}

	[SerializeField]
	private GameObject prefab;

	[SerializeField]
	private RectTransform rtcPcm;

	[SerializeField]
	private RectTransform node;

	private Dictionary<Transform, SetData> dic = new Dictionary<Transform, SetData>();

	public override void Init()
	{
		IReadOnlyDictionary<int, VoiceSystem.Voice> chara = Voice._Config.chara;
		Add(rtcPcm);
		for (int i = 0; i < node.transform.childCount; i++)
		{
			RectTransform trans = node.transform.GetChild(i) as RectTransform;
			Add(trans);
		}
		int num = node.transform.childCount + ((node.transform.childCount % 2 != 0) ? 1 : 0);
		foreach (KeyValuePair<int, VoiceInfo.Param> item in Voice.infoTable)
		{
			if (chara.ContainsKey(item.Key))
			{
				Create(num++, item.Key, item.Value.Personality);
			}
		}
	}

	protected override void ValueToUI()
	{
		foreach (SetData value in dic.Values)
		{
			value.toggle.isOn = value.sd.Switch;
			value.slider.value = value.sd.Volume;
		}
	}

	private void Create(int num, int key, string name)
	{
		RectTransform component = Object.Instantiate(prefab, node.transform).GetComponent<RectTransform>();
		component.name = key.ToString();
		component.GetComponentInChildren<Text>().text = name;
		Add(key, component);
	}

	private bool Add(Transform trans)
	{
		if (dic.ContainsKey(trans))
		{
			return false;
		}
		SetData setData = new SetData
		{
			sd = Voice._Config.PCM,
			slider = trans.GetComponentInChildren<Slider>(),
			toggle = trans.GetComponentInChildren<Toggle>(),
			image = trans.GetComponentInChildren<Image>()
		};
		AddEvent(setData);
		dic.Add(trans, setData);
		return true;
	}

	private bool Add(int key, Transform trans)
	{
		if (dic.ContainsKey(trans))
		{
			return false;
		}
		SetData setData = new SetData
		{
			sd = Voice._Config.chara[key].sound,
			slider = trans.GetComponentInChildren<Slider>(),
			toggle = trans.GetComponentInChildren<Toggle>(),
			image = trans.GetComponentInChildren<Image>(),
			voiceKey = key
		};
		AddEvent(setData);
		dic.Add(trans, setData);
		return true;
	}

	private void AddEvent(SetData data)
	{
		data.toggle.onValueChanged.AsObservable().Subscribe(delegate(bool isOn)
		{
			data.sd.Switch = isOn;
			data.image.enabled = !isOn;
			EnterSE();
		});
		(from b in data.toggle.OnValueChangedAsObservable()
			select (b)).SubscribeToInteractable(data.slider);
		(from value in data.slider.onValueChanged.AsObservable()
			select (int)value).Subscribe(delegate(int value)
		{
			data.sd.Volume = value;
		});
		(from _ in data.slider.OnPointerDownAsObservable()
			where Input.GetMouseButtonDown(0)
			select _).Subscribe(delegate
		{
			EnterSE();
		});
		(from _ in data.slider.OnPointerUpAsObservable()
			where Input.GetMouseButtonUp(0)
			select _).Subscribe(delegate
		{
			if (data.voiceKey != -9999)
			{
				VoiceInfo.Param param = Voice.infoTable[data.voiceKey];
				Voice.OncePlay(new Voice.Loader
				{
					no = data.voiceKey,
					bundle = param.samplebundle,
					asset = param.sampleasset
				});
			}
		});
	}
}
