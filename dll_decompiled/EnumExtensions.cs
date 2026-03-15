using System;

public static class EnumExtensions
{
	public static bool HasFlag(this Enum self, Enum flag)
	{
		if (self.GetType() != flag.GetType())
		{
			throw new ArgumentException("flag の型が、現在のインスタンスの型と異なっています。");
		}
		ulong num = Convert.ToUInt64(self);
		ulong num2 = Convert.ToUInt64(flag);
		return (num & num2) == num2;
	}
}
