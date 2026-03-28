using System;
using System.Collections;
using Ionic.Zip;
using UniRx;

public class FileZip
{
	private ZipAssist zipAssist = new ZipAssist();

	public void ZipSaveProgress(object sender, SaveProgressEventArgs e)
	{
		_ = e.EventType;
		_ = 9;
	}

	public IEnumerator FileZipCor(IObserver<byte[]> observer, byte[] srcBytes, string entryName)
	{
		byte[] data = null;
		yield return Observable.Start(() => zipAssist.SaveZipBytes(srcBytes, entryName, ZipSaveProgress)).StartAsCoroutine(delegate(byte[] x)
		{
			data = x;
		});
		if (data == null)
		{
			observer.OnError(new Exception("圧縮処理に失敗しました。"));
			yield break;
		}
		observer.OnNext(data);
		observer.OnCompleted();
	}

	public IEnumerator FileUnzipCor(IObserver<byte[]> observer, byte[] srcBytes)
	{
		byte[] data = null;
		yield return Observable.Start(() => zipAssist.SaveUnzipFile(srcBytes, ZipSaveProgress)).StartAsCoroutine(delegate(byte[] x)
		{
			data = x;
		});
		if (data == null)
		{
			observer.OnError(new Exception("解凍処理に失敗しました。"));
			yield break;
		}
		observer.OnNext(data);
		observer.OnCompleted();
	}
}
