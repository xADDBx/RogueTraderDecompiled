using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Quests.Logic;

[AllowedOn(typeof(BlueprintQuest))]
[TypeId("6aa1a615a26ec9f48a284357c85d35f9")]
public class QuestRelatesToCompanionStory : BlueprintComponent
{
	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("Companion")]
	private BlueprintUnitReference m_Companion;

	public BlueprintUnit Companion => m_Companion?.Get();
}
