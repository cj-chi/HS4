using System;
using System.Collections.Generic;
using Illusion.Component.Correct.Process;
using UniRx;
using UnityEngine;

namespace Illusion.Component.Correct;

public abstract class BaseCorrect : MonoBehaviour
{
	[Serializable]
	public class Info
	{
		public enum ProcOrderType
		{
			First,
			Second,
			Last
		}

		public ProcOrderType type;

		[SerializeField]
		private UnityEngine.Component component;

		[SerializeField]
		private BaseProcess _process;

		public BaseProcess process => this.GetCacheObject(ref _process, CreateProcess);

		public BaseData data => process.data;

		public bool enabled
		{
			get
			{
				return process.enabled;
			}
			set
			{
				process.enabled = value;
			}
		}

		public Transform bone
		{
			get
			{
				return data.bone;
			}
			set
			{
				data.bone = value;
			}
		}

		public Vector3 pos
		{
			get
			{
				return data.pos;
			}
			set
			{
				data.pos = value;
			}
		}

		public Quaternion rot
		{
			get
			{
				return data.rot;
			}
			set
			{
				data.rot = value;
			}
		}

		public Vector3 ang
		{
			get
			{
				return data.ang;
			}
			set
			{
				data.ang = value;
			}
		}

		public Info(UnityEngine.Component component)
		{
			this.component = component;
		}

		public void ReSetup()
		{
			_process = CreateProcess();
		}

		private BaseProcess CreateProcess()
		{
			BaseProcess[] components = component.GetComponents<BaseProcess>();
			foreach (BaseProcess baseProcess in components)
			{
				BaseProcess delete = baseProcess;
				Observable.NextFrame().Subscribe(delegate
				{
					UnityEngine.Object.DestroyImmediate(delete);
				});
			}
			return type switch
			{
				ProcOrderType.First => component.gameObject.AddComponent<IKBeforeOfDankonProcess>(), 
				ProcOrderType.Second => component.gameObject.AddComponent<IKBeforeProcess>(), 
				ProcOrderType.Last => component.gameObject.AddComponent<IKAfterProcess>(), 
				_ => null, 
			};
		}
	}

	[SerializeField]
	private List<Info> _list = new List<Info>();

	public List<Info> list => _list;

	public abstract string[] GetFrameNames { get; }

	public bool isEnabled
	{
		set
		{
			_list.ForEach(delegate(Info item)
			{
				item.enabled = value;
			});
		}
	}
}
