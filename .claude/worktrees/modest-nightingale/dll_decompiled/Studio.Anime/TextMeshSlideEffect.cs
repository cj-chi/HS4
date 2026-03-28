using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Studio.Anime;

public class TextMeshSlideEffect : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private RectTransform transBase;

	[SerializeField]
	private TextMeshProUGUI textMesh;

	public float speed = 50f;

	private SingleAssignmentDisposable sildeDisposable;

	private bool isPlay;

	private bool isMove;

	private float ParentWidth
	{
		get
		{
			if (!(transBase != null))
			{
				return 0f;
			}
			return transBase.sizeDelta.x;
		}
	}

	private float PreferredWidth
	{
		get
		{
			if (!(textMesh != null))
			{
				return 0f;
			}
			return textMesh.preferredWidth;
		}
	}

	public void Stop()
	{
		if (isPlay)
		{
			isPlay = false;
			textMesh.margin = Vector4.zero;
			textMesh.overflowMode = TextOverflowModes.Ellipsis;
		}
	}

	public void OnChangedText()
	{
		if (sildeDisposable != null)
		{
			sildeDisposable.Dispose();
			sildeDisposable = null;
		}
		textMesh.margin = Vector4.zero;
		textMesh.alignment = TextAlignmentOptions.Center;
		textMesh.overflowMode = TextOverflowModes.Overflow;
		textMesh.enableWordWrapping = false;
		CheckText();
	}

	private async void CheckText()
	{
		await Observable.EveryLateUpdate().First();
		if (PreferredWidth <= ParentWidth)
		{
			isMove = false;
			return;
		}
		isMove = true;
		textMesh.alignment = TextAlignmentOptions.MidlineLeft;
		textMesh.overflowMode = TextOverflowModes.Ellipsis;
		sildeDisposable = new SingleAssignmentDisposable();
		sildeDisposable.Disposable = Observable.EveryUpdate().Subscribe(delegate
		{
			if (isPlay)
			{
				Vector4 margin = textMesh.margin;
				if (Mathf.Abs(margin.x) > PreferredWidth)
				{
					margin.x += PreferredWidth + ParentWidth;
				}
				margin.x -= speed * Time.deltaTime;
				margin.z = 0f - margin.x;
				textMesh.margin = margin;
			}
		}).AddTo(this);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (isMove)
		{
			isPlay = true;
			textMesh.overflowMode = TextOverflowModes.Overflow;
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (isMove)
		{
			isPlay = false;
			textMesh.margin = Vector4.zero;
			textMesh.overflowMode = TextOverflowModes.Ellipsis;
		}
	}
}
