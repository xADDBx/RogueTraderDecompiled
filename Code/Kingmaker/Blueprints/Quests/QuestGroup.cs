using System;
using Kingmaker.Enums;
using Kingmaker.Localization;
using Newtonsoft.Json;

namespace Kingmaker.Blueprints.Quests;

[Serializable]
public class QuestGroup
{
	public QuestGroupId Id;

	public LocalizedString Name;

	public int Order;

	[JsonProperty]
	private bool m_IsCollapse;

	public bool IsCollapse
	{
		get
		{
			return m_IsCollapse;
		}
		set
		{
			m_IsCollapse = value;
		}
	}
}
