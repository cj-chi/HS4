using AIChara;
using Manager;

namespace Studio;

public class OCICharMale : OCIChar
{
	public ChaControl male => charInfo;

	public override void OnDelete()
	{
		base.OnDelete();
		Singleton<Character>.Instance.DeleteChara(male);
	}

	public override void SetClothesStateAll(int _state)
	{
		base.SetClothesStateAll(_state);
		male.SetClothesStateAll((byte)_state);
	}

	public override void ChangeChara(string _path)
	{
		base.ChangeChara(_path);
	}

	public override void LoadClothesFile(string _path)
	{
		base.LoadClothesFile(_path);
	}
}
