using System;
using System.Collections.Generic;
using UniRx;
using UniRx.Async;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ADV;

public static class Setup
{
	private class ADVParames : SceneParameter
	{
		public ADVParames(IData data)
			: base(data)
		{
		}

		public override void Init()
		{
			List<Program.Transfer> transferList = base.data.pack?.Create() ?? Program.Transfer.NewList();
			SceneParameter.advScene.Scenario.openData.Set(base.data.openData);
			SceneParameter.advScene.Scenario.transferList = transferList;
			SceneParameter.advScene.Scenario.SetPackage(base.data.pack);
		}

		public override void Release()
		{
			base.data.pack?.Receive(SceneParameter.advScene.Scenario);
			SceneParameter.advScene.Scenario.SetPackage(null);
			SceneParameter.advScene.Scenario.ChoicesInit();
			SceneParameter.advScene.gameObject.SetActive(value: false);
		}
	}

	public static bool initialized => advSceneHandleDisposable != null;

	private static IDisposable advSceneHandleDisposable { get; set; }

	public static ADVScene _advScene { get; private set; }

	private static ADVData _advData => ParameterList.nowData as ADVData;

	public static void Load(Transform parent)
	{
		LoadAsync(parent).Forget();
	}

	public static async UniTask<GameObject> LoadAsync(Transform parent)
	{
		Dispose();
		AsyncOperationHandle<GameObject> op = Addressables.InstantiateAsync("ADVScene");
		await UniTask.WaitUntil(() => op.IsDone);
		GameObject result = op.Result;
		result.transform.SetParent(parent, worldPositionStays: false);
		_advScene = result.GetComponent<ADVScene>();
		ParameterList.Add(new ADVParames(new ADVData()));
		advSceneHandleDisposable = Disposable.Create(delegate
		{
			_advScene = null;
			ParameterList.Remove(_advData);
			Addressables.ReleaseInstance(op);
		});
		return result;
	}

	public static void Dispose()
	{
		advSceneHandleDisposable?.Dispose();
		advSceneHandleDisposable = null;
	}

	public static void Open(OpenData openData, IPack pack, bool _isCameraPosDontMove = false, bool _isUseCorrectCamera = true, bool _isCharaBackUpPos = true, bool _isCameraPosDontMoveRelase = false)
	{
		ADVData advData = _advData;
		advData.openData = openData;
		advData.pack = pack;
		_advScene.isCameraPosDontMoveFirst = _isCameraPosDontMove;
		_advScene.isCameraPosDontMoveRelease = _isCameraPosDontMoveRelase;
		_advScene.gameObject.SetActive(value: true);
		_advScene.Scenario.commandController.useCorrectCamera = _isUseCorrectCamera;
		_advScene.Scenario.commandController.IsCharaReleaseBackUpPos = _isCharaBackUpPos;
	}
}
