using System;
using System.Collections.Generic;
using System.Linq;
using Illusion.Extensions;
using LuxWater;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Studio;

public class MapComponent : MonoBehaviour
{
	[Serializable]
	public class OptionInfo
	{
		public GameObject[] objectsOn;

		public GameObject[] objectsOff;

		public bool Visible
		{
			set
			{
				if (value)
				{
					SetVisible(objectsOff, _value: false);
					SetVisible(objectsOn, _value: true);
				}
				else
				{
					SetVisible(objectsOn, _value: false);
					SetVisible(objectsOff, _value: true);
				}
			}
		}

		private void SetVisible(GameObject[] _objects, bool _value)
		{
			foreach (GameObject item in _objects.Where((GameObject v) => v != null))
			{
				item.SetActiveIfDifferent(_value);
			}
		}
	}

	private class SeaInfo
	{
		public Collider Collider { get; private set; }

		public LuxWater_WaterVolume WaterVolume { get; private set; }

		public bool Enable
		{
			set
			{
				if (Collider != null)
				{
					Collider.enabled = value;
				}
				if (WaterVolume != null)
				{
					WaterVolume.enabled = value;
				}
			}
		}

		public SeaInfo(Collider _collider, LuxWater_WaterVolume _waterVolume)
		{
			Collider = _collider;
			WaterVolume = _waterVolume;
		}
	}

	[Header("オプション")]
	public OptionInfo[] optionInfos;

	[Header("海面関係")]
	public GameObject objSeaParent;

	public Renderer[] renderersSea;

	[Header("ライト")]
	public OptionInfo[] lightInfos;

	public bool CheckOption => !((IReadOnlyCollection<OptionInfo>)(object)optionInfos).IsNullOrEmpty();

	public bool IsLight => !((IReadOnlyCollection<OptionInfo>)(object)lightInfos).IsNullOrEmpty();

	public void SetOptionVisible(bool _value)
	{
		if (!((IReadOnlyCollection<OptionInfo>)(object)optionInfos).IsNullOrEmpty())
		{
			OptionInfo[] array = optionInfos;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Visible = _value;
			}
		}
	}

	public void SetOptionVisible(int _idx, bool _value)
	{
		optionInfos.SafeProc(_idx, delegate(OptionInfo _info)
		{
			_info.Visible = _value;
		});
	}

	public void SetSeaRenderer()
	{
		if (!(objSeaParent == null))
		{
			renderersSea = objSeaParent.GetComponentsInChildren<Renderer>();
		}
	}

	public void SetupSea()
	{
		if (((IReadOnlyCollection<Renderer>)(object)renderersSea).IsNullOrEmpty())
		{
			return;
		}
		this.LateUpdateAsObservable().Take(1).Subscribe(delegate
		{
			foreach (Renderer item in renderersSea.Where((Renderer v) => v != null))
			{
				Material material = item.material;
				material.DisableKeyword("USINGWATERVOLUME");
				item.material = material;
			}
		});
	}

	public void SetLightVisible(bool _value)
	{
		if (!((IReadOnlyCollection<OptionInfo>)(object)lightInfos).IsNullOrEmpty())
		{
			OptionInfo[] array = lightInfos;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Visible = _value;
			}
		}
	}
}
