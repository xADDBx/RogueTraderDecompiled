using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Code.Blueprints.JsonSystem.EditorDatabase.FileDatabaseClient.PayloadData;

public class PayloadEntryData
{
	[JsonIgnore]
	[NotNull]
	private string m_Id;

	[JsonIgnore]
	[NotNull]
	private string m_Path;

	[JsonIgnore]
	private bool m_IsShadowDeleted;

	[JsonIgnore]
	private bool m_ContainsShadowDeletedBlueprints;

	[JsonProperty("id")]
	public string Id
	{
		get
		{
			return m_Id;
		}
		set
		{
			m_Id = value ?? string.Empty;
		}
	}

	[JsonProperty("path")]
	public string Path
	{
		get
		{
			return m_Path;
		}
		set
		{
			m_Path = value ?? string.Empty;
		}
	}

	[JsonProperty("is_shadow_deleted")]
	public bool? IsShadowDeleted
	{
		get
		{
			return m_IsShadowDeleted;
		}
		set
		{
			m_IsShadowDeleted = value.GetValueOrDefault();
		}
	}

	[JsonProperty("contains_shadow_deleted_blueprints")]
	public bool? ContainsShadowDeletedBlueprints
	{
		get
		{
			return m_ContainsShadowDeletedBlueprints;
		}
		set
		{
			m_ContainsShadowDeletedBlueprints = value.GetValueOrDefault();
		}
	}

	public static PayloadEntryData FromJson(string json)
	{
		return JsonConvert.DeserializeObject<PayloadEntryData>(json);
	}

	public static PayloadEntryData Create(string id, string path, bool isShadowDeleted, bool containsShadowDeletedBlueprints)
	{
		return new PayloadEntryData(id, path, isShadowDeleted, containsShadowDeletedBlueprints);
	}

	public PayloadEntryData()
		: this(string.Empty, string.Empty, isShadowDeleted: false, containsShadowDeletedBlueprints: false)
	{
	}

	public PayloadEntryData(string id, string path, bool isShadowDeleted, bool containsShadowDeletedBlueprints)
	{
		m_Id = id;
		m_Path = path;
		m_IsShadowDeleted = isShadowDeleted;
		m_ContainsShadowDeletedBlueprints = containsShadowDeletedBlueprints;
	}
}
