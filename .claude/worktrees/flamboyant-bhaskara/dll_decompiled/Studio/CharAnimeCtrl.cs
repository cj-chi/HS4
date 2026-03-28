using RootMotion.FinalIK;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Studio;

public class CharAnimeCtrl : MonoBehaviour
{
	public Animator animator;

	public Transform transSon;

	public bool isForceLoop
	{
		get
		{
			if (oiCharInfo == null)
			{
				return false;
			}
			return oiCharInfo.isAnimeForceLoop;
		}
		set
		{
			oiCharInfo.isAnimeForceLoop = value;
		}
	}

	public OICharInfo oiCharInfo { get; set; }

	public int nameHadh { get; set; }

	public float normalizedTime
	{
		get
		{
			if (animator == null)
			{
				return 0f;
			}
			return animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		}
	}

	public bool FixTransform { get; set; } = true;

	public IKMappingSpine spineMapping { get; set; }

	public IKMappingBone[] mappingBones { get; set; }

	public IKMappingLimb[] limbMappings { get; set; }

	private bool isSon
	{
		get
		{
			if (oiCharInfo == null)
			{
				return false;
			}
			return oiCharInfo.visibleSon;
		}
	}

	public void Play(string _name)
	{
		animator?.Play(_name);
	}

	public void Play(string _name, float _normalizedTime)
	{
		if (!(animator == null))
		{
			animator.Play(_name, 0, _normalizedTime);
		}
	}

	public void Play(string _name, float _normalizedTime, int _layer)
	{
		if (!(animator == null))
		{
			if (_normalizedTime != 0f)
			{
				animator.Play(_name, _layer, _normalizedTime);
			}
			else
			{
				animator.Play(_name, _layer);
			}
		}
	}

	private void Awake()
	{
		(from _ in this.LateUpdateAsObservable()
			where isForceLoop
			select _).Subscribe(delegate
		{
			AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
			if (!currentAnimatorStateInfo.loop && currentAnimatorStateInfo.normalizedTime >= 1f)
			{
				animator.Play(nameHadh, 0, 0f);
			}
		});
		this.LateUpdateAsObservable().Subscribe(delegate
		{
			if (isSon && (bool)transSon)
			{
				transSon.localScale = new Vector3(oiCharInfo.sonLength, oiCharInfo.sonLength, oiCharInfo.sonLength);
			}
		});
	}
}
