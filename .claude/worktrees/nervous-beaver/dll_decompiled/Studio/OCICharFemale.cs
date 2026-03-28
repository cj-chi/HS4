using AIChara;
using Manager;

namespace Studio;

public class OCICharFemale : OCIChar
{
	public ChaControl female => charInfo;

	public override void OnDelete()
	{
		base.OnDelete();
		Singleton<Character>.Instance.DeleteChara(female);
	}

	public override void SetSiruFlags(ChaFileDefine.SiruParts _parts, byte _state)
	{
		base.SetSiruFlags(_parts, _state);
		female.SetSiruFlag(_parts, _state);
	}

	public override byte GetSiruFlags(ChaFileDefine.SiruParts _parts)
	{
		return female.GetSiruFlag(_parts);
	}

	public override void SetNipStand(float _value)
	{
		base.SetNipStand(_value);
		base.oiCharInfo.nipple = _value;
		female.ChangeNipRate(_value);
	}

	public override void ChangeChara(string _path)
	{
		base.ChangeChara(_path);
		female.UpdateBustSoftnessAndGravity();
		optionItemCtrl.ReCounterScale();
		optionItemCtrl.height = base.animeOptionParam1;
		base.animeOptionParam1 = base.animeOptionParam1;
		base.animeOptionParam2 = base.animeOptionParam2;
	}

	public override void SetClothesStateAll(int _state)
	{
		base.SetClothesStateAll(_state);
		female.SetClothesStateAll((byte)_state);
	}

	public override void LoadClothesFile(string _path)
	{
		base.LoadClothesFile(_path);
		female.UpdateBustSoftnessAndGravity();
		bool active = base.oiCharInfo.activeFK[6];
		ActiveFK(OIBoneInfo.BoneGroup.Skirt, _active: false, base.oiCharInfo.enableFK);
		fkCtrl.ResetUsedBone(this);
		skirtDynamic = AddObjectFemale.GetSkirtDynamic(charInfo.objClothes);
		ActiveFK(OIBoneInfo.BoneGroup.Skirt, active, base.oiCharInfo.enableFK);
	}
}
