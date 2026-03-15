using System.Collections.Generic;
using System.Linq;
using AIChara;
using Config;
using Illusion.CustomAttributes;
using Manager;
using UnityEngine;

namespace Map;

public class MapBobList : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> _list_m = new List<GameObject>();

	[SerializeField]
	private List<Renderer> _renderList_m = new List<Renderer>();

	[SerializeField]
	private List<GameObject> _list_f = new List<GameObject>();

	[SerializeField]
	private List<Renderer> _renderList_f = new List<Renderer>();

	[SerializeField]
	[Button("Set", "男ON", new object[] { true, 0 })]
	private int onButton_m;

	[SerializeField]
	[Button("Set", "男OFF", new object[] { false, 0 })]
	private int offButton_m;

	[SerializeField]
	[Button("ReCreateMaterialList", "男のRendererを取得し直す", new object[] { 0 })]
	private int materiaCreateButton_m;

	[SerializeField]
	[Button("Set", "女ON", new object[] { true, 1 })]
	private int onButton_f;

	[SerializeField]
	[Button("Set", "女OFF", new object[] { false, 1 })]
	private int offButton_f;

	[SerializeField]
	[Button("ReCreateMaterialList", "女のRendererを取得し直す", new object[] { 1 })]
	private int materiaCreateButton_f;

	private MaterialPropertyBlock mpbMob;

	public List<GameObject> list_m => _list_m;

	public List<Renderer> RenderList_m => _renderList_m;

	public List<GameObject> list_f => _list_f;

	public List<Renderer> RenderList_f => _renderList_f;

	private void Awake()
	{
		mpbMob = new MaterialPropertyBlock();
	}

	private void Update()
	{
		if (!Manager.Config.initialized)
		{
			return;
		}
		AppendSystem appendData = Manager.Config.AppendData;
		mpbMob.SetColor(ChaShader.Color, appendData.MobMColor_M);
		foreach (Renderer item in _renderList_m)
		{
			item.SetPropertyBlock(mpbMob);
		}
		mpbMob.SetColor(ChaShader.Color, appendData.MobMColor_F);
		foreach (Renderer item2 in _renderList_f)
		{
			item2.SetPropertyBlock(mpbMob);
		}
	}

	public void ReCreateMaterialList(int _sex)
	{
		if (_sex != 1)
		{
			_renderList_m = (from o in _list_m
				select o.GetComponent<Renderer>() into c
				where c != null
				select c).ToList();
		}
		if (_sex != 0)
		{
			_renderList_f = (from o in _list_f
				select o.GetComponent<Renderer>() into c
				where c != null
				select c).ToList();
		}
	}

	public void ON()
	{
		Set(visible: true, -1);
	}

	public void OFF()
	{
		Set(visible: false, -1);
	}

	public void Set(bool visible, int _sex)
	{
		if (_sex != 1)
		{
			list_m.ForEach(delegate(GameObject item)
			{
				item.SetActive(visible);
			});
		}
		if (_sex != 0)
		{
			list_f.ForEach(delegate(GameObject item)
			{
				item.SetActive(visible);
			});
		}
	}
}
