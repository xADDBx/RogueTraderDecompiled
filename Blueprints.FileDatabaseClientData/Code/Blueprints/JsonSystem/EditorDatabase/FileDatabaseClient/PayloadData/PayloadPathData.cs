using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Code.Blueprints.JsonSystem.EditorDatabase.FileDatabaseClient.PayloadData;

public class PayloadPathData
{
	[JsonIgnore]
	[NotNull]
	private string m_Path;

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

	public static PayloadPathData FromJson(string json)
	{
		return JsonConvert.DeserializeObject<PayloadPathData>(json);
	}

	public static PayloadPathData Create(string path)
	{
		return new PayloadPathData(path);
	}

	public PayloadPathData()
		: this(string.Empty)
	{
	}

	public PayloadPathData(string path)
	{
		m_Path = path ?? string.Empty;
	}
}
