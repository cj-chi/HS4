using System.Collections.Generic;

namespace Illusion.Component.Correct;

public class FrameCorrect : BaseCorrect
{
	public static string[] FrameNames { get; } = new string[50]
	{
		"cf_J_Hips", "cf_J_Spine01", "cf_J_Spine02", "cf_J_Spine03", "cf_J_Neck", "cf_J_Head", "cf_J_Kosi01", "cf_J_Kosi02", "cf_J_Shoulder_L", "cf_J_Shoulder_R",
		"cf_J_Toes01_L", "cf_J_Toes01_R", "cf_J_Hand_Thumb01_L", "cf_J_Hand_Thumb02_L", "cf_J_Hand_Thumb03_L", "cf_J_Hand_Index01_L", "cf_J_Hand_Index02_L", "cf_J_Hand_Index03_L", "cf_J_Hand_Middle01_L", "cf_J_Hand_Middle02_L",
		"cf_J_Hand_Middle03_L", "cf_J_Hand_Ring01_L", "cf_J_Hand_Ring02_L", "cf_J_Hand_Ring03_L", "cf_J_Hand_Little01_L", "cf_J_Hand_Little02_L", "cf_J_Hand_Little03_L", "cf_J_Hand_Thumb01_R", "cf_J_Hand_Thumb02_R", "cf_J_Hand_Thumb03_R",
		"cf_J_Hand_Index01_R", "cf_J_Hand_Index02_R", "cf_J_Hand_Index03_R", "cf_J_Hand_Middle01_R", "cf_J_Hand_Middle02_R", "cf_J_Hand_Middle03_R", "cf_J_Hand_Ring01_R", "cf_J_Hand_Ring02_R", "cf_J_Hand_Ring03_R", "cf_J_Hand_Little01_R",
		"cf_J_Hand_Little02_R", "cf_J_Hand_Little03_R", "cf_J_Mune00_L", "cf_J_Mune01_L", "cf_J_Mune02_L", "cf_J_Mune03_L", "cf_J_Mune00_R", "cf_J_Mune01_R", "cf_J_Mune02_R", "cf_J_Mune03_R"
	};

	public override string[] GetFrameNames => FrameNames;

	private void Start()
	{
		List<Info> list = new List<Info>();
		List<Info> list2 = base.list;
		string[] frameNames = GetFrameNames;
		int i = 0;
		while (i < frameNames.Length)
		{
			Info info = list2.Find((Info x) => x.data.name == frameNames[i]);
			if (info != null)
			{
				if (info.data.bone == null)
				{
					info.data.bone = info.data.transform;
				}
				list.Add(info);
			}
			int num = i + 1;
			i = num;
		}
		list2.Clear();
		list2.AddRange(list);
	}
}
