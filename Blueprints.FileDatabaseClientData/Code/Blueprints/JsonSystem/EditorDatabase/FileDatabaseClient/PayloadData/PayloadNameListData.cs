using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Code.Blueprints.JsonSystem.EditorDatabase.FileDatabaseClient.PayloadData;

public class PayloadNameListData
{
	[JsonIgnore]
	[NotNull]
	private List<string> m_NameList;

	[JsonProperty("name_list")]
	public List<string> NameList
	{
		get
		{
			return m_NameList;
		}
		set
		{
			m_NameList = value ?? new List<string>();
		}
	}

	public static PayloadNameListData FromJson(string json)
	{
		return JsonConvert.DeserializeObject<PayloadNameListData>(json);
	}

	public static PayloadNameListData CreateEmpty()
	{
		return new PayloadNameListData();
	}

	public static PayloadNameListData Create(IEnumerable<string> nameList)
	{
		return new PayloadNameListData(nameList);
	}

	public PayloadNameListData()
	{
		m_NameList = new List<string>();
	}

	public PayloadNameListData(IEnumerable<string> nameList)
	{
		m_NameList = ((nameList != null) ? new List<string>(nameList) : new List<string>());
	}

	public void Add(string name)
	{
		m_NameList.Add(name);
	}
}
