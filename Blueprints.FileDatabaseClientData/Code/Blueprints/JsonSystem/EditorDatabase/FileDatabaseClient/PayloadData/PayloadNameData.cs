using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Code.Blueprints.JsonSystem.EditorDatabase.FileDatabaseClient.PayloadData;

public class PayloadNameData
{
	[JsonIgnore]
	[NotNull]
	private string m_Name;

	[JsonProperty("name")]
	public string Name
	{
		get
		{
			return m_Name;
		}
		set
		{
			m_Name = value ?? string.Empty;
		}
	}

	public static PayloadNameData FromJson(string json)
	{
		return JsonConvert.DeserializeObject<PayloadNameData>(json);
	}

	public static PayloadNameData Create(string name)
	{
		return new PayloadNameData(name);
	}

	public PayloadNameData()
		: this(string.Empty)
	{
	}

	public PayloadNameData(string name)
	{
		m_Name = name;
	}
}
