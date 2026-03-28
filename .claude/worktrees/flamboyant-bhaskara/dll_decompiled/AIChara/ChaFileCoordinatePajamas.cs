using System;
using System.IO;
using Manager;
using MessagePack;
using UnityEngine;

namespace AIChara;

public class ChaFileCoordinatePajamas : ChaFileAssist
{
	public static readonly string BlockName = "CoordinatePajamas";

	public int loadProductNo;

	public Version loadVersion = new Version(ChaFileDefine.ChaFileCoordinateVersion.ToString());

	public int language;

	public ChaFileClothes clothes;

	public ChaFileAccessory accessory;

	public string coordinateName = "";

	public byte[] pngData;

	private int lastLoadErrorCode;

	public string coordinateFileName { get; private set; }

	public int GetLastErrorCode()
	{
		return lastLoadErrorCode;
	}

	public ChaFileCoordinatePajamas()
	{
		MemberInit();
	}

	public void MemberInit()
	{
		clothes = new ChaFileClothes();
		accessory = new ChaFileAccessory();
		coordinateFileName = "";
		coordinateName = "";
		pngData = null;
		lastLoadErrorCode = 0;
	}

	public byte[] SaveBytes()
	{
		byte[] array = MessagePackSerializer.Serialize(clothes);
		byte[] array2 = MessagePackSerializer.Serialize(accessory);
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(array.Length);
		binaryWriter.Write(array);
		binaryWriter.Write(array2.Length);
		binaryWriter.Write(array2);
		return memoryStream.ToArray();
	}

	public bool LoadBytes(byte[] data, Version ver)
	{
		using MemoryStream input = new MemoryStream(data);
		using BinaryReader binaryReader = new BinaryReader(input);
		try
		{
			int count = binaryReader.ReadInt32();
			byte[] bytes = binaryReader.ReadBytes(count);
			clothes = MessagePackSerializer.Deserialize<ChaFileClothes>(bytes);
			count = binaryReader.ReadInt32();
			bytes = binaryReader.ReadBytes(count);
			accessory = MessagePackSerializer.Deserialize<ChaFileAccessory>(bytes);
		}
		catch (EndOfStreamException)
		{
			return false;
		}
		clothes.ComplementWithVersion();
		accessory.ComplementWithVersion();
		return true;
	}

	public void SaveFile(string path, int lang)
	{
		string directoryName = Path.GetDirectoryName(path);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		coordinateFileName = Path.GetFileName(path);
		using FileStream output = new FileStream(path, FileMode.Create, FileAccess.Write);
		using BinaryWriter binaryWriter = new BinaryWriter(output);
		if (pngData != null)
		{
			binaryWriter.Write(pngData);
		}
		binaryWriter.Write(100);
		binaryWriter.Write("【AIS_Clothes】");
		binaryWriter.Write(ChaFileDefine.ChaFileClothesVersion.ToString());
		binaryWriter.Write(lang);
		binaryWriter.Write(coordinateName);
		byte[] array = SaveBytes();
		binaryWriter.Write(array.Length);
		binaryWriter.Write(array);
	}

	public static int GetProductNo(string path)
	{
		if (!File.Exists(path))
		{
			return -1;
		}
		using FileStream input = new FileStream(path, FileMode.Open, FileAccess.Read);
		using BinaryReader binaryReader = new BinaryReader(input);
		try
		{
			PngFile.SkipPng(binaryReader);
			if (binaryReader.BaseStream.Length - binaryReader.BaseStream.Position == 0L)
			{
				return -1;
			}
			return binaryReader.ReadInt32();
		}
		catch (EndOfStreamException)
		{
			return -1;
		}
	}

	public bool LoadFile(TextAsset ta)
	{
		using MemoryStream memoryStream = new MemoryStream();
		memoryStream.Write(ta.bytes, 0, ta.bytes.Length);
		memoryStream.Seek(0L, SeekOrigin.Begin);
		return LoadFile(memoryStream, (int)Singleton<GameSystem>.Instance.language);
	}

	public bool LoadFile(string path)
	{
		if (!File.Exists(path))
		{
			lastLoadErrorCode = -6;
			return false;
		}
		coordinateFileName = Path.GetFileName(path);
		using FileStream st = new FileStream(path, FileMode.Open, FileAccess.Read);
		return LoadFile(st, (int)Singleton<GameSystem>.Instance.language);
	}

	public bool LoadFile(Stream st, int lang)
	{
		using BinaryReader binaryReader = new BinaryReader(st);
		try
		{
			PngFile.SkipPng(binaryReader);
			if (binaryReader.BaseStream.Length - binaryReader.BaseStream.Position == 0L)
			{
				lastLoadErrorCode = -5;
				return false;
			}
			loadProductNo = binaryReader.ReadInt32();
			if (loadProductNo > 100)
			{
				lastLoadErrorCode = -3;
				return false;
			}
			if (binaryReader.ReadString() != "【AIS_Clothes】")
			{
				lastLoadErrorCode = -1;
				return false;
			}
			loadVersion = new Version(binaryReader.ReadString());
			if (loadVersion > ChaFileDefine.ChaFileClothesVersion)
			{
				lastLoadErrorCode = -2;
				return false;
			}
			language = binaryReader.ReadInt32();
			coordinateName = binaryReader.ReadString();
			int count = binaryReader.ReadInt32();
			byte[] data = binaryReader.ReadBytes(count);
			if (LoadBytes(data, loadVersion))
			{
				if (lang != language)
				{
					coordinateName = "";
				}
				lastLoadErrorCode = 0;
				return true;
			}
			lastLoadErrorCode = -999;
			return false;
		}
		catch (EndOfStreamException)
		{
			lastLoadErrorCode = -999;
			return false;
		}
	}

	protected void SaveClothes(string path)
	{
		SaveFileAssist(path, clothes);
	}

	protected void LoadClothes(string path)
	{
		LoadFileAssist<ChaFileClothes>(path, out clothes);
		clothes.ComplementWithVersion();
	}

	protected void SaveAccessory(string path)
	{
		SaveFileAssist(path, accessory);
	}

	protected void LoadAccessory(string path)
	{
		LoadFileAssist<ChaFileAccessory>(path, out accessory);
		accessory.ComplementWithVersion();
	}
}
