using System.Collections.Generic;
using System.IO;
using System.Text;
using AIChara;
using Manager;

namespace HS2;

public class STRCharaFileInfoAssist
{
	private void AddList(List<STRCharaFileInfo> _list, List<string> _lstFileName, int _state)
	{
		(new string[1])[0] = "*.png";
		_ = Singleton<GameSystem>.Instance.UserUUID;
		_ = Singleton<Game>.Instance.saveData;
		string value = UserData.Path + "chara/female/";
		int num = 0;
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < _lstFileName.Count; i++)
		{
			ChaFileControl chaFileControl = new ChaFileControl();
			stringBuilder.Clear();
			stringBuilder.Append(value).Append(_lstFileName[i]).Append(".png");
			if (!chaFileControl.LoadCharaFile(stringBuilder.ToString()))
			{
				chaFileControl.GetLastErrorCode();
			}
			else if (chaFileControl.parameter.sex == 1 && chaFileControl.gameinfo2.hCount != 0 && (_state == -1 || chaFileControl.gameinfo2.nowDrawState == (ChaFileDefine.State)_state))
			{
				if (Voice.infoTable.TryGetValue(chaFileControl.parameter2.personality, out var value2))
				{
					_ = value2.Personality;
				}
				_list.Add(new STRCharaFileInfo
				{
					index = num++,
					name = chaFileControl.parameter.fullname,
					personality = chaFileControl.parameter2.personality,
					FullPath = stringBuilder.ToString(),
					FileName = _lstFileName[i],
					time = File.GetLastWriteTime(stringBuilder.ToString()),
					futanari = chaFileControl.parameter.futanari,
					state = chaFileControl.gameinfo2.nowDrawState,
					trait = chaFileControl.parameter2.trait,
					hAttribute = chaFileControl.parameter2.hAttribute
				});
			}
		}
	}

	public List<STRCharaFileInfo> CreateCharaFileInfoList(int _state, List<string> _lstFileName)
	{
		List<STRCharaFileInfo> list = new List<STRCharaFileInfo>();
		AddList(list, _lstFileName, _state);
		return list;
	}
}
