using System;

namespace Kingmaker.Blueprints.Quests;

[Flags]
public enum QuestObjectiveReferenceType
{
	None = 0,
	CheckStatus = 1,
	Give = 2,
	Complete = 4,
	Fail = 8
}
