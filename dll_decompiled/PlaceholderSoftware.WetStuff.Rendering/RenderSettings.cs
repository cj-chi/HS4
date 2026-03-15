using System.IO;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff.Rendering;

public class RenderSettings : ScriptableObject
{
	private const string SettingsFileResourceName = "RenderSettings";

	public static readonly string SettingsFilePath = Path.Combine("Assets/Plugins/PlaceholderSoftware/WetStuff/Resources", "RenderSettings.asset");

	[SerializeField]
	private bool _disableInstancing;

	[SerializeField]
	private bool _disableStencil;

	[SerializeField]
	private bool _disableNormalSmoothing;

	private static RenderSettings _instance;

	public bool EnableInstancing
	{
		get
		{
			if (Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.OSXPlayer)
			{
				return !_disableInstancing;
			}
			return false;
		}
		set
		{
			_disableInstancing = !value;
		}
	}

	public bool EnableStencil
	{
		get
		{
			return !_disableStencil;
		}
		set
		{
			_disableStencil = !value;
		}
	}

	public bool EnableNormalSmoothing
	{
		get
		{
			return !_disableNormalSmoothing;
		}
		set
		{
			_disableNormalSmoothing = !value;
		}
	}

	[NotNull]
	public static RenderSettings Instance => _instance ?? (_instance = Load());

	private static RenderSettings Load()
	{
		return Resources.Load<RenderSettings>("RenderSettings") ?? ScriptableObject.CreateInstance<RenderSettings>();
	}

	public static void Preload()
	{
		if (_instance == null)
		{
			_instance = Load();
		}
	}
}
