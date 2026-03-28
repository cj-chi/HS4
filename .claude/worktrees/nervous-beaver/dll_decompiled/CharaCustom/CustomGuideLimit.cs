using UnityEngine;

namespace CharaCustom;

public class CustomGuideLimit : MonoBehaviour
{
	public bool limited;

	public Transform trfParent;

	public Vector3 limitMin = Vector3.zero;

	public Vector3 limitMax = Vector3.zero;
}
