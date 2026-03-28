using System.Diagnostics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GameSetup;

internal static class Setup
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void InitializeBeforeSceneLoad()
	{
		AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync("Manager");
		asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> op)
		{
			Object.DontDestroyOnLoad(op.Result);
		};
		asyncOperationHandle = Addressables.InstantiateAsync("ExitDialog");
		asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> op)
		{
			Object.DontDestroyOnLoad(op.Result);
		};
	}

	[Conditional("__DEBUG_PROC__")]
	private static void DebugAddComponents(GameObject go)
	{
		go.AddComponent<DebugShow>();
		go.AddComponent<AllocMem>();
		go.AddComponent<QualityDebug>();
		go.AddComponent<DebugExample>();
		go.AddComponent<DebugRenderLog>();
	}
}
