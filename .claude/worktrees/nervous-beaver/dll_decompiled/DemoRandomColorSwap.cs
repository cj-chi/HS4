using UnityEngine;

public class DemoRandomColorSwap : MonoBehaviour
{
	[SerializeField]
	private Gradient gradient;

	private Material mat;

	private Texture texture;

	private void Start()
	{
		if (GetComponent<SpriteRenderer>() != null)
		{
			mat = GetComponent<Renderer>().material;
			mat.SetFloat("_Alpha", 1f);
			mat.SetColor("_Color", new Color(0.5f, 1f, 0f, 1f));
			mat.SetTexture("_MainTex", texture);
			InvokeRepeating("NewColor", 0f, 0.6f);
		}
	}

	private void NewColor()
	{
		mat.SetColor("_ColorSwapRed", gradient.Evaluate(Random.value));
		mat.SetColor("_ColorSwapGreen", gradient.Evaluate(Random.value));
		mat.SetColor("_ColorSwapBlue", gradient.Evaluate(Random.value));
	}
}
