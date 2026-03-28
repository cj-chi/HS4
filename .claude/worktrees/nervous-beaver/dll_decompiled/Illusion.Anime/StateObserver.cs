using UnityEngine;

namespace Illusion.Anime;

public class StateObserver
{
	private Animator _animator { get; }

	public int nameHash
	{
		get
		{
			return _nameHash;
		}
		set
		{
			_nameHash = value;
		}
	}

	private int _nameHash { get; set; }

	public bool isLoop { get; private set; }

	public float normalizedTime => _normalizedTime;

	private float _normalizedTime { get; set; }

	private float _prevNormalizedTime { get; set; }

	private int _loopCount { get; set; }

	public StateObserver(Animator animator)
	{
		_animator = animator;
	}

	public bool Update()
	{
		if (_animator.runtimeAnimatorController == null)
		{
			return false;
		}
		AnimatorStateInfo currentAnimatorStateInfo = _animator.GetCurrentAnimatorStateInfo(0);
		bool num = _nameHash != currentAnimatorStateInfo.shortNameHash;
		if (num)
		{
			_nameHash = currentAnimatorStateInfo.shortNameHash;
			_prevNormalizedTime = 0f;
			_loopCount = 0;
		}
		_normalizedTime = currentAnimatorStateInfo.normalizedTime;
		bool loop = currentAnimatorStateInfo.loop;
		int num2 = (int)_normalizedTime;
		if (loop)
		{
			_normalizedTime = Mathf.Repeat(_normalizedTime, 1f);
		}
		isLoop = loop && (_loopCount < num2 || _normalizedTime < _prevNormalizedTime);
		_prevNormalizedTime = _normalizedTime;
		_loopCount = num2;
		return num;
	}
}
