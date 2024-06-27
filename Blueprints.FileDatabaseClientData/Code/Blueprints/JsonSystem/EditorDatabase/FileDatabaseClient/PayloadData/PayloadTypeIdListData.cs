using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Code.Blueprints.JsonSystem.EditorDatabase.FileDatabaseClient.PayloadData;

public class PayloadTypeIdListData
{
	[JsonIgnore]
	[NotNull]
	private List<string> m_TypeIdList;

	[JsonProperty("type_id_list")]
	public List<string> TypeIdList
	{
		get
		{
			return m_TypeIdList;
		}
		set
		{
			m_TypeIdList = value ?? new List<string>();
		}
	}

	public static PayloadTypeIdListData FromJson(string json)
	{
		return JsonConvert.DeserializeObject<PayloadTypeIdListData>(json);
	}

	public static PayloadTypeIdListData CreateEmpty()
	{
		return new PayloadTypeIdListData();
	}

	public static PayloadTypeIdListData Create(IEnumerable<string> typeIdList)
	{
		return new PayloadTypeIdListData(typeIdList);
	}

	public PayloadTypeIdListData()
	{
		m_TypeIdList = new List<string>();
	}

	public PayloadTypeIdListData(IEnumerable<string> typeIdList)
	{
		m_TypeIdList = ((typeIdList != null) ? new List<string>(typeIdList) : new List<string>());
	}

	public void Add(string typeId)
	{
		m_TypeIdList.Add(typeId);
	}
}
