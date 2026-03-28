namespace KK_Lib;

public class KK_Lib
{
	public static AssetBundleLoadAssetOperation LoadFile<Type>(string _assetBundleName, string _assetName, string _manifest = "")
	{
		if (!AssetBundleCheck.IsFile(_assetBundleName, _assetName))
		{
			_ = "読み込みエラー\r\nassetBundleName：" + _assetBundleName + "\tassetName：" + _assetName;
			return null;
		}
		AssetBundleLoadAssetOperation assetBundleLoadAssetOperation = AssetBundleManager.LoadAsset(_assetBundleName, _assetName, typeof(Type), _manifest.IsNullOrEmpty() ? null : _manifest);
		if (assetBundleLoadAssetOperation == null)
		{
			return null;
		}
		return assetBundleLoadAssetOperation;
	}

	public static bool Range(int _value, int _min, int _max)
	{
		if (_min > _value || _value > _max)
		{
			return false;
		}
		return true;
	}

	public static bool Range(float _value, float _min, float _max)
	{
		if (!(_min <= _value) || !(_value <= _max))
		{
			return false;
		}
		return true;
	}

	public static float GCD(float _a, float _b)
	{
		if (_a == 0f || _b == 0f)
		{
			return 0f;
		}
		while (_a != _b)
		{
			if (_a > _b)
			{
				_a -= _b;
			}
			else
			{
				_b -= _a;
			}
		}
		return _a;
	}

	public static float LCM(float _a, float _b)
	{
		if (_a != 0f && _b != 0f)
		{
			return _a / GCD(_a, _b) * _b;
		}
		return 0f;
	}

	public static void Ratio(ref float _outA, ref float _outB, float _a, float _b)
	{
		float num = GCD(_a, _b);
		_outA = _a / num;
		_outB = _b / num;
	}

	public static int Search(int _value)
	{
		if (_value < 2)
		{
			return 0;
		}
		int num = 1;
		for (int i = 2; i < _value; i++)
		{
			int num2 = 0;
			for (int j = 2; j <= i; j++)
			{
				if (i % j <= 0)
				{
					num2++;
				}
			}
			if (num2 == 1)
			{
				num++;
			}
		}
		return num;
	}
}
