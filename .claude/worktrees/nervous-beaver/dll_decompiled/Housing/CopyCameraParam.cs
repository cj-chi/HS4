using UniRx;
using UnityEngine;

namespace Housing;

public class CopyCameraParam : MonoBehaviour
{
	[SerializeField]
	private Camera mainCamera;

	[SerializeField]
	private Camera targetCamera;

	private void Start()
	{
		mainCamera.ObserveEveryValueChanged((Camera _c) => _c.fieldOfView).Subscribe(delegate(float _f)
		{
			targetCamera.fieldOfView = _f;
		}).AddTo(this);
		mainCamera.ObserveEveryValueChanged((Camera _c) => _c.nearClipPlane).Subscribe(delegate(float _f)
		{
			targetCamera.nearClipPlane = _f;
		}).AddTo(this);
		mainCamera.ObserveEveryValueChanged((Camera _c) => _c.farClipPlane).Subscribe(delegate(float _f)
		{
			targetCamera.farClipPlane = _f;
		}).AddTo(this);
	}
}
