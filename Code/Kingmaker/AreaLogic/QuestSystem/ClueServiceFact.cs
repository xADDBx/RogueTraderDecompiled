using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Quests;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.QuestSystem;

public class ClueServiceFact : EntityFact<QuestBook>, IHashable
{
	public ClueServiceFact(BlueprintQuestObjective blueprintQuestObjective)
		: base((BlueprintFact)blueprintQuestObjective)
	{
		Activate();
	}

	protected ClueServiceFact(JsonConstructorMark _)
	{
		Activate();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
