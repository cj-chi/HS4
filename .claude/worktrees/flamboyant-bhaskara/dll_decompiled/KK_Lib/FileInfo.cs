namespace KK_Lib;

public class FileInfo
{
	public string manifest = "";

	public string bundlePath = "";

	public string fileName = "";

	public bool check => !bundlePath.IsNullOrEmpty() & !fileName.IsNullOrEmpty();

	public void Clear()
	{
		manifest = "";
		bundlePath = "";
		fileName = "";
	}

	public static bool Compare(FileInfo _a, FileInfo _b)
	{
		if (string.CompareOrdinal(_a.bundlePath, _b.bundlePath) == 0 && string.CompareOrdinal(_a.fileName, _b.fileName) == 0)
		{
			return string.CompareOrdinal(_a.manifest, _b.manifest) == 0;
		}
		return false;
	}
}
