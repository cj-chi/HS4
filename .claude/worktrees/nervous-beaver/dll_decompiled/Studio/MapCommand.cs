using UnityEngine;

namespace Studio;

public static class MapCommand
{
	public class EqualsInfo
	{
		public Vector3 oldValue;

		public Vector3 newValue;
	}

	public class MoveEqualsCommand : ICommand
	{
		private EqualsInfo changeAmountInfo;

		public MoveEqualsCommand(EqualsInfo _changeAmountInfo)
		{
			changeAmountInfo = _changeAmountInfo;
		}

		public void Do()
		{
			Singleton<Studio>.Instance.sceneInfo.mapInfo.ca.pos = changeAmountInfo.newValue;
		}

		public void Redo()
		{
			Do();
		}

		public void Undo()
		{
			Singleton<Studio>.Instance.sceneInfo.mapInfo.ca.pos = changeAmountInfo.oldValue;
		}
	}

	public class RotationEqualsCommand : ICommand
	{
		private EqualsInfo changeAmountInfo;

		public RotationEqualsCommand(EqualsInfo _changeAmountInfo)
		{
			changeAmountInfo = _changeAmountInfo;
		}

		public void Do()
		{
			Singleton<Studio>.Instance.sceneInfo.mapInfo.ca.rot = changeAmountInfo.newValue;
		}

		public void Redo()
		{
			Do();
		}

		public void Undo()
		{
			Singleton<Studio>.Instance.sceneInfo.mapInfo.ca.rot = changeAmountInfo.oldValue;
		}
	}
}
