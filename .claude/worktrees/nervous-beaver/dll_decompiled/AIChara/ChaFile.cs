using System;
using System.IO;
using MessagePack;

namespace AIChara;

public class ChaFile
{
	public class ProductInfo
	{
		public int productNo = -1;

		public string tag = "";

		public Version version = new Version(0, 0, 0);

		public int language;

		public string userID = "";

		public string dataID = "";
	}

	public int loadProductNo;

	public Version loadVersion = new Version(ChaFileDefine.ChaFileVersion.ToString());

	public int language;

	public byte[] pngData;

	public string userID = "";

	public string dataID = "";

	public ChaFileCustom custom;

	public ChaFileCoordinate coordinate;

	public ChaFileParameter parameter;

	public ChaFileGameInfo gameinfo;

	public ChaFileParameter2 parameter2;

	public ChaFileGameInfo2 gameinfo2;

	public ChaFileStatus status;

	public ChaFileCoordinateBath coordinateBath;

	public ChaFileCoordinatePajamas coordinatePajamas;

	private int lastLoadErrorCode;

	public string charaFileName { get; protected set; }

	public int GetLastErrorCode()
	{
		return lastLoadErrorCode;
	}

	public ChaFile()
	{
		custom = new ChaFileCustom();
		coordinate = new ChaFileCoordinate();
		parameter = new ChaFileParameter();
		gameinfo = new ChaFileGameInfo();
		parameter2 = new ChaFileParameter2();
		gameinfo2 = new ChaFileGameInfo2();
		status = new ChaFileStatus();
		coordinateBath = new ChaFileCoordinateBath();
		coordinatePajamas = new ChaFileCoordinatePajamas();
		lastLoadErrorCode = 0;
	}

	protected bool SaveFile(string path, int lang)
	{
		string directoryName = Path.GetDirectoryName(path);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		charaFileName = Path.GetFileName(path);
		using FileStream st = new FileStream(path, FileMode.Create, FileAccess.Write);
		return SaveFile(st, savePng: true, lang);
	}

	protected bool SaveFile(Stream st, bool savePng, int lang)
	{
		using BinaryWriter bw = new BinaryWriter(st);
		return SaveFile(bw, savePng, lang);
	}

	protected bool SaveFile(BinaryWriter bw, bool savePng, int lang)
	{
		if (savePng && pngData != null)
		{
			bw.Write(pngData);
		}
		bw.Write(100);
		bw.Write("【AIS_Chara】");
		bw.Write(ChaFileDefine.ChaFileVersion.ToString());
		bw.Write(lang);
		bw.Write(userID);
		bw.Write(dataID);
		byte[] customBytes = GetCustomBytes();
		byte[] coordinateBytes = GetCoordinateBytes();
		byte[] parameterBytes = GetParameterBytes();
		byte[] gameInfoBytes = GetGameInfoBytes();
		byte[] statusBytes = GetStatusBytes();
		byte[] parameter2Bytes = GetParameter2Bytes();
		byte[] gameInfo2Bytes = GetGameInfo2Bytes();
		int num = 7;
		long num2 = 0L;
		string[] array = new string[7]
		{
			ChaFileCustom.BlockName,
			ChaFileCoordinate.BlockName,
			ChaFileParameter.BlockName,
			ChaFileGameInfo.BlockName,
			ChaFileStatus.BlockName,
			ChaFileParameter2.BlockName,
			ChaFileGameInfo2.BlockName
		};
		string[] array2 = new string[7]
		{
			ChaFileDefine.ChaFileCustomVersion.ToString(),
			ChaFileDefine.ChaFileCoordinateVersion.ToString(),
			ChaFileDefine.ChaFileParameterVersion.ToString(),
			ChaFileDefine.ChaFileGameInfoVersion.ToString(),
			ChaFileDefine.ChaFileStatusVersion.ToString(),
			ChaFileDefine.ChaFileParameterVersion2.ToString(),
			ChaFileDefine.ChaFileGameInfoVersion2.ToString()
		};
		long[] array3 = new long[num];
		array3[0] = ((customBytes != null) ? customBytes.Length : 0);
		array3[1] = ((coordinateBytes != null) ? coordinateBytes.Length : 0);
		array3[2] = ((parameterBytes != null) ? parameterBytes.Length : 0);
		array3[3] = ((gameInfoBytes != null) ? gameInfoBytes.Length : 0);
		array3[4] = ((statusBytes != null) ? statusBytes.Length : 0);
		array3[5] = ((parameter2Bytes != null) ? parameter2Bytes.Length : 0);
		array3[6] = ((gameInfo2Bytes != null) ? gameInfo2Bytes.Length : 0);
		long[] array4 = new long[7]
		{
			num2,
			num2 + array3[0],
			num2 + array3[0] + array3[1],
			num2 + array3[0] + array3[1] + array3[2],
			num2 + array3[0] + array3[1] + array3[2] + array3[3],
			num2 + array3[0] + array3[1] + array3[2] + array3[3] + array3[4],
			num2 + array3[0] + array3[1] + array3[2] + array3[3] + array3[4] + array3[5]
		};
		BlockHeader blockHeader = new BlockHeader();
		for (int i = 0; i < num; i++)
		{
			BlockHeader.Info item = new BlockHeader.Info
			{
				name = array[i],
				version = array2[i],
				size = array3[i],
				pos = array4[i]
			};
			blockHeader.lstInfo.Add(item);
		}
		byte[] array5 = MessagePackSerializer.Serialize(blockHeader);
		bw.Write(array5.Length);
		bw.Write(array5);
		long num3 = 0L;
		long[] array6 = array3;
		foreach (long num4 in array6)
		{
			num3 += num4;
		}
		bw.Write(num3);
		bw.Write(customBytes);
		bw.Write(coordinateBytes);
		bw.Write(parameterBytes);
		bw.Write(gameInfoBytes);
		bw.Write(statusBytes);
		bw.Write(parameter2Bytes);
		bw.Write(gameInfo2Bytes);
		return true;
	}

	public static bool GetProductInfo(string path, out ProductInfo info)
	{
		info = new ProductInfo();
		if (!File.Exists(path))
		{
			return false;
		}
		using FileStream input = new FileStream(path, FileMode.Open, FileAccess.Read);
		using BinaryReader binaryReader = new BinaryReader(input);
		long pngSize = PngFile.GetPngSize(binaryReader);
		if (pngSize != 0L)
		{
			binaryReader.BaseStream.Seek(pngSize, SeekOrigin.Current);
			if (binaryReader.BaseStream.Length - binaryReader.BaseStream.Position == 0L)
			{
				return false;
			}
		}
		try
		{
			info.productNo = binaryReader.ReadInt32();
			info.tag = binaryReader.ReadString();
			if (info.tag != "【AIS_Chara】")
			{
				return false;
			}
			info.version = new Version(binaryReader.ReadString());
			if (info.version > ChaFileDefine.ChaFileVersion)
			{
				return false;
			}
			info.language = binaryReader.ReadInt32();
			info.userID = binaryReader.ReadString();
			info.dataID = binaryReader.ReadString();
			return true;
		}
		catch (EndOfStreamException)
		{
			return false;
		}
	}

	protected bool LoadFile(string path, int lang, bool noLoadPNG = false, bool noLoadStatus = true)
	{
		if (!File.Exists(path))
		{
			lastLoadErrorCode = -6;
			return false;
		}
		charaFileName = Path.GetFileName(path);
		using FileStream st = new FileStream(path, FileMode.Open, FileAccess.Read);
		return LoadFile(st, lang, noLoadPNG, noLoadStatus);
	}

	protected bool LoadFile(Stream st, int lang, bool noLoadPNG = false, bool noLoadStatus = true)
	{
		using BinaryReader br = new BinaryReader(st);
		return LoadFile(br, lang, noLoadPNG, noLoadStatus);
	}

	protected bool LoadFile(BinaryReader br, int lang, bool noLoadPNG = false, bool noLoadStatus = true)
	{
		long pngSize = PngFile.GetPngSize(br);
		if (pngSize != 0L)
		{
			if (noLoadPNG)
			{
				br.BaseStream.Seek(pngSize, SeekOrigin.Current);
			}
			else
			{
				pngData = br.ReadBytes((int)pngSize);
			}
			if (br.BaseStream.Length - br.BaseStream.Position == 0L)
			{
				lastLoadErrorCode = -5;
				return false;
			}
		}
		try
		{
			loadProductNo = br.ReadInt32();
			if (loadProductNo > 100)
			{
				lastLoadErrorCode = -3;
				return false;
			}
			if (br.ReadString() != "【AIS_Chara】")
			{
				lastLoadErrorCode = -1;
				return false;
			}
			loadVersion = new Version(br.ReadString());
			if (loadVersion > ChaFileDefine.ChaFileVersion)
			{
				lastLoadErrorCode = -2;
				return false;
			}
			language = br.ReadInt32();
			userID = br.ReadString();
			dataID = br.ReadString();
			int count = br.ReadInt32();
			BlockHeader blockHeader = MessagePackSerializer.Deserialize<BlockHeader>(br.ReadBytes(count));
			long num = br.ReadInt64();
			long position = br.BaseStream.Position;
			BlockHeader.Info info = blockHeader.SearchInfo(ChaFileCustom.BlockName);
			if (info != null)
			{
				Version version = new Version(info.version);
				if (version > ChaFileDefine.ChaFileCustomVersion)
				{
					lastLoadErrorCode = -2;
				}
				else
				{
					br.BaseStream.Seek(position + info.pos, SeekOrigin.Begin);
					byte[] data = br.ReadBytes((int)info.size);
					SetCustomBytes(data, version);
				}
			}
			info = blockHeader.SearchInfo(ChaFileCoordinate.BlockName);
			if (info != null)
			{
				Version version2 = new Version(info.version);
				if (version2 > ChaFileDefine.ChaFileCoordinateVersion)
				{
					lastLoadErrorCode = -2;
				}
				else
				{
					br.BaseStream.Seek(position + info.pos, SeekOrigin.Begin);
					byte[] data2 = br.ReadBytes((int)info.size);
					SetCoordinateBytes(data2, version2);
				}
			}
			info = blockHeader.SearchInfo(ChaFileParameter.BlockName);
			if (info != null)
			{
				if (new Version(info.version) > ChaFileDefine.ChaFileParameterVersion)
				{
					lastLoadErrorCode = -2;
				}
				else
				{
					br.BaseStream.Seek(position + info.pos, SeekOrigin.Begin);
					byte[] parameterBytes = br.ReadBytes((int)info.size);
					SetParameterBytes(parameterBytes);
				}
			}
			info = blockHeader.SearchInfo(ChaFileGameInfo.BlockName);
			if (info != null)
			{
				if (new Version(info.version) > ChaFileDefine.ChaFileGameInfoVersion)
				{
					lastLoadErrorCode = -2;
				}
				else
				{
					br.BaseStream.Seek(position + info.pos, SeekOrigin.Begin);
					byte[] gameInfoBytes = br.ReadBytes((int)info.size);
					SetGameInfoBytes(gameInfoBytes);
				}
			}
			if (!noLoadStatus)
			{
				info = blockHeader.SearchInfo(ChaFileStatus.BlockName);
				if (info != null)
				{
					if (new Version(info.version) > ChaFileDefine.ChaFileStatusVersion)
					{
						lastLoadErrorCode = -2;
					}
					else
					{
						br.BaseStream.Seek(position + info.pos, SeekOrigin.Begin);
						byte[] statusBytes = br.ReadBytes((int)info.size);
						SetStatusBytes(statusBytes);
					}
				}
			}
			info = blockHeader.SearchInfo(ChaFileParameter2.BlockName);
			if (info != null)
			{
				if (new Version(info.version) > ChaFileDefine.ChaFileParameterVersion2)
				{
					lastLoadErrorCode = -2;
				}
				else
				{
					br.BaseStream.Seek(position + info.pos, SeekOrigin.Begin);
					byte[] parameter2Bytes = br.ReadBytes((int)info.size);
					SetParameter2Bytes(parameter2Bytes);
				}
			}
			info = blockHeader.SearchInfo(ChaFileGameInfo2.BlockName);
			if (info != null)
			{
				if (new Version(info.version) > ChaFileDefine.ChaFileGameInfoVersion2)
				{
					lastLoadErrorCode = -2;
				}
				else
				{
					br.BaseStream.Seek(position + info.pos, SeekOrigin.Begin);
					byte[] gameInfo2Bytes = br.ReadBytes((int)info.size);
					SetGameInfo2Bytes(gameInfo2Bytes);
				}
			}
			br.BaseStream.Seek(position + num, SeekOrigin.Begin);
		}
		catch (EndOfStreamException)
		{
			lastLoadErrorCode = -999;
			return false;
		}
		lastLoadErrorCode = 0;
		return true;
	}

	public byte[] GetCustomBytes()
	{
		return GetCustomBytes(custom);
	}

	public static byte[] GetCustomBytes(ChaFileCustom _custom)
	{
		return _custom.SaveBytes();
	}

	public byte[] GetCoordinateBytes()
	{
		return GetCoordinateBytes(coordinate);
	}

	public static byte[] GetCoordinateBytes(ChaFileCoordinate _coordinate)
	{
		return _coordinate.SaveBytes();
	}

	public byte[] GetParameterBytes()
	{
		return GetParameterBytes(parameter);
	}

	public static byte[] GetParameterBytes(ChaFileParameter _parameter)
	{
		return MessagePackSerializer.Serialize(_parameter);
	}

	public byte[] GetParameter2Bytes()
	{
		return GetParameter2Bytes(parameter2);
	}

	public static byte[] GetParameter2Bytes(ChaFileParameter2 _parameter)
	{
		return MessagePackSerializer.Serialize(_parameter);
	}

	public byte[] GetGameInfoBytes()
	{
		return GetGameInfoBytes(gameinfo);
	}

	public static byte[] GetGameInfoBytes(ChaFileGameInfo _gameinfo)
	{
		return MessagePackSerializer.Serialize(_gameinfo);
	}

	public byte[] GetGameInfo2Bytes()
	{
		return GetGameInfo2Bytes(gameinfo2);
	}

	public static byte[] GetGameInfo2Bytes(ChaFileGameInfo2 _gameinfo)
	{
		return MessagePackSerializer.Serialize(_gameinfo);
	}

	public byte[] GetStatusBytes()
	{
		return GetStatusBytes(status);
	}

	public static byte[] GetStatusBytes(ChaFileStatus _status)
	{
		return MessagePackSerializer.Serialize(_status);
	}

	public void SetCustomBytes(byte[] data, Version ver)
	{
		custom.LoadBytes(data, ver);
	}

	public void SetCoordinateBytes(byte[] data, Version ver)
	{
		coordinate.LoadBytes(data, ver);
	}

	public void SetParameterBytes(byte[] data)
	{
		ChaFileParameter chaFileParameter = MessagePackSerializer.Deserialize<ChaFileParameter>(data);
		chaFileParameter.ComplementWithVersion();
		parameter.Copy(chaFileParameter);
	}

	public void SetParameter2Bytes(byte[] data)
	{
		ChaFileParameter2 chaFileParameter = MessagePackSerializer.Deserialize<ChaFileParameter2>(data);
		chaFileParameter.ComplementWithVersion();
		parameter2.Copy(chaFileParameter);
	}

	public void SetGameInfoBytes(byte[] data)
	{
		ChaFileGameInfo chaFileGameInfo = MessagePackSerializer.Deserialize<ChaFileGameInfo>(data);
		chaFileGameInfo.ComplementWithVersion();
		gameinfo.Copy(chaFileGameInfo);
	}

	public void SetGameInfo2Bytes(byte[] data)
	{
		ChaFileGameInfo2 chaFileGameInfo = MessagePackSerializer.Deserialize<ChaFileGameInfo2>(data);
		chaFileGameInfo.ComplementWithVersion();
		gameinfo2.Copy(chaFileGameInfo);
	}

	public void SetStatusBytes(byte[] data)
	{
		ChaFileStatus chaFileStatus = MessagePackSerializer.Deserialize<ChaFileStatus>(data);
		chaFileStatus.ComplementWithVersion();
		status.Copy(chaFileStatus);
	}

	public static void CopyChaFile(ChaFile dst, ChaFile src, bool _custom = true, bool _coordinate = true, bool _parameter = true, bool _gameinfo = true, bool _status = true)
	{
		dst.CopyAll(src, _custom, _coordinate, _parameter, _gameinfo, _status);
	}

	public void CopyAll(ChaFile _chafile, bool _custom = true, bool _coordinate = true, bool _parameter = true, bool _gameinfo = true, bool _status = true)
	{
		if (_custom)
		{
			CopyCustom(_chafile.custom);
		}
		if (_coordinate)
		{
			CopyCoordinate(_chafile.coordinate);
		}
		if (_status)
		{
			CopyStatus(_chafile.status);
		}
		if (_parameter)
		{
			CopyParameter(_chafile.parameter);
			CopyParameter2(_chafile.parameter2);
		}
		if (_gameinfo)
		{
			CopyGameInfo(_chafile.gameinfo);
			CopyGameInfo2(_chafile.gameinfo2);
		}
	}

	public void CopyCustom(ChaFileCustom _custom)
	{
		byte[] customBytes = GetCustomBytes(_custom);
		SetCustomBytes(customBytes, ChaFileDefine.ChaFileCustomVersion);
	}

	public void CopyCoordinate(ChaFileCoordinate _coordinate)
	{
		byte[] coordinateBytes = GetCoordinateBytes(_coordinate);
		SetCoordinateBytes(coordinateBytes, ChaFileDefine.ChaFileCoordinateVersion);
	}

	public void CopyParameter(ChaFileParameter _parameter)
	{
		byte[] parameterBytes = GetParameterBytes(_parameter);
		SetParameterBytes(parameterBytes);
	}

	public void CopyParameter2(ChaFileParameter2 _parameter)
	{
		byte[] parameter2Bytes = GetParameter2Bytes(_parameter);
		SetParameter2Bytes(parameter2Bytes);
	}

	public void CopyGameInfo(ChaFileGameInfo _gameinfo)
	{
		byte[] gameInfoBytes = GetGameInfoBytes(_gameinfo);
		SetGameInfoBytes(gameInfoBytes);
	}

	public void CopyGameInfo2(ChaFileGameInfo2 _gameinfo)
	{
		byte[] gameInfo2Bytes = GetGameInfo2Bytes(_gameinfo);
		SetGameInfo2Bytes(gameInfo2Bytes);
	}

	public void CopyStatus(ChaFileStatus _status)
	{
		byte[] statusBytes = GetStatusBytes(_status);
		SetStatusBytes(statusBytes);
	}
}
