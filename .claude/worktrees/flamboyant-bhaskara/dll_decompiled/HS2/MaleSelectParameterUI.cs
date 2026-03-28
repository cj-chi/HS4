using AIChara;
using GameLoadCharaFileSystem;
using Illusion.Component.UI;
using Illusion.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace HS2;

public class MaleSelectParameterUI : MonoBehaviour
{
	[SerializeField]
	private Text txtCharaName;

	[SerializeField]
	private RawImage riCard;

	[SerializeField]
	private Texture2D texNone;

	[SerializeField]
	private SpriteChangeCtrl scc;

	public void SetParameter(GameCharaFileInfo _info)
	{
		txtCharaName.text = _info.name;
		if (riCard.texture != texNone && (bool)riCard.texture)
		{
			Object.Destroy(riCard.texture);
		}
		riCard.texture = null;
		riCard.texture = PngAssist.ChangeTextureFromByte(_info.pngData ?? PngFile.LoadPngBytes(_info.FullPath));
	}

	public void SetParameter(ChaFileControl _info)
	{
		txtCharaName.text = _info.parameter.fullname;
		if (riCard.texture != texNone && (bool)riCard.texture)
		{
			Object.Destroy(riCard.texture);
		}
		riCard.texture = null;
		riCard.texture = PngAssist.ChangeTextureFromByte(_info.pngData ?? PngFile.LoadPngBytes(UserData.Path + "chara/female/" + _info.charaFileName + ".png"));
		if ((bool)scc)
		{
			scc.gameObject.SetActiveIfDifferent(active: true);
			scc.ChangeValue(GlobalHS2Calc.GetStateIconNum((int)_info.gameinfo2.nowDrawState, _info.parameter2.personality));
		}
	}

	public void InitParameter()
	{
		txtCharaName.text = "";
		riCard.texture = texNone;
		if ((bool)scc)
		{
			scc.gameObject.SetActiveIfDifferent(active: false);
		}
	}
}
