using Newtonsoft.Json;

namespace Code.Blueprints.JsonSystem.EditorDatabase.FileDatabaseClient.PayloadData;

public class PayloadContainsShadowDeletedBlueprintsData
{
	[JsonIgnore]
	private bool m_ContainsShadowDeletedBlueprints;

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

	public static PayloadContainsShadowDeletedBlueprintsData FromJson(string json)
	{
		return JsonConvert.DeserializeObject<PayloadContainsShadowDeletedBlueprintsData>(json);
	}

	public static PayloadContainsShadowDeletedBlueprintsData Create(bool containsShadowDeletedBlueprints)
	{
		return new PayloadContainsShadowDeletedBlueprintsData(containsShadowDeletedBlueprints);
	}

	public PayloadContainsShadowDeletedBlueprintsData()
		: this(containsShadowDeletedBlueprints: false)
	{
	}

	public PayloadContainsShadowDeletedBlueprintsData(bool containsShadowDeletedBlueprints)
	{
		m_ContainsShadowDeletedBlueprints = containsShadowDeletedBlueprints;
	}
}
