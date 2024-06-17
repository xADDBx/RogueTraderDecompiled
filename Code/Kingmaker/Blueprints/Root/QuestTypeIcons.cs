using System;
using Kingmaker.Enums;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class QuestTypeIcons
{
	public Sprite QuestPaperIcon;

	public Sprite RumourPaperIcon;

	public Sprite OrderPaperIcon;

	public Sprite QuestMonitorIcon;

	public Sprite RumourMonitorIcon;

	public Sprite OrderMonitorIcon;

	public Sprite GetQuestPaperTypeIcon(QuestType type)
	{
		switch (type)
		{
		case QuestType.Rumour:
		case QuestType.RumourAboutUs:
			return RumourPaperIcon;
		case QuestType.Order:
			return OrderPaperIcon;
		default:
			return QuestPaperIcon;
		}
	}

	public Sprite GetQuestMonitorTypeIcon(QuestType type)
	{
		switch (type)
		{
		case QuestType.Rumour:
		case QuestType.RumourAboutUs:
			return RumourMonitorIcon;
		case QuestType.Order:
			return OrderMonitorIcon;
		default:
			return QuestMonitorIcon;
		}
	}
}
