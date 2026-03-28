using UnityEngine;

public class UVScroll : MonoBehaviour
{
	[SerializeField]
	private float scrollSpeedX = 0.1f;

	[SerializeField]
	private float scrollSpeedY = 0.1f;

	private void Start()
	{
		GetComponent<Renderer>().sharedMaterial.SetTextureOffset("_MainTex", Vector2.zero);
	}

	private void Update()
	{
		float x = Mathf.Repeat(Time.time * scrollSpeedX, 1f);
		float y = Mathf.Repeat(Time.time * scrollSpeedY, 1f);
		Vector2 value = new Vector2(x, y);
		GetComponent<Renderer>().sharedMaterial.SetTextureOffset("_MainTex", value);
	}
}
