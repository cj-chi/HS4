using System.IO;

namespace Studio;

public static class BackgroundListAssist
{
	public static string GetFilePath(string _path, bool _default = false)
	{
		if (Path.GetDirectoryName(_path).IsNullOrEmpty())
		{
			if (File.Exists(UserData.Create(BackgroundList.dirName) + _path))
			{
				return UserData.RootName + "/" + BackgroundList.dirName + "/" + _path;
			}
			if (_default && File.Exists(DefaultData.Create(BackgroundList.dirName) + _path))
			{
				return DefaultData.RootName + "/" + BackgroundList.dirName + "/" + _path;
			}
		}
		return _path;
	}
}
