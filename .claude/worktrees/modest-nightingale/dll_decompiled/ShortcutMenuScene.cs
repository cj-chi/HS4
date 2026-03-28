using Manager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class ShortcutMenuScene : MonoBehaviour
{
	[SerializeField]
	private Image image;

	[SerializeField]
	private Sprite[] sprites;

	[SerializeField]
	private Toggle[] toggles;

	private IntReactiveProperty selectReactive = new IntReactiveProperty(-1);

	public int select
	{
		get
		{
			return selectReactive.Value;
		}
		set
		{
			selectReactive.Value = value;
		}
	}

	private void Awake()
	{
		selectReactive.Where((int _i) => MathfEx.RangeEqualOn(0, _i, sprites.Length - 1)).Subscribe(delegate(int _i)
		{
			image.sprite = sprites[_i];
			toggles[_i].isOn = true;
		});
		for (int num = 0; num < toggles.Length; num++)
		{
			int kind = num;
			(from _b in toggles[num].OnValueChangedAsObservable()
				where _b
				select _b).Subscribe(delegate
			{
				select = kind;
			});
		}
		(from _ in this.UpdateAsObservable()
			where Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.F2)
			select _).Subscribe(delegate
		{
			Scene.Unload();
		}).AddTo(this);
	}
}
