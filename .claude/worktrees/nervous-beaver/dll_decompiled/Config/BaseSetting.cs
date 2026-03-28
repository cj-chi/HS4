using System;
using System.Collections.Generic;
using System.Linq;
using Illusion.Game;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Config;

public abstract class BaseSetting : MonoBehaviour
{
	private bool _isPlaySE = true;

	public bool isPlaySE
	{
		get
		{
			return _isPlaySE;
		}
		set
		{
			_isPlaySE = value;
		}
	}

	protected void EnterSE()
	{
		if (_isPlaySE)
		{
			Utils.Sound.Play(SystemSE.ok_s);
		}
	}

	protected void LinkToggle(Toggle toggle, Action<bool> act)
	{
		toggle.onValueChanged.AsObservable().Subscribe(delegate(bool isOn)
		{
			EnterSE();
			act(isOn);
		});
	}

	protected void LinkToggleArray(Toggle[] _tgls, Action<int> _action)
	{
		(from list in _tgls.Select((Toggle t) => t.OnValueChangedAsObservable()).CombineLatest()
			select list.IndexOf(item: true) into i
			where i >= 0
			select i).ToReadOnlyReactiveProperty().Skip(1).Subscribe(delegate(int i)
		{
			_action?.Invoke(i);
			EnterSE();
		});
	}

	protected void LinkSlider(Slider slider, Action<float> act)
	{
		slider.onValueChanged.AsObservable().Subscribe(delegate(float value)
		{
			act(value);
		});
		(from _ in slider.OnPointerDownAsObservable()
			where Input.GetMouseButtonDown(0)
			select _).Subscribe(delegate
		{
			EnterSE();
		});
	}

	protected void LinkTmpDropdown(TMP_Dropdown dropdown, Action<float> act)
	{
		dropdown.onValueChanged.AsObservable().Subscribe(delegate(int value)
		{
			act(value);
		});
		(from _ in dropdown.OnPointerDownAsObservable()
			where Input.GetMouseButtonDown(0)
			select _).Subscribe(delegate
		{
			EnterSE();
		});
	}

	protected void SetToggleUIArray(Toggle[] _toggles, Action<Toggle, int> _action)
	{
		foreach (var item in _toggles.Select((Toggle tgl, int index) => new { tgl, index }))
		{
			_action(item.tgl, item.index);
		}
	}

	public abstract void Init();

	public virtual void Setup()
	{
	}

	protected abstract void ValueToUI();

	public void UIPresenter()
	{
		bool flag = _isPlaySE;
		_isPlaySE = false;
		ValueToUI();
		_isPlaySE = flag;
	}
}
