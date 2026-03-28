using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class FadeCanvasOnEvent : FadeCanvas
{
	public IReadOnlyList<Selectable> selectables => _selectables;

	private IReadOnlyList<Selectable> _selectables { get; set; }

	private bool[] _initInteractables { get; set; }

	private bool refreshed { get; set; }

	public void Enable(bool interactable)
	{
		for (int i = 0; i < _selectables.Count; i++)
		{
			_selectables[i].interactable = _initInteractables[i] && interactable;
		}
	}

	public void Refresh()
	{
		_selectables = GetComponentsInChildren<Selectable>(includeInactive: true);
		_initInteractables = _selectables.Select((Selectable x) => x.interactable).ToArray();
		refreshed = true;
	}

	protected override void Awake()
	{
		base.Awake();
		if (!refreshed)
		{
			Refresh();
		}
		base.onStart += delegate(Fade fade)
		{
			if (fade == Fade.Out)
			{
				Enable(interactable: false);
			}
		};
		base.onComplete += delegate(Fade fade)
		{
			if (fade == Fade.In)
			{
				Enable(interactable: true);
			}
		};
	}
}
