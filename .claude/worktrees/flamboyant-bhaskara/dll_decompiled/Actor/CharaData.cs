using System;
using System.Collections.Generic;
using ADV;
using AIChara;
using UnityEngine;

namespace Actor;

[Serializable]
public abstract class CharaData : IParams, ICommandData
{
	public abstract int voiceNo { get; }

	Params IParams.param => param;

	public abstract CharaParams param { get; }

	public GameObject root { get; private set; }

	public ChaControl chaCtrl { get; private set; }

	public Transform transform
	{
		get
		{
			if (!(root == null))
			{
				return root.transform;
			}
			return null;
		}
	}

	public string Name => parameter.fullname;

	public int personality => parameter2.personality;

	public int birthMonth => parameter.birthMonth;

	public int birthDay => parameter.birthDay;

	public float voicePitch => parameter2.voicePitch;

	public bool chaFileInitialized { get; set; }

	public ChaFileControl chaFile { get; private set; }

	public ChaFileParameter parameter => chaFile.parameter;

	public ChaFileParameter2 parameter2 => chaFile.parameter2;

	public ChaFileGameInfo gameinfo => chaFile.gameinfo;

	public ChaFileGameInfo2 gameinfo2 => chaFile.gameinfo2;

	public abstract IEnumerable<CommandData> CreateCommandData(string head);

	public void SetRoot(GameObject root)
	{
		this.root = root;
		if (root == null)
		{
			chaCtrl = null;
		}
		else
		{
			chaCtrl = root.GetComponent<ChaControl>();
		}
	}

	public void SetCharFile(ChaFileControl chaFile)
	{
		this.chaFile = chaFile;
		ChaFileUpdate();
	}

	public abstract void ChaFileUpdate();

	public abstract void Initialize();

	private void Create(bool isRandomize)
	{
		Initialize();
		if (isRandomize)
		{
			Randomize();
		}
	}

	public CharaData(ChaFileControl chaFile, bool isRandomize)
	{
		SetCharFile(chaFile);
		Create(isRandomize);
	}

	public CharaData(bool isRandomize)
	{
		SetCharFile(new ChaFileControl());
		Create(isRandomize);
	}

	public abstract void Randomize();
}
