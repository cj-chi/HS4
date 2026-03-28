using UnityEngine;

public class AnimatedFountainUVs : MonoBehaviour
{
	public int _uvTieX = 1;

	public int _uvTieY = 1;

	public int _fps = 10;

	private Vector2 _size;

	private Renderer _myRenderer;

	private int _lastIndex = -1;

	private void Start()
	{
		_size = new Vector2(1f / (float)_uvTieX, 1f / (float)_uvTieY);
		_myRenderer = GetComponent<Renderer>();
		if (_myRenderer == null)
		{
			base.enabled = false;
		}
	}

	private void Update()
	{
		int num = (int)(Time.timeSinceLevelLoad * (float)_fps) % (_uvTieX * _uvTieY);
		if (num != _lastIndex)
		{
			int num2 = num % _uvTieX;
			int num3 = num / _uvTieY;
			Vector2 value = new Vector2((float)num2 * _size.x, 1f - _size.y - (float)num3 * _size.y);
			_myRenderer.material.SetTextureOffset("_MainTex", value);
			_myRenderer.material.SetTextureScale("_MainTex", _size);
			_lastIndex = num;
		}
	}
}
