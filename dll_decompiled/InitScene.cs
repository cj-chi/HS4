using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Manager;
using UnityEngine;

public class InitScene : MonoBehaviour
{
	private XElement dataXml;

	private int width = 1280;

	private int height = 720;

	private int quality = 2;

	private bool full;

	private IEnumerator Start()
	{
		yield return new WaitUntil(() => SingletonInitializer<Scene>.initialized);
		if (SystemInfo.graphicsShaderLevel < 30)
		{
			MonoBehaviour.print("shaders Non support");
		}
		if (File.Exists("UserData/setup.xml"))
		{
			xmlRead();
		}
		Scene.Data data = new Scene.Data
		{
			levelName = "StudioStart"
		};
		Scene.sceneFadeCanvas.SetColor(Color.black);
		Scene.LoadReserve(data, isLoadingImageDraw: true);
		yield return null;
	}

	private void xmlRead()
	{
		try
		{
			dataXml = XElement.Load("UserData/setup.xml");
			if (dataXml == null)
			{
				return;
			}
			foreach (XElement item in dataXml.Elements())
			{
				switch (item.Name.ToString())
				{
				case "Width":
					width = int.Parse(item.Value);
					break;
				case "Height":
					height = int.Parse(item.Value);
					break;
				case "FullScreen":
					full = bool.Parse(item.Value);
					break;
				case "Quality":
					quality = int.Parse(item.Value);
					break;
				}
			}
			Screen.SetResolution(width, height, full);
			switch (quality)
			{
			case 0:
				QualitySettings.SetQualityLevel((!Manager.Config.GraphicData.SelfShadow) ? 1 : 0);
				break;
			case 1:
				QualitySettings.SetQualityLevel(Manager.Config.GraphicData.SelfShadow ? 2 : 3);
				break;
			case 2:
				QualitySettings.SetQualityLevel(Manager.Config.GraphicData.SelfShadow ? 4 : 5);
				break;
			}
		}
		catch (XmlException)
		{
			File.Delete("UserData/setup.xml");
		}
	}
}
