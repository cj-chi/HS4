using MessagePack;
using UnityEngine;

namespace AIChara;

[MessagePackObject(true)]
public class PaintInfo
{
	public int id { get; set; }

	public Color color { get; set; }

	public float glossPower { get; set; }

	public float metallicPower { get; set; }

	public int layoutId { get; set; }

	public Vector4 layout { get; set; }

	public float rotation { get; set; }

	public PaintInfo()
	{
		MemberInit();
	}

	public void MemberInit()
	{
		id = 0;
		color = Color.red;
		glossPower = 0.5f;
		metallicPower = 0.5f;
		layoutId = 0;
		layout = new Vector4(0.5f, 0.5f, 0.5f, 0.5f);
		rotation = 0.5f;
	}

	public void Copy(PaintInfo src)
	{
		id = src.id;
		color = src.color;
		glossPower = src.glossPower;
		metallicPower = src.metallicPower;
		layoutId = src.layoutId;
		layout = src.layout;
		rotation = src.rotation;
	}
}
