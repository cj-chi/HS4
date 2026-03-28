using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Studio;

public class ItemColorData : ScriptableObject
{
	[Serializable]
	public class ColorData
	{
		[SerializeField]
		private int group = -1;

		[SerializeField]
		private int category = -1;

		[SerializeField]
		private int id = -1;

		[SerializeField]
		private bool[] colors;

		public int Group => group;

		public int Category => category;

		public int ID => id;

		public bool IsColor1 => colors.SafeGet(0);

		public bool IsColor2 => colors.SafeGet(1);

		public bool IsColor3 => colors.SafeGet(2);

		public bool IsColor4 => colors.SafeGet(3);

		public int Count => colors.Count((bool _b) => _b);

		public ColorData(int _group, int _category, int _id, ItemComponent _itemComponent, ParticleComponent _particleComponent)
		{
			bool[] useColor = _itemComponent.useColor;
			bool flag = _particleComponent != null && _particleComponent.check;
			colors = new bool[4]
			{
				useColor.SafeGet(0) || flag,
				useColor.SafeGet(1),
				useColor.SafeGet(2),
				_itemComponent.checkGlass
			};
			group = _group;
			category = _category;
			id = _id;
		}

		public ColorData(ColorData _src)
		{
			colors = new bool[4] { _src.IsColor1, _src.IsColor2, _src.IsColor3, _src.IsColor4 };
		}
	}

	[SerializeField]
	private List<ColorData> colorDatas;

	public List<ColorData> ColorDatas
	{
		get
		{
			return colorDatas;
		}
		set
		{
			colorDatas = value;
		}
	}
}
