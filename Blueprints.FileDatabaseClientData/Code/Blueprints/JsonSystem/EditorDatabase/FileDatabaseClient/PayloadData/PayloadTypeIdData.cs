using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Code.Blueprints.JsonSystem.EditorDatabase.FileDatabaseClient.PayloadData;

public class PayloadTypeIdData
{
	[JsonIgnore]
	[NotNull]
	private string m_TypeId;

	[JsonProperty("type_id")]
	public string TypeId
	{
		get
		{
			return m_TypeId;
		}
		set
		{
			m_TypeId = value ?? string.Empty;
		}
	}

	public static PayloadTypeIdData FromJson(string json)
	{
		return JsonConvert.DeserializeObject<PayloadTypeIdData>(json);
	}

	public static PayloadTypeIdData Create(string typeId)
	{
		return new PayloadTypeIdData(typeId);
	}

	public PayloadTypeIdData()
		: this(string.Empty)
	{
	}

	public PayloadTypeIdData(string typeId)
	{
		m_TypeId = typeId ?? string.Empty;
	}
}
