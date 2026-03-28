using Config;
using Illusion.Elements.Xml;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Manager;

public static class Config
{
	public static bool initialized { get; private set; }

	public static SoundSystem SoundData { get; }

	public static CameraSystem CameraData { get; }

	public static GraphicSystem GraphicData { get; }

	public static TextSystem TextData { get; }

	public static HSystem HData { get; }

	public static TitleSystem TitleData { get; }

	public static DebugSystem DebugStatus { get; }

	public static AppendSystem AppendData { get; }

	private static Control xmlCtrl { get; }

	private static void Initialize()
	{
		initialized = true;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void InitializeBeforeSceneLoad()
	{
		Initialize();
	}

	public static void Reset()
	{
		xmlCtrl.Init();
	}

	static Config()
	{
		xmlCtrl = new Control("config", "system.xml", "System", SoundData = new SoundSystem("Sound"), TitleData = new TitleSystem("Title"), CameraData = new CameraSystem("Camera"), GraphicData = new GraphicSystem("Graphic"), TextData = new TextSystem("Text"), HData = new HSystem("H"), AppendData = new AppendSystem("Append"), DebugStatus = new DebugSystem("Debug"));
		xmlCtrl.Read();
		GameObject gameObject = new GameObject("Config_Token");
		Object.DontDestroyOnLoad(gameObject);
		gameObject.OnDestroyAsObservable().Subscribe(delegate
		{
			xmlCtrl.Write();
		});
	}
}
