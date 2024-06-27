using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Code.Blueprints.JsonSystem.EditorDatabase.FileDatabaseClient.PayloadData;

public class PayloadTypeData
{
	[JsonIgnore]
	[NotNull]
	private string m_Type;

	[JsonProperty("type")]
	public string Type
	{
		get
		{
			return m_Type;
		}
		set
		{
			m_Type = value ?? string.Empty;
		}
	}

	public static PayloadTypeData FromJson(string json)
	{
		return JsonConvert.DeserializeObject<PayloadTypeData>(json);
	}

	public static PayloadTypeData Create(string type)
	{
		return new PayloadTypeData(type);
	}

	public PayloadTypeData()
		: this(string.Empty)
	{
	}

	public PayloadTypeData(string type)
	{
		m_Type = type ?? string.Empty;
	}
}
