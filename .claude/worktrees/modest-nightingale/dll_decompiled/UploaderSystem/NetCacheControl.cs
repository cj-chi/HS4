using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Manager;
using UnityEngine;

namespace UploaderSystem;

public class NetCacheControl : MonoBehaviour
{
	public class CacheHeader
	{
		public int idx;

		public int update_idx;

		public long pos;

		public int size;
	}

	private const int cacheFileMax = 50;

	public bool enableCache = true;

	private Dictionary<string, List<CacheHeader>>[] dictCacheHeaderInfo = new Dictionary<string, List<CacheHeader>>[Enum.GetNames(typeof(DataType)).Length];

	private NetworkInfo netInfo => Singleton<NetworkInfo>.Instance;

	private Dictionary<int, string> GetCacheFileList(DataType type, bool isHS2)
	{
		string[] array = new string[2] { "cache/chara_hs2/", "cache/chara_ai/" };
		string text = UserData.Path + array[(!isHS2) ? 1u : 0u];
		Dictionary<int, string> dictionary = new Dictionary<int, string>();
		string text2 = "";
		for (int i = 0; i < 50; i++)
		{
			text2 = text + i.ToString("00") + ".dat";
			if (File.Exists(text2))
			{
				dictionary[i] = text2;
			}
		}
		if (dictionary.Count >= 50)
		{
			var list = (from x in dictionary
				select new
				{
					k = x.Key,
					v = new FileInfo(x.Value)
				} into x
				orderby x.v.LastAccessTime
				select x).ToList();
			dictionary.Remove(list[0].k);
			File.Delete(list[0].v.FullName);
		}
		return dictionary;
	}

	public void UpdateCacheHeaderInfo(DataType type, bool isHS2)
	{
		if (dictCacheHeaderInfo[(int)type] == null)
		{
			dictCacheHeaderInfo[(int)type] = new Dictionary<string, List<CacheHeader>>();
		}
		else
		{
			dictCacheHeaderInfo[(int)type].Clear();
		}
		foreach (KeyValuePair<int, string> cacheFile in GetCacheFileList(type, isHS2))
		{
			using FileStream input = new FileStream(cacheFile.Value, FileMode.Open, FileAccess.Read);
			using BinaryReader binaryReader = new BinaryReader(input);
			binaryReader.ReadString();
			binaryReader.ReadInt32();
			int num = binaryReader.ReadInt32();
			List<CacheHeader> list = new List<CacheHeader>();
			for (int i = 0; i < num; i++)
			{
				CacheHeader cacheHeader = new CacheHeader();
				cacheHeader.idx = binaryReader.ReadInt32();
				cacheHeader.update_idx = binaryReader.ReadInt32();
				cacheHeader.pos = binaryReader.ReadInt64();
				cacheHeader.size = binaryReader.ReadInt32();
				list.Add(cacheHeader);
			}
			dictCacheHeaderInfo[(int)type][cacheFile.Value] = list;
		}
	}

	public string GetCacheHeader(DataType type, bool isHS2, int idx, out CacheHeader ch)
	{
		ch = null;
		foreach (KeyValuePair<string, List<CacheHeader>> item in dictCacheHeaderInfo[(int)type])
		{
			foreach (CacheHeader item2 in item.Value)
			{
				if (item2.idx == idx)
				{
					ch = new CacheHeader();
					ch.idx = item2.idx;
					ch.update_idx = item2.update_idx;
					ch.pos = item2.pos;
					ch.size = item2.size;
					return item.Key;
				}
			}
		}
		return "";
	}

	public void DeleteCache(DataType type, bool isHS2)
	{
		foreach (KeyValuePair<int, string> cacheFile in GetCacheFileList(type, isHS2))
		{
			if (File.Exists(cacheFile.Value))
			{
				File.Delete(cacheFile.Value);
			}
		}
		UpdateCacheHeaderInfo(type, isHS2);
	}

	public bool CreateCache(DataType type, bool isHS2, Dictionary<int, Tuple<int, byte[]>> dictGet)
	{
		if (dictGet.Count == 0)
		{
			return false;
		}
		string[] array = new string[2] { "cache/chara_hs2/", "cache/chara_ai/" };
		string text = UserData.Path + array[(!isHS2) ? 1u : 0u];
		Dictionary<int, Tuple<int, byte[]>> dictionary = null;
		string text2 = "";
		for (int i = 0; i < 50; i++)
		{
			text2 = text + i.ToString("00") + ".dat";
			if (File.Exists(text2))
			{
				List<CacheHeader> value = null;
				if (!dictCacheHeaderInfo[(int)type].TryGetValue(text2, out value) || value.Count < 1000)
				{
					dictionary = LoadCacheFile(text2);
					break;
				}
				continue;
			}
			dictionary = new Dictionary<int, Tuple<int, byte[]>>();
			break;
		}
		foreach (KeyValuePair<int, Tuple<int, byte[]>> item in dictGet)
		{
			if (item.Value.Item2 != null)
			{
				dictionary[item.Key] = new Tuple<int, byte[]>(item.Value.Item1, item.Value.Item2);
			}
		}
		SaveCacheFile(text2, dictionary);
		return true;
	}

	public void SaveCacheFile(string path, Dictionary<int, Tuple<int, byte[]>> dictPNG)
	{
		string directoryName = Path.GetDirectoryName(path);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		int[] array = dictPNG.Keys.ToArray();
		Dictionary<int, long> dictionary = new Dictionary<int, long>();
		byte[] buffer = null;
		int num = 20;
		long num2 = Encoding.UTF8.GetByteCount("【CacheFile】") + 4 + 4 + num * array.Length + 1;
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			foreach (int key in array)
			{
				dictionary[key] = num2;
				num2 += dictPNG[key].Item2.Length;
				binaryWriter.Write(dictPNG[key].Item2);
			}
			buffer = memoryStream.ToArray();
		}
		using FileStream output = new FileStream(path, FileMode.Create, FileAccess.Write);
		using BinaryWriter binaryWriter2 = new BinaryWriter(output);
		binaryWriter2.Write("【CacheFile】");
		binaryWriter2.Write(100);
		binaryWriter2.Write(array.Length);
		foreach (int num3 in array)
		{
			binaryWriter2.Write(num3);
			binaryWriter2.Write(dictPNG[num3].Item1);
			binaryWriter2.Write(dictionary[num3]);
			binaryWriter2.Write(dictPNG[num3].Item2.Length);
		}
		binaryWriter2.Write(buffer);
	}

	public Dictionary<int, Tuple<int, byte[]>> LoadCacheFile(string path)
	{
		Dictionary<int, Tuple<int, byte[]>> dictionary = new Dictionary<int, Tuple<int, byte[]>>();
		using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
		using BinaryReader binaryReader = new BinaryReader(fileStream);
		binaryReader.ReadString();
		binaryReader.ReadInt32();
		List<CacheHeader> list = new List<CacheHeader>();
		int num = binaryReader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			CacheHeader cacheHeader = new CacheHeader();
			cacheHeader.idx = binaryReader.ReadInt32();
			cacheHeader.update_idx = binaryReader.ReadInt32();
			cacheHeader.pos = binaryReader.ReadInt64();
			cacheHeader.size = binaryReader.ReadInt32();
			list.Add(cacheHeader);
		}
		int count = list.Count;
		for (int j = 0; j < count; j++)
		{
			fileStream.Seek(list[j].pos, SeekOrigin.Begin);
			byte[] item = binaryReader.ReadBytes(list[j].size);
			dictionary[list[j].idx] = new Tuple<int, byte[]>(list[j].update_idx, item);
		}
		return dictionary;
	}

	public byte[] LoadCache(string path, long pos, int size)
	{
		using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
		fileStream.Seek(pos, SeekOrigin.Begin);
		using BinaryReader binaryReader = new BinaryReader(fileStream);
		return binaryReader.ReadBytes(size);
	}

	public void GetCache(DataType type, bool isHS2, Dictionary<int, Tuple<int, byte[]>> dictPNG)
	{
		int[] array = dictPNG.Keys.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			CacheHeader ch = null;
			string cacheHeader = GetCacheHeader(type, isHS2, array[i], out ch);
			if (ch != null)
			{
				dictPNG[ch.idx] = new Tuple<int, byte[]>(ch.update_idx, LoadCache(cacheHeader, ch.pos, ch.size));
			}
		}
	}

	private void Start()
	{
		bool isHS = Singleton<GameSystem>.Instance.networkType == 0;
		UpdateCacheHeaderInfo(DataType.Chara, isHS);
	}

	private void Update()
	{
	}
}
