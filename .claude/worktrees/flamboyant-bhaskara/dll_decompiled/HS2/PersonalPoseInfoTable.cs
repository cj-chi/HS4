using System.Collections.Generic;
using Manager;

namespace HS2;

public static class PersonalPoseInfoTable
{
	public class PoseInfo
	{
		public int id { get; }

		public List<int> poseIDs { get; } = new List<int>();

		public string exp { get; }

		public PoseInfo(PersonalPoseInfo.Param param)
		{
			id = param.id;
			poseIDs = new List<int>(param.poseIDs);
			exp = param.exp;
		}
	}

	public static Dictionary<int, IReadOnlyDictionary<int, PoseInfo>> InfoTable { get; private set; } = new Dictionary<int, IReadOnlyDictionary<int, PoseInfo>>();

	public static void LoadTable(string path)
	{
		List<string> assetBundleNameListFromPath = CommonLib.GetAssetBundleNameListFromPath(path, subdirCheck: true);
		assetBundleNameListFromPath.Sort();
		foreach (string item in assetBundleNameListFromPath)
		{
			if (!GameSystem.IsPathAdd50(item))
			{
				continue;
			}
			PersonalPoseInfo[] allAssets = AssetBundleManager.LoadAllAsset(item, typeof(PersonalPoseInfo)).GetAllAssets<PersonalPoseInfo>();
			foreach (PersonalPoseInfo obj in allAssets)
			{
				Dictionary<int, PoseInfo> dictionary = new Dictionary<int, PoseInfo>();
				int.TryParse(obj.name.Substring(1), out var result);
				foreach (PersonalPoseInfo.Param item2 in obj.param)
				{
					dictionary[item2.id] = new PoseInfo(item2);
				}
				InfoTable[result] = dictionary;
			}
			AssetBundleManager.UnloadAssetBundle(item, isUnloadForceRefCount: false);
		}
	}
}
