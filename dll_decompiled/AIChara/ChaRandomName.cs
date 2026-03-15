using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AIChara;

public class ChaRandomName
{
	public List<string> lstRandLastNameH { get; set; } = new List<string>();

	public List<string> lstRandLastNameK { get; set; } = new List<string>();

	public List<string> lstRandFirstNameHF { get; set; } = new List<string>();

	public List<string> lstRandFirstNameKF { get; set; } = new List<string>();

	public List<string> lstRandFirstNameHM { get; set; } = new List<string>();

	public List<string> lstRandFirstNameKM { get; set; } = new List<string>();

	public List<string> lstRandMiddleName { get; set; } = new List<string>();

	public void Initialize()
	{
		List<ExcelData.Param> source = ChaListControl.LoadExcelData("list/characustom/namelist.unity3d", "RandNameList_Name", 2, 1);
		lstRandLastNameH = (from x in source
			select x.list[0] into x
			where x != "0" && x != ""
			select x).ToList();
		lstRandLastNameK = (from x in source
			select x.list[1] into x
			where x != "0" && x != ""
			select x).ToList();
		lstRandFirstNameHF = (from x in source
			select x.list[2] into x
			where x != "0" && x != ""
			select x).ToList();
		lstRandFirstNameKF = (from x in source
			select x.list[3] into x
			where x != "0" && x != ""
			select x).ToList();
		lstRandFirstNameHM = (from x in source
			select x.list[4] into x
			where x != "0" && x != ""
			select x).ToList();
		lstRandFirstNameKM = (from x in source
			select x.list[5] into x
			where x != "0" && x != ""
			select x).ToList();
		lstRandMiddleName = (from x in source
			select x.list[6] into x
			where x != "0" && x != ""
			select x).ToList();
	}

	public string GetRandName(byte Sex)
	{
		StringBuilder stringBuilder = new StringBuilder(64);
		if (GetRandomIndex(5, 95) == 0)
		{
			if (GetRandomIndex(10, 90) == 0)
			{
				if (Sex == 0)
				{
					if (lstRandFirstNameKM.Count != 0)
					{
						stringBuilder.Append(lstRandFirstNameKM[Random.Range(0, lstRandFirstNameKM.Count)]);
					}
				}
				else if (lstRandFirstNameKF.Count != 0)
				{
					stringBuilder.Append(lstRandFirstNameKF[Random.Range(0, lstRandFirstNameKF.Count)]);
				}
			}
			else if (Sex == 0)
			{
				if (lstRandFirstNameHM.Count != 0)
				{
					stringBuilder.Append(lstRandFirstNameHM[Random.Range(0, lstRandFirstNameKM.Count)]);
				}
			}
			else if (lstRandFirstNameHF.Count != 0)
			{
				stringBuilder.Append(lstRandFirstNameHF[Random.Range(0, lstRandFirstNameKF.Count)]);
			}
		}
		else if (GetRandomIndex(10, 90) == 0)
		{
			if (Sex == 0)
			{
				if (lstRandFirstNameKM.Count != 0)
				{
					stringBuilder.Append(lstRandFirstNameKM[Random.Range(0, lstRandFirstNameKM.Count)]);
				}
			}
			else if (lstRandFirstNameKF.Count != 0)
			{
				stringBuilder.Append(lstRandFirstNameKF[Random.Range(0, lstRandFirstNameKF.Count)]);
			}
			stringBuilder.Append("・");
			string text = "";
			while (lstRandLastNameK.Count != 0)
			{
				text = lstRandLastNameK[Random.Range(0, lstRandLastNameK.Count)];
				if (text.Length + stringBuilder.Length < 16)
				{
					break;
				}
			}
			if ("" != text && stringBuilder.Length + text.Length < 10)
			{
				if (GetRandomIndex(10, 90) == 0)
				{
					string value = lstRandMiddleName[Random.Range(0, lstRandMiddleName.Count)];
					stringBuilder.Append(value).Append("・").Append(text);
				}
				else
				{
					stringBuilder.Append(text);
				}
			}
			else
			{
				stringBuilder.Append(text);
			}
		}
		else
		{
			if (lstRandLastNameH.Count != 0)
			{
				stringBuilder.Append(lstRandLastNameH[Random.Range(0, lstRandLastNameH.Count)]);
			}
			stringBuilder.Append(" ");
			if (Sex == 0)
			{
				if (lstRandFirstNameHM.Count != 0)
				{
					stringBuilder.Append(lstRandFirstNameHM[Random.Range(0, lstRandFirstNameHM.Count)]);
				}
			}
			else if (lstRandFirstNameHF.Count != 0)
			{
				stringBuilder.Append(lstRandFirstNameHF[Random.Range(0, lstRandFirstNameHF.Count)]);
			}
		}
		if (stringBuilder.Length == 0)
		{
			return "";
		}
		return stringBuilder.ToString();
	}

	public static int GetRandomIndex(params int[] weightTable)
	{
		int num = weightTable.Sum();
		int num2 = Random.Range(1, num + 1);
		int result = -1;
		for (int i = 0; i < weightTable.Length; i++)
		{
			if (weightTable[i] >= num2)
			{
				result = i;
				break;
			}
			num2 -= weightTable[i];
		}
		return result;
	}
}
