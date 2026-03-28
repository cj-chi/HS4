using UnityEngine;

public class WaterUvAnimation : MonoBehaviour
{
	public bool IsReverse;

	public float Speed = 1f;

	public int MaterialNomber;

	private Material mat;

	private float deltaFps;

	private bool isVisible;

	private bool isCorutineStarted;

	private float offset;

	private float delta;

	private void Awake()
	{
		mat = GetComponent<Renderer>().materials[MaterialNomber];
	}

	private void Update()
	{
		if (IsReverse)
		{
			offset -= Time.deltaTime * Speed;
			if (offset < 0f)
			{
				offset = 1f;
			}
		}
		else
		{
			offset += Time.deltaTime * Speed;
			if (offset > 1f)
			{
				offset = 0f;
			}
		}
		Vector2 value = new Vector2(0f, offset);
		mat.SetTextureOffset("_BumpMap", value);
		mat.SetFloat("_OffsetYHeightMap", offset);
	}
}
