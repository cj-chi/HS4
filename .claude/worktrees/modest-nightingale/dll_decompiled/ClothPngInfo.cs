using MessagePack;

[MessagePackObject(false)]
public class ClothPngInfo
{
	[Key(0)]
	public string bathFile { get; set; } = "";

	[Key(1)]
	public string roomWearFile { get; set; } = "";
}
