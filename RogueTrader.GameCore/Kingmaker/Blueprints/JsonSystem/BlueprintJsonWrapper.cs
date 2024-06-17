using System;
using System.IO;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Kingmaker.Utility.CodeTimer;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.Blueprints.JsonSystem;

[Serializable]
public class BlueprintJsonWrapper
{
	public string AssetId;

	public SimpleBlueprint Data;

	[JsonIgnore]
	public string LoadedFromPath;

	[JsonIgnore]
	public bool IsDirty { get; set; }

	[JsonIgnore]
	public bool IsChangedOnDisk { get; set; }

	public BlueprintJsonWrapper()
	{
	}

	public BlueprintJsonWrapper(SimpleBlueprint bp)
	{
		Data = bp;
		AssetId = bp.AssetGuid;
	}

	[OnDeserializing]
	internal void OnDeserializing(StreamingContext context)
	{
		Json.BlueprintBeingRead = this;
	}

	public void Save(string path)
	{
		using (CodeTimer.New("Saving blueprint"))
		{
			Data.name = Path.GetFileNameWithoutExtension(path);
			AssetId = Data.AssetGuid;
			using (FileStream stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				using StreamWriter streamWriter = new StreamWriter(stream);
				Json.Serializer.Serialize(streamWriter, this);
				streamWriter.Flush();
			}
			IsDirty = false;
		}
	}

	public static BlueprintJsonWrapper Load(string path, [CanBeNull] string idHint = null)
	{
		BlueprintJsonWrapper blueprintJsonWrapper = null;
		using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
		{
			using StreamReader reader = new StreamReader(stream);
			using JsonTextReader reader2 = new JsonTextReader(reader);
			try
			{
				blueprintJsonWrapper = Json.Serializer.Deserialize<BlueprintJsonWrapper>(reader2);
			}
			catch (Exception ex)
			{
				PFLog.System.Exception(ex);
				if (Application.isEditor)
				{
					blueprintJsonWrapper = new BlueprintJsonWrapper
					{
						AssetId = (idHint ?? Guid.NewGuid().ToString()),
						Data = new BlueprintBroken
						{
							Exception = ex
						}
					};
				}
			}
		}
		if (blueprintJsonWrapper?.Data != null)
		{
			blueprintJsonWrapper.Data.name = ((blueprintJsonWrapper.Data is BlueprintBroken) ? ("[BROKEN]" + Path.GetFileNameWithoutExtension(path)) : Path.GetFileNameWithoutExtension(path));
			blueprintJsonWrapper.Data.AssetGuid = blueprintJsonWrapper.AssetId;
			blueprintJsonWrapper.LoadedFromPath = path;
		}
		return blueprintJsonWrapper;
	}
}
