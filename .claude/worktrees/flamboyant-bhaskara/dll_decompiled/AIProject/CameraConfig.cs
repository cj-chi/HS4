using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace AIProject;

public class CameraConfig : MonoBehaviour
{
	[SerializeField]
	private PostProcessProfile _ppProfile;

	[SerializeField]
	private PostProcessLayer _ppLayer;

	public PostProcessProfile PPProfile => _ppProfile;

	public PostProcessLayer PPLayer => _ppLayer;

	private void Reset()
	{
	}
}
