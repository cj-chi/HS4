using System;
using System.Collections.Generic;
using IllusionUtility.GetUtility;
using Manager;
using UniRx;
using UnityEngine;

public class HParticleCtrl
{
	public class ParticleInfo
	{
		public string assetPath;

		public string file;

		public string manifest;
	}

	public class ParticleSetInfo
	{
		public int numParent;

		public string nameParent;

		public Vector3 pos;

		public Vector3 rot;

		public List<Timing> timings = new List<Timing>();
	}

	public class Timing
	{
		public float playStartTime;

		public int playID = -1;
	}

	public class HParticle
	{
		public int patternID;

		public List<GameObject> particleObj = new List<GameObject>();

		public int particleStart;

		public bool play;

		public float time;
	}

	public class ParticleInfoAI
	{
		public string assetPath;

		public string file;

		public string manifest;

		public int numParent;

		public string nameParent;

		public Vector3 pos;

		public Vector3 rot;

		public ParticleSystem particle;

		public GameObject particleCacheObj;

		public Transform particleCacheTrs;

		public Transform Parent;
	}

	private List<ParticleInfo> particleInfos = new List<ParticleInfo>();

	private List<ParticleSetInfo> lstParticleInfo = new List<ParticleSetInfo>();

	private List<HParticle> lstParticlePtns = new List<HParticle>();

	private List<ParticleInfoAI> lstParticle = new List<ParticleInfoAI>();

	private static List<int> urine = new List<int> { 2, 3, 4, 5 };

	private Dictionary<int, Dictionary<int, MeshFilter[]>> textureAnimators = new Dictionary<int, Dictionary<int, MeshFilter[]>>();

	private Dictionary<int, Dictionary<int, Dictionary<int, Renderer>>> textureAnimatorsRenderer = new Dictionary<int, Dictionary<int, Dictionary<int, Renderer>>>();

	private Dictionary<int, Dictionary<int, Dictionary<int, BoxCollider>>> textureAnimatorsCol = new Dictionary<int, Dictionary<int, Dictionary<int, BoxCollider>>>();

	private IDisposable disposable;

	private Transform Place;

	public void Init(Transform place)
	{
		particleInfos = HSceneManager.HResourceTables.lstHParticleCtrl;
		lstParticleInfo = HSceneManager.HResourceTables.lstHParticleSetInfo;
		lstParticle = new List<ParticleInfoAI>();
		Place = place;
		for (int i = 0; i < HSceneManager.HResourceTables.lstHParticleAICtrl.Count; i++)
		{
			ParticleInfoAI particleInfoAI = HSceneManager.HResourceTables.lstHParticleAICtrl[i];
			ParticleInfoAI particleInfoAI2 = new ParticleInfoAI();
			particleInfoAI2.assetPath = particleInfoAI.assetPath;
			particleInfoAI2.file = particleInfoAI.file;
			particleInfoAI2.manifest = particleInfoAI.manifest;
			particleInfoAI2.numParent = particleInfoAI.numParent;
			particleInfoAI2.nameParent = particleInfoAI.nameParent;
			particleInfoAI2.pos = particleInfoAI.pos;
			particleInfoAI2.rot = particleInfoAI.rot;
			lstParticle.Add(particleInfoAI2);
		}
		if (disposable != null)
		{
			disposable.Dispose();
			disposable = null;
		}
		disposable = Observable.EveryEndOfFrame().Subscribe(delegate
		{
			UpdatePosition();
		});
	}

	private void UpdatePosition()
	{
		for (int i = 0; i < lstParticle.Count; i++)
		{
			ParticleInfoAI particleInfoAI = lstParticle[i];
			if (particleInfoAI != null && !(particleInfoAI.Parent == null) && !(particleInfoAI.particleCacheTrs == null))
			{
				particleInfoAI.particleCacheTrs.SetParent(particleInfoAI.Parent, worldPositionStays: false);
				particleInfoAI.particleCacheTrs.localPosition = particleInfoAI.pos;
				particleInfoAI.particleCacheTrs.localRotation = Quaternion.Euler(particleInfoAI.rot);
				if (Place != null)
				{
					particleInfoAI.particleCacheTrs.SetParent(Place);
				}
				else
				{
					particleInfoAI.particleCacheTrs.parent = null;
				}
				particleInfoAI.particleCacheTrs.localScale = Vector3.one;
			}
		}
	}

	public void ParticleLoad(GameObject _objBody, int _sex)
	{
		if (_objBody == null)
		{
			return;
		}
		HParticle hParticle = null;
		for (int i = 0; i < lstParticleInfo.Count; i++)
		{
			if (lstParticleInfo[i].numParent != _sex)
			{
				continue;
			}
			hParticle = null;
			for (int j = 0; j < lstParticlePtns.Count; j++)
			{
				if (lstParticlePtns[j].patternID == i)
				{
					hParticle = lstParticlePtns[j];
				}
			}
			Transform transform = _objBody.transform.FindLoop(lstParticleInfo[i].nameParent);
			if (transform == null)
			{
				continue;
			}
			if (hParticle == null)
			{
				int count = lstParticlePtns.Count;
				lstParticlePtns.Add(new HParticle());
				if (!urine.Contains(i))
				{
					foreach (Timing timing in lstParticleInfo[i].timings)
					{
						GameObject gameObject = CommonLib.LoadAsset<GameObject>(particleInfos[timing.playID].assetPath, particleInfos[timing.playID].file, clone: true, particleInfos[timing.playID].manifest);
						Singleton<HSceneManager>.Instance.hashUseAssetBundle.Add(particleInfos[timing.playID].assetPath);
						if (!(gameObject == null))
						{
							gameObject.transform.SetParent(transform, worldPositionStays: false);
							gameObject.transform.localPosition = lstParticleInfo[i].pos;
							gameObject.transform.localRotation = Quaternion.Euler(lstParticleInfo[i].rot);
							gameObject.transform.localScale = Vector3.one;
							gameObject.SetActive(value: false);
							SetMeshCollider(gameObject);
							lstParticlePtns[count].particleObj.Add(gameObject);
						}
					}
				}
				lstParticlePtns[count].patternID = i;
				lstParticlePtns[count].time = 0f;
				lstParticlePtns[count].particleStart = 0;
				lstParticlePtns[count].play = false;
				continue;
			}
			if (!urine.Contains(i))
			{
				if (hParticle.particleObj.Count > 0)
				{
					for (int k = 0; k < hParticle.particleObj.Count; k++)
					{
						if (!(hParticle.particleObj[k] == null))
						{
							UnityEngine.Object.Destroy(hParticle.particleObj[k]);
							hParticle.particleObj[k] = null;
						}
					}
				}
				foreach (Timing timing2 in lstParticleInfo[i].timings)
				{
					GameObject gameObject2 = CommonLib.LoadAsset<GameObject>(particleInfos[timing2.playID].assetPath, particleInfos[timing2.playID].file, clone: true, particleInfos[timing2.playID].manifest);
					Singleton<HSceneManager>.Instance.hashUseAssetBundle.Add(particleInfos[timing2.playID].assetPath);
					if (!(gameObject2 == null))
					{
						gameObject2.transform.SetParent(transform, worldPositionStays: false);
						gameObject2.transform.localPosition = lstParticleInfo[i].pos;
						gameObject2.transform.localRotation = Quaternion.Euler(lstParticleInfo[i].rot);
						gameObject2.transform.localScale = Vector3.one;
						SetMeshCollider(gameObject2);
						hParticle.particleObj.Add(gameObject2);
					}
				}
			}
			hParticle.patternID = i;
			hParticle.time = 0f;
			hParticle.particleStart = 0;
			hParticle.play = false;
		}
		for (int l = 0; l < lstParticle.Count; l++)
		{
			ParticleInfoAI particleInfoAI = lstParticle[l];
			if (particleInfoAI.numParent != _sex)
			{
				continue;
			}
			Transform transform2 = _objBody.transform.FindLoop(lstParticle[l].nameParent);
			if (!(transform2 == null))
			{
				GameObject gameObject3 = CommonLib.LoadAsset<GameObject>(particleInfoAI.assetPath, particleInfoAI.file, clone: true, particleInfoAI.manifest);
				Singleton<HSceneManager>.Instance.hashUseAssetBundle.Add(particleInfoAI.assetPath);
				particleInfoAI.particle = gameObject3.GetComponent<ParticleSystem>();
				particleInfoAI.particleCacheObj = gameObject3;
				particleInfoAI.particleCacheObj.SetActive(value: false);
				particleInfoAI.particleCacheTrs = gameObject3.transform;
				particleInfoAI.Parent = transform2;
				particleInfoAI.particleCacheTrs.SetParent(transform2, worldPositionStays: false);
				particleInfoAI.particleCacheTrs.localPosition = particleInfoAI.pos;
				particleInfoAI.particleCacheTrs.localRotation = Quaternion.Euler(particleInfoAI.rot);
				if (Place != null)
				{
					particleInfoAI.particleCacheTrs.SetParent(Place);
				}
				else
				{
					particleInfoAI.particleCacheTrs.parent = null;
				}
				particleInfoAI.particleCacheTrs.localScale = Vector3.one;
			}
		}
		textureAnimatorsRenderer = new Dictionary<int, Dictionary<int, Dictionary<int, Renderer>>>();
		textureAnimatorsCol = new Dictionary<int, Dictionary<int, Dictionary<int, BoxCollider>>>();
		textureAnimators = new Dictionary<int, Dictionary<int, MeshFilter[]>>();
		for (int m = 0; m < lstParticlePtns.Count; m++)
		{
			textureAnimatorsRenderer.Add(m, new Dictionary<int, Dictionary<int, Renderer>>());
			textureAnimatorsCol.Add(m, new Dictionary<int, Dictionary<int, BoxCollider>>());
			textureAnimators.Add(m, new Dictionary<int, MeshFilter[]>());
			for (int n = 0; n < lstParticlePtns[m].particleObj.Count; n++)
			{
				textureAnimators[m].Add(n, lstParticlePtns[m].particleObj[n].GetComponentsInChildren<MeshFilter>(includeInactive: true));
				textureAnimatorsRenderer[m].Add(n, new Dictionary<int, Renderer>());
				textureAnimatorsCol[m].Add(n, new Dictionary<int, BoxCollider>());
				for (int num = 0; num < textureAnimators[m][n].Length; num++)
				{
					textureAnimatorsRenderer[m][n].Add(num, textureAnimators[m][n][num].GetComponent<Renderer>());
					textureAnimatorsCol[m][n].Add(num, textureAnimators[m][n][num].GetComponent<BoxCollider>());
				}
			}
		}
	}

	public void Proc()
	{
		for (int i = 0; i < lstParticlePtns.Count; i++)
		{
			if (!lstParticlePtns[i].play || lstParticlePtns[i].particleObj.Count <= 0)
			{
				continue;
			}
			int num = 0;
			lstParticlePtns[i].time += Time.deltaTime;
			int count = lstParticlePtns[i].particleObj.Count;
			for (int j = 0; j < count; j++)
			{
				if (lstParticlePtns[i].particleObj[j].activeSelf)
				{
					num++;
					continue;
				}
				int patternID = lstParticlePtns[i].patternID;
				if (!(lstParticleInfo[patternID].timings[j].playStartTime > lstParticlePtns[i].time))
				{
					lstParticlePtns[i].particleObj[j].SetActive(value: true);
					lstParticlePtns[i].particleStart++;
				}
			}
			if (count <= 0)
			{
				continue;
			}
			bool flag = false;
			for (int k = 0; k < lstParticlePtns[i].particleObj.Count; k++)
			{
				for (int l = 0; l < textureAnimators[i][k].Length; l++)
				{
					bool enabled = textureAnimatorsRenderer[i][k][l].enabled;
					textureAnimatorsCol[i][k][l].enabled = enabled;
					flag = flag || enabled;
				}
			}
			if (flag || lstParticlePtns[i].particleStart != lstParticlePtns[i].particleObj.Count)
			{
				continue;
			}
			lstParticlePtns[i].play = false;
			for (int m = 0; m < lstParticlePtns[i].particleObj.Count; m++)
			{
				lstParticlePtns[i].particleObj[m].SetActive(value: false);
				for (int n = 0; n < textureAnimators[i][m].Length; n++)
				{
					textureAnimatorsCol[i][m][n].enabled = true;
				}
			}
		}
	}

	public bool ReleaseObject(int _sex)
	{
		for (int i = 0; i < lstParticleInfo.Count; i++)
		{
			if (lstParticleInfo[i] != null && lstParticleInfo[i].numParent == _sex)
			{
				for (int j = 0; j < lstParticlePtns[i].particleObj.Count; j++)
				{
					UnityEngine.Object.Destroy(lstParticlePtns[i].particleObj[j]);
					lstParticlePtns[i].particleObj[j] = null;
				}
				lstParticlePtns[i] = null;
			}
		}
		for (int k = 0; k < lstParticle.Count; k++)
		{
			if (lstParticle[k].numParent == _sex)
			{
				UnityEngine.Object.Destroy(lstParticle[k].particleCacheObj);
				lstParticle[k].particleCacheObj = null;
				lstParticle[k].particleCacheTrs = null;
			}
		}
		return true;
	}

	public bool ReleaseObject()
	{
		for (int i = 0; i < lstParticlePtns.Count; i++)
		{
			HParticle hParticle = lstParticlePtns[i];
			if (hParticle != null && hParticle.particleObj != null)
			{
				for (int j = 0; j < hParticle.particleObj.Count; j++)
				{
					UnityEngine.Object.Destroy(hParticle.particleObj[j]);
					hParticle.particleObj[j] = null;
				}
				hParticle.particleObj.Clear();
			}
		}
		lstParticlePtns.Clear();
		textureAnimators.Clear();
		textureAnimatorsRenderer.Clear();
		textureAnimatorsCol.Clear();
		for (int k = 0; k < lstParticle.Count; k++)
		{
			UnityEngine.Object.Destroy(lstParticle[k].particleCacheObj);
			lstParticle[k].particleCacheObj = null;
			lstParticle[k].particleCacheTrs = null;
		}
		return true;
	}

	public void EndProc()
	{
		for (int i = 0; i < lstParticlePtns.Count; i++)
		{
			HParticle hParticle = lstParticlePtns[i];
			if (hParticle != null && hParticle.particleObj != null)
			{
				for (int j = 0; j < hParticle.particleObj.Count; j++)
				{
					UnityEngine.Object.Destroy(hParticle.particleObj[j]);
					hParticle.particleObj[j] = null;
				}
				hParticle.particleObj.Clear();
			}
		}
		lstParticlePtns.Clear();
		textureAnimators.Clear();
		textureAnimatorsRenderer.Clear();
		textureAnimatorsCol.Clear();
		for (int k = 0; k < lstParticle.Count; k++)
		{
			UnityEngine.Object.Destroy(lstParticle[k].particleCacheObj);
			lstParticle[k].particleCacheObj = null;
			lstParticle[k].particleCacheTrs = null;
			lstParticle[k].Parent = null;
		}
		if (disposable != null)
		{
			disposable.Dispose();
			disposable = null;
		}
	}

	public bool Play(int _particle)
	{
		bool flag = Manager.Config.HData.SioDraw == 1;
		if ((_particle == 0 || _particle == 1) && flag)
		{
			return false;
		}
		if (!urine.Contains(_particle))
		{
			for (int i = 0; i < lstParticlePtns.Count; i++)
			{
				if (lstParticlePtns[i].patternID != _particle || lstParticlePtns[i].particleObj == null)
				{
					continue;
				}
				lstParticlePtns[i].play = true;
				lstParticlePtns[i].time = 0f;
				lstParticlePtns[i].particleStart = 0;
				for (int j = 0; j < lstParticlePtns[i].particleObj.Count; j++)
				{
					lstParticlePtns[i].particleObj[j].SetActive(value: false);
					for (int k = 0; k < textureAnimators[i][j].Length; k++)
					{
						textureAnimatorsCol[i][j][k].enabled = true;
					}
				}
			}
		}
		else
		{
			int index = 1;
			switch (_particle)
			{
			case 3:
				index = 5;
				break;
			case 4:
				index = 2;
				break;
			case 5:
				index = 6;
				break;
			}
			if (lstParticle.Count > _particle)
			{
				if (lstParticle[index].particle == null)
				{
					return false;
				}
				if (!lstParticle[index].particleCacheObj.activeSelf)
				{
					lstParticle[index].particleCacheObj.SetActive(value: true);
				}
				lstParticle[index].particle.Simulate(0f);
				lstParticle[index].particle.Play();
			}
		}
		return true;
	}

	public bool SiruPlay(int _particle)
	{
		if (lstParticle.Count > _particle)
		{
			if (lstParticle[_particle].particle == null)
			{
				return false;
			}
			if (!lstParticle[_particle].particleCacheObj.activeSelf)
			{
				lstParticle[_particle].particleCacheObj.SetActive(value: true);
			}
			lstParticle[_particle].particle.Simulate(0f);
			lstParticle[_particle].particle.Play();
		}
		return true;
	}

	public bool IsPlaying(int _particle)
	{
		if (!urine.Contains(_particle))
		{
			for (int i = 0; i < lstParticlePtns.Count; i++)
			{
				if (lstParticlePtns[i].patternID == _particle && lstParticlePtns[i].particleObj != null)
				{
					return lstParticlePtns[i].play;
				}
			}
			return false;
		}
		int index = 1;
		switch (_particle)
		{
		case 3:
			index = 5;
			break;
		case 4:
			index = 2;
			break;
		case 5:
			index = 6;
			break;
		}
		if (lstParticle.Count <= _particle)
		{
			return false;
		}
		if (lstParticle[index].particle == null)
		{
			return false;
		}
		return lstParticle[index].particle.isPlaying;
	}

	private void SetMeshCollider(GameObject particle)
	{
		MeshFilter[] componentsInChildren = particle.GetComponentsInChildren<MeshFilter>(includeInactive: true);
		BoxCollider boxCollider = null;
		MeshFilter[] array = componentsInChildren;
		foreach (MeshFilter meshFilter in array)
		{
			boxCollider = meshFilter.GetComponent<BoxCollider>();
			if (boxCollider == null)
			{
				boxCollider = meshFilter.gameObject.AddComponent<BoxCollider>();
				boxCollider.enabled = false;
				boxCollider.isTrigger = true;
				boxCollider.size = new Vector3(1f, 1f, 0.0001f);
			}
			else
			{
				boxCollider.enabled = false;
			}
		}
	}

	public List<GameObject> GetPlayParticles()
	{
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < lstParticlePtns.Count; i++)
		{
			if (lstParticlePtns[i].play)
			{
				list.AddRange(lstParticlePtns[i].particleObj);
			}
		}
		return list;
	}
}
