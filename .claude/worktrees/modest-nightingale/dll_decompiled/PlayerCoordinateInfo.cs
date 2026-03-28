using MessagePack;

[MessagePackObject(false)]
public class PlayerCoordinateInfo
{
	[Key(0)]
	public string file { get; set; } = "";

	[Key(1)]
	public int sex { get; set; }
}
