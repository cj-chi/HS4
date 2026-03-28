namespace SuperScrollView;

public class GridViewLayoutParam
{
	public int mColumnOrRowCount;

	public float mItemWidthOrHeight;

	public float mPadding1;

	public float mPadding2;

	public float[] mCustomColumnOrRowOffsetArray;

	public bool CheckParam()
	{
		if (mColumnOrRowCount <= 0)
		{
			return false;
		}
		if (mItemWidthOrHeight <= 0f)
		{
			return false;
		}
		if (mCustomColumnOrRowOffsetArray != null && mCustomColumnOrRowOffsetArray.Length != mColumnOrRowCount)
		{
			return false;
		}
		return true;
	}
}
