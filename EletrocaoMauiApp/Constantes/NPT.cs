using System.Net.Sockets;

namespace Constantes;

public class NPT
{
	public static async Task<long> GetNtpUnixTimeMs()
	{
		using var client = new UdpClient();
		client.Connect("pool.ntp.br", 123);

		var ntpData = new byte[48];
		ntpData[0] = 0x1B; // NTP request header

		await client.SendAsync(ntpData, ntpData.Length);
		var result = await client.ReceiveAsync();
		ntpData = result.Buffer;

		const byte offsetTransmitTime = 40;

		ulong intPart = BitConverter.ToUInt32(ntpData, offsetTransmitTime);
		ulong fractPart = BitConverter.ToUInt32(ntpData, offsetTransmitTime + 4);

		intPart = SwapEndianness(intPart);
		fractPart = SwapEndianness(fractPart);

		ulong milliseconds = (intPart * 1000UL) + ((fractPart * 1000UL) / 0x100000000UL);

		return (long)(milliseconds - 2208988800000UL);
	}

	static uint SwapEndianness(ulong x)
	{
		return (uint)(
			((x & 0x000000ff) << 24) |
			((x & 0x0000ff00) << 8) |
			((x & 0x00ff0000) >> 8) |
			((x & 0xff000000) >> 24)
		);
	}
}
