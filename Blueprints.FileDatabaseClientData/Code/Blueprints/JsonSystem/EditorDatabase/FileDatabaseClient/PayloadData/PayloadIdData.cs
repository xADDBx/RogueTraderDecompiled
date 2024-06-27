using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Code.Blueprints.JsonSystem.EditorDatabase.FileDatabaseClient.PayloadData;

public class PayloadIdData
{
	[JsonIgnore]
	[NotNull]
	private string m_Id;

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

	public static PayloadIdData FromJson(string json)
	{
		return JsonConvert.DeserializeObject<PayloadIdData>(json);
	}

	public static PayloadIdData Create(string id)
	{
		return new PayloadIdData(id);
	}

	public PayloadIdData()
		: this(string.Empty)
	{
	}

	public PayloadIdData(string id)
	{
		m_Id = id ?? string.Empty;
	}
}
