using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints.Quests.Logic;

[AllowedOn(typeof(BlueprintQuest))]
[TypeId("f12a47c527df9684f81b0d31f42b1b9c")]
public class QuestComponentDelegate : EntityFactComponentDelegate<QuestBook>, IHashable
{
	protected Quest Quest => (Quest)base.Fact;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
