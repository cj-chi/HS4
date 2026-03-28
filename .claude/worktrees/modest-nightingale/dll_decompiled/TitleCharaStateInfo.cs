using System;
using System.Collections.Generic;
using UnityEngine;

public class TitleCharaStateInfo : ScriptableObject
{
	[Serializable]
	public class Param
	{
		public int id;

		public int number;

		public int pose;

		public int state;

		public int animationID;

		public string posManifest;

		public string posBundle;

		public string posFile;

		public string camManifest;

		public string camBundle;

		public string camFile;

		public Param()
		{
		}

		public Param(Param _param)
		{
			id = _param.id;
			number = _param.number;
			pose = _param.pose;
			state = _param.state;
			animationID = _param.animationID;
			posManifest = _param.posManifest;
			posBundle = _param.posBundle;
			posFile = _param.posFile;
			camManifest = _param.camManifest;
			camBundle = _param.camBundle;
			camFile = _param.camFile;
		}

		public void Copy(Param _param)
		{
			id = _param.id;
			number = _param.number;
			pose = _param.pose;
			state = _param.state;
			animationID = _param.animationID;
			posManifest = _param.posManifest;
			posBundle = _param.posBundle;
			posFile = _param.posFile;
			camManifest = _param.camManifest;
			camBundle = _param.camBundle;
			camFile = _param.camFile;
		}
	}

	public List<Param> param = new List<Param>();
}
