using System;
using System.IO;
using Kingmaker.Networking.Hash;
using Kingmaker.Networking.Serialization;
using Kingmaker.Utility.Serialization;
using Kingmaker.Utility.UnityExtensions;
using Newtonsoft.Json;

namespace Kingmaker.Networking.Desync;

public class SaveToFolderDesyncHandler : IDesyncHandler
{
	private static JsonSerializer JsonSerializer => GameStateJsonSerializer.Serializer;

	public void RaiseDesync(HashableState data, DesyncMeta meta)
	{
		string contents = JsonSerializer.SerializeObject(data);
		string text = Path.Combine(ApplicationPaths.persistentDataPath, "Net", "Desync");
		Directory.CreateDirectory(text);
		string text2 = Path.Combine(text, $"{DateTime.Now.ToFileTime()}_{meta.Tick}.json");
		File.WriteAllText(text2, contents);
		PFLog.Net.Error("[Desync] Desynced state saved to file: '" + text2 + "'");
	}
}
