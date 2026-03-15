namespace ADV;

public abstract class SceneParameter
{
	public static ADVScene advScene { get; set; }

	public IData data { get; }

	public SceneParameter(IData data)
	{
		this.data = data;
	}

	public virtual void Init()
	{
	}

	public virtual void Release()
	{
	}
}
