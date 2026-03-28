using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Studio;

public class BackgroundList : MonoBehaviour
{
	public static string dirName = "bg";

	[SerializeField]
	private GameObject objectNode;

	[SerializeField]
	private Transform transformRoot;

	[SerializeField]
	private BackgroundCtrl backgroundCtrl;

	private List<string> listPath = new List<string>();

	private Dictionary<int, StudioNode> dicNode = new Dictionary<int, StudioNode>();

	private int select = -1;

	public void UpdateUI()
	{
		SetSelect(select, _flag: false);
		select = listPath.FindIndex((string v) => v == Singleton<Studio>.Instance.sceneInfo.background);
		SetSelect(select, _flag: true);
	}

	private void OnClickSelect(int _idx)
	{
		SetSelect(select, _flag: false);
		select = _idx;
		SetSelect(select, _flag: true);
		backgroundCtrl.Load((select != -1) ? listPath[_idx] : "");
	}

	private void SetSelect(int _idx, bool _flag)
	{
		StudioNode value = null;
		if (dicNode.TryGetValue(_idx, out value))
		{
			value.select = _flag;
		}
	}

	private void InitList()
	{
		for (int i = 0; i < transformRoot.childCount; i++)
		{
			Object.Destroy(transformRoot.GetChild(i).gameObject);
		}
		transformRoot.DetachChildren();
		listPath = (from _s in Directory.GetFiles(DefaultData.Create(dirName), "*.png")
			select DefaultData.RootName + "/" + dirName + "/" + Path.GetFileName(_s)).ToList();
		IEnumerable<string> collection = from _s in Directory.GetFiles(UserData.Create(dirName), "*.png")
			select UserData.RootName + "/" + dirName + "/" + Path.GetFileName(_s);
		listPath.AddRange(collection);
		CreateNode(-1, "なし");
		int count = listPath.Count;
		for (int num = 0; num < count; num++)
		{
			CreateNode(num, Path.GetFileNameWithoutExtension(listPath[num]));
		}
		select = listPath.FindIndex((string v) => v == Singleton<Studio>.Instance.sceneInfo.background);
		SetSelect(select, _flag: true);
	}

	private void CreateNode(int _idx, string _text)
	{
		GameObject obj = Object.Instantiate(objectNode);
		obj.transform.SetParent(transformRoot, worldPositionStays: false);
		StudioNode component = obj.GetComponent<StudioNode>();
		component.active = true;
		component.addOnClick = delegate
		{
			OnClickSelect(_idx);
		};
		component.text = _text;
		dicNode.Add(_idx, component);
	}

	private void Start()
	{
		InitList();
	}
}
