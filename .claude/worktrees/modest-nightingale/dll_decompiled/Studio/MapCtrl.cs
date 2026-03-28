using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio;

public class MapCtrl : Singleton<MapCtrl>
{
	[SerializeField]
	private TMP_InputField[] inputPos;

	[SerializeField]
	private TMP_InputField[] inputRot;

	[SerializeField]
	private MapDragButton[] mapDragButton;

	[SerializeField]
	private Toggle toggleOption;

	[SerializeField]
	private Toggle toggleLight;

	private Vector3 oldValue = Vector3.zero;

	private Transform transMap;

	private bool isUpdate;

	public bool active
	{
		set
		{
			base.gameObject.SetActive(value);
			if (value)
			{
				UpdateUI();
			}
		}
	}

	public void UpdateUI()
	{
		isUpdate = true;
		SetInputTextPos();
		SetInputTextRot();
		if (Singleton<Map>.Instance.IsOption)
		{
			toggleOption.interactable = true;
			toggleOption.isOn = Singleton<Studio>.Instance.sceneInfo.mapInfo.option;
		}
		else
		{
			toggleOption.interactable = false;
			toggleOption.isOn = false;
		}
		if (Singleton<Map>.Instance.IsLight)
		{
			toggleLight.interactable = true;
			toggleLight.isOn = Singleton<Studio>.Instance.sceneInfo.mapInfo.light;
		}
		else
		{
			toggleLight.interactable = false;
			toggleLight.isOn = false;
		}
		isUpdate = false;
	}

	public void Reflect()
	{
		GameObject mapRoot = Singleton<Map>.Instance.MapRoot;
		if (mapRoot != null)
		{
			Transform obj = mapRoot.transform;
			obj.localPosition = Singleton<Studio>.Instance.sceneInfo.mapInfo.ca.pos;
			obj.localRotation = Quaternion.Euler(Singleton<Studio>.Instance.sceneInfo.mapInfo.ca.rot);
		}
		Singleton<Map>.Instance.VisibleOption = Singleton<Studio>.Instance.sceneInfo.mapInfo.option;
		Singleton<Map>.Instance.VisibleLight = Singleton<Studio>.Instance.sceneInfo.mapInfo.light;
		UpdateUI();
	}

	public void OnEndEditPos(int _target)
	{
		float num = InputToFloat(inputPos[_target]);
		Vector3 pos = Singleton<Studio>.Instance.sceneInfo.mapInfo.ca.pos;
		if (pos[_target] != num)
		{
			Vector3 vector = pos;
			pos[_target] = num;
			Singleton<Studio>.Instance.sceneInfo.mapInfo.ca.pos = pos;
			Singleton<UndoRedoManager>.Instance.Push(new MapCommand.MoveEqualsCommand(new MapCommand.EqualsInfo
			{
				newValue = pos,
				oldValue = vector
			}));
		}
		SetInputTextPos();
	}

	public void OnEndEditRot(int _target)
	{
		float num = InputToFloat(inputRot[_target]) % 360f;
		Vector3 rot = Singleton<Studio>.Instance.sceneInfo.mapInfo.ca.rot;
		if (rot[_target] != num)
		{
			Vector3 vector = rot;
			rot[_target] = num;
			Singleton<Studio>.Instance.sceneInfo.mapInfo.ca.rot = rot;
			Singleton<UndoRedoManager>.Instance.Push(new MapCommand.RotationEqualsCommand(new MapCommand.EqualsInfo
			{
				newValue = rot,
				oldValue = vector
			}));
		}
		SetInputTextRot();
	}

	private float InputToFloat(TMP_InputField _input)
	{
		float result = 0f;
		if (!float.TryParse(_input.text, out result))
		{
			return 0f;
		}
		return result;
	}

	private void SetInputTextPos()
	{
		Vector3 pos = Singleton<Studio>.Instance.sceneInfo.mapInfo.ca.pos;
		for (int i = 0; i < 3; i++)
		{
			inputPos[i].text = pos[i].ToString("0.000");
		}
	}

	private void SetInputTextRot()
	{
		Vector3 rot = Singleton<Studio>.Instance.sceneInfo.mapInfo.ca.rot;
		for (int i = 0; i < 3; i++)
		{
			inputRot[i].text = rot[i].ToString("0.000");
		}
	}

	private void OnBeginDragTrans()
	{
		oldValue = Singleton<Studio>.Instance.sceneInfo.mapInfo.ca.pos;
		transMap = Singleton<Map>.Instance.MapRoot.transform;
	}

	private void OnEndDragTrans()
	{
		MapCommand.MoveEqualsCommand command = new MapCommand.MoveEqualsCommand(new MapCommand.EqualsInfo
		{
			newValue = Singleton<Studio>.Instance.sceneInfo.mapInfo.ca.pos,
			oldValue = oldValue
		});
		Singleton<UndoRedoManager>.Instance.Push(command);
		transMap = null;
	}

	private void OnDragTransXZ()
	{
		Vector3 direction = new Vector3(Input.GetAxis("Mouse X"), 0f, Input.GetAxis("Mouse Y"));
		Singleton<Studio>.Instance.sceneInfo.mapInfo.ca.pos += transMap.TransformDirection(direction);
	}

	private void OnDragTransY()
	{
		Vector3 direction = new Vector3(0f, Input.GetAxis("Mouse Y"), 0f);
		Singleton<Studio>.Instance.sceneInfo.mapInfo.ca.pos += transMap.TransformDirection(direction);
	}

	private void OnBeginDragRot()
	{
		oldValue = Singleton<Studio>.Instance.sceneInfo.mapInfo.ca.rot;
	}

	private void OnEndDragRot()
	{
		MapCommand.RotationEqualsCommand command = new MapCommand.RotationEqualsCommand(new MapCommand.EqualsInfo
		{
			newValue = Singleton<Studio>.Instance.sceneInfo.mapInfo.ca.rot,
			oldValue = oldValue
		});
		Singleton<UndoRedoManager>.Instance.Push(command);
	}

	private void OnDragRotX()
	{
		Vector3 rot = Singleton<Studio>.Instance.sceneInfo.mapInfo.ca.rot;
		rot.x = (rot.x + Input.GetAxis("Mouse Y")) % 360f;
		Singleton<Studio>.Instance.sceneInfo.mapInfo.ca.rot = rot;
	}

	private void OnDragRotY()
	{
		Vector3 rot = Singleton<Studio>.Instance.sceneInfo.mapInfo.ca.rot;
		rot.y = (rot.y + Input.GetAxis("Mouse X")) % 360f;
		Singleton<Studio>.Instance.sceneInfo.mapInfo.ca.rot = rot;
	}

	private void OnDragRotZ()
	{
		Vector3 rot = Singleton<Studio>.Instance.sceneInfo.mapInfo.ca.rot;
		rot.z = (rot.z + Input.GetAxis("Mouse X")) % 360f;
		Singleton<Studio>.Instance.sceneInfo.mapInfo.ca.rot = rot;
	}

	private void OnValueChangedOption(bool _value)
	{
		if (!isUpdate)
		{
			Singleton<Map>.Instance.VisibleOption = _value;
		}
	}

	private void OnValueChangedLight(bool _value)
	{
		if (!isUpdate)
		{
			Singleton<Map>.Instance.VisibleLight = _value;
		}
	}

	protected override void Awake()
	{
		if (CheckInstance())
		{
			MapDragButton obj = mapDragButton[0];
			obj.onBeginDragFunc = (Action)Delegate.Combine(obj.onBeginDragFunc, new Action(OnBeginDragTrans));
			MapDragButton obj2 = mapDragButton[0];
			obj2.onDragFunc = (Action)Delegate.Combine(obj2.onDragFunc, new Action(OnDragTransXZ));
			MapDragButton obj3 = mapDragButton[0];
			obj3.onEndDragFunc = (Action)Delegate.Combine(obj3.onEndDragFunc, new Action(OnEndDragTrans));
			MapDragButton obj4 = mapDragButton[1];
			obj4.onBeginDragFunc = (Action)Delegate.Combine(obj4.onBeginDragFunc, new Action(OnBeginDragTrans));
			MapDragButton obj5 = mapDragButton[1];
			obj5.onDragFunc = (Action)Delegate.Combine(obj5.onDragFunc, new Action(OnDragTransY));
			MapDragButton obj6 = mapDragButton[1];
			obj6.onEndDragFunc = (Action)Delegate.Combine(obj6.onEndDragFunc, new Action(OnEndDragTrans));
			for (int i = 0; i < 3; i++)
			{
				MapDragButton obj7 = mapDragButton[2 + i];
				obj7.onBeginDragFunc = (Action)Delegate.Combine(obj7.onBeginDragFunc, new Action(OnBeginDragRot));
				MapDragButton obj8 = mapDragButton[2 + i];
				obj8.onEndDragFunc = (Action)Delegate.Combine(obj8.onEndDragFunc, new Action(OnEndDragRot));
			}
			MapDragButton obj9 = mapDragButton[2];
			obj9.onDragFunc = (Action)Delegate.Combine(obj9.onDragFunc, new Action(OnDragRotX));
			MapDragButton obj10 = mapDragButton[3];
			obj10.onDragFunc = (Action)Delegate.Combine(obj10.onDragFunc, new Action(OnDragRotY));
			MapDragButton obj11 = mapDragButton[4];
			obj11.onDragFunc = (Action)Delegate.Combine(obj11.onDragFunc, new Action(OnDragRotZ));
			toggleOption.onValueChanged.AddListener(OnValueChangedOption);
			toggleLight.onValueChanged.AddListener(OnValueChangedLight);
		}
	}
}
