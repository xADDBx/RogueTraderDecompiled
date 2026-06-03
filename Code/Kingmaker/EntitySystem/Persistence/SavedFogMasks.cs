using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;

namespace Kingmaker.EntitySystem.Persistence;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class SavedFogMasks
{
	private static Dictionary<string, SavedFogMasks> KnownAreas = new Dictionary<string, SavedFogMasks>();

	private readonly HashSet<string> m_KnownMasks = new HashSet<string>();

	public static SavedFogMasks[] AllMasks
	{
		get
		{
			return KnownAreas.Values.ToArray();
		}
		set
		{
			KnownAreas = value.ToDictionary((SavedFogMasks v) => v.AreaGuid, (SavedFogMasks v) => v);
		}
	}

	[JsonProperty]
	private string AreaGuid { get; }

	[JsonProperty]
	private List<string> SavedFogOfWarMasks
	{
		get
		{
			return m_KnownMasks.ToList();
		}
		set
		{
			m_KnownMasks.Clear();
			m_KnownMasks.AddRange(value);
		}
	}

	public static SavedFogMasks Get(string areaId)
	{
		if (KnownAreas.TryGetValue(areaId, out var value))
		{
			return value;
		}
		value = new SavedFogMasks(areaId);
		KnownAreas.Add(areaId, value);
		return value;
	}

	[JsonConstructor]
	private SavedFogMasks(string areaGuid)
	{
		AreaGuid = areaGuid;
	}

	public IEnumerable<string> EnumerateFogMasks()
	{
		return m_KnownMasks.Select((string mask) => GenerateFowMaskFileName(AreaGuid, mask));
	}

	public void Wipe()
	{
		foreach (string item in EnumerateFogMasks())
		{
			File.Delete(item);
		}
		m_KnownMasks.Clear();
	}

	private static string GenerateFowMaskFileName(string areaGuid, string sceneName)
	{
		string text = AreaDataStash.Encode(areaGuid + "." + sceneName);
		return Path.Combine(AreaDataStash.Folder, text + ".fog");
	}

	public async Task Save(string sceneName, byte[] data)
	{
		m_KnownMasks.Add(sceneName);
		await File.WriteAllBytesAsync(GenerateFowMaskFileName(AreaGuid, sceneName), data);
	}

	public async Task<byte[]> Load(string sceneName)
	{
		string path = GenerateFowMaskFileName(AreaGuid, sceneName);
		if (!File.Exists(path))
		{
			return null;
		}
		m_KnownMasks.Add(sceneName);
		return await File.ReadAllBytesAsync(path);
	}

	public void Register(string sceneName)
	{
		m_KnownMasks.Add(sceneName);
	}
}
