using UnityEngine;
using UnityEngine.UI;

public class InputFieldLimit : MonoBehaviour
{
	[SerializeField]
	private InputField input;

	[SerializeField]
	private int numLimit;

	private void Start()
	{
		if (input == null)
		{
			input = GetComponent<InputField>();
		}
		if (input != null)
		{
			input.onValueChanged.RemoveListener(FloatLimitForce);
			input.onValueChanged.AddListener(FloatLimitForce);
		}
	}

	public void FloatLimitForce(string text)
	{
		if (numLimit >= 0 && float.TryParse(text, out var result))
		{
			float num = 1f;
			for (int i = 0; i < numLimit; i++)
			{
				num *= 10f;
			}
			result *= num;
			if (result % 1f != 0f)
			{
				result = Mathf.Floor(result);
				text = (result / num).ToString();
			}
			input.text = text;
		}
	}
}
