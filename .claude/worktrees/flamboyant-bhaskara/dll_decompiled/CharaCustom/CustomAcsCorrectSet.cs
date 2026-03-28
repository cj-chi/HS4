using System;
using System.Collections.Generic;
using System.Linq;
using AIChara;
using Illusion.Extensions;
using Manager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharaCustom;

public class CustomAcsCorrectSet : MonoBehaviour
{
	private readonly float[] movePosValue = new float[3] { 0.1f, 1f, 10f };

	private readonly float[] moveRotValue = new float[3] { 1f, 5f, 10f };

	private readonly float[] moveSclValue = new float[3] { 0.01f, 0.1f, 1f };

	[SerializeField]
	private Text title;

	[SerializeField]
	private Toggle[] tglPosRate;

	[SerializeField]
	private Button[] btnPos;

	[SerializeField]
	private InputField[] inpPos;

	[SerializeField]
	private Button[] btnPosReset;

	[SerializeField]
	private Toggle[] tglRotRate;

	[SerializeField]
	private Button[] btnRot;

	[SerializeField]
	private InputField[] inpRot;

	[SerializeField]
	private Button[] btnRotReset;

	[SerializeField]
	private Toggle[] tglSclRate;

	[SerializeField]
	private Button[] btnScl;

	[SerializeField]
	private InputField[] inpScl;

	[SerializeField]
	private Button[] btnSclReset;

	[SerializeField]
	private Button btnAllReset;

	[SerializeField]
	private Toggle tglDrawCtrl;

	[SerializeField]
	private Toggle[] tglCtrlType;

	[SerializeField]
	private Slider sldCtrlSpeed;

	[SerializeField]
	private Slider sldCtrlSize;

	private List<IDisposable> lstDisposable = new List<IDisposable>();

	[SerializeField]
	private GameObject tmpObjGuid;

	private CustomGuideObject cmpGuid;

	private bool isDrag;

	private CustomBase customBase => Singleton<CustomBase>.Instance;

	private ChaControl chaCtrl => customBase.chaCtrl;

	private ChaFileAccessory orgAcs => chaCtrl.chaFile.coordinate.accessory;

	private ChaFileAccessory nowAcs => chaCtrl.nowCoordinate.accessory;

	private CustomBase.CustomSettingSave.AcsCtrlSetting acsCtrlSetting => customBase.customSettingSave.acsCtrlSetting;

	public int slotNo { get; set; } = -1;

	public int correctNo { get; set; } = -1;

	public void UpdateCustomUI()
	{
		if (-1 == slotNo || -1 == correctNo)
		{
			return;
		}
		int posRate = acsCtrlSetting.correctSetting[correctNo].posRate;
		int rotRate = acsCtrlSetting.correctSetting[correctNo].rotRate;
		int sclRate = acsCtrlSetting.correctSetting[correctNo].sclRate;
		tglPosRate[posRate].SetIsOnWithoutCallback(isOn: true);
		tglRotRate[rotRate].SetIsOnWithoutCallback(isOn: true);
		tglSclRate[sclRate].SetIsOnWithoutCallback(isOn: true);
		for (int i = 0; i < 3; i++)
		{
			if (i != posRate)
			{
				tglPosRate[i].SetIsOnWithoutCallback(isOn: false);
			}
			if (i != rotRate)
			{
				tglRotRate[i].SetIsOnWithoutCallback(isOn: false);
			}
			if (i != sclRate)
			{
				tglSclRate[i].SetIsOnWithoutCallback(isOn: false);
			}
		}
		for (int j = 0; j < 3; j++)
		{
			inpPos[j].text = nowAcs.parts[slotNo].addMove[correctNo, 0][j].ToString();
			inpRot[j].text = nowAcs.parts[slotNo].addMove[correctNo, 1][j].ToString();
			inpScl[j].text = nowAcs.parts[slotNo].addMove[correctNo, 2][j].ToString();
		}
		tglDrawCtrl.SetIsOnWithoutCallback(acsCtrlSetting.correctSetting[correctNo].draw);
		tglCtrlType[acsCtrlSetting.correctSetting[correctNo].type].SetIsOnWithoutCallback(isOn: true);
		tglCtrlType[acsCtrlSetting.correctSetting[correctNo].type & 1].SetIsOnWithoutCallback(isOn: false);
		sldCtrlSpeed.value = acsCtrlSetting.correctSetting[correctNo].speed;
		sldCtrlSize.value = acsCtrlSetting.correctSetting[correctNo].scale;
	}

	public void UpdateDragValue(int type, int xyz, float move)
	{
		int[] array = new int[3] { 1, 2, 4 };
		switch (type)
		{
		case 0:
		{
			float value3 = move * movePosValue[acsCtrlSetting.correctSetting[correctNo].posRate];
			chaCtrl.SetAccessoryPos(slotNo, correctNo, value3, add: true, array[xyz]);
			orgAcs.parts[slotNo].addMove[correctNo, 0] = nowAcs.parts[slotNo].addMove[correctNo, 0];
			inpPos[xyz].text = nowAcs.parts[slotNo].addMove[correctNo, 0][xyz].ToString();
			break;
		}
		case 1:
		{
			float value2 = move * moveRotValue[acsCtrlSetting.correctSetting[correctNo].rotRate];
			chaCtrl.SetAccessoryRot(slotNo, correctNo, value2, add: true, array[xyz]);
			orgAcs.parts[slotNo].addMove[correctNo, 1] = nowAcs.parts[slotNo].addMove[correctNo, 1];
			inpRot[xyz].text = nowAcs.parts[slotNo].addMove[correctNo, 1][xyz].ToString();
			break;
		}
		case 2:
		{
			float value = move * moveSclValue[acsCtrlSetting.correctSetting[correctNo].sclRate];
			chaCtrl.SetAccessoryScl(slotNo, correctNo, value, add: true, array[xyz]);
			orgAcs.parts[slotNo].addMove[correctNo, 2] = nowAcs.parts[slotNo].addMove[correctNo, 2];
			inpScl[xyz].text = nowAcs.parts[slotNo].addMove[correctNo, 2][xyz].ToString();
			break;
		}
		}
		SetControllerTransform();
	}

	public void SetControllerTransform()
	{
		Transform transform = chaCtrl.trfAcsMove[slotNo, correctNo];
		if (!(null == transform) && !(null == cmpGuid))
		{
			cmpGuid.amount.position = transform.position;
			cmpGuid.amount.rotation = transform.eulerAngles;
		}
	}

	public void SetAccessoryTransform(bool updateInfo)
	{
		Transform transform = chaCtrl.trfAcsMove[slotNo, correctNo];
		if (null == transform || null == cmpGuid)
		{
			return;
		}
		if (customBase.customSettingSave.acsCtrlSetting.correctSetting[correctNo].type == 0)
		{
			transform.position = cmpGuid.amount.position;
			if (updateInfo)
			{
				Vector3 localPosition = transform.localPosition;
				localPosition.x = Mathf.Clamp(localPosition.x * 10f, -100f, 100f);
				localPosition.y = Mathf.Clamp(localPosition.y * 10f, -100f, 100f);
				localPosition.z = Mathf.Clamp(localPosition.z * 10f, -100f, 100f);
				chaCtrl.SetAccessoryPos(slotNo, correctNo, localPosition.x, add: false, 1);
				chaCtrl.SetAccessoryPos(slotNo, correctNo, localPosition.y, add: false, 2);
				chaCtrl.SetAccessoryPos(slotNo, correctNo, localPosition.z, add: false, 4);
				orgAcs.parts[slotNo].addMove[correctNo, 0] = nowAcs.parts[slotNo].addMove[correctNo, 0];
				chaCtrl.UpdateAccessoryMoveFromInfo(slotNo);
				cmpGuid.amount.position = transform.position;
			}
		}
		else
		{
			transform.eulerAngles = cmpGuid.amount.rotation;
			if (updateInfo)
			{
				Vector3 localEulerAngles = transform.localEulerAngles;
				chaCtrl.SetAccessoryRot(slotNo, correctNo, localEulerAngles.x, add: false, 1);
				chaCtrl.SetAccessoryRot(slotNo, correctNo, localEulerAngles.y, add: false, 2);
				chaCtrl.SetAccessoryRot(slotNo, correctNo, localEulerAngles.z, add: false, 4);
				orgAcs.parts[slotNo].addMove[correctNo, 1] = nowAcs.parts[slotNo].addMove[correctNo, 1];
				chaCtrl.UpdateAccessoryMoveFromInfo(slotNo);
				cmpGuid.amount.rotation = transform.eulerAngles;
			}
		}
		UpdateCustomUI();
	}

	public void UpdateDrawControllerState()
	{
		int type = customBase.customSettingSave.acsCtrlSetting.correctSetting[correctNo].type;
		bool draw = customBase.customSettingSave.acsCtrlSetting.correctSetting[correctNo].draw;
		float speed = customBase.customSettingSave.acsCtrlSetting.correctSetting[correctNo].speed;
		float scale = customBase.customSettingSave.acsCtrlSetting.correctSetting[correctNo].scale;
		tglDrawCtrl.SetIsOnWithoutCallback(draw);
		tglCtrlType[type].SetIsOnWithoutCallback(isOn: true);
		sldCtrlSpeed.value = speed;
		sldCtrlSize.value = scale;
	}

	public bool IsDrag()
	{
		if (null != cmpGuid && cmpGuid.isDrag)
		{
			return true;
		}
		return false;
	}

	public void ShortcutChangeGuidType(int type)
	{
		if (null != cmpGuid && !cmpGuid.isDrag)
		{
			tglCtrlType[type].isOn = true;
		}
	}

	public void CreateGuid(GameObject objParent)
	{
		Transform parent = ((null == objParent) ? null : objParent.transform);
		GameObject gameObject = UnityEngine.Object.Instantiate(tmpObjGuid);
		gameObject.transform.SetParent(parent);
		cmpGuid = gameObject.GetComponent<CustomGuideObject>();
		gameObject.SetActiveIfDifferent(active: true);
	}

	public void Initialize(int _slotNo, int _correctNo)
	{
		slotNo = _slotNo;
		correctNo = _correctNo;
		if (-1 == slotNo || -1 == correctNo)
		{
			return;
		}
		if ((bool)title)
		{
			title.text = $"{CharaCustomDefine.CustomCorrectTitle[Singleton<GameSystem>.Instance.languageInt]}{correctNo + 1:00}";
		}
		UpdateCustomUI();
		if (lstDisposable != null && lstDisposable.Count != 0)
		{
			int count = lstDisposable.Count;
			for (int i = 0; i < count; i++)
			{
				lstDisposable[i].Dispose();
			}
		}
		IDisposable disposable = null;
		tglPosRate.Select((Toggle p, int num) => new
		{
			toggle = p,
			index = (byte)num
		}).ToList().ForEach(p =>
		{
			disposable = (from isOn in p.toggle.OnValueChangedAsObservable()
				where isOn
				select isOn).Subscribe(delegate
			{
				acsCtrlSetting.correctSetting[correctNo].posRate = p.index;
			});
			lstDisposable.Add(disposable);
		});
		tglRotRate.Select((Toggle p, int num) => new
		{
			toggle = p,
			index = (byte)num
		}).ToList().ForEach(p =>
		{
			disposable = (from isOn in p.toggle.OnValueChangedAsObservable()
				where isOn
				select isOn).Subscribe(delegate
			{
				acsCtrlSetting.correctSetting[correctNo].rotRate = p.index;
			});
			lstDisposable.Add(disposable);
		});
		tglSclRate.Select((Toggle p, int num) => new
		{
			toggle = p,
			index = (byte)num
		}).ToList().ForEach(p =>
		{
			disposable = (from isOn in p.toggle.OnValueChangedAsObservable()
				where isOn
				select isOn).Subscribe(delegate
			{
				acsCtrlSetting.correctSetting[correctNo].sclRate = p.index;
			});
			lstDisposable.Add(disposable);
		});
		float downTimeCnt = 0f;
		float loopTimeCnt = 0f;
		bool change = false;
		int[] flag = new int[3] { 1, 2, 4 };
		btnPos.Select((Button p, int index) => new
		{
			btn = p,
			index = index
		}).ToList().ForEach(p =>
		{
			disposable = p.btn.OnClickAsObservable().Subscribe(delegate
			{
				if (!change)
				{
					int num = p.index / 2;
					int num2 = ((p.index % 2 != 0) ? 1 : (-1));
					if (num == 0)
					{
						num2 *= -1;
					}
					float value = (float)num2 * movePosValue[acsCtrlSetting.correctSetting[correctNo].posRate];
					chaCtrl.SetAccessoryPos(slotNo, correctNo, value, add: true, flag[num]);
					orgAcs.parts[slotNo].addMove[correctNo, 0] = nowAcs.parts[slotNo].addMove[correctNo, 0];
					inpPos[num].text = nowAcs.parts[slotNo].addMove[correctNo, 0][num].ToString();
					SetControllerTransform();
				}
			});
			lstDisposable.Add(disposable);
			disposable = p.btn.UpdateAsObservable().SkipUntil(p.btn.OnPointerDownAsObservable().Do(delegate
			{
				downTimeCnt = 0f;
				loopTimeCnt = 0f;
				change = false;
			})).TakeUntil(p.btn.OnPointerUpAsObservable())
				.RepeatUntilDestroy(this)
				.Subscribe(delegate
				{
					int num = p.index / 2;
					int num2 = ((p.index % 2 != 0) ? 1 : (-1));
					if (num == 0)
					{
						num2 *= -1;
					}
					float num3 = (float)num2 * movePosValue[acsCtrlSetting.correctSetting[correctNo].posRate];
					float num4 = 0f;
					downTimeCnt += Time.deltaTime;
					if (downTimeCnt > 0.3f)
					{
						for (loopTimeCnt += Time.deltaTime; loopTimeCnt > 0.05f; loopTimeCnt -= 0.05f)
						{
							num4 += num3;
						}
						if (num4 != 0f)
						{
							chaCtrl.SetAccessoryPos(slotNo, correctNo, num4, add: true, flag[num]);
							orgAcs.parts[slotNo].addMove[correctNo, 0] = nowAcs.parts[slotNo].addMove[correctNo, 0];
							inpPos[num].text = nowAcs.parts[slotNo].addMove[correctNo, 0][num].ToString();
							change = true;
							SetControllerTransform();
						}
					}
				})
				.AddTo(this);
			lstDisposable.Add(disposable);
		});
		inpPos.Select((InputField p, int index) => new
		{
			inp = p,
			index = index
		}).ToList().ForEach(p =>
		{
			disposable = p.inp.onEndEdit.AsObservable().Subscribe(delegate(string value)
			{
				int num = p.index % 3;
				float value2 = CustomBase.ConvertValueFromTextLimit(-100f, 100f, 1, value);
				p.inp.text = value2.ToString();
				chaCtrl.SetAccessoryPos(slotNo, correctNo, value2, add: false, flag[num]);
				orgAcs.parts[slotNo].addMove[correctNo, 0] = nowAcs.parts[slotNo].addMove[correctNo, 0];
				SetControllerTransform();
			});
			lstDisposable.Add(disposable);
		});
		btnPosReset.Select((Button p, int index) => new
		{
			btn = p,
			index = index
		}).ToList().ForEach(p =>
		{
			disposable = p.btn.OnClickAsObservable().Subscribe(delegate
			{
				inpPos[p.index].text = "0";
				chaCtrl.SetAccessoryPos(slotNo, correctNo, 0f, add: false, flag[p.index]);
				orgAcs.parts[slotNo].addMove[correctNo, 0] = nowAcs.parts[slotNo].addMove[correctNo, 0];
				SetControllerTransform();
			});
			lstDisposable.Add(disposable);
		});
		btnRot.Select((Button p, int index) => new
		{
			btn = p,
			index = index
		}).ToList().ForEach(p =>
		{
			disposable = p.btn.OnClickAsObservable().Subscribe(delegate
			{
				if (!change)
				{
					int num = p.index / 2;
					float value = (float)((p.index % 2 != 0) ? 1 : (-1)) * moveRotValue[acsCtrlSetting.correctSetting[correctNo].rotRate];
					chaCtrl.SetAccessoryRot(slotNo, correctNo, value, add: true, flag[num]);
					orgAcs.parts[slotNo].addMove[correctNo, 1] = nowAcs.parts[slotNo].addMove[correctNo, 1];
					inpRot[num].text = nowAcs.parts[slotNo].addMove[correctNo, 1][num].ToString();
					SetControllerTransform();
				}
			});
			lstDisposable.Add(disposable);
			disposable = p.btn.UpdateAsObservable().SkipUntil(p.btn.OnPointerDownAsObservable().Do(delegate
			{
				downTimeCnt = 0f;
				loopTimeCnt = 0f;
				change = false;
			})).TakeUntil(p.btn.OnPointerUpAsObservable())
				.RepeatUntilDestroy(this)
				.Subscribe(delegate
				{
					int num = p.index / 2;
					float num2 = (float)((p.index % 2 != 0) ? 1 : (-1)) * moveRotValue[acsCtrlSetting.correctSetting[correctNo].rotRate];
					float num3 = 0f;
					downTimeCnt += Time.deltaTime;
					if (downTimeCnt > 0.3f)
					{
						for (loopTimeCnt += Time.deltaTime; loopTimeCnt > 0.05f; loopTimeCnt -= 0.05f)
						{
							num3 += num2;
						}
						if (num3 != 0f)
						{
							chaCtrl.SetAccessoryRot(slotNo, correctNo, num3, add: true, flag[num]);
							orgAcs.parts[slotNo].addMove[correctNo, 1] = nowAcs.parts[slotNo].addMove[correctNo, 1];
							inpRot[num].text = nowAcs.parts[slotNo].addMove[correctNo, 1][num].ToString();
							change = true;
							SetControllerTransform();
						}
					}
				})
				.AddTo(this);
			lstDisposable.Add(disposable);
		});
		inpRot.Select((InputField p, int index) => new
		{
			inp = p,
			index = index
		}).ToList().ForEach(p =>
		{
			disposable = p.inp.onEndEdit.AsObservable().Subscribe(delegate(string value)
			{
				int num = p.index % 3;
				float value2 = CustomBase.ConvertValueFromTextLimit(0f, 360f, 0, value);
				p.inp.text = value2.ToString();
				chaCtrl.SetAccessoryRot(slotNo, correctNo, value2, add: false, flag[num]);
				orgAcs.parts[slotNo].addMove[correctNo, 1] = nowAcs.parts[slotNo].addMove[correctNo, 1];
				SetControllerTransform();
			});
			lstDisposable.Add(disposable);
		});
		btnRotReset.Select((Button p, int index) => new
		{
			btn = p,
			index = index
		}).ToList().ForEach(p =>
		{
			disposable = p.btn.OnClickAsObservable().Subscribe(delegate
			{
				inpRot[p.index].text = "0";
				chaCtrl.SetAccessoryRot(slotNo, correctNo, 0f, add: false, flag[p.index]);
				orgAcs.parts[slotNo].addMove[correctNo, 1] = nowAcs.parts[slotNo].addMove[correctNo, 1];
				SetControllerTransform();
			});
			lstDisposable.Add(disposable);
		});
		btnScl.Select((Button p, int index) => new
		{
			btn = p,
			index = index
		}).ToList().ForEach(p =>
		{
			disposable = p.btn.OnClickAsObservable().Subscribe(delegate
			{
				if (!change)
				{
					int num = p.index / 2;
					float value = (float)((p.index % 2 != 0) ? 1 : (-1)) * moveSclValue[acsCtrlSetting.correctSetting[correctNo].sclRate];
					chaCtrl.SetAccessoryScl(slotNo, correctNo, value, add: true, flag[num]);
					orgAcs.parts[slotNo].addMove[correctNo, 2] = nowAcs.parts[slotNo].addMove[correctNo, 2];
					inpScl[num].text = nowAcs.parts[slotNo].addMove[correctNo, 2][num].ToString();
				}
			});
			lstDisposable.Add(disposable);
			disposable = p.btn.UpdateAsObservable().SkipUntil(p.btn.OnPointerDownAsObservable().Do(delegate
			{
				downTimeCnt = 0f;
				loopTimeCnt = 0f;
				change = false;
			})).TakeUntil(p.btn.OnPointerUpAsObservable())
				.RepeatUntilDestroy(this)
				.Subscribe(delegate
				{
					int num = p.index / 2;
					float num2 = (float)((p.index % 2 != 0) ? 1 : (-1)) * moveSclValue[acsCtrlSetting.correctSetting[correctNo].sclRate];
					float num3 = 0f;
					downTimeCnt += Time.deltaTime;
					if (downTimeCnt > 0.3f)
					{
						for (loopTimeCnt += Time.deltaTime; loopTimeCnt > 0.05f; loopTimeCnt -= 0.05f)
						{
							num3 += num2;
						}
						if (num3 != 0f)
						{
							chaCtrl.SetAccessoryScl(slotNo, correctNo, num3, add: true, flag[num]);
							orgAcs.parts[slotNo].addMove[correctNo, 2] = nowAcs.parts[slotNo].addMove[correctNo, 2];
							inpScl[num].text = nowAcs.parts[slotNo].addMove[correctNo, 2][num].ToString();
							change = true;
						}
					}
				})
				.AddTo(this);
			lstDisposable.Add(disposable);
		});
		inpScl.Select((InputField p, int index) => new
		{
			inp = p,
			index = index
		}).ToList().ForEach(p =>
		{
			disposable = p.inp.onEndEdit.AsObservable().Subscribe(delegate(string value)
			{
				int num = p.index % 3;
				float value2 = CustomBase.ConvertValueFromTextLimit(0.01f, 100f, 2, value);
				p.inp.text = value2.ToString();
				chaCtrl.SetAccessoryScl(slotNo, correctNo, value2, add: false, flag[num]);
				orgAcs.parts[slotNo].addMove[correctNo, 2] = nowAcs.parts[slotNo].addMove[correctNo, 2];
			});
			lstDisposable.Add(disposable);
		});
		btnSclReset.Select((Button p, int index) => new
		{
			btn = p,
			index = index
		}).ToList().ForEach(p =>
		{
			disposable = p.btn.OnClickAsObservable().Subscribe(delegate
			{
				inpScl[p.index].text = "1";
				chaCtrl.SetAccessoryScl(slotNo, correctNo, 1f, add: false, flag[p.index]);
				orgAcs.parts[slotNo].addMove[correctNo, 2] = nowAcs.parts[slotNo].addMove[correctNo, 2];
			});
			lstDisposable.Add(disposable);
		});
		disposable = btnAllReset.OnClickAsObservable().Subscribe(delegate
		{
			for (int j = 0; j < 3; j++)
			{
				inpPos[j].text = "0";
				chaCtrl.SetAccessoryPos(slotNo, correctNo, 0f, add: false, flag[j]);
				orgAcs.parts[slotNo].addMove[correctNo, 0] = nowAcs.parts[slotNo].addMove[correctNo, 0];
				SetControllerTransform();
				inpRot[j].text = "0";
				chaCtrl.SetAccessoryRot(slotNo, correctNo, 0f, add: false, flag[j]);
				orgAcs.parts[slotNo].addMove[correctNo, 1] = nowAcs.parts[slotNo].addMove[correctNo, 1];
				SetControllerTransform();
				inpScl[j].text = "1";
				chaCtrl.SetAccessoryScl(slotNo, correctNo, 1f, add: false, flag[j]);
				orgAcs.parts[slotNo].addMove[correctNo, 2] = nowAcs.parts[slotNo].addMove[correctNo, 2];
			}
		});
		lstDisposable.Add(disposable);
		disposable = tglDrawCtrl.OnValueChangedAsObservable().Subscribe(delegate(bool isOn)
		{
			customBase.customSettingSave.acsCtrlSetting.correctSetting[correctNo].draw = isOn;
		});
		lstDisposable.Add(disposable);
		if (tglCtrlType.Any())
		{
			(from item in tglCtrlType.Select((Toggle val, int idx) => new { val, idx })
				where item.val != null
				select item).ToList().ForEach(item =>
			{
				disposable = (from isOn in item.val.OnValueChangedAsObservable()
					where isOn
					select isOn).Subscribe(delegate
				{
					customBase.customSettingSave.acsCtrlSetting.correctSetting[correctNo].type = item.idx;
					if ((bool)cmpGuid)
					{
						cmpGuid.SetMode(item.idx);
					}
				});
				lstDisposable.Add(disposable);
			});
		}
		disposable = sldCtrlSpeed.OnValueChangedAsObservable().Subscribe(delegate(float val)
		{
			customBase.customSettingSave.acsCtrlSetting.correctSetting[correctNo].speed = val;
			if ((bool)cmpGuid)
			{
				cmpGuid.speedMove = val;
			}
		});
		lstDisposable.Add(disposable);
		disposable = sldCtrlSpeed.OnScrollAsObservable().Subscribe(delegate(PointerEventData scl)
		{
			if (customBase.sliderControlWheel)
			{
				sldCtrlSpeed.value = Mathf.Clamp(sldCtrlSpeed.value + scl.scrollDelta.y * -0.01f, 0.1f, 1f);
			}
		});
		lstDisposable.Add(disposable);
		disposable = sldCtrlSize.OnValueChangedAsObservable().Subscribe(delegate(float val)
		{
			customBase.customSettingSave.acsCtrlSetting.correctSetting[correctNo].scale = val;
			if ((bool)cmpGuid)
			{
				cmpGuid.scaleAxis = val;
				cmpGuid.UpdateScale();
			}
		});
		lstDisposable.Add(disposable);
		disposable = sldCtrlSize.OnScrollAsObservable().Subscribe(delegate(PointerEventData scl)
		{
			if (customBase.sliderControlWheel)
			{
				sldCtrlSize.value = Mathf.Clamp(sldCtrlSize.value + scl.scrollDelta.y * -0.01f, 0.3f, 3f);
			}
		});
		lstDisposable.Add(disposable);
		UpdateDrawControllerState();
	}

	private void Start()
	{
		for (int i = 0; i < 3; i++)
		{
			customBase.lstInputField.Add(inpPos[i]);
			customBase.lstInputField.Add(inpRot[i]);
			customBase.lstInputField.Add(inpScl[i]);
		}
	}

	private void LateUpdate()
	{
		if (null != cmpGuid && cmpGuid.gameObject.activeInHierarchy)
		{
			if (cmpGuid.isDrag)
			{
				SetAccessoryTransform(updateInfo: false);
			}
			else if (isDrag)
			{
				SetAccessoryTransform(updateInfo: true);
			}
			else
			{
				SetControllerTransform();
			}
			isDrag = cmpGuid.isDrag;
			customBase.cursorDraw = !cmpGuid.isDrag;
		}
	}
}
