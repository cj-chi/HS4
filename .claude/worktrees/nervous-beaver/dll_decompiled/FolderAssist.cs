using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class FolderAssist
{
	public class FileInfo
	{
		private string fullPath = "";

		private string fileName = "";

		public string FullPath => fullPath;

		public string FileName => fileName;

		public DateTime time { get; private set; }

		public FileInfo(string fullPath, string fileName, DateTime? time = null)
		{
			this.fullPath = fullPath;
			this.fileName = fileName;
			if (time.HasValue)
			{
				this.time = time.Value;
			}
		}

		public FileInfo(FileInfo src)
		{
			Copy(src);
		}

		public void Copy(FileInfo src)
		{
			fullPath = src.FullPath;
			fileName = src.FileName;
			time = src.time;
		}

		public void UpdateTime(FileInfo info)
		{
			UpdateTime(info.time);
		}

		public void UpdateTime(DateTime time)
		{
			this.time = time;
		}
	}

	public List<FileInfo> lstFile = new List<FileInfo>();

	public static FileInfo[] CreateFolderInfoToArray(string folder, string searchPattern, bool getFiles = true)
	{
		if (!Directory.Exists(folder))
		{
			return null;
		}
		string[] source = (getFiles ? Directory.GetFiles(folder, searchPattern) : Directory.GetDirectories(folder));
		if (!source.Any())
		{
			return null;
		}
		return source.Select((string path) => new FileInfo(path, (!getFiles) ? string.Empty : Path.GetFileNameWithoutExtension(path), File.GetLastWriteTime(path))).ToArray();
	}

	public static FileInfo[] CreateFolderInfoExToArray(string folder, params string[] searchPattern)
	{
		if (!Directory.Exists(folder))
		{
			return null;
		}
		string[] source = searchPattern.SelectMany((string pattern) => Directory.GetFiles(folder, pattern)).ToArray();
		if (!source.Any())
		{
			return null;
		}
		return source.Select((string path) => new FileInfo(path, Path.GetFileNameWithoutExtension(path), File.GetLastWriteTime(path))).ToArray();
	}

	public int GetFileCount()
	{
		return lstFile.Count;
	}

	public bool CreateFolderInfo(string folder, string searchPattern, bool getFiles = true, bool clear = true)
	{
		if (clear)
		{
			lstFile.Clear();
		}
		FileInfo[] array = CreateFolderInfoToArray(folder, searchPattern, getFiles);
		bool num = array != null;
		if (num)
		{
			lstFile.AddRange(array);
		}
		return num;
	}

	public bool CreateFolderInfoEx(string folder, string[] searchPattern, bool clear = true)
	{
		if (clear)
		{
			lstFile.Clear();
		}
		FileInfo[] array = CreateFolderInfoExToArray(folder, searchPattern);
		bool num = array != null;
		if (num)
		{
			lstFile.AddRange(array);
		}
		return num;
	}

	public int GetIndexFromFileName(string filename)
	{
		int num = 0;
		foreach (FileInfo item in lstFile)
		{
			if (item.FileName == filename)
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public int GetIndexFromFullPath(string fullpath)
	{
		int num = 0;
		foreach (FileInfo item in lstFile)
		{
			if (item.FullPath == fullpath)
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public void SortFullPath(bool ascend = true)
	{
		if (lstFile.Count == 0)
		{
			return;
		}
		if (ascend)
		{
			lstFile.Sort((FileInfo a, FileInfo b) => a.FullPath.CompareTo(b.FullPath));
		}
		else
		{
			lstFile.Sort((FileInfo a, FileInfo b) => b.FullPath.CompareTo(a.FullPath));
		}
	}

	public void SortName(bool ascend = true)
	{
		if (lstFile.Count == 0)
		{
			return;
		}
		if (ascend)
		{
			lstFile.Sort((FileInfo a, FileInfo b) => a.FileName.CompareTo(b.FileName));
		}
		else
		{
			lstFile.Sort((FileInfo a, FileInfo b) => b.FileName.CompareTo(a.FileName));
		}
	}

	public void SortDate(bool ascend = true)
	{
		if (lstFile.Count == 0)
		{
			return;
		}
		if (ascend)
		{
			lstFile.Sort((FileInfo a, FileInfo b) => a.time.CompareTo(b.time));
		}
		else
		{
			lstFile.Sort((FileInfo a, FileInfo b) => b.time.CompareTo(a.time));
		}
	}
}
