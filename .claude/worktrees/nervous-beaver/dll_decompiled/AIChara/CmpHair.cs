using System;
using System.Collections.Generic;
using System.Linq;
using Illusion.CustomAttributes;
using UnityEngine;

namespace AIChara;

[DisallowMultipleComponent]
public class CmpHair : CmpBase
{
	[Serializable]
	public class BoneInfo
	{
		public Transform trfCorrect;

		public DynamicBone[] dynamicBone;

		[HideInInspector]
		public Vector3 basePos = Vector3.zero;

		[HideInInspector]
		public Vector3 baseRot = Vector3.zero;

		[Header("[位置 制限]---------------------")]
		public Vector3 posMin = new Vector3(0f, 0f, 0f);

		public Vector3 posMax = new Vector3(0f, 0f, 0f);

		[Header("[回転 制限]---------------------")]
		public Vector3 rotMin = new Vector3(0f, 0f, 0f);

		public Vector3 rotMax = new Vector3(0f, 0f, 0f);

		[HideInInspector]
		public Vector3 moveRate = Vector3.zero;

		[HideInInspector]
		public Vector3 rotRate = Vector3.zero;
	}

	[Header("< 髪の毛 >-------------------")]
	public Renderer[] rendHair;

	[Tooltip("根本の色を使用")]
	public bool useTopColor = true;

	[Tooltip("毛先の色を使用")]
	public bool useUnderColor = true;

	[Tooltip("毛先（肌ボタン）")]
	public bool useSameSkinColorButton;

	[Tooltip("メッシュが可能")]
	public bool useMesh;

	public BoneInfo[] boneInfo;

	[Space]
	[Button("SetDefaultPosition", "初期位置", new object[] { })]
	public int setdefaultposition;

	[Button("SetDefaultRotation", "初期回転", new object[] { })]
	public int setdefaultrotation;

	[Header("< 飾り >---------------------")]
	public bool useAcsColor01;

	public bool useAcsColor02;

	public bool useAcsColor03;

	public Renderer[] rendAccessory;

	public Color[] acsDefColor;

	[Button("SetColor", "アクセサリの初期色を設定", new object[] { })]
	public int setcolor;

	public CmpHair()
		: base(_baseDB: false)
	{
	}

	protected override void Reacquire()
	{
		base.Reacquire();
		if (boneInfo == null || boneInfo.Length == 0)
		{
			return;
		}
		FindAssist findAssist = new FindAssist();
		findAssist.Initialize(base.transform);
		KeyValuePair<string, GameObject> keyValuePair = findAssist.dictObjName.FirstOrDefault((KeyValuePair<string, GameObject> x) => x.Key.Contains("_top"));
		if (keyValuePair.Equals(default(KeyValuePair<string, GameObject>)))
		{
			return;
		}
		DynamicBone[] components = GetComponents<DynamicBone>();
		for (int num = 0; num < boneInfo.Length; num++)
		{
			Transform child = keyValuePair.Value.transform.GetChild(num);
			findAssist.Initialize(child);
			List<DynamicBone> list = new List<DynamicBone>();
			DynamicBone[] array = components;
			foreach (DynamicBone n in array)
			{
				if (!findAssist.dictObjName.FirstOrDefault((KeyValuePair<string, GameObject> x) => x.Key == n.m_Root.name).Equals(default(KeyValuePair<string, GameObject>)))
				{
					list.Add(n);
				}
			}
			boneInfo[num].dynamicBone = list.ToArray();
		}
	}

	public void SetColor()
	{
		if (rendAccessory.Length == 0)
		{
			return;
		}
		Material sharedMaterial = rendAccessory[0].sharedMaterial;
		if (null != sharedMaterial)
		{
			acsDefColor = new Color[3];
			if (sharedMaterial.HasProperty("_Color"))
			{
				acsDefColor[0] = sharedMaterial.GetColor("_Color");
			}
			if (sharedMaterial.HasProperty("_Color2"))
			{
				acsDefColor[1] = sharedMaterial.GetColor("_Color2");
			}
			if (sharedMaterial.HasProperty("_Color3"))
			{
				acsDefColor[2] = sharedMaterial.GetColor("_Color3");
			}
		}
	}

	public void SetDefaultPosition()
	{
		if (this.boneInfo == null || this.boneInfo.Length == 0)
		{
			return;
		}
		BoneInfo[] array = this.boneInfo;
		foreach (BoneInfo boneInfo in array)
		{
			if (null != boneInfo.trfCorrect)
			{
				boneInfo.trfCorrect.transform.localPosition = boneInfo.basePos;
			}
		}
	}

	public void SetDefaultRotation()
	{
		if (this.boneInfo == null || this.boneInfo.Length == 0)
		{
			return;
		}
		BoneInfo[] array = this.boneInfo;
		foreach (BoneInfo boneInfo in array)
		{
			if (null != boneInfo.trfCorrect)
			{
				boneInfo.trfCorrect.transform.localEulerAngles = boneInfo.baseRot;
			}
		}
	}

	public override void SetReferenceObject()
	{
		FindAssist findAssist = new FindAssist();
		findAssist.Initialize(base.transform);
		rendHair = (from x in GetComponentsInChildren<Renderer>(includeInactive: true)
			where !x.name.Contains("_acs")
			select x).ToArray();
		DynamicBone[] components = GetComponents<DynamicBone>();
		KeyValuePair<string, GameObject> keyValuePair = findAssist.dictObjName.FirstOrDefault((KeyValuePair<string, GameObject> x) => x.Key.Contains("_top"));
		if (keyValuePair.Equals(default(KeyValuePair<string, GameObject>)))
		{
			return;
		}
		this.boneInfo = new BoneInfo[keyValuePair.Value.transform.childCount];
		for (int num = 0; num < this.boneInfo.Length; num++)
		{
			Transform child = keyValuePair.Value.transform.GetChild(num);
			findAssist.Initialize(child);
			BoneInfo boneInfo = new BoneInfo();
			KeyValuePair<string, GameObject> keyValuePair2 = findAssist.dictObjName.FirstOrDefault((KeyValuePair<string, GameObject> x) => x.Key.Contains("_s"));
			if (!keyValuePair2.Equals(default(KeyValuePair<string, GameObject>)))
			{
				Transform trfCorrect = keyValuePair2.Value.transform;
				boneInfo.trfCorrect = trfCorrect;
				boneInfo.basePos = boneInfo.trfCorrect.transform.localPosition;
				boneInfo.posMin.x = boneInfo.trfCorrect.transform.localPosition.x + 0.1f;
				boneInfo.posMin.y = boneInfo.trfCorrect.transform.localPosition.y;
				boneInfo.posMin.z = boneInfo.trfCorrect.transform.localPosition.z + 0.1f;
				boneInfo.posMax.x = boneInfo.trfCorrect.transform.localPosition.x - 0.1f;
				boneInfo.posMax.y = boneInfo.trfCorrect.transform.localPosition.y - 0.2f;
				boneInfo.posMax.z = boneInfo.trfCorrect.transform.localPosition.z - 0.1f;
				boneInfo.baseRot = boneInfo.trfCorrect.transform.localEulerAngles;
				boneInfo.rotMin.x = boneInfo.trfCorrect.transform.localEulerAngles.x - 15f;
				boneInfo.rotMin.y = boneInfo.trfCorrect.transform.localEulerAngles.y - 15f;
				boneInfo.rotMin.z = boneInfo.trfCorrect.transform.localEulerAngles.z - 15f;
				boneInfo.rotMax.x = boneInfo.trfCorrect.transform.localEulerAngles.x + 15f;
				boneInfo.rotMax.y = boneInfo.trfCorrect.transform.localEulerAngles.y + 15f;
				boneInfo.rotMax.z = boneInfo.trfCorrect.transform.localEulerAngles.z + 15f;
				boneInfo.moveRate.x = Mathf.InverseLerp(boneInfo.posMin.x, boneInfo.posMax.x, boneInfo.basePos.x);
				boneInfo.moveRate.y = Mathf.InverseLerp(boneInfo.posMin.y, boneInfo.posMax.y, boneInfo.basePos.y);
				boneInfo.moveRate.z = Mathf.InverseLerp(boneInfo.posMin.z, boneInfo.posMax.z, boneInfo.basePos.z);
				boneInfo.rotRate.x = Mathf.InverseLerp(boneInfo.rotMin.x, boneInfo.rotMax.x, boneInfo.baseRot.x);
				boneInfo.rotRate.y = Mathf.InverseLerp(boneInfo.rotMin.y, boneInfo.rotMax.y, boneInfo.baseRot.y);
				boneInfo.rotRate.z = Mathf.InverseLerp(boneInfo.rotMin.z, boneInfo.rotMax.z, boneInfo.baseRot.z);
			}
			List<DynamicBone> list = new List<DynamicBone>();
			DynamicBone[] array = components;
			foreach (DynamicBone n in array)
			{
				if (!findAssist.dictObjName.FirstOrDefault((KeyValuePair<string, GameObject> x) => x.Key == n.m_Root.name).Equals(default(KeyValuePair<string, GameObject>)))
				{
					list.Add(n);
				}
			}
			boneInfo.dynamicBone = list.ToArray();
			this.boneInfo[num] = boneInfo;
		}
		findAssist = new FindAssist();
		findAssist.Initialize(base.transform);
		rendAccessory = (from x in findAssist.dictObjName
			where x.Key.Contains("_acs")
			select x.Value.GetComponent<Renderer>() into r
			where null != r
			select r).ToArray();
		SetColor();
	}

	public void ResetDynamicBonesHair(bool includeInactive = false)
	{
		if (this.boneInfo == null || this.boneInfo.Length == 0)
		{
			return;
		}
		BoneInfo[] array = this.boneInfo;
		foreach (BoneInfo boneInfo in array)
		{
			if (boneInfo.dynamicBone == null)
			{
				continue;
			}
			for (int j = 0; j < boneInfo.dynamicBone.Length; j++)
			{
				if (null != boneInfo.dynamicBone[j] && (boneInfo.dynamicBone[j].enabled || includeInactive))
				{
					boneInfo.dynamicBone[j].ResetParticlesPosition();
				}
			}
		}
	}

	public void EnableDynamicBonesHair(bool enable, ChaFileHair.PartsInfo parts = null)
	{
		if (this.boneInfo == null || this.boneInfo.Length == 0)
		{
			return;
		}
		if (enable)
		{
			if (parts.dictBundle == null || parts.dictBundle.Count == 0 || this.boneInfo.Length != parts.dictBundle.Count)
			{
				return;
			}
			for (int i = 0; i < this.boneInfo.Length; i++)
			{
				if (this.boneInfo[i].dynamicBone == null || !parts.dictBundle.TryGetValue(i, out var value))
				{
					continue;
				}
				for (int j = 0; j < this.boneInfo[i].dynamicBone.Length; j++)
				{
					DynamicBone dynamicBone = this.boneInfo[i].dynamicBone[j];
					if (!(null == dynamicBone) && dynamicBone.enabled != !value.noShake)
					{
						dynamicBone.enabled = !value.noShake;
						if (dynamicBone.enabled)
						{
							dynamicBone.ResetParticlesPosition();
						}
					}
				}
			}
			return;
		}
		BoneInfo[] array = this.boneInfo;
		foreach (BoneInfo boneInfo in array)
		{
			if (boneInfo.dynamicBone == null)
			{
				continue;
			}
			for (int l = 0; l < boneInfo.dynamicBone.Length; l++)
			{
				if (null != boneInfo.dynamicBone[l] && boneInfo.dynamicBone[l].enabled)
				{
					boneInfo.dynamicBone[l].enabled = false;
				}
			}
		}
	}

	private void Update()
	{
		if (this.boneInfo == null || this.boneInfo.Length == 0)
		{
			return;
		}
		BoneInfo[] array = this.boneInfo;
		foreach (BoneInfo boneInfo in array)
		{
			if (!(null == boneInfo.trfCorrect))
			{
				boneInfo.trfCorrect.transform.localPosition = new Vector3(Mathf.Lerp(boneInfo.posMin.x, boneInfo.posMax.x, boneInfo.moveRate.x), Mathf.Lerp(boneInfo.posMin.y, boneInfo.posMax.y, boneInfo.moveRate.y), Mathf.Lerp(boneInfo.posMin.z, boneInfo.posMax.z, boneInfo.moveRate.z));
				boneInfo.trfCorrect.transform.localEulerAngles = new Vector3(Mathf.Lerp(boneInfo.rotMin.x, boneInfo.rotMax.x, boneInfo.rotRate.x), Mathf.Lerp(boneInfo.rotMin.y, boneInfo.rotMax.y, boneInfo.rotRate.y), Mathf.Lerp(boneInfo.rotMin.z, boneInfo.rotMax.z, boneInfo.rotRate.z));
			}
		}
	}
}
