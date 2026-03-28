using System.Collections;

public abstract class AssetBundleLoadOperation : IEnumerator
{
	object IEnumerator.Current => null;

	bool IEnumerator.MoveNext()
	{
		return !IsDone();
	}

	void IEnumerator.Reset()
	{
	}

	public abstract bool IsDone();
}
