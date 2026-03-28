namespace Studio;

public abstract class ObjectCtrlInfo
{
	public ObjectInfo objectInfo;

	public TreeNodeObject treeNodeObject;

	public GuideObject guideObject;

	public ObjectCtrlInfo parentInfo;

	public int kind
	{
		get
		{
			if (objectInfo == null)
			{
				return -1;
			}
			return objectInfo.kind;
		}
	}

	public int[] kinds
	{
		get
		{
			if (objectInfo == null)
			{
				return new int[1] { -1 };
			}
			return objectInfo.kinds;
		}
	}

	public virtual float animeSpeed { get; set; }

	public virtual ObjectCtrlInfo this[int _idx]
	{
		get
		{
			if (_idx != 0)
			{
				return parentInfo;
			}
			return this;
		}
	}

	public abstract void OnDelete();

	public abstract void OnAttach(TreeNodeObject _parent, ObjectCtrlInfo _child);

	public abstract void OnDetach();

	public abstract void OnDetachChild(ObjectCtrlInfo _child);

	public abstract void OnSelect(bool _select);

	public abstract void OnLoadAttach(TreeNodeObject _parent, ObjectCtrlInfo _child);

	public abstract void OnVisible(bool _visible);

	public virtual void OnSavePreprocessing()
	{
		if (objectInfo != null && treeNodeObject != null)
		{
			objectInfo.treeState = treeNodeObject.treeState;
		}
		if (objectInfo != null && treeNodeObject != null)
		{
			objectInfo.visible = treeNodeObject.visible;
		}
	}
}
