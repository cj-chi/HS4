using System;
using System.IO;
using System.Text;
using Ionic.Zip;
using Ionic.Zlib;

public class ZipAssist
{
	public float progress { get; private set; }

	public int cntSaved { get; private set; }

	public int cntTotal { get; private set; }

	public ZipAssist()
	{
		progress = 0f;
		cntSaved = 0;
		cntTotal = 0;
	}

	public void SaveProgress(object sender, SaveProgressEventArgs e)
	{
	}

	public byte[] SaveUnzipFile(byte[] srcBytes, EventHandler<SaveProgressEventArgs> callBack = null)
	{
		byte[] result = null;
		try
		{
			using MemoryStream zipStream = new MemoryStream(srcBytes);
			ReadOptions options = new ReadOptions
			{
				Encoding = Encoding.GetEncoding("shift_jis")
			};
			using ZipFile zipFile = ZipFile.Read(zipStream, options);
			ZipEntry zipEntry = zipFile[0];
			using MemoryStream memoryStream = new MemoryStream();
			zipEntry.Extract(memoryStream);
			result = memoryStream.ToArray();
		}
		catch (Exception)
		{
		}
		return result;
	}

	public byte[] SaveZipBytes(byte[] srcBytes, string entryName, EventHandler<SaveProgressEventArgs> callBack = null)
	{
		byte[] result = null;
		try
		{
			using ZipFile zipFile = new ZipFile(Encoding.GetEncoding("shift_jis"));
			if (callBack != null)
			{
				zipFile.SaveProgress += callBack;
			}
			else
			{
				zipFile.SaveProgress += SaveProgress;
			}
			zipFile.AlternateEncodingUsage = ZipOption.Always;
			zipFile.CompressionLevel = CompressionLevel.BestCompression;
			zipFile.AddEntry(entryName, srcBytes);
			long num = srcBytes.Length;
			using MemoryStream memoryStream = new MemoryStream();
			if (num % 65536 == 0L)
			{
				zipFile.ParallelDeflateThreshold = -1L;
			}
			zipFile.Save(memoryStream);
			result = memoryStream.ToArray();
		}
		catch (Exception)
		{
		}
		return result;
	}
}
