using System.Net.NetworkInformation;

namespace Utility;

public static class Network
{
	public static string GetMACAddress()
	{
		string text = "";
		NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
		if (allNetworkInterfaces != null)
		{
			NetworkInterface[] array = allNetworkInterfaces;
			for (int i = 0; i < array.Length; i++)
			{
				PhysicalAddress physicalAddress = array[i].GetPhysicalAddress();
				if (physicalAddress != null && physicalAddress.GetAddressBytes().Length == 6)
				{
					string text2 = physicalAddress.ToString();
					text += text2;
				}
			}
		}
		return text;
	}
}
