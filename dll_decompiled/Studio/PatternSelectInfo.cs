namespace Studio;

public class PatternSelectInfo
{
	public int index = -1;

	public string name = "";

	public string assetBundle = "";

	public string assetName = "";

	public int category;

	public bool disable;

	public bool disvisible;

	public PatternSelectInfoComponent sic;

	public bool activeSelf => sic.gameObject.activeSelf;

	public bool interactable => sic.tgl.interactable;

	public bool isOn => sic.tgl.isOn;
}
