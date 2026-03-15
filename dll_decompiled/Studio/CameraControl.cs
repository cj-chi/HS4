using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cinemachine;
using Manager;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Studio;

public class CameraControl : CinemachineVirtualCameraBase
{
	public delegate bool NoCtrlFunc();

	[Serializable]
	public class CameraData
	{
		private const int ver = 2;

		public Vector3 pos = Vector3.zero;

		public Vector3 rotate = Vector3.zero;

		public Vector3 distance = Vector3.zero;

		public float parse = 23f;

		public Quaternion rotation
		{
			get
			{
				return Quaternion.Euler(rotate);
			}
			set
			{
				rotate = value.eulerAngles;
			}
		}

		public CameraData()
		{
		}

		public CameraData(CameraData _src)
		{
			Copy(_src);
		}

		public void Set(Vector3 _pos, Vector3 _rotate, Vector3 _distance, float _parse)
		{
			pos = _pos;
			rotate = _rotate;
			distance = _distance;
			parse = _parse;
		}

		public void Save(BinaryWriter _writer)
		{
			_writer.Write(2);
			_writer.Write(pos.x);
			_writer.Write(pos.y);
			_writer.Write(pos.z);
			_writer.Write(rotate.x);
			_writer.Write(rotate.y);
			_writer.Write(rotate.z);
			_writer.Write(distance.x);
			_writer.Write(distance.y);
			_writer.Write(distance.z);
			_writer.Write(parse);
		}

		public void Load(BinaryReader _reader)
		{
			int num = _reader.ReadInt32();
			pos.x = _reader.ReadSingle();
			pos.y = _reader.ReadSingle();
			pos.z = _reader.ReadSingle();
			rotate.x = _reader.ReadSingle();
			rotate.y = _reader.ReadSingle();
			rotate.z = _reader.ReadSingle();
			if (num == 1)
			{
				_reader.ReadSingle();
			}
			else
			{
				distance.x = _reader.ReadSingle();
				distance.y = _reader.ReadSingle();
				distance.z = _reader.ReadSingle();
			}
			parse = _reader.ReadSingle();
		}

		public void Copy(CameraData _src)
		{
			pos = _src.pos;
			rotate = _src.rotate;
			distance = _src.distance;
			parse = _src.parse;
		}
	}

	public enum Config
	{
		MoveXZ,
		Rotation,
		Translation,
		MoveXY
	}

	public class VisibleObject
	{
		public string nameCollider;

		public float delay;

		public bool isVisible = true;

		public List<MeshRenderer> listRender = new List<MeshRenderer>();
	}

	private int m_MapLayer = -1;

	public Transform transBase;

	public Transform targetObj;

	public float xRotSpeed = 5f;

	public float yRotSpeed = 5f;

	public float zoomSpeed = 5f;

	public float moveSpeed = 0.05f;

	public float noneTargetDir = 5f;

	public bool isLimitPos;

	public float limitPos = 2f;

	public bool isLimitDir;

	public float limitDir = 10f;

	public float limitFov = 40f;

	[SerializeField]
	private Camera m_SubCamera;

	public NoCtrlFunc noCtrlCondition;

	public NoCtrlFunc zoomCondition;

	public NoCtrlFunc keyCondition;

	public readonly int CONFIG_SIZE = Enum.GetNames(typeof(Config)).Length;

	[SerializeField]
	protected CameraData cameraData = new CameraData();

	protected CameraData cameraReset = new CameraData();

	protected bool isInit;

	private const float INIT_FOV = 23f;

	protected CapsuleCollider viewCollider;

	protected float rateAddSpeed = 1f;

	private bool dragging;

	private bool m_ConfigVanish = true;

	[SerializeField]
	private Transform m_TargetTex;

	[SerializeField]
	private Renderer m_TargetRender;

	[SerializeField]
	private GameObject objRoot;

	private List<VisibleObject> lstMapVanish = new List<VisibleObject>();

	private List<Collider> listCollider = new List<Collider>();

	public bool isFlashVisible;

	[SerializeField]
	private LensSettings lensSettings = LensSettings.Default;

	private CameraState cameraState = CameraState.Default;

	private int mapLayer
	{
		get
		{
			if (m_MapLayer == -1)
			{
				m_MapLayer = LayerMask.GetMask("Map", "MapNoShadow");
			}
			return m_MapLayer;
		}
	}

	public Camera mainCmaera { get; protected set; }

	public Camera subCamera => m_SubCamera;

	public bool isControlNow { get; protected set; }

	public bool isOutsideTargetTex { get; set; }

	public bool isCursorLock { get; set; }

	public bool isConfigTargetTex { get; set; }

	public bool isConfigVanish
	{
		get
		{
			return m_ConfigVanish;
		}
		set
		{
			if (Utility.SetStruct(ref m_ConfigVanish, value))
			{
				VisibleFroceVanish(_visible: true);
			}
		}
	}

	public Transform targetTex => m_TargetTex;

	public bool active
	{
		get
		{
			return objRoot.activeSelf;
		}
		set
		{
			objRoot.SetActive(value);
		}
	}

	public bool IsOutsideSetting { get; set; }

	public Vector3 targetPos
	{
		get
		{
			return cameraData.pos;
		}
		set
		{
			cameraData.pos = value;
		}
	}

	public Vector3 cameraAngle
	{
		get
		{
			return cameraData.rotate;
		}
		set
		{
			base.transform.rotation = Quaternion.Euler(value);
			cameraData.rotate = value;
		}
	}

	public float fieldOfView
	{
		get
		{
			return cameraData.parse;
		}
		set
		{
			cameraData.parse = value;
			if (mainCmaera != null)
			{
				mainCmaera.fieldOfView = value;
			}
			if (subCamera != null)
			{
				subCamera.fieldOfView = value;
			}
			lensSettings.FieldOfView = value;
			cameraState.Lens = lensSettings;
		}
	}

	public override CameraState State => cameraState;

	public override Transform LookAt
	{
		get
		{
			return targetObj;
		}
		set
		{
			targetObj = value;
		}
	}

	public override Transform Follow
	{
		get
		{
			return transBase;
		}
		set
		{
			transBase = value;
		}
	}

	public CameraControl()
	{
		cameraData.parse = 23f;
		cameraReset.parse = 23f;
	}

	public CameraData Export()
	{
		return new CameraData(cameraData);
	}

	public CameraData ExportResetData()
	{
		return new CameraData(cameraReset);
	}

	public void Import(CameraData _src)
	{
		if (_src != null)
		{
			cameraData.Copy(_src);
			fieldOfView = cameraData.parse;
		}
	}

	public bool LoadVanish(string _assetbundle, string _file, GameObject _objMap)
	{
		lstMapVanish.Clear();
		return false;
	}

	public void CloerListCollider()
	{
		listCollider.Clear();
		lstMapVanish.Clear();
	}

	public void VisibleFroceVanish(bool _visible)
	{
		foreach (VisibleObject item in lstMapVanish)
		{
			foreach (MeshRenderer item2 in item.listRender)
			{
				if ((bool)item2)
				{
					item2.enabled = _visible;
				}
			}
			item.isVisible = _visible;
			item.delay = (_visible ? 0.3f : 0f);
		}
	}

	private void VisibleFroceVanish(VisibleObject _obj, bool _visible)
	{
		if (_obj == null || _obj.listRender == null)
		{
			return;
		}
		foreach (MeshRenderer item in _obj.listRender)
		{
			item.enabled = _visible;
		}
		_obj.delay = (_visible ? 0.3f : 0f);
		_obj.isVisible = _visible;
	}

	private void VanishProc()
	{
		if (!isConfigVanish)
		{
			return;
		}
		int count = lstMapVanish.Count;
		int i = 0;
		while (i < count)
		{
			if (listCollider.Find((Collider x) => lstMapVanish[i].nameCollider == x.name) == null)
			{
				VanishDelayVisible(lstMapVanish[i]);
			}
			else if (lstMapVanish[i].isVisible)
			{
				VisibleFroceVanish(lstMapVanish[i], _visible: false);
			}
			int num = i + 1;
			i = num;
		}
	}

	private void VanishDelayVisible(VisibleObject _visible)
	{
		if (_visible.isVisible)
		{
			return;
		}
		if (!isFlashVisible)
		{
			_visible.delay += Time.deltaTime;
			if (_visible.delay >= 0.3f)
			{
				VisibleFroceVanish(_visible, _visible: true);
			}
		}
		else
		{
			VisibleFroceVanish(_visible, _visible: true);
		}
	}

	public void Reset(int _mode)
	{
		switch (_mode)
		{
		case 0:
			cameraData.Copy(cameraReset);
			fieldOfView = cameraData.parse;
			break;
		case 1:
			cameraData.pos = cameraReset.pos;
			break;
		case 2:
			base.transform.rotation = cameraReset.rotation;
			break;
		case 3:
			cameraData.distance = cameraReset.distance;
			break;
		}
	}

	protected virtual bool InputMouseWheelZoomProc()
	{
		float num = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
		if (num != 0f)
		{
			cameraData.distance.z += num;
			cameraData.distance.z = Mathf.Min(0f, cameraData.distance.z);
			return true;
		}
		return false;
	}

	protected virtual bool InputMouseProc()
	{
		bool result = false;
		float axis = Input.GetAxis("Mouse X");
		float axis2 = Input.GetAxis("Mouse Y");
		if ((!EventSystem.current || !EventSystem.current.IsPointerOverGameObject()) && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)))
		{
			dragging = true;
		}
		else if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
		{
			dragging = false;
		}
		if (!dragging)
		{
			return false;
		}
		if (Input.GetMouseButton(0) && Input.GetMouseButton(1))
		{
			Vector3 zero = Vector3.zero;
			zero.x = axis * moveSpeed * rateAddSpeed;
			zero.z = axis2 * moveSpeed * rateAddSpeed;
			if (transBase != null)
			{
				cameraData.pos += transBase.InverseTransformDirection(base.transform.TransformDirection(zero));
			}
			else
			{
				cameraData.pos += base.transform.TransformDirection(zero);
			}
			result = true;
		}
		else if (Input.GetMouseButton(0))
		{
			Vector3 zero2 = Vector3.zero;
			zero2.y += axis * xRotSpeed * rateAddSpeed;
			zero2.x -= axis2 * yRotSpeed * rateAddSpeed;
			cameraData.rotate.y = (cameraData.rotate.y + zero2.y) % 360f;
			cameraData.rotate.x = (cameraData.rotate.x + zero2.x) % 360f;
			result = true;
		}
		else if (Input.GetMouseButton(1))
		{
			cameraData.pos.y += axis2 * moveSpeed * rateAddSpeed;
			cameraData.distance.z -= axis * moveSpeed * rateAddSpeed;
			cameraData.distance.z = Mathf.Min(0f, cameraData.distance.z);
			result = true;
		}
		else if (Input.GetMouseButton(2))
		{
			Vector3 zero3 = Vector3.zero;
			zero3.x = axis * moveSpeed * rateAddSpeed;
			zero3.y = axis2 * moveSpeed * rateAddSpeed;
			if (transBase != null)
			{
				cameraData.pos += transBase.InverseTransformDirection(base.transform.TransformDirection(zero3));
			}
			else
			{
				cameraData.pos += base.transform.TransformDirection(zero3);
			}
			result = true;
		}
		return result;
	}

	protected virtual bool InputKeyProc()
	{
		bool flag = false;
		if (Input.GetKeyDown(KeyCode.A))
		{
			Reset(0);
		}
		else if (Input.GetKeyDown(KeyCode.Keypad5))
		{
			cameraData.rotate.x = cameraReset.rotate.x;
			cameraData.rotate.y = cameraReset.rotate.y;
		}
		else if (Input.GetKeyDown(KeyCode.Slash))
		{
			cameraData.rotate.z = 0f;
		}
		else if (Input.GetKeyDown(KeyCode.Semicolon))
		{
			fieldOfView = cameraReset.parse;
		}
		float deltaTime = Time.deltaTime;
		if (Input.GetKey(KeyCode.Home))
		{
			flag = true;
			cameraData.distance.z += deltaTime;
			cameraData.distance.z = Mathf.Min(0f, cameraData.distance.z);
		}
		else if (Input.GetKey(KeyCode.End))
		{
			flag = true;
			cameraData.distance.z -= deltaTime;
		}
		if (Input.GetKey(KeyCode.RightArrow))
		{
			flag = true;
			if (transBase != null)
			{
				cameraData.pos += transBase.InverseTransformDirection(base.transform.TransformDirection(new Vector3(deltaTime, 0f, 0f)));
			}
			else
			{
				cameraData.pos += base.transform.TransformDirection(new Vector3(deltaTime, 0f, 0f));
			}
		}
		else if (Input.GetKey(KeyCode.LeftArrow))
		{
			flag = true;
			if (transBase != null)
			{
				cameraData.pos += transBase.InverseTransformDirection(base.transform.TransformDirection(new Vector3(0f - deltaTime, 0f, 0f)));
			}
			else
			{
				cameraData.pos += base.transform.TransformDirection(new Vector3(0f - deltaTime, 0f, 0f));
			}
		}
		if (Input.GetKey(KeyCode.UpArrow))
		{
			flag = true;
			if (transBase != null)
			{
				cameraData.pos += transBase.InverseTransformDirection(base.transform.TransformDirection(new Vector3(0f, 0f, deltaTime)));
			}
			else
			{
				cameraData.pos += base.transform.TransformDirection(new Vector3(0f, 0f, deltaTime));
			}
		}
		else if (Input.GetKey(KeyCode.DownArrow))
		{
			flag = true;
			if (transBase != null)
			{
				cameraData.pos += transBase.InverseTransformDirection(base.transform.TransformDirection(new Vector3(0f, 0f, 0f - deltaTime)));
			}
			else
			{
				cameraData.pos += base.transform.TransformDirection(new Vector3(0f, 0f, 0f - deltaTime));
			}
		}
		if (Input.GetKey(KeyCode.PageUp))
		{
			flag = true;
			cameraData.pos.y += deltaTime;
		}
		else if (Input.GetKey(KeyCode.PageDown))
		{
			flag = true;
			cameraData.pos.y -= deltaTime;
		}
		float num = 10f * Time.deltaTime;
		Vector3 zero = Vector3.zero;
		if (Input.GetKey(KeyCode.Period))
		{
			flag = true;
			zero.z += num;
		}
		else if (Input.GetKey(KeyCode.Backslash))
		{
			flag = true;
			zero.z -= num;
		}
		if (Input.GetKey(KeyCode.Keypad2))
		{
			flag = true;
			zero.x -= num * yRotSpeed;
		}
		else if (Input.GetKey(KeyCode.Keypad8))
		{
			flag = true;
			zero.x += num * yRotSpeed;
		}
		if (Input.GetKey(KeyCode.Keypad4))
		{
			flag = true;
			zero.y += num * xRotSpeed;
		}
		else if (Input.GetKey(KeyCode.Keypad6))
		{
			flag = true;
			zero.y -= num * xRotSpeed;
		}
		if (flag)
		{
			cameraData.rotate.y = (cameraData.rotate.y + zero.y) % 360f;
			cameraData.rotate.x = (cameraData.rotate.x + zero.x) % 360f;
			cameraData.rotate.z = (cameraData.rotate.z + zero.z) % 360f;
		}
		float deltaTime2 = Time.deltaTime;
		if (Input.GetKey(KeyCode.Equals))
		{
			flag = true;
			fieldOfView = Mathf.Max(cameraData.parse - deltaTime2 * 15f, 1f);
		}
		else if (Input.GetKey(KeyCode.RightBracket))
		{
			flag = true;
			fieldOfView = Mathf.Min(cameraData.parse + deltaTime2 * 15f, limitFov);
		}
		return flag;
	}

	public void TargetSet(Transform target, bool isReset)
	{
		if ((bool)target)
		{
			targetObj = target;
		}
		if ((bool)targetObj)
		{
			cameraData.pos = targetObj.position;
		}
		Transform transform = base.transform;
		cameraData.distance = Vector3.zero;
		cameraData.distance.z = 0f - Vector3.Distance(cameraData.pos, transform.position);
		transform.LookAt(cameraData.pos);
		cameraData.rotate = base.transform.rotation.eulerAngles;
		if (isReset)
		{
			cameraReset.Copy(cameraData);
		}
	}

	public void FrontTarget(Transform target, bool isReset, float dir = float.MinValue)
	{
		if ((bool)target)
		{
			targetObj = target;
		}
		if ((bool)targetObj)
		{
			target = targetObj;
			cameraData.pos = target.position;
		}
		if ((bool)target)
		{
			if (dir != float.MinValue)
			{
				cameraData.distance = Vector3.zero;
				cameraData.distance.z = 0f - dir;
			}
			Transform transform = base.transform;
			transform.position = target.position;
			transform.rotation.eulerAngles.Set(cameraData.rotate.x, cameraData.rotate.y, cameraData.rotate.z);
			transform.position += transform.forward * cameraData.distance.z;
			transform.LookAt(cameraData.pos);
			cameraData.rotate = base.transform.rotation.eulerAngles;
			if (isReset)
			{
				cameraReset.Copy(cameraData);
			}
		}
	}

	public void SetCamera(Vector3 pos, Vector3 angle, Quaternion rot, Vector3 dir)
	{
		base.transform.localPosition = pos;
		base.transform.localRotation = rot;
		cameraData.rotate = angle;
		cameraData.distance = dir;
		cameraData.pos = -(base.transform.localRotation * cameraData.distance - base.transform.localPosition);
		cameraReset.Copy(cameraData);
	}

	public void SetCamera(Vector3 _pos, Quaternion _rot, float _dis, bool _update = true, bool _reset = true)
	{
		cameraData.pos = _pos;
		cameraData.rotation = _rot;
		cameraData.distance = new Vector3(0f, 0f, 0f - _dis);
		if (_reset)
		{
			cameraReset.Copy(cameraData);
		}
		if (_update)
		{
			InternalUpdateCameraState(Vector3.zero, 0f);
		}
	}

	public void SetBase(Transform _trans)
	{
		if (!(transBase == null))
		{
			transBase.transform.position = _trans.position;
			transBase.transform.rotation = _trans.rotation;
		}
	}

	public void ReflectOption()
	{
		rateAddSpeed = Studio.optionSystem.cameraSpeed;
		xRotSpeed = Studio.optionSystem.cameraSpeedX;
		yRotSpeed = Studio.optionSystem.cameraSpeedY;
		List<string> list = new List<string>();
		if (Singleton<Studio>.Instance.workInfo.visibleAxis)
		{
			if (Studio.optionSystem.selectedState == 0)
			{
				list.Add("Studio/Col");
			}
			list.Add("Studio/Select");
		}
		list.Add("Studio/Route");
		m_SubCamera.cullingMask = LayerMask.GetMask(list.ToArray());
	}

	private void Awake()
	{
		m_MapLayer = -1;
		mainCmaera = GetComponent<Camera>();
		if (mainCmaera == null)
		{
			mainCmaera = Camera.main;
		}
		fieldOfView = cameraReset.parse;
		zoomCondition = () => false;
		isControlNow = false;
		if (!targetObj)
		{
			Vector3 vector = base.transform.TransformDirection(Vector3.forward);
			cameraData.pos = base.transform.position + vector * noneTargetDir;
		}
		TargetSet(targetObj, isReset: true);
		isOutsideTargetTex = true;
		isConfigTargetTex = true;
		isCursorLock = true;
	}

	private new IEnumerator Start()
	{
		if (m_TargetTex == null)
		{
			m_TargetTex = base.transform.Find("CameraTarget");
		}
		if ((bool)m_TargetTex)
		{
			m_TargetTex.localScale = Vector3.one * 0.1f;
			if (m_TargetRender == null)
			{
				m_TargetRender = m_TargetTex.GetComponent<Renderer>();
			}
		}
		if (m_SubCamera != null)
		{
			m_SubCamera.enabled = true;
		}
		ReflectOption();
		yield return new WaitWhile(() => !Manager.Config.initialized);
		lensSettings = cameraState.Lens;
		m_ConfigVanish = Manager.Config.GraphicData.Shield;
		listCollider.Clear();
		isInit = true;
	}

	private void LateUpdate()
	{
		if (SingletonInitializer<Scene>.initialized && !(Scene.AddSceneName != string.Empty) && !Scene.IsNowLoadingFade && !IsOutsideSetting && (isControlNow || !Input.GetKey(KeyCode.B)))
		{
			isControlNow = false;
			xRotSpeed = Studio.optionSystem.cameraSpeedX;
			yRotSpeed = Studio.optionSystem.cameraSpeedY;
			if (!isControlNow)
			{
				isControlNow |= (zoomCondition == null || zoomCondition()) && InputMouseWheelZoomProc();
			}
			if (!isControlNow && (noCtrlCondition == null || !noCtrlCondition()) && InputMouseProc())
			{
				isControlNow = true;
			}
			if (!isControlNow)
			{
				isControlNow |= (keyCondition == null || keyCondition()) && InputKeyProc();
			}
		}
	}

	protected void OnTriggerEnter(Collider other)
	{
		if (!(other == null) && (mapLayer & (1 << other.gameObject.layer)) != 0 && listCollider.Find((Collider x) => other.name == x.name) == null)
		{
			listCollider.Add(other);
		}
	}

	protected void OnTriggerStay(Collider other)
	{
		if (!(other == null) && (mapLayer & (1 << other.gameObject.layer)) != 0 && listCollider.Find((Collider x) => other.name == x.name) == null)
		{
			listCollider.Add(other);
		}
	}

	protected void OnTriggerExit(Collider other)
	{
		listCollider.Clear();
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = ((cameraData.distance.z > 0f) ? Color.red : Color.blue);
		Gizmos.DrawRay(direction: (!(transBase != null)) ? (cameraData.pos - base.transform.position) : (transBase.TransformPoint(cameraData.pos) - base.transform.position), from: base.transform.position);
	}

	public override void InternalUpdateCameraState(Vector3 worldUp, float deltaTime)
	{
		if (!base.enabled || IsOutsideSetting)
		{
			return;
		}
		if (isLimitDir)
		{
			cameraData.distance.z = Mathf.Clamp(cameraData.distance.z, 0f - limitDir, 0f);
		}
		if (isLimitPos)
		{
			cameraData.pos = Vector3.ClampMagnitude(cameraData.pos, limitPos);
		}
		if (transBase != null)
		{
			cameraState.RawOrientation = transBase.rotation * Quaternion.Euler(cameraData.rotate);
			cameraState.RawPosition = cameraState.RawOrientation * cameraData.distance + transBase.TransformPoint(cameraData.pos);
		}
		else
		{
			cameraState.RawOrientation = Quaternion.Euler(cameraData.rotate);
			cameraState.RawPosition = cameraState.RawOrientation * cameraData.distance + cameraData.pos;
		}
		base.transform.position = cameraState.RawPosition;
		base.transform.rotation = cameraState.RawOrientation;
		if ((bool)targetTex)
		{
			if (transBase != null)
			{
				targetTex.position = transBase.TransformPoint(cameraData.pos);
			}
			else
			{
				targetTex.position = cameraData.pos;
			}
			Vector3 position = base.transform.position;
			position.y = targetTex.position.y;
			targetTex.transform.LookAt(position);
			targetTex.Rotate(90f, 0f, 0f);
			if ((bool)m_TargetRender)
			{
				m_TargetRender.enabled = isControlNow & isOutsideTargetTex & isConfigTargetTex;
			}
			if (Singleton<GameCursor>.IsInstance() && isCursorLock)
			{
				Singleton<GameCursor>.Instance.SetCursorLock(isControlNow & isOutsideTargetTex);
			}
		}
		if (viewCollider != null)
		{
			viewCollider.height = cameraData.distance.z;
			viewCollider.center = -Vector3.forward * cameraData.distance.z * 0.5f;
			VanishProc();
		}
	}

	public void SetPositionAndRotation(Vector3 _position, Quaternion _orientation)
	{
		cameraState.RawPosition = _position;
		cameraState.RawOrientation = _orientation;
	}
}
