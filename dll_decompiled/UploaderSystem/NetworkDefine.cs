using System;
using UnityEngine;

namespace UploaderSystem;

public static class NetworkDefine
{
	public const string CacheFileMark = "【CacheFile】";

	public const int CacheFileVersion = 100;

	public static readonly Version NetInfoVersion = new Version("1.0.0");

	public const string CacheSettingPath = "cache/cachesetting.dat";

	public const string CacheCharaHS2Dir = "cache/chara_hs2/";

	public const string CacheCharaAIDir = "cache/chara_ai/";

	public const string cryptPW = "aisyoujyo";

	public const string cryptSALT = "phpaddress";

	public const string HS2_Check_URLFile = "hs2_check_url.dat";

	public const string HS2_Version_URLFile = "hs2_version_url.dat";

	public const string HS2_System_URLFile = "hs2_system_url.dat";

	public const string HS2_UploadChara_URLFile = "hs2_uploadChara_url.dat";

	public const string AIS_Check_URLFile = "ais_check_url.dat";

	public const string AIS_Version_URLFile = "ais_version_url.dat";

	public const string AIS_System_URLFile = "ais_system_url.dat";

	public const string AIS_UploadChara_URLFile = "ais_uploadChara_url.dat";

	public static readonly Color colorWhite = new Color(0.9294118f, (float)Math.E * 105f / 302f, 83f / 85f);

	public static readonly string[] msgPressAnyKey = new string[4] { "何かキーを押して下さい", "Press any key to continue", "请按任意键", "請按任意鍵……" };

	public static readonly string[] msgServerCheck = new string[4] { "サーバーをチェックしています", "Checking the server...", "正在检查服务器", "正在檢查伺服器……" };

	public static readonly string[] msgServerAccessInfoField = new string[4] { "サーバーへアクセスするための情報の読み込みに失敗しました。", "Failed to acquire data required to connect to server.", "无法读取连接服务器所需的信息。", "無法取得連接到伺服器的資料。" };

	public static readonly string[] msgServerAccessField = new string[4] { "サーバーへのアクセスに失敗しました。", "Failed to connect to server.", "无法连接服务器。", "無法連接伺服器。" };

	public static readonly string[,] strBtnUpload = new string[4, 2]
	{
		{ "更新", "アップロード" },
		{ "Updated", "Upload" },
		{ "更新", "上传" },
		{ "更新", "上傳" }
	};

	public static readonly string[,] msgUpConfirmation = new string[4, 2]
	{
		{ "本当に更新しますか？", "本当にアップロードしますか？" },
		{ "Are you sure you want to update?", "Are you sure you want to upload this?" },
		{ "确定更新吗？", "确定要上传吗？" },
		{ "確定要更新嗎？", "確定要上傳嗎？" }
	};

	public static readonly string[,] msgUpFailer = new string[4, 2]
	{
		{ "更新に失敗しました。", "アップロードに失敗しました。" },
		{ "Update failed.", "Upload failed." },
		{ "更新失败。", "上传失败。" },
		{ "更新失敗。", "上傳失敗。" }
	};

	public static readonly string[,] msgUpCannotRead = new string[4, 2]
	{
		{ "更新するデータが読み込めませんでした。", "アップロードするデータが読み込めませんでした。" },
		{ "Updated data could not be loaded.", "Upload data could not be loaded." },
		{ "无法读取更新数据。", "无法读取上传数据。" },
		{ "無法讀取欲更新的資料。", "無法讀取欲上傳的資料。" }
	};

	public static readonly string[] msgUpCannotBeIdentified = new string[4] { "更新するデータが特定出来ませんでした。", "Updated data could not be found.", "无法找到更新数据。", "找不到欲更新的資料。" };

	public static readonly string[] msgUpAlreadyUploaded = new string[4] { "そのデータは既にアップされています。", "This data has already been uploaded.", "该存档已读取", "此角色資料已存在。" };

	public static readonly string[,] msgUpDone = new string[4, 2]
	{
		{ "データを更新しました。", "データをアップロードしました。" },
		{ "Update complete.", "Upload complete." },
		{ "已更新数据。", "已上传数据。" },
		{ "資料已更新。", "資料已上傳。" }
	};

	public static readonly string[] msgUpCompressionHousing = new string[4] { "ハウジングの圧縮を行っています…", "Compressing housing data...", "正在压缩建设数据……", "正在進行建設的壓縮……" };

	public static readonly string[] msgUpStartUploadHousing = new string[4] { "ハウジングのアップロードを開始します…", "Starting housing data upload...", "开始上传建设数据……", "開始上傳建設……" };

	public static readonly string[] msgDownDeleteData = new string[4] { "データを削除しました。", "Data has been deleted.", "已删除数据。", "已刪除資料。" };

	public static readonly string[] msgDownDeleteCache = new string[4] { "キャッシュを削除しました。", "Cache has been deleted.", "已删除缓存。", "已刪除快取。" };

	public static readonly string[] msgDownUnknown = new string[4] { "不明", "Unknown", "未知", "不明" };

	public static readonly string[] msgDownDownloadData = new string[4] { "データをダウンロードしています…", "Downloading data...", "正在下载数据……", "正在下載資料……" };

	public static readonly string[] msgDownDownloaded = new string[4] { "データのダウンロードが完了しました。", "Download complete.", "已完成数据下载。", "資料下載完畢。" };

	public static readonly string[] msgDownFailed = new string[4] { "データのダウンロードに失敗しました。", "Download failed.", "数据下载失败。", "資料下載失敗。" };

	public static readonly string[] msgDownLikes = new string[4] { "拍手に失敗しました。", "Like failed.", "点赞失败。", "拍手失敗。" };

	public static readonly string[] msgDownFailedGetThumbnail = new string[4] { "サムネイル画像の取得に失敗しました。", "Failed to obtain thumbnail(s).", "获取缩略图失败。", "縮略圖取得失敗。" };

	public static readonly string[] msgDownNotUploadDataFound = new string[4] { "アップロードされたデータが見つかりませんでした。", "No uploaded data was found.", "无法找到已上传数据。", "搜尋不到上傳的資料。" };

	public static readonly string[] msgDownDecompressingFile = new string[4] { "ファイルの解凍を行っています…", "Unpacking file...", "正在进行文件解压缩……", "正在解壓縮資料夾……" };

	public static readonly string[] msgDownFailedDecompressingFile = new string[4] { "ファイルの解凍に失敗しました。", "Unpacking failed.", "文件解压缩失败。", "資料夾解壓縮失敗。" };

	public static readonly string[] msgDownConfirmDelete = new string[4] { "本当に削除しますか？", "Are you sure you want to\ndelete this?", "确定删除吗？", "確定要刪除嗎？" };

	public static readonly string[] msgDownFailedDelete = new string[4] { "データの削除に失敗しました。", "Delete failed.", "数据删除失败。", "資料刪除失敗。" };

	public static readonly string[] msgNetGetInfoFromServer = new string[4] { "サーバーから情報を取得しています。", "Obtaining information from server...", "正在从服务器获取信息。", "正在從伺服器取得資訊……" };

	public static readonly string[] msgNetGetVersion = new string[4] { "最新のバージョン情報の取得を開始します。", "Obtaining latest version information...", "开始获取最新版本信息。", "正在取得最新版本的資訊……" };

	public static readonly string[] msgNetConfirmUser = new string[4] { "ユーザー情報の確認を開始します。", "Checking user information...", "开始确认用户信息。", "正在確認玩家資訊……" };

	public static readonly string[] msgNetStartEntryHN = new string[4] { "ハンドル名の登録を開始します。", "Registering handle name...", "开始登陆网名。", "正在登錄線上名稱……" };

	public static readonly string[] msgNetGetAllHN = new string[4] { "登録された全ハンドル名の取得を開始します。", "Obtaining all registered handle names...", "开始获取已登录的全部网名。", "正在取得已登錄的所有線上名稱……" };

	public static readonly string[] msgNetGetAllCharaInfo = new string[4] { "全てのキャラ情報の取得を開始します。", "Obtaining all character data...", "开始获取全部角色信息。", "正在取得所有的角色資訊……" };

	public static readonly string[] msgNetGetAllHousingInfo = new string[4] { "全てのハウジング情報の取得を開始します。", "Obtaining all housing data...", "开始获取全部建设信息。", "正在取得所有的建設資訊……" };

	public static readonly string[] msgNetReady = new string[4] { "準備が完了しました。", "Preparations complete.", "已完成准备。", "準備完畢。" };

	public static readonly string[] msgNetNotReady = new string[4] { "ネットワークシステムの準備が完了しませんでした。", "Could not complete preparations for the network system.", "无法完成网络系统的准备。", "網路系統未準備完畢。" };

	public static readonly string[] msgNetFailedGetCharaInfo = new string[4] { "キャラ情報の取得に失敗しました", "Failed to obtain character data.", "获取角色信息失败。", "角色資訊取得失敗。" };

	public static readonly string[] msgNetFailedGetHousingInfo = new string[4] { "ハウジング情報の取得に失敗しました", "Failed to obtain housing data.", "获取建设信息失败。", "建設資訊取得失敗。" };

	public static readonly string[] msgNetReadyGetData = new string[4] { "データの取得が完了しました。", "Data received.", "已获取数据。", "資料取得完畢。" };

	public static readonly string[] msgNetFailedGetVersion = new string[4] { "バージョン情報の取得に失敗しました。", "Failed to obtain version information.", "获取版本信息失败。", "版本資訊取得失敗。" };

	public static readonly string[] msgNetFailedConfirmUser = new string[4] { "ユーザー情報の確認に失敗しました。", "Failed to confirm user information.", "用户信息确认失败。", "玩家資訊確認失敗。" };

	public static readonly string[] msgNetFailedUpdateHN = new string[4] { "ハンドル名の更新に失敗しました。", "Failed to update handle name.", "网名更新失败。", "線上名稱更新失敗。" };

	public static readonly string[] msgNetUpdatedHN = new string[4] { "ハンドル名を更新しました。", "Handle name updated.", "已更新网名。", "已更新線上名稱。" };

	public static readonly string[] msgNetFailedGetAllHN = new string[4] { "登録された全ハンドル名の取得に失敗しました。", "Failed to obtain all registered handle names.", "无法获取已登录的全部网名。", "已登錄的所有線上名稱取得失敗。" };

	public static readonly string[] strHandleNameNoSelect = new string[4] { "No Select", "No Select", "No Select", "No Select" };
}
