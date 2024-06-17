using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kingmaker.EntitySystem.Persistence.Versioning;

public static class JsonUpgradeSystem
{
	private readonly struct UpgraderEntry
	{
		public readonly int Version;

		[NotNull]
		public readonly string Description;

		[NotNull]
		public readonly IJsonUpgrader Upgrader;

		public UpgraderEntry(int version, string description, [NotNull] IJsonUpgrader upgrader)
		{
			Version = version;
			Description = description;
			Upgrader = upgrader;
		}
	}

	private static readonly List<UpgraderEntry> Updaters;

	private static readonly object Lock;

	public static List<int> KnownVersions => Updaters.Select((UpgraderEntry u) => u.Version).ToList();

	static JsonUpgradeSystem()
	{
		Updaters = new List<UpgraderEntry>();
		Lock = new object();
	}

	internal static void Register(int version, string description, IJsonUpgrader upgrader)
	{
		Updaters.Add(new UpgraderEntry(version, description, upgrader));
	}

	public static bool ShouldPrioritisePlayer(SaveInfo saveInfo)
	{
		return GetUpgraders(saveInfo).Any((UpgraderEntry u) => u.Upgrader.NeedsPlayerPriorityLoad);
	}

	public static bool ShouldUpgrade(SaveInfo saveInfo, string fileName)
	{
		return GetUpgraders(saveInfo, fileName).Count > 0;
	}

	private static IEnumerable<UpgraderEntry> GetUpgraders(SaveInfo saveInfo)
	{
		HashSet<int> saveVersionsSet = new HashSet<int>(saveInfo.Versions);
		List<int> list = Updaters.Select((UpgraderEntry u) => u.Version).ToList();
		HashSet<int> hashSet = new HashSet<int>(list);
		foreach (int version in saveInfo.Versions)
		{
			if (!hashSet.Contains(version))
			{
				throw new JsonUpgradeException($"Unknown version in save info: {version}" + $"\nSave versions: {saveInfo.Versions}" + $"\nKnown versions: {list}");
			}
		}
		return Updaters.Where((UpgraderEntry u) => !saveVersionsSet.Contains(u.Version));
	}

	private static List<UpgraderEntry> GetUpgraders(SaveInfo saveInfo, string fileName)
	{
		return (from u in GetUpgraders(saveInfo)
			where AffectsFile(u, fileName)
			select u).ToList();
	}

	[NotNull]
	public static string Upgrade(SaveInfo saveInfo, string fileName, [NotNull] string json)
	{
		lock (Lock)
		{
			List<UpgraderEntry> upgraders = GetUpgraders(saveInfo, fileName);
			if (upgraders.Count <= 0)
			{
				return json;
			}
			PFLog.System.Log("Applying upgraders to " + fileName);
			JObject jObject = JObject.Parse(json);
			foreach (UpgraderEntry item in upgraders)
			{
				try
				{
					item.Upgrader.Upgrade(jObject);
					PFLog.System.Log($"Applied upgrader {item.Version} ({item.Description}) to {fileName}");
				}
				catch (Exception ex)
				{
					PFLog.Default.Exception(ex);
					throw new JsonUpgradeException($"Exception during applying upgrader {item.Version} ({item.Description}) to {fileName}", ex);
				}
			}
			string result = jObject.ToString(Formatting.None);
			GC.Collect();
			return result;
		}
	}

	private static bool AffectsFile(UpgraderEntry upgraderEntry, string fileName)
	{
		return upgraderEntry.Upgrader.WillUpgrade(fileName);
	}
}
