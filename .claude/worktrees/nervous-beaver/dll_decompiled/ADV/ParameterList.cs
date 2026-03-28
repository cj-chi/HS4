using System.Collections.Generic;
using System.Linq;

namespace ADV;

public static class ParameterList
{
	public static SceneParameter nowParameter => list.LastOrDefault();

	public static IData nowData => nowParameter?.data;

	private static List<SceneParameter> list { get; } = new List<SceneParameter>();

	public static void Add(SceneParameter param)
	{
		list.Add(param);
	}

	public static void Remove(IData data)
	{
		list.RemoveAll((SceneParameter p) => p.data == null || p.data == data);
	}

	public static void Init()
	{
		nowParameter?.Init();
	}

	public static void Release()
	{
		nowParameter?.Release();
	}
}
