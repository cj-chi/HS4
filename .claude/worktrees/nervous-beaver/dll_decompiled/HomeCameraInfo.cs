using System;
using System.Collections.Generic;
using UnityEngine;

public class HomeCameraInfo : ScriptableObject
{
	[Serializable]
	public class Param
	{
		public int id;

		public bool isTransformInfoData;

		public string camManifest;

		public string camBundle;

		public string camFile;

		public string dofManifest;

		public string dofBundle;

		public string dofFile;

		public Param()
		{
		}

		public Param(Param _param)
		{
			id = _param.id;
			isTransformInfoData = _param.isTransformInfoData;
			camManifest = _param.camManifest;
			camBundle = _param.camBundle;
			camFile = _param.camFile;
			dofManifest = _param.dofManifest;
			dofBundle = _param.dofBundle;
			dofFile = _param.dofFile;
		}
	}

	public List<Param> param = new List<Param>();
}
