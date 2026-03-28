namespace UnityEngine.UI.ProceduralImage;

[DisallowMultipleComponent]
public abstract class ProceduralImageModifier : MonoBehaviour
{
	public abstract Vector4 CalculateRadius(Rect imageRect);
}
