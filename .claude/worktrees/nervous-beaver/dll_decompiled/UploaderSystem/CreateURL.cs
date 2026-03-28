using System.IO;
using System.Text;
using Illusion.CustomAttributes;
using UnityEngine;

namespace UploaderSystem;

public class CreateURL : MonoBehaviour
{
	[Space]
	public string HS2_Check_URL = "";

	[Button("Create_URL", "HS2_Check_URL作成", new object[] { 0 })]
	public int excuteCreateHS2_Check_URL;

	[Space]
	[Space]
	public string HS2_System_URL = "";

	[Button("Create_URL", "HS2_System_URL作成", new object[] { 1 })]
	public int excuteCreateHS2_System_URL;

	[Space]
	[Space]
	public string HS2_UploadChara_URL = "";

	[Button("Create_URL", "HS2_UploadChara_URL作成", new object[] { 2 })]
	public int excuteCreateHS2_UploadChara_URL;

	[Space]
	[Space]
	public string HS2_Version_URL = "";

	[Button("Create_URL", "HS2_Version_URL作成", new object[] { 3 })]
	public int excuteCreateHS2_Version_URL;

	[Space]
	public string AIS_Check_URL = "";

	[Button("Create_URL", "AIS_Check_URL作成", new object[] { 4 })]
	public int excuteCreateAIS_Check_URL;

	[Space]
	[Space]
	public string AIS_System_URL = "";

	[Button("Create_URL", "AIS_System_URL作成", new object[] { 5 })]
	public int excuteCreateAIS_System_URL;

	[Space]
	[Space]
	public string AIS_UploadChara_URL = "";

	[Button("Create_URL", "AIS_UploadChara_URL作成", new object[] { 6 })]
	public int excuteCreateAIS_UploadChara_URL;

	[Space]
	[Space]
	public string AIS_Version_URL = "";

	[Button("Create_URL", "AIS_Version_URL作成", new object[] { 7 })]
	public int excuteCreateAIS_Version_URL;

	public void Create_URL(int kind)
	{
		string s = "";
		string text = "";
		switch (kind)
		{
		case 0:
			s = HS2_Check_URL;
			text = "hs2_check_url.dat";
			break;
		case 1:
			s = HS2_System_URL;
			text = "hs2_system_url.dat";
			break;
		case 2:
			s = HS2_UploadChara_URL;
			text = "hs2_uploadChara_url.dat";
			break;
		case 3:
			s = HS2_Version_URL;
			text = "hs2_version_url.dat";
			break;
		case 4:
			s = AIS_Check_URL;
			text = "ais_check_url.dat";
			break;
		case 5:
			s = AIS_System_URL;
			text = "ais_system_url.dat";
			break;
		case 6:
			s = AIS_UploadChara_URL;
			text = "ais_uploadChara_url.dat";
			break;
		case 7:
			s = AIS_Version_URL;
			text = "ais_version_url.dat";
			break;
		}
		byte[] buffer = YS_Assist.EncryptAES(Encoding.UTF8.GetBytes(s), "aisyoujyo", "phpaddress");
		string path = Application.dataPath + "/../DefaultData/url/" + text;
		string directoryName = Path.GetDirectoryName(path);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		using FileStream output = new FileStream(path, FileMode.Create, FileAccess.Write);
		using BinaryWriter binaryWriter = new BinaryWriter(output);
		binaryWriter.Write(buffer);
	}

	public static string LoadURL(string urlFile)
	{
		byte[] array = null;
		string path = Application.dataPath + "/../DefaultData/url/" + urlFile;
		if (!File.Exists(path))
		{
			return "";
		}
		using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
		{
			using BinaryReader binaryReader = new BinaryReader(fileStream);
			array = binaryReader.ReadBytes((int)fileStream.Length);
		}
		if (array == null)
		{
			return "";
		}
		byte[] bytes = YS_Assist.DecryptAES(array, "aisyoujyo", "phpaddress");
		return Encoding.UTF8.GetString(bytes);
	}
}
