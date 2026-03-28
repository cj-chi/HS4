using System.Collections.Generic;
using System.Linq;
using AIChara;
using IllusionUtility.GetUtility;
using Obi;
using UnityEngine;

public class ObiFluidManager : Singleton<ObiFluidManager>
{
	private class EmitterTargetInfo
	{
		public int Sex { get; private set; }

		public string Parent { get; private set; }

		public Vector3 Pos { get; private set; }

		public Vector3 Rot { get; private set; }

		public int RendererIndex { get; private set; }

		public ObiEmitterCtrl.SetupInfo SetupInfo { get; private set; }

		public EmitterTargetInfo(List<string> _lst)
		{
			int count = 0;
			Sex = int.Parse(_lst[count++]);
			Parent = _lst[count++];
			Pos = new Vector3(float.Parse(_lst[count++]), float.Parse(_lst[count++]), float.Parse(_lst[count++]));
			Rot = new Vector3(float.Parse(_lst[count++]), float.Parse(_lst[count++]), float.Parse(_lst[count++]));
			RendererIndex = int.Parse(_lst[count++]);
			SetupInfo = new ObiEmitterCtrl.SetupInfo(_lst.Skip(count));
		}
	}

	public class AddTargetParam
	{
		public Transform parent;

		public Vector3 pos = Vector3.zero;

		public Vector3 rot = Vector3.zero;

		public ObiEmitterCtrl.SetupInfo setupInfo;

		public int rendererIndex;

		public AddTargetParam()
		{
		}

		public AddTargetParam(Transform _parent, Vector3 _pos, Vector3 _rot, ObiEmitterCtrl.SetupInfo _setupInfo, int _rendererIndex = 0)
		{
			parent = _parent;
			pos = _pos;
			rot = _rot;
			setupInfo = _setupInfo;
			rendererIndex = _rendererIndex;
		}
	}

	[SerializeField]
	private ObiSolver obiSolver;

	[SerializeField]
	private ObiFluidRenderer[] obiFluidRenderer;

	[Space]
	[SerializeField]
	private GameObject objPrefab;

	private List<EmitterTargetInfo> emitterTargetInfos = new List<EmitterTargetInfo>();

	private Dictionary<ChaControl, ObiFluidCtrl> dicObjFluidCtrls = new Dictionary<ChaControl, ObiFluidCtrl>();

	private List<ObiFluidCtrl> lstFluidCtrls = new List<ObiFluidCtrl>();

	public ObiSolver ObiSolver => obiSolver;

	public ObiFluidCtrl Setup(ChaControl _control, int _sex = -1)
	{
		if (_control == null)
		{
			return null;
		}
		ObiFluidCtrl value = null;
		if (dicObjFluidCtrls.TryGetValue(_control, out value))
		{
			value.Release();
			dicObjFluidCtrls.Remove(_control);
		}
		if (_sex == -1)
		{
			_sex = _control.sex;
		}
		List<(ObiEmitterCtrl, ObiFluidRenderer)> list = new List<(ObiEmitterCtrl, ObiFluidRenderer)>();
		foreach (EmitterTargetInfo item in emitterTargetInfos.Where((EmitterTargetInfo v) => v.Sex == _sex))
		{
			Transform transform = _control.objBodyBone.transform.FindLoop(item.Parent);
			if (transform == null)
			{
				list.Add((null, null));
				continue;
			}
			GameObject obj = Object.Instantiate(objPrefab, transform);
			obj.SetActive(value: true);
			obj.transform.localPosition = item.Pos;
			obj.transform.localRotation = Quaternion.Euler(item.Rot);
			ObiEmitterCtrl component = obj.GetComponent<ObiEmitterCtrl>();
			component.Setup(item.SetupInfo);
			list.Add((component, obiFluidRenderer.SafeGet(item.RendererIndex)));
		}
		foreach (int item2 in new HashSet<int>(from v in emitterTargetInfos
			where v.Sex == _sex
			select v.RendererIndex))
		{
			ObiFluidRenderer obiRenderer = obiFluidRenderer.SafeGet(item2);
			if (!(obiRenderer == null))
			{
				List<ObiParticleRenderer> list2 = new List<ObiParticleRenderer>(obiRenderer.particleRenderers);
				list2.AddRange(from v in list
					where v.Item2 == obiRenderer
					select v.Item1.ObiParticleRenderer);
				obiRenderer.particleRenderers = list2.ToArray();
			}
		}
		ObiFluidCtrl obiFluidCtrl = new ObiFluidCtrl(list.ToArray());
		dicObjFluidCtrls.Add(_control, obiFluidCtrl);
		return obiFluidCtrl;
	}

	public ObiFluidCtrl Add(Transform _parent, Vector3 _pos, Vector3 _rot, ObiEmitterCtrl.SetupInfo _setupInfo, int _rendererIndex = 0)
	{
		if (_parent == null)
		{
			return null;
		}
		GameObject obj = Object.Instantiate(objPrefab, _parent);
		obj.SetActive(value: true);
		ObiEmitterCtrl component = obj.GetComponent<ObiEmitterCtrl>();
		component.transform.SetParent(_parent);
		component.transform.localPosition = _pos;
		component.transform.localRotation = Quaternion.Euler(_rot);
		component.Setup(_setupInfo);
		ObiFluidRenderer obiFluidRenderer = this.obiFluidRenderer.SafeGet(_rendererIndex);
		if (obiFluidRenderer != null)
		{
			List<ObiParticleRenderer> list = new List<ObiParticleRenderer>(obiFluidRenderer.particleRenderers);
			list.Add(component.ObiParticleRenderer);
			obiFluidRenderer.particleRenderers = list.ToArray();
		}
		ObiFluidCtrl obiFluidCtrl = new ObiFluidCtrl(new(ObiEmitterCtrl, ObiFluidRenderer)[1] { (component, obiFluidRenderer) });
		lstFluidCtrls.Add(obiFluidCtrl);
		return obiFluidCtrl;
	}

	public ObiFluidCtrl Add(AddTargetParam param)
	{
		if (param == null)
		{
			return null;
		}
		return Add(param.parent, param.pos, param.rot, param.setupInfo, param.rendererIndex);
	}

	public ObiFluidCtrl Add(AddTargetParam[] param)
	{
		if (((IReadOnlyCollection<AddTargetParam>)(object)param).IsNullOrEmpty())
		{
			return null;
		}
		List<(ObiEmitterCtrl, ObiFluidRenderer)> list = new List<(ObiEmitterCtrl, ObiFluidRenderer)>();
		foreach (AddTargetParam addTargetParam in param)
		{
			GameObject obj = Object.Instantiate(objPrefab, addTargetParam.parent);
			obj.SetActive(value: true);
			ObiEmitterCtrl component = obj.GetComponent<ObiEmitterCtrl>();
			component.transform.localPosition = addTargetParam.pos;
			component.transform.localRotation = Quaternion.Euler(addTargetParam.rot);
			component.Setup(addTargetParam.setupInfo);
			list.Add((component, obiFluidRenderer.SafeGet(addTargetParam.rendererIndex)));
		}
		foreach (int item in new HashSet<int>(param.Select((AddTargetParam v) => v.rendererIndex)))
		{
			ObiFluidRenderer obiRenderer = obiFluidRenderer.SafeGet(item);
			if (!(obiRenderer == null))
			{
				List<ObiParticleRenderer> list2 = new List<ObiParticleRenderer>(obiRenderer.particleRenderers);
				list2.AddRange(from v in list
					where v.Item2 == obiRenderer
					select v.Item1.ObiParticleRenderer);
				obiRenderer.particleRenderers = list2.ToArray();
			}
		}
		ObiFluidCtrl obiFluidCtrl = new ObiFluidCtrl(list.ToArray());
		lstFluidCtrls.Add(obiFluidCtrl);
		return obiFluidCtrl;
	}

	public void ReleaseAll()
	{
		foreach (KeyValuePair<ChaControl, ObiFluidCtrl> dicObjFluidCtrl in dicObjFluidCtrls)
		{
			dicObjFluidCtrl.Value.Release();
		}
		dicObjFluidCtrls.Clear();
		foreach (ObiFluidCtrl lstFluidCtrl in lstFluidCtrls)
		{
			lstFluidCtrl.Release();
		}
		lstFluidCtrls.Clear();
	}

	public void Release(ChaControl _control)
	{
		ObiFluidCtrl value = null;
		if (dicObjFluidCtrls.TryGetValue(_control, out value))
		{
			value.Release();
			dicObjFluidCtrls.Remove(_control);
		}
	}

	public void Release(ObiFluidCtrl _objFluidCtrl)
	{
		if (_objFluidCtrl != null)
		{
			_objFluidCtrl.Release();
			lstFluidCtrls.Remove(_objFluidCtrl);
		}
	}

	private void LoadInfo()
	{
		ExcelData excelData = LoadExcelData("list/h/siruobi/30.unity3d", "Emitter_def", "");
		if (!(excelData != null))
		{
			return;
		}
		foreach (List<string> item in from v in excelData.list.Skip(1)
			select v.list)
		{
			int result = -1;
			if (int.TryParse(item.SafeGet(0), out result))
			{
				emitterTargetInfos.Add(new EmitterTargetInfo(item));
			}
		}
	}

	private ExcelData LoadExcelData(string _bundlePath, string _fileName, string _manifest)
	{
		return CommonLib.LoadAsset<ExcelData>(_bundlePath, _fileName, clone: false, _manifest);
	}

	protected override void Awake()
	{
		if (CheckInstance())
		{
			LoadInfo();
		}
	}
}
