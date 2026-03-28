using System.Collections.Generic;
using System.Text;
using MessagePack;
using UnityEngine;

namespace WorkSpace.KK;

public class PackTest : MonoBehaviour
{
	public enum TreeState
	{
		Open,
		Close
	}

	[MessagePackObject(true)]
	public class MemberInfo
	{
		public int No { get; set; }

		public bool Light { get; set; }

		public void Initialize()
		{
			No = -1;
			Light = true;
		}
	}

	[MessagePackObject(true)]
	public class ChangeAmount
	{
		protected Vector3 m_Pos = Vector3.zero;

		protected Vector3 m_Rot = Vector3.zero;

		protected Vector3 m_Scale = Vector3.one;

		public Vector3 Pos
		{
			get
			{
				return m_Pos;
			}
			set
			{
				m_Pos = value;
			}
		}

		public Vector3 Rot
		{
			get
			{
				return m_Rot;
			}
			set
			{
				m_Rot = value;
			}
		}

		public Vector3 Scale
		{
			get
			{
				return m_Scale;
			}
			set
			{
				m_Scale = value;
			}
		}

		public ChangeAmount()
		{
			m_Pos = Vector3.zero;
			m_Rot = Vector3.zero;
			m_Scale = Vector3.one;
		}

		[SerializationConstructor]
		public ChangeAmount(Vector3 pos, Vector3 rot, Vector3 scale)
		{
			m_Pos = pos;
			m_Rot = rot;
			m_Scale = scale;
		}

		public override string ToString()
		{
			return $"Pos[{Pos}] : Rot[{Rot}] : Scale[{Scale}]";
		}
	}

	[Union(0, typeof(OIItem))]
	[Union(1, typeof(OIChara))]
	[MessagePackObject(true)]
	public abstract class ObjectInfo
	{
		public int dicKey { get; private set; }

		[IgnoreMember]
		public abstract int kind { get; }

		public ChangeAmount changeAmount { get; protected set; }

		public TreeState treeState { get; set; }

		public bool visible { get; set; }

		[IgnoreMember]
		public virtual int[] kinds => new int[1] { kind };

		public abstract void DeleteKey();

		public ObjectInfo(int _key)
		{
			dicKey = _key;
			changeAmount = new ChangeAmount();
			treeState = TreeState.Close;
			visible = true;
		}

		[SerializationConstructor]
		public ObjectInfo(int dickey, ChangeAmount changeamount)
		{
			dicKey = dickey;
			changeAmount = changeamount;
		}

		public override string ToString()
		{
			return $"dicKey[{dicKey}]\nchangeAmount[{changeAmount.ToString()}]\ntreeState[{treeState}]\nvisible[{visible}]";
		}
	}

	[MessagePackObject(true)]
	public class OIItem : ObjectInfo
	{
		[IgnoreMember]
		public override int kind => 0;

		public Dictionary<int, ObjectInfo> child { get; private set; }

		public override void DeleteKey()
		{
		}

		public OIItem(int _key)
			: base(_key)
		{
			child = new Dictionary<int, ObjectInfo>();
		}

		[SerializationConstructor]
		public OIItem(int dickey, ChangeAmount changeamount, Dictionary<int, ObjectInfo> child)
			: base(dickey, changeamount)
		{
			this.child = child;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<int, ObjectInfo> item in child)
			{
				stringBuilder.AppendLine(item.ToString());
			}
			return base.ToString() + "\n\tchild\n" + stringBuilder.ToString();
		}
	}

	[MessagePackObject(true)]
	public class OIChara : ObjectInfo
	{
		[IgnoreMember]
		public override int kind => 1;

		public override void DeleteKey()
		{
		}

		public OIChara(int _key)
			: base(_key)
		{
		}

		[SerializationConstructor]
		public OIChara(int dickey, ChangeAmount changeamount)
			: base(dickey, changeamount)
		{
		}
	}

	[MessagePackObject(true)]
	public class TestInfo
	{
		public ChangeAmount caMap;

		public MemberInfo MemberInfo { get; set; }

		public Dictionary<int, ObjectInfo> Items { get; set; }

		public void Initialize()
		{
			MemberInfo = new MemberInfo();
			MemberInfo.Initialize();
			Items = new Dictionary<int, ObjectInfo>();
			OIItem oIItem = new OIItem(0);
			oIItem.changeAmount.Pos = new Vector3(1f, 5f, 9f);
			Items.Add(0, oIItem);
			OIChara oIChara = new OIChara(4);
			oIChara.changeAmount.Pos = new Vector3(9f, 6f, 3f);
			Items.Add(4, oIChara);
			OIItem oIItem2 = new OIItem(7);
			oIItem2.changeAmount.Pos = new Vector3(7f, 4f, 1f);
			OIItem oIItem3 = new OIItem(1);
			oIItem3.changeAmount.Rot = new Vector3(45f, 90f, 0f);
			oIItem2.child.Add(1, oIItem3);
			Items.Add(7, oIItem2);
			OIChara oIChara2 = new OIChara(2);
			oIChara2.changeAmount.Pos = new Vector3(8f, 5f, 2f);
			Items.Add(2, oIChara2);
			caMap = new ChangeAmount();
		}
	}
}
