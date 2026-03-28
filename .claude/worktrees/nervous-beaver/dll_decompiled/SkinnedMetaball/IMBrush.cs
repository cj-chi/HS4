using UnityEngine;

namespace SkinnedMetaball;

public abstract class IMBrush : MonoBehaviour
{
	public IncrementalModeling im;

	public float fadeRadius = 0.2f;

	public bool bSubtract;

	public float PowerScale
	{
		get
		{
			if (!bSubtract)
			{
				return 1f;
			}
			return -1f;
		}
	}

	[ContextMenu("Draw")]
	public void Draw()
	{
		if (im == null)
		{
			im = Utils.FindComponentInParents<IncrementalModeling>(base.transform);
		}
		if (im != null)
		{
			DoDraw();
		}
	}

	protected abstract void DoDraw();
}
