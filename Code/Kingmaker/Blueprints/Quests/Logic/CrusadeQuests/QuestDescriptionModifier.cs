using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints.Quests.Logic.CrusadeQuests;

[AllowedOn(typeof(BlueprintQuest))]
[AllowMultipleComponents]
[TypeId("c544d112d5664976bfa5903c39e8c530")]
public abstract class QuestDescriptionModifier : QuestComponentDelegate, IHashable
{
	public abstract string Modify(string originalString);

	public abstract bool IsComplete();

	public abstract bool IsFailed();

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
