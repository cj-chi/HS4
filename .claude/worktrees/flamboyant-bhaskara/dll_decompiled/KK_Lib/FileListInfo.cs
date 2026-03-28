using System.Collections.Generic;
using System.Linq;

namespace KK_Lib;

public class FileListInfo
{
	public Dictionary<string, string[]> dicFile;

	public FileListInfo(List<string> _list)
	{
		dicFile = _list.ToDictionary((string s) => s, (string s) => AssetBundleCheck.GetAllAssetName(s, _WithExtension: false, null, isAllCheck: true));
	}

	public bool Check(string _path, string _file)
	{
		string[] value = null;
		if (!AssetBundleCheck.IsSimulation)
		{
			_file = _file.ToLower();
		}
		if (!dicFile.TryGetValue(_path, out value))
		{
			return false;
		}
		return value.Contains(_file);
	}
}
