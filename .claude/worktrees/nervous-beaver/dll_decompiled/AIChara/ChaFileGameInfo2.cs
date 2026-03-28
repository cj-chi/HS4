using System;
using System.Collections.Generic;
using ADV;
using Illusion.Extensions;
using MessagePack;
using UnityEngine;

namespace AIChara;

[MessagePackObject(true)]
public class ChaFileGameInfo2 : ICommandData
{
	[IgnoreMember]
	public static readonly string BlockName = "GameInfo2";

	private int favor;

	private int enjoyment;

	private int aversion;

	private int slavery;

	private int broken;

	private int dependence;

	public ChaFileDefine.State nowState;

	public ChaFileDefine.State nowDrawState;

	public bool lockNowState;

	public bool lockBroken;

	public bool lockDependence;

	private int dirty;

	private int tiredness;

	private int toilet;

	private int libido;

	public int alertness;

	public ChaFileDefine.State calcState;

	public byte escapeFlag;

	public bool escapeExperienced;

	public bool firstHFlag;

	public bool[][] genericVoice = new bool[2][];

	public bool genericBrokenVoice;

	public bool genericDependencepVoice;

	public bool genericAnalVoice;

	public bool genericPainVoice;

	public bool genericFlag;

	public bool genericBefore;

	public bool[] inviteVoice = new bool[5];

	public int hCount;

	public HashSet<int> map = new HashSet<int>();

	public bool arriveRoom50;

	public bool arriveRoom80;

	public bool arriveRoomHAfter;

	public int resistH;

	public int resistPain;

	public int resistAnal;

	public int usedItem;

	public bool isChangeParameter;

	public bool isConcierge;

	public Version version { get; set; }

	public int Favor
	{
		get
		{
			return favor;
		}
		set
		{
			favor = Mathf.Clamp(value, 0, 100);
		}
	}

	public int Enjoyment
	{
		get
		{
			return enjoyment;
		}
		set
		{
			enjoyment = Mathf.Clamp(value, 0, 100);
		}
	}

	public int Aversion
	{
		get
		{
			return aversion;
		}
		set
		{
			aversion = Mathf.Clamp(value, 0, 100);
		}
	}

	public int Slavery
	{
		get
		{
			return slavery;
		}
		set
		{
			slavery = Mathf.Clamp(value, 0, 100);
		}
	}

	public int Broken
	{
		get
		{
			return broken;
		}
		set
		{
			broken = Mathf.Clamp(value, 0, 100);
		}
	}

	public int Dependence
	{
		get
		{
			return dependence;
		}
		set
		{
			dependence = Mathf.Clamp(value, 0, 100);
		}
	}

	public int Dirty
	{
		get
		{
			return dirty;
		}
		set
		{
			dirty = Mathf.Clamp(value, 0, 100);
		}
	}

	public int Tiredness
	{
		get
		{
			return tiredness;
		}
		set
		{
			tiredness = Mathf.Clamp(value, 0, 100);
		}
	}

	public int Toilet
	{
		get
		{
			return toilet;
		}
		set
		{
			toilet = Mathf.Clamp(value, 0, 100);
		}
	}

	public int Libido
	{
		get
		{
			return libido;
		}
		set
		{
			libido = Mathf.Clamp(value, 0, 100);
		}
	}

	public ChaFileGameInfo2()
	{
		MemberInit();
	}

	public void MemberInit()
	{
		version = ChaFileDefine.ChaFileGameInfoVersion;
		favor = 0;
		enjoyment = 0;
		aversion = 0;
		slavery = 0;
		broken = 0;
		dependence = 0;
		nowState = ChaFileDefine.State.Blank;
		nowDrawState = ChaFileDefine.State.Blank;
		lockNowState = false;
		lockBroken = false;
		lockDependence = false;
		dirty = 0;
		tiredness = 0;
		toilet = 0;
		libido = 0;
		alertness = 0;
		calcState = ChaFileDefine.State.Blank;
		escapeFlag = 0;
		escapeExperienced = false;
		firstHFlag = false;
		genericVoice = new bool[2][];
		genericVoice[0] = new bool[13];
		genericVoice[1] = new bool[13];
		genericBrokenVoice = false;
		genericDependencepVoice = false;
		genericAnalVoice = false;
		genericPainVoice = false;
		genericFlag = false;
		genericBefore = false;
		inviteVoice = new bool[5];
		hCount = 0;
		map = new HashSet<int>();
		arriveRoom50 = false;
		arriveRoom80 = false;
		arriveRoomHAfter = false;
		resistH = 0;
		resistPain = 0;
		resistAnal = 0;
		usedItem = 0;
		isChangeParameter = false;
		isConcierge = false;
	}

	public void Copy(ChaFileGameInfo2 src)
	{
		version = src.version;
		favor = src.favor;
		enjoyment = src.enjoyment;
		aversion = src.aversion;
		slavery = src.slavery;
		broken = src.broken;
		dependence = src.dependence;
		nowState = src.nowState;
		nowDrawState = src.nowDrawState;
		lockNowState = src.lockNowState;
		lockBroken = src.lockBroken;
		lockDependence = src.lockDependence;
		dirty = src.dirty;
		tiredness = src.tiredness;
		toilet = src.toilet;
		libido = src.libido;
		alertness = src.alertness;
		calcState = src.calcState;
		escapeFlag = src.escapeFlag;
		escapeExperienced = src.escapeExperienced;
		firstHFlag = src.firstHFlag;
		Array.Copy(src.genericVoice, genericVoice, src.genericVoice.Length);
		genericBrokenVoice = src.genericBrokenVoice;
		genericDependencepVoice = src.genericDependencepVoice;
		genericAnalVoice = src.genericAnalVoice;
		genericPainVoice = src.genericPainVoice;
		genericFlag = src.genericFlag;
		genericBefore = src.genericBefore;
		Array.Copy(src.inviteVoice, inviteVoice, src.inviteVoice.Length);
		hCount = src.hCount;
		map = new HashSet<int>(src.map);
		arriveRoom50 = src.arriveRoom50;
		arriveRoom80 = src.arriveRoom80;
		arriveRoomHAfter = src.arriveRoomHAfter;
		resistH = src.resistH;
		resistPain = src.resistPain;
		resistAnal = src.resistAnal;
		usedItem = src.usedItem;
		isChangeParameter = src.isChangeParameter;
		isConcierge = src.isConcierge;
	}

	public void ComplementWithVersion()
	{
		version = ChaFileDefine.ChaFileGameInfoVersion;
	}

	public IEnumerable<CommandData> CreateCommandData(string head)
	{
		List<CommandData> list = new List<CommandData>();
		string key = head + "favor";
		list.Add(new CommandData(CommandData.Command.Int, key, () => favor, delegate(object o)
		{
			favor = Mathf.Clamp((int)o, 0, 100);
		}));
		string key2 = head + "enjoyment";
		list.Add(new CommandData(CommandData.Command.Int, key2, () => enjoyment, delegate(object o)
		{
			enjoyment = Mathf.Clamp((int)o, 0, 100);
		}));
		string key3 = head + "slavery";
		list.Add(new CommandData(CommandData.Command.Int, key3, () => slavery, delegate(object o)
		{
			slavery = Mathf.Clamp((int)o, 0, 100);
		}));
		string key4 = head + "aversion";
		list.Add(new CommandData(CommandData.Command.Int, key4, () => aversion, delegate(object o)
		{
			aversion = Mathf.Clamp((int)o, 0, 100);
		}));
		string key5 = head + "broken";
		list.Add(new CommandData(CommandData.Command.Int, key5, () => broken, delegate(object o)
		{
			broken = Mathf.Clamp((int)o, 0, 100);
		}));
		string key6 = head + "dependence";
		list.Add(new CommandData(CommandData.Command.Int, key6, () => dependence, delegate(object o)
		{
			dependence = Mathf.Clamp((int)o, 0, 100);
		}));
		string key7 = head + "nowState";
		list.Add(new CommandData(CommandData.Command.Int, key7, () => (int)nowState));
		string key8 = head + "nowDrawState";
		list.Add(new CommandData(CommandData.Command.Int, key8, () => (int)nowDrawState));
		string key9 = head + "dirty";
		list.Add(new CommandData(CommandData.Command.Int, key9, () => dirty, delegate(object o)
		{
			dirty = Mathf.Clamp((int)o, 0, 100);
		}));
		string key10 = head + "tiredness";
		list.Add(new CommandData(CommandData.Command.Int, key10, () => tiredness, delegate(object o)
		{
			tiredness = Mathf.Clamp((int)o, 0, 100);
		}));
		string key11 = head + "toilet";
		list.Add(new CommandData(CommandData.Command.Int, key11, () => toilet, delegate(object o)
		{
			toilet = Mathf.Clamp((int)o, 0, 100);
		}));
		string key12 = head + "libido";
		list.Add(new CommandData(CommandData.Command.Int, key12, () => libido, delegate(object o)
		{
			libido = Mathf.Clamp((int)o, 0, 100);
		}));
		string key13 = head + "alertness";
		list.Add(new CommandData(CommandData.Command.Int, key13, () => alertness, delegate(object o)
		{
			alertness = Mathf.Clamp((int)o, 0, 100);
		}));
		string text = head + "genericVoice";
		foreach (var item4 in genericVoice.ToForEachTuples())
		{
			bool[] item = item4.value;
			int item2 = item4.index;
			int key14 = item2;
			foreach (var item5 in item.ToForEachTuples())
			{
				bool array1 = item5.value;
				int item3 = item5.index;
				int key15 = item3;
				list.Add(new CommandData(CommandData.Command.BOOL, text + $"[{key14}]" + $"[{key15}]", () => array1, delegate(object o)
				{
					genericVoice[key14][key15] = (bool)o;
				}));
			}
		}
		string key16 = head + "genericBrokenVoice";
		list.Add(new CommandData(CommandData.Command.BOOL, key16, () => genericBrokenVoice, delegate(object o)
		{
			genericBrokenVoice = (bool)o;
		}));
		string key17 = head + "genericDependencepVoice";
		list.Add(new CommandData(CommandData.Command.BOOL, key17, () => genericDependencepVoice, delegate(object o)
		{
			genericDependencepVoice = (bool)o;
		}));
		string key18 = head + "genericAnalVoice";
		list.Add(new CommandData(CommandData.Command.BOOL, key18, () => genericAnalVoice, delegate(object o)
		{
			genericAnalVoice = (bool)o;
		}));
		string key19 = head + "genericPainVoice";
		list.Add(new CommandData(CommandData.Command.BOOL, key19, () => genericPainVoice, delegate(object o)
		{
			genericPainVoice = (bool)o;
		}));
		string key20 = head + "resistH";
		list.Add(new CommandData(CommandData.Command.Int, key20, () => resistH, delegate(object o)
		{
			resistH = (int)o;
		}));
		string key21 = head + "resistPain";
		list.Add(new CommandData(CommandData.Command.Int, key21, () => resistPain, delegate(object o)
		{
			resistPain = (int)o;
		}));
		string key22 = head + "resistAnal";
		list.Add(new CommandData(CommandData.Command.Int, key22, () => resistAnal, delegate(object o)
		{
			resistAnal = (int)o;
		}));
		string key23 = head + "hCount";
		list.Add(new CommandData(CommandData.Command.Int, key23, () => hCount, delegate(object o)
		{
			hCount = (int)o;
		}));
		string key24 = head + "arriveRoom50";
		list.Add(new CommandData(CommandData.Command.BOOL, key24, () => arriveRoom50, delegate(object o)
		{
			arriveRoom50 = (bool)o;
		}));
		string key25 = head + "arriveRoom80";
		list.Add(new CommandData(CommandData.Command.BOOL, key25, () => arriveRoom80, delegate(object o)
		{
			arriveRoom80 = (bool)o;
		}));
		string key26 = head + "arriveRoomHAfter";
		list.Add(new CommandData(CommandData.Command.BOOL, key26, () => arriveRoomHAfter, delegate(object o)
		{
			arriveRoomHAfter = (bool)o;
		}));
		return list;
	}
}
