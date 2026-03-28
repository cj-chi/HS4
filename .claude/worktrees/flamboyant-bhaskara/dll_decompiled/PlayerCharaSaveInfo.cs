using MessagePack;

[MessagePackObject(false)]
public class PlayerCharaSaveInfo
{
	[Key(0)]
	public string FileName { get; set; } = "";

	[Key(1)]
	public int Sex { get; set; }

	[Key(2)]
	public bool Futanari { get; set; }

	public void Copy(PlayerCharaSaveInfo _info)
	{
		FileName = _info.FileName;
		Sex = _info.Sex;
		Futanari = _info.Futanari;
	}
}
