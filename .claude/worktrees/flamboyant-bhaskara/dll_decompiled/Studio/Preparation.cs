using UnityEngine;

namespace Studio;

public class Preparation : MonoBehaviour
{
	[SerializeField]
	private Transform _lookAtTarget;

	[SerializeField]
	private CharAnimeCtrl _charAnimeCtrl;

	[SerializeField]
	private IKCtrl _IKCtrl;

	[SerializeField]
	private PVCopy _pvCopy;

	[SerializeField]
	private YureCtrl _yureCtrl;

	public Transform LookAtTarget => _lookAtTarget;

	public CharAnimeCtrl CharAnimeCtrl => _charAnimeCtrl;

	public IKCtrl IKCtrl => _IKCtrl;

	public PVCopy PvCopy => _pvCopy;

	public YureCtrl YureCtrl => _yureCtrl;
}
