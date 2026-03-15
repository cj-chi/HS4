using System.Collections.Generic;
using System.IO;
using System.Linq;
using AIChara;
using Illusion;
using Illusion.Component.Correct;
using Illusion.Extensions;
using UnityEngine;

public class MotionIKData
{
	public class State
	{
		private Parts[] _partsCached;

		private int? _nameHashCached;

		public Parts this[int index] => parts[index];

		public Parts[] parts
		{
			get
			{
				Parts[] array = _partsCached;
				if (array == null)
				{
					Parts[] obj = new Parts[4] { leftHand, rightHand, leftFoot, rightFoot };
					Parts[] array2 = obj;
					_partsCached = obj;
					array = array2;
				}
				return array;
			}
		}

		public string name { get; set; } = "";

		public int nameHash => (_nameHashCached ?? (_nameHashCached = Animator.StringToHash(name))).Value;

		public Parts leftHand { get; } = new Parts();

		public Parts rightHand { get; } = new Parts();

		public Parts leftFoot { get; } = new Parts();

		public Parts rightFoot { get; } = new Parts();

		public Frame[] frames { get; set; }

		public State()
		{
		}

		public State(State src)
		{
			name = src.name;
			Copy(parts, src.parts);
			frames = Copy(src.frames);
		}

		private static void Copy(Parts[] destArray, Parts[] srcArray)
		{
			int num = srcArray.Length;
			for (int i = 0; i < num; i++)
			{
				Parts obj = destArray[i];
				Parts parts = srcArray[i];
				obj.param2.Copy(parts.param2);
				obj.param3.Copy(parts.param3);
			}
		}

		private static Frame[] Copy(Frame[] srcArray)
		{
			int num = srcArray.Length;
			Frame[] array = new Frame[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = new Frame(srcArray[i]);
			}
			return array;
		}
	}

	public class Parts
	{
		public Param2 param2 { get; } = new Param2();

		public Param3 param3 { get; } = new Param3();
	}

	public class Param2
	{
		public int sex;

		public string target = "";

		public float weightPos;

		public float weightAng;

		public List<BlendWeightInfo>[] blendInfos { get; } = new List<BlendWeightInfo>[2]
		{
			new List<BlendWeightInfo>(),
			new List<BlendWeightInfo>()
		};

		public static int Length { get; } = Utils.Type.GetPublicFields(typeof(Param2)).Length;

		public object this[int index]
		{
			get
			{
				return index switch
				{
					0 => sex, 
					1 => target, 
					2 => weightPos, 
					3 => weightAng, 
					_ => null, 
				};
			}
			set
			{
				switch (index)
				{
				case 0:
					sex = ((value is string) ? int.Parse((string)value) : ((int)value));
					break;
				case 1:
					target = (string)value;
					break;
				case 2:
					weightPos = ((value is string) ? float.Parse((string)value) : ((float)value));
					break;
				case 3:
					weightAng = ((value is string) ? float.Parse((string)value) : ((float)value));
					break;
				}
			}
		}

		public Param2()
		{
		}

		public Param2(Param2 src)
		{
			Copy(src);
		}

		public void Copy(Param2 src)
		{
			sex = src.sex;
			target = src.target;
			weightPos = src.weightPos;
			weightAng = src.weightAng;
			for (int i = 0; i < blendInfos.Length && i < src.blendInfos.Length; i++)
			{
				blendInfos[i].Clear();
				blendInfos[i].AddRange(src.blendInfos[i].Select((BlendWeightInfo item) => new BlendWeightInfo(item)));
			}
		}
	}

	public class Param3
	{
		public string chein = "";

		public float weight;

		public List<BlendWeightInfo> blendInfos { get; } = new List<BlendWeightInfo>();

		public static int Length { get; } = Utils.Type.GetPublicFields(typeof(Param3)).Length;

		public object this[int index]
		{
			get
			{
				return index switch
				{
					0 => chein, 
					1 => weight, 
					_ => null, 
				};
			}
			set
			{
				switch (index)
				{
				case 0:
					chein = (string)value;
					break;
				case 1:
					weight = ((value is string) ? float.Parse((string)value) : ((float)value));
					break;
				}
			}
		}

		public Param3()
		{
		}

		public Param3(Param3 src)
		{
			Copy(src);
		}

		public void Copy(Param3 src)
		{
			chein = src.chein;
			weight = src.weight;
			blendInfos.Clear();
			blendInfos.AddRange(src.blendInfos.Select((BlendWeightInfo item) => new BlendWeightInfo(item)));
		}
	}

	public class BlendWeightInfo
	{
		public int pattern;

		public float StartKey;

		public float EndKey;

		public Shape shape { get; set; }

		public BlendWeightInfo()
		{
		}

		public BlendWeightInfo(BlendWeightInfo src)
		{
			pattern = src.pattern;
			StartKey = src.StartKey;
			EndKey = src.EndKey;
			shape = new Shape(src.shape);
		}
	}

	public class Frame
	{
		public int frameNo { get; set; } = -1;

		public int editNo { get; set; }

		public Shape[] shapes { get; set; }

		public Frame()
		{
		}

		public Frame(Frame src)
		{
			frameNo = src.frameNo;
			editNo = src.editNo;
			int num = src.shapes.Length;
			shapes = new Shape[num];
			for (int i = 0; i < num; i++)
			{
				shapes[i] = new Shape(src.shapes[i]);
			}
		}
	}

	public class Shape
	{
		public PosAng this[int index]
		{
			get
			{
				return index switch
				{
					0 => small, 
					1 => mediam, 
					2 => large, 
					_ => null, 
				};
			}
			set
			{
				if (value != null)
				{
					switch (index)
					{
					case 0:
						small = value;
						break;
					case 1:
						mediam = value;
						break;
					case 2:
						large = value;
						break;
					}
				}
			}
		}

		public int shapeNo { get; set; } = -1;

		public PosAng small { get; set; }

		public PosAng mediam { get; set; }

		public PosAng large { get; set; }

		public Shape()
		{
		}

		public Shape(Shape src)
		{
			Copy(src);
		}

		public void Copy(Shape src)
		{
			shapeNo = src.shapeNo;
			for (int i = 0; i < 3; i++)
			{
				this[i] = new PosAng(src[i]);
			}
		}

		public Shape(PosAng small, PosAng large)
			: this(small, new PosAng(), large)
		{
		}

		public Shape(PosAng small, PosAng mediam, PosAng large)
		{
			this.small = small;
			this.mediam = mediam;
			this.large = large;
		}
	}

	public class PosAng
	{
		public Vector3 pos;

		public Vector3 ang;

		public Vector3 this[int index]
		{
			get
			{
				return index switch
				{
					0 => pos, 
					1 => ang, 
					_ => Vector3.zero, 
				};
			}
			set
			{
				switch (index)
				{
				case 0:
					pos = value;
					break;
				case 1:
					ang = value;
					break;
				}
			}
		}

		public float[] posArray => new float[3] { pos.x, pos.y, pos.z };

		public float[] angArray => new float[3] { ang.x, ang.y, ang.z };

		public PosAng()
		{
		}

		public PosAng(PosAng src)
			: this(src.pos, src.ang)
		{
		}

		public PosAng(Vector3 pos, Vector3 ang)
		{
			this.pos = pos;
			this.ang = ang;
		}

		public PosAng((float[] pos, float[] ang) x)
			: this(x.pos, x.ang)
		{
		}

		public PosAng(float[] pos, float[] ang)
		{
			for (int i = 0; i < 3; i++)
			{
				this.pos[i] = pos[i];
				this.ang[i] = ang[i];
			}
		}
	}

	public bool isBlend { get; set; }

	public State[] states => _states;

	private State[] _states { get; set; }

	public MotionIKData()
	{
	}

	public MotionIKData(TextAsset ta)
	{
		Read(ta);
	}

	public MotionIKData(string path)
	{
		Read(path);
	}

	public MotionIKData(State[] state)
	{
		_states = Copy(state);
	}

	public MotionIKData Copy()
	{
		return Copy(this);
	}

	public static MotionIKData Copy(MotionIKData src)
	{
		return new MotionIKData(src._states)
		{
			isBlend = src.isBlend
		};
	}

	public static State[] Copy(State[] srcArray)
	{
		int num = srcArray.Length;
		State[] array = new State[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = new State(srcArray[i]);
		}
		return array;
	}

	public State InitState(string stateName)
	{
		int num = _states?.Check((State p) => p.name == stateName) ?? (-1);
		if (num == -1)
		{
			State[] array = new State[1]
			{
				new State
				{
					name = stateName
				}
			};
			_states = ((_states == null) ? array : _states.Concat(array).ToArray());
			num = _states.Length - 1;
		}
		State obj = _states[num];
		InitFrame(obj);
		return obj;
	}

	public void Release()
	{
		_states = null;
	}

	public static void InitFrame(State state)
	{
		int ikLen = MotionIK.IKTargetPair.IKTargetLength;
		IEnumerable<Frame> first = from i in Enumerable.Range(0, ikLen)
			select new Frame
			{
				frameNo = i
			};
		IEnumerable<Frame> second = from i in Enumerable.Range(0, FrameCorrect.FrameNames.Length)
			select new Frame
			{
				frameNo = i + ikLen
			};
		IEnumerable<Frame> source = first.Concat(second);
		state.frames = source.ToArray();
		for (int num = 0; num < state.frames.Length; num++)
		{
			InitShape(state.frames[num]);
		}
	}

	public static void InitShape(Frame frame)
	{
		frame.shapes = (from i in Enumerable.Range(0, ChaFileDefine.cf_bodyshapename.Length)
			select new Shape
			{
				shapeNo = i
			}).ToArray();
		for (int num = 0; num < frame.shapes.Length; num++)
		{
			Shape obj = frame.shapes[num];
			obj.small = new PosAng();
			obj.mediam = new PosAng();
			obj.large = new PosAng();
		}
	}

	public bool Read(TextAsset ta)
	{
		bool flag = ta != null;
		if (flag)
		{
			using MemoryStream stream = new MemoryStream(ta.bytes);
			Read(stream);
		}
		return flag;
	}

	public bool Read(string path)
	{
		return Utils.File.OpenRead(path, Read);
	}

	public void SetStates(State[] states)
	{
		_states = states;
	}

	private void Read(Stream stream)
	{
		using BinaryReader binaryReader = new BinaryReader(stream);
		isBlend = binaryReader.ReadBoolean();
		if (!isBlend)
		{
			ReadNormal(binaryReader);
		}
		else
		{
			ReadBlend(binaryReader);
		}
	}

	private void ReadNormal(BinaryReader r)
	{
		int num = r.ReadInt32();
		_states = new State[num];
		for (int i = 0; i < num; i++)
		{
			State state = new State();
			state.name = r.ReadString();
			Parts[] parts = state.parts;
			foreach (Parts obj in parts)
			{
				obj.param2.sex = r.ReadInt32();
				obj.param2.target = r.ReadString();
				obj.param2.weightPos = r.ReadSingle();
				obj.param2.weightAng = r.ReadSingle();
				obj.param3.chein = r.ReadString();
				obj.param3.weight = r.ReadSingle();
			}
			int num2 = r.ReadInt32();
			state.frames = new Frame[num2];
			for (int k = 0; k < num2; k++)
			{
				Frame frame = new Frame();
				frame.frameNo = r.ReadInt32();
				frame.editNo = r.ReadInt32();
				int num3 = r.ReadInt32();
				frame.shapes = new Shape[num3];
				for (int l = 0; l < num3; l++)
				{
					Shape shape = new Shape();
					shape.shapeNo = r.ReadInt32();
					for (int m = 0; m < 3; m++)
					{
						shape[m] = new PosAng();
						for (int n = 0; n < 3; n++)
						{
							shape[m].pos[n] = r.ReadSingle();
						}
						for (int num4 = 0; num4 < 3; num4++)
						{
							shape[m].ang[num4] = r.ReadSingle();
						}
					}
					frame.shapes[l] = shape;
				}
				state.frames[k] = frame;
			}
			_states[i] = state;
		}
	}

	private void ReadBlend(BinaryReader r)
	{
		int num = r.ReadInt32();
		_states = new State[num];
		for (int i = 0; i < num; i++)
		{
			State state = new State();
			state.name = r.ReadString();
			Parts[] parts = state.parts;
			foreach (Parts parts2 in parts)
			{
				parts2.param2.sex = r.ReadInt32();
				parts2.param2.target = r.ReadString();
				parts2.param2.weightPos = r.ReadSingle();
				parts2.param2.weightAng = r.ReadSingle();
				for (int k = 0; k < 2; k++)
				{
					ReadBlend(parts2.param2.blendInfos[k]);
				}
				parts2.param3.chein = r.ReadString();
				parts2.param3.weight = r.ReadSingle();
				ReadBlend(parts2.param3.blendInfos);
			}
			int num2 = r.ReadInt32();
			state.frames = new Frame[num2];
			for (int l = 0; l < num2; l++)
			{
				Frame frame = new Frame();
				frame.frameNo = r.ReadInt32();
				frame.editNo = r.ReadInt32();
				int num3 = r.ReadInt32();
				frame.shapes = new Shape[num3];
				for (int m = 0; m < num3; m++)
				{
					Shape shape = new Shape();
					ReadShape(shape);
					frame.shapes[m] = shape;
				}
				state.frames[l] = frame;
			}
			_states[i] = state;
		}
		void ReadBlend(List<BlendWeightInfo> blendInfos)
		{
			blendInfos.Clear();
			int num4 = r.ReadInt32();
			for (int n = 0; n < num4; n++)
			{
				BlendWeightInfo blendWeightInfo = new BlendWeightInfo
				{
					pattern = r.ReadInt32(),
					StartKey = r.ReadSingle(),
					EndKey = r.ReadSingle(),
					shape = new Shape()
				};
				ReadShape(blendWeightInfo.shape);
				blendInfos.Add(blendWeightInfo);
			}
		}
		void ReadShape(Shape shape2)
		{
			shape2.shapeNo = r.ReadInt32();
			for (int n = 0; n < 3; n++)
			{
				shape2[n] = new PosAng();
				for (int num4 = 0; num4 < 3; num4++)
				{
					shape2[n].pos[num4] = r.ReadSingle();
				}
				for (int num5 = 0; num5 < 3; num5++)
				{
					shape2[n].ang[num5] = r.ReadSingle();
				}
			}
		}
	}
}
