using UnityEngine;
using UnityEngine.UI;

namespace Config;

public class SettingData : MonoBehaviour
{
	[SerializeField]
	private string _paramName;

	[SerializeField]
	private Selectable _target;

	public string paramName => _paramName;

	public Selectable target => _target;
}
