namespace UnityEx.Misc;

public static class StringExtension
{
	public static bool IsNullOrWhiteSpace(this string source)
	{
		if (source != null)
		{
			return source.Trim() == "";
		}
		return true;
	}

	public static bool IsChar2Byte(char c)
	{
		if (c < '\0' || c >= '\u0081')
		{
			switch (c)
			{
			default:
				if (c >= '\uf8f1')
				{
					return c >= '\uf8f4';
				}
				return true;
			case '\uf8f0':
			case '｡':
			case '｢':
			case '｣':
			case '､':
			case '･':
			case 'ｦ':
			case 'ｧ':
			case 'ｨ':
			case 'ｩ':
			case 'ｪ':
			case 'ｫ':
			case 'ｬ':
			case 'ｭ':
			case 'ｮ':
			case 'ｯ':
			case 'ｰ':
			case 'ｱ':
			case 'ｲ':
			case 'ｳ':
			case 'ｴ':
			case 'ｵ':
			case 'ｶ':
			case 'ｷ':
			case 'ｸ':
			case 'ｹ':
			case 'ｺ':
			case 'ｻ':
			case 'ｼ':
			case 'ｽ':
			case 'ｾ':
			case 'ｿ':
			case 'ﾀ':
			case 'ﾁ':
			case 'ﾂ':
			case 'ﾃ':
			case 'ﾄ':
			case 'ﾅ':
			case 'ﾆ':
			case 'ﾇ':
			case 'ﾈ':
			case 'ﾉ':
			case 'ﾊ':
			case 'ﾋ':
			case 'ﾌ':
			case 'ﾍ':
			case 'ﾎ':
			case 'ﾏ':
			case 'ﾐ':
			case 'ﾑ':
			case 'ﾒ':
			case 'ﾓ':
			case 'ﾔ':
			case 'ﾕ':
			case 'ﾖ':
			case 'ﾗ':
			case 'ﾘ':
			case 'ﾙ':
			case 'ﾚ':
			case 'ﾛ':
			case 'ﾜ':
			case 'ﾝ':
			case 'ﾞ':
			case 'ﾟ':
				break;
			}
		}
		return false;
	}

	public static int ByteCount(this string source)
	{
		int num = 0;
		for (int i = 0; i < source.Length; i++)
		{
			if (IsChar2Byte(source[i]))
			{
				num++;
			}
			num++;
		}
		return num;
	}
}
