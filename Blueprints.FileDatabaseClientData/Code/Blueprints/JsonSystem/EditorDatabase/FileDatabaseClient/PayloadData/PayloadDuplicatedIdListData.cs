using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Code.Blueprints.JsonSystem.EditorDatabase.FileDatabaseClient.PayloadData;

public class PayloadDuplicatedIdListData
{
	[JsonIgnore]
	[NotNull]
	private List<string> m_DuplicatedIdList;

	[JsonProperty("duplicated_Id_list")]
	public List<string> DuplicatedIdList
	{
		get
		{
			return m_DuplicatedIdList;
		}
		set
		{
			m_DuplicatedIdList = value ?? new List<string>();
		}
	}

	public static PayloadDuplicatedIdListData FromJson(string json)
	{
		return JsonConvert.DeserializeObject<PayloadDuplicatedIdListData>(json);
	}

	public static PayloadDuplicatedIdListData CreateEmpty()
	{
		return new PayloadDuplicatedIdListData();
	}

	public static PayloadDuplicatedIdListData Create(IEnumerable<string> duplicatedIdList)
	{
		return new PayloadDuplicatedIdListData(duplicatedIdList);
	}

	public PayloadDuplicatedIdListData()
	{
		m_DuplicatedIdList = new List<string>();
	}

	public PayloadDuplicatedIdListData(IEnumerable<string> nameList)
	{
		m_DuplicatedIdList = ((nameList != null) ? new List<string>(nameList) : new List<string>());
	}

	public void Add(string name)
	{
		m_DuplicatedIdList.Add(name);
	}
}
