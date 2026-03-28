using System;
using System.Collections.Generic;
using CharaCustom;
using Illusion.Extensions;
using UnityEngine;

namespace UploaderSystem;

public class NetworkInfo : Singleton<NetworkInfo>
{
	public class SelectHNInfo
	{
		public int userIdx = -1;

		public string handlename = "";

		public string drawname = "";
	}

	public class Profile
	{
		public int userIdx = -1;
	}

	public class UserInfo
	{
		public string handleName = "";
	}

	public class BaseIndex
	{
		public int idx = -1;

		public string data_uid = "";

		public int update_idx;

		public int user_idx = -1;

		public string name = "";

		public string comment = "";

		public DateTime updateTime;

		public int dlCount;

		public int weekCount;

		public int applause;

		public int rankingT = 999999;

		public int rankingW = 999999;

		public DateTime createTime;
	}

	public class CharaInfo : BaseIndex
	{
		public int type;

		public int birthmonth = 1;

		public int birthday = 1;

		public string strBirthDay = "";

		public int sex;

		public int height = 1;

		public int bust = 1;

		public int hair;

		public int trait;

		public int mind;

		public int hAttribute;

		public int isChangeParameter;
	}

	public Profile profile = new Profile();

	public Dictionary<int, UserInfo> dictUserInfo = new Dictionary<int, UserInfo>();

	public List<CharaInfo> lstCharaInfo = new List<CharaInfo>();

	[SerializeField]
	public Net_PopupMsg popupMsg;

	[SerializeField]
	public LogView logview;

	[SerializeField]
	public Net_PopupCheck popupCheck;

	[SerializeField]
	public NetCacheControl cacheCtrl;

	[SerializeField]
	public NetSelectHNScrollController netSelectHN;

	[SerializeField]
	public CustomCharaWindow selectCharaFWindow;

	[SerializeField]
	public CustomCharaWindow selectCharaMWindow;

	[SerializeField]
	private GameObject objBlockUI;

	[HideInInspector]
	public bool changeCharaList;

	[HideInInspector]
	public Version newestVersion = new Version(0, 0, 0);

	[HideInInspector]
	public bool updateProfile;

	[HideInInspector]
	public bool updateVersion;

	public Dictionary<string, int>[] dictUploaded = new Dictionary<string, int>[Enum.GetNames(typeof(DataType)).Length];

	public bool noUserControl;

	public void DrawMessage(Color color, string msg)
	{
		if (null != logview && logview.IsActive)
		{
			logview.AddLog(color, msg);
		}
		else
		{
			popupMsg.StartMessage(0.2f, 2f, 0.2f, msg, noUserControl ? 2 : 0);
		}
	}

	public void BlockUI()
	{
		if ((bool)objBlockUI)
		{
			objBlockUI.SetActiveIfDifferent(active: true);
		}
	}

	public void UnblockUI()
	{
		if ((bool)objBlockUI)
		{
			objBlockUI.SetActiveIfDifferent(active: false);
		}
	}

	public void Start()
	{
		for (int i = 0; i < dictUploaded.Length; i++)
		{
			dictUploaded[i] = new Dictionary<string, int>();
		}
	}
}
