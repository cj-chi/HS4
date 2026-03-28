using Illusion.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Illusion.Component.UI;

public class SpriteChangeCtrl : MonoBehaviour
{
	public Sprite[] sprites;

	private Image _image;

	public Image image => _image;

	private void Awake()
	{
		_image = GetComponent<Image>();
	}

	public void ChangeValue(int _num, bool _isEnableChange = true)
	{
		if (!(_image == null) && sprites.Length > _num)
		{
			bool flag = _num >= 0;
			if (_isEnableChange)
			{
				_image.enabled = flag;
			}
			if (flag)
			{
				_image.sprite = sprites[_num];
			}
		}
	}

	public int GetCount()
	{
		return sprites.Length;
	}

	public int GetVisibleNumber()
	{
		if (_image == null)
		{
			return -1;
		}
		for (int i = 0; i < sprites.Length; i++)
		{
			if (_image.sprite == sprites[i])
			{
				return i;
			}
		}
		return 0;
	}

	public void Visible(bool _visible)
	{
		base.gameObject.SetActiveIfDifferent(_visible);
	}
}
