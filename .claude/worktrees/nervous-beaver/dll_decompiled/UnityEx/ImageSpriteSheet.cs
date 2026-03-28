using UnityEngine;
using UnityEngine.UI;

namespace UnityEx;

[RequireComponent(typeof(Image))]
public class ImageSpriteSheet : MonoBehaviour
{
	[SerializeField]
	private Image _imageComponent;

	[SerializeField]
	private bool _isForwardAnimation = true;

	[SerializeField]
	private float _fps = 30f;

	[SerializeField]
	private Sprite[] _sprites;

	[SerializeField]
	private bool _ignoreTimeScale;

	private float _elapsedTime;

	private int _index;

	public float FPS
	{
		get
		{
			return _fps;
		}
		set
		{
			_fps = value;
		}
	}

	public Sprite[] Sprites
	{
		get
		{
			return _sprites;
		}
		set
		{
			_sprites = value;
			_index = 0;
			if (_imageComponent != null)
			{
				if (value == null || value.Length == 0)
				{
					_imageComponent.sprite = null;
				}
				else
				{
					_imageComponent.sprite = _sprites[_index];
				}
			}
		}
	}

	public bool IgnoreTimeScale
	{
		get
		{
			return _ignoreTimeScale;
		}
		set
		{
			_ignoreTimeScale = value;
		}
	}

	private void Reset()
	{
		_imageComponent = GetComponent<Image>();
	}

	private void Update()
	{
		if (_imageComponent == null || _sprites == null || _sprites.Length == 0)
		{
			return;
		}
		if (_ignoreTimeScale)
		{
			_elapsedTime += Time.unscaledDeltaTime;
		}
		else
		{
			_elapsedTime += Time.deltaTime;
		}
		float num = 1f / _fps;
		if (_elapsedTime > num)
		{
			if (_isForwardAnimation)
			{
				_index++;
			}
			else
			{
				_index--;
			}
			_elapsedTime %= num;
		}
		if (_index >= _sprites.Length)
		{
			_index = 0;
		}
		else if (_index < 0)
		{
			_index = _sprites.Length - 1;
		}
		_imageComponent.sprite = _sprites[_index];
	}
}
