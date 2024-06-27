using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Code.Blueprints.JsonSystem.EditorDatabase.FileDatabaseClient.PayloadData;

public class PayloadEntryListData
{
	[JsonIgnore]
	[NotNull]
	private List<PayloadEntryData> m_EntryDataList;

	[JsonProperty("entry_data_list")]
	public List<PayloadEntryData> EntryDataList
	{
		get
		{
			return m_EntryDataList;
		}
		set
		{
			m_EntryDataList = value ?? new List<PayloadEntryData>();
		}
	}

	public static PayloadEntryListData FromJson(string json)
	{
		return JsonConvert.DeserializeObject<PayloadEntryListData>(json);
	}

	public static PayloadEntryListData CreateEmpty()
	{
		return new PayloadEntryListData();
	}

	public static PayloadEntryListData Create(IEnumerable<PayloadEntryData> entryDataLists)
	{
		return new PayloadEntryListData(entryDataLists);
	}

	public PayloadEntryListData()
	{
		m_EntryDataList = new List<PayloadEntryData>();
	}

	public PayloadEntryListData(IEnumerable<PayloadEntryData> entryDataList)
	{
		m_EntryDataList = ((entryDataList != null) ? new List<PayloadEntryData>(entryDataList) : new List<PayloadEntryData>());
	}

	public void Add(PayloadEntryData payloadEntryData)
	{
		m_EntryDataList.Add(payloadEntryData);
	}
}
