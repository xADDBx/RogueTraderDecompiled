using Newtonsoft.Json;

namespace Code.Blueprints.JsonSystem.EditorDatabase.FileDatabaseClient.PayloadData;

public class PayloadIsShadowDeletedData
{
	[JsonIgnore]
	private bool m_IsShadowDeleted;

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

	public static PayloadIsShadowDeletedData FromJson(string json)
	{
		return JsonConvert.DeserializeObject<PayloadIsShadowDeletedData>(json);
	}

	public static PayloadIsShadowDeletedData Create(bool isShadowDeleted)
	{
		return new PayloadIsShadowDeletedData(isShadowDeleted);
	}

	public PayloadIsShadowDeletedData()
		: this(isShadowDeleted: false)
	{
	}

	public PayloadIsShadowDeletedData(bool isShadowDeleted)
	{
		m_IsShadowDeleted = isShadowDeleted;
	}
}
