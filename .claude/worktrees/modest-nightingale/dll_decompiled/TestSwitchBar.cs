using UnityEngine;
using UnityEngine.UI;

public class TestSwitchBar : MonoBehaviour
{
	[SerializeField]
	private ProgressBarPro[] progressBarPrefabs;

	[SerializeField]
	private Transform prefabParent;

	[SerializeField]
	private int currentPrefab;

	[SerializeField]
	private Text prefabName;

	private ProgressBarPro bar;

	private float currentValue = 1f;

	private void Start()
	{
		InstantiateBar(currentPrefab);
	}

	public void SetRandomBar()
	{
		InstantiateBar(Random.Range(0, progressBarPrefabs.Length));
	}

	public void ShiftBar(int shift)
	{
		int num = currentPrefab + shift;
		if (num >= progressBarPrefabs.Length)
		{
			InstantiateBar(0);
		}
		if (num < 0)
		{
			InstantiateBar(progressBarPrefabs.Length - 1);
		}
		else
		{
			InstantiateBar(num);
		}
	}

	private void InstantiateBar(int num)
	{
		if (num >= 0 && num < progressBarPrefabs.Length)
		{
			currentPrefab = num;
			if (bar != null)
			{
				Object.Destroy(bar.gameObject);
			}
			bar = Object.Instantiate(progressBarPrefabs[num], prefabParent);
			bar.gameObject.SetActive(value: false);
			bar.transform.localScale = Vector3.one;
			bar.SetValue(currentValue);
			bar.gameObject.SetActive(value: true);
			prefabName.text = progressBarPrefabs[currentPrefab].gameObject.name;
			Invoke("EnableBar", 0.01f);
		}
	}

	private void EnableBar()
	{
		if (bar != null)
		{
			bar.gameObject.SetActive(value: true);
		}
	}

	public void SetValue(float value)
	{
		currentValue = value;
		if (bar != null)
		{
			bar.SetValue(value);
		}
	}

	public void SetBarColor(Color color)
	{
		if (bar != null)
		{
			bar.SetBarColor(color);
		}
	}
}
