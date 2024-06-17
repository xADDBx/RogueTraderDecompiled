using System;

namespace Kingmaker.Blueprints.Quests;

[Flags]
public enum QuestReferenceType
{
	None = 0,
	CheckStatus = 1,
	Complete = 2
}
