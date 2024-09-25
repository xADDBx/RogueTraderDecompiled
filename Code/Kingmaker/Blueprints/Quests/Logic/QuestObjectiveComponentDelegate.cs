using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints.Quests.Logic;

[AllowedOn(typeof(BlueprintQuestObjective))]
[TypeId("a175e554d3ec9624f8f4075ca17036dd")]
public abstract class QuestObjectiveComponentDelegate : EntityFactComponentDelegate<QuestBook>, INodeEditorDescriptionProvider, IHashable
{
	protected QuestObjective Objective => (QuestObjective)base.Fact;

	public abstract string GetDescription();

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
