using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.Networking;

namespace Kingmaker.Utility.UnityExtensions;

public class WebSender
{
	public static IEnumerable Send(WebSenderEntry[] Entries, string Address, string FileName = "data.zip", string FieldName = "upload")
	{
		using MemoryStream stream = Compress(Entries);
		WWWForm wWWForm = new WWWForm();
		wWWForm.AddField("line", Environment.CommandLine);
		wWWForm.AddBinaryData(FieldName, stream.ToArray(), FileName);
		UnityWebRequest unityWebRequest = UnityWebRequest.Post(Address, wWWForm);
		yield return unityWebRequest.SendWebRequest();
	}

	public static void Save(WebSenderEntry[] Entries, string FileName = "data.zip")
	{
		using MemoryStream memoryStream = Compress(Entries);
		File.WriteAllBytes(FileName, memoryStream.ToArray());
	}

	private static MemoryStream Compress(WebSenderEntry[] entries)
	{
		MemoryStream memoryStream = new MemoryStream();
		using ZipArchive zipArchive = new ZipArchive(memoryStream);
		foreach (WebSenderEntry webSenderEntry in entries)
		{
			using Stream stream = zipArchive.CreateEntry(webSenderEntry.Name).Open();
			stream.Write(webSenderEntry.Data);
		}
		return memoryStream;
	}
}
