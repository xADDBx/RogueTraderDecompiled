using Code.GameCore.Blueprints;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Utility.GuidUtility;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[AllowMultipleComponents]
[TypeId("4ba3c72bb22d4da7857a6fbcdfd82f46")]
[PlayerUpgraderAllowed(true)]
public class SwitchFaction : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Faction")]
	private BlueprintFactionReference m_Faction;

	public bool IncludeGroup = true;

	public BlueprintFaction Faction => m_Faction?.Get();

	protected override void RunAction()
	{
		if (!(Target.GetValue() is BaseUnitEntity { CombatGroup: var combatGroup } baseUnitEntity))
		{
			return;
		}
		if (combatGroup.Count > 1 && !IncludeGroup)
		{
			baseUnitEntity.CombatGroup.Id = Uuid.Instance.CreateString();
			combatGroup = baseUnitEntity.CombatGroup;
		}
		for (int i = 0; i < combatGroup.Count; i++)
		{
			BaseUnitEntity baseUnitEntity2 = combatGroup[i];
			if (baseUnitEntity2 != null && baseUnitEntity2.Faction.Blueprint != Faction)
			{
				baseUnitEntity2.Faction.Set(Faction);
			}
		}
		combatGroup.ResetFactionSet();
	}

	public override string GetCaption()
	{
		return $"Switch {Target} faction to {Faction.NameSafe()}";
	}
}
