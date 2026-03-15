using System;
using UnityEngine;

namespace PlayfulSystems.LoadingScreen;

[Serializable]
public class LoadingTip
{
	public string header;

	[Multiline(4)]
	public string description;
}
