using System;

namespace CharaCustom;

public static class CharaCustomDefine
{
	public const string CustomSettingSave = "custom/customscene.dat";

	public static readonly Version CustomSettingVersion = new Version("0.0.3");

	public const float SliderWheelSensitive = -0.01f;

	public const int defEyebrowPtn = 0;

	public const int defEyePtn = 0;

	public const int defMouthPtn = 0;

	public static readonly string[] CustomCorrectTitle = new string[4] { "調整", "Adjust", "调整", "調整" };

	public static readonly string[] CustomColorTitle = new string[4] { "カラー", "Color", "颜色", "顏色" };

	public static readonly string[] CustomCapSave = new string[4] { "保存", "Save", "保存", "儲存" };

	public static readonly string[] CustomCapUpdate = new string[4] { "撮影", "Take Photo", "拍摄", "拍攝" };

	public static readonly string[] CustomNoneStr = new string[4] { "なし", "None", "无", "無" };

	public static readonly string[,] ColorPresetMessage = new string[4, 2]
	{
		{ "左クリックで適用、右クリックで削除", "左クリックで適用" },
		{ "Left-click to set, right-click to remove", "Left-click to set" },
		{ "点击左键应用，右键删除", "点击左键应用" },
		{ "以左鍵套用，以右鍵移除", "以左鍵選取" }
	};

	public static readonly string[] ColorPresetNewMessage = new string[4] { "左クリックで現在のカラーを登録", "Left click to register current color", "点击左键登录现在的颜色", "按下左鍵反映目前的顏色" };

	public static readonly string[] CustomHandBaseMsg = new string[4] { "ポーズ", "Pose", "姿势", "姿勢" };

	public static readonly string[] CustomConfirmDelete = new string[4] { "本当に削除しますか？", "Are you sure you want to delete this character?", "确定删除吗？", "確定要刪除？" };

	public static readonly string[] CustomConfirmDeleteWithIncludeParam = new string[4] { "本当に削除しますか？\n<color=#DE4529FF>このキャラにはパラメータが含まれています。</color>", "<color=#DE4529FF>Character parameters will also be deleted.</color>\nDo you wish to continue?", "确定删除吗？\n<color=#DE4529FF>该角色已有数值</color>", "確定要刪除？\n<color=#DE4529FF>這個角色含有經驗值。</color>" };

	public static readonly string[] CustomConfirmOverwrite = new string[4] { "本当に上書きしますか？", "Overwrite file?", "确定覆盖保存吗？", "確定要覆蓋？" };

	public static readonly string[] CustomConfirmOverwriteWithInitializeParam = new string[4] { "本当に上書きしますか？\n<color=#DE4529FF>上書きするとパラメータは初期化されます。</color>", "<color=#DE4529FF>Overwriting this file will restore parameters to their default settings.</color>\nDo you wish to continue?", "确定覆盖保存吗？\n<color=#DE4529FF>覆盖保存后数值会初始化</color>", "確定要覆蓋？\n<color=#DE4529FF>覆蓋後原有經驗值將被初始化。</color>" };
}
