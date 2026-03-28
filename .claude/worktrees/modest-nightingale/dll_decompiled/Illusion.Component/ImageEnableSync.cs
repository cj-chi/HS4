using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Illusion.Component;

internal class ImageEnableSync : MonoBehaviour
{
	[Header("参照Image")]
	[SerializeField]
	private Image refImage;

	[Header("同期するImage")]
	[SerializeField]
	private Image image;

	private void Awake()
	{
		if (refImage == null)
		{
			Object.Destroy(this);
		}
		else if (!image)
		{
			image = GetComponent<Image>();
		}
	}

	private void Start()
	{
		BoolReactiveProperty isInteract = new BoolReactiveProperty(refImage.enabled);
		isInteract.Subscribe(delegate(bool isOn)
		{
			if ((bool)image)
			{
				image.enabled = isOn;
			}
		});
		this.OnEnableAsObservable().Subscribe(delegate
		{
			isInteract.Value = refImage.enabled;
		});
		(from _ in this.UpdateAsObservable()
			select refImage.enabled).DistinctUntilChanged().Subscribe(delegate(bool interactable)
		{
			isInteract.Value = interactable;
		});
	}
}
