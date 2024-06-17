using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Enums;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class QuestsRoot
{
	[SerializeField]
	private BlueprintQuestGroupsReference m_Groups;

	public IEnumerable<QuestGroup> Groups
	{
		get
		{
			if (!m_Groups.IsEmpty())
			{
				return m_Groups.Get().Groups;
			}
			return Enumerable.Empty<QuestGroup>();
		}
	}

	public QuestGroup GetGroup(QuestGroupId groupId)
	{
		QuestGroup questGroup = Groups.FirstOrDefault((QuestGroup g) => g.Id == groupId);
		if (questGroup == null)
		{
			PFLog.Default.Error("Can't find quest group with id '{0}'", groupId);
			questGroup = new QuestGroup();
		}
		return questGroup;
	}
}
