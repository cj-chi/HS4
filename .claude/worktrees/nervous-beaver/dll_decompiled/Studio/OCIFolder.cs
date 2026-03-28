using Manager;
using UnityEngine;

namespace Studio;

public class OCIFolder : ObjectCtrlInfo
{
	public GameObject objectItem;

	public Transform childRoot;

	public OIFolderInfo folderInfo => objectInfo as OIFolderInfo;

	public string name
	{
		get
		{
			return folderInfo.name;
		}
		set
		{
			folderInfo.name = value;
			treeNodeObject.textName = value;
		}
	}

	public override void OnDelete()
	{
		Singleton<GuideObjectManager>.Instance.Delete(guideObject);
		Object.Destroy(objectItem);
		if (parentInfo != null)
		{
			parentInfo.OnDetachChild(this);
		}
		Studio.DeleteInfo(objectInfo);
	}

	public override void OnAttach(TreeNodeObject _parent, ObjectCtrlInfo _child)
	{
		if (_child.parentInfo == null)
		{
			Studio.DeleteInfo(_child.objectInfo, _delKey: false);
		}
		else
		{
			_child.parentInfo.OnDetachChild(_child);
		}
		if (!folderInfo.child.Contains(_child.objectInfo))
		{
			folderInfo.child.Add(_child.objectInfo);
		}
		bool flag = false;
		if (_child is OCIItem)
		{
			flag = (_child as OCIItem).IsParticleArray;
		}
		if (!flag)
		{
			_child.guideObject.transformTarget.SetParent(childRoot);
		}
		_child.guideObject.parent = childRoot;
		_child.guideObject.mode = GuideObject.Mode.World;
		_child.guideObject.moveCalc = GuideMove.MoveCalc.TYPE2;
		if (!flag)
		{
			_child.objectInfo.changeAmount.pos = _child.guideObject.transformTarget.localPosition;
			_child.objectInfo.changeAmount.rot = _child.guideObject.transformTarget.localEulerAngles;
		}
		else if (_child.guideObject.nonconnect)
		{
			_child.objectInfo.changeAmount.pos = _child.guideObject.parent.InverseTransformPoint(_child.guideObject.transformTarget.position);
			Quaternion quaternion = _child.guideObject.transformTarget.rotation * Quaternion.Inverse(_child.guideObject.parent.rotation);
			_child.objectInfo.changeAmount.rot = quaternion.eulerAngles;
		}
		else
		{
			_child.objectInfo.changeAmount.pos = _child.guideObject.parent.InverseTransformPoint(_child.objectInfo.changeAmount.pos);
			Quaternion quaternion2 = Quaternion.Euler(_child.objectInfo.changeAmount.rot) * Quaternion.Inverse(_child.guideObject.parent.rotation);
			_child.objectInfo.changeAmount.rot = quaternion2.eulerAngles;
		}
		_child.guideObject.nonconnect = flag;
		_child.guideObject.calcScale = !flag;
		_child.parentInfo = this;
	}

	public override void OnLoadAttach(TreeNodeObject _parent, ObjectCtrlInfo _child)
	{
		if (_child.parentInfo == null)
		{
			Studio.DeleteInfo(_child.objectInfo, _delKey: false);
		}
		else
		{
			_child.parentInfo.OnDetachChild(_child);
		}
		if (!folderInfo.child.Contains(_child.objectInfo))
		{
			folderInfo.child.Add(_child.objectInfo);
		}
		bool flag = false;
		if (_child is OCIItem)
		{
			flag = (_child as OCIItem).IsParticleArray;
		}
		if (!flag)
		{
			_child.guideObject.transformTarget.SetParent(childRoot, worldPositionStays: false);
		}
		_child.guideObject.parent = childRoot;
		_child.guideObject.nonconnect = flag;
		_child.guideObject.calcScale = !flag;
		_child.guideObject.mode = GuideObject.Mode.World;
		_child.guideObject.moveCalc = GuideMove.MoveCalc.TYPE2;
		_child.objectInfo.changeAmount.OnChange();
		_child.parentInfo = this;
	}

	public override void OnDetach()
	{
		parentInfo.OnDetachChild(this);
		guideObject.parent = null;
		Studio.AddInfo(objectInfo, this);
		objectItem.transform.SetParent(Scene.commonSpace.transform);
		objectInfo.changeAmount.pos = objectItem.transform.localPosition;
		objectInfo.changeAmount.rot = objectItem.transform.localEulerAngles;
		guideObject.mode = GuideObject.Mode.Local;
		guideObject.moveCalc = GuideMove.MoveCalc.TYPE1;
		treeNodeObject.ResetVisible();
	}

	public override void OnSelect(bool _select)
	{
	}

	public override void OnDetachChild(ObjectCtrlInfo _child)
	{
		folderInfo.child.Remove(_child.objectInfo);
		_child.parentInfo = null;
	}

	public override void OnSavePreprocessing()
	{
		base.OnSavePreprocessing();
	}

	public override void OnVisible(bool _visible)
	{
	}
}
