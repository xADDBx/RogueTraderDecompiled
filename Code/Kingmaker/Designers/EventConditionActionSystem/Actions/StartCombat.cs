using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Combat;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("cbc450c881049cb43a3d474b0a176122")]
public class StartCombat : GameAction
{
	[ValidateNotNull]
	[InfoBox(Text = "Unit1 will become enemy of Unit2's Faction")]
	[SerializeReference]
	public AbstractUnitEvaluator Unit1;

	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit2;

	public bool AmbushPlayer;

	public override void RunAction()
	{
		BaseUnitEntity baseUnitEntity = Unit1.GetValue() as BaseUnitEntity;
		if (baseUnitEntity == null)
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {Unit1} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
			return;
		}
		BaseUnitEntity baseUnitEntity2 = Unit2.GetValue() as BaseUnitEntity;
		if (baseUnitEntity2 == null)
		{
			string message2 = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {Unit2} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message2))
			{
				UberDebug.LogError(message2);
			}
			return;
		}
		if (baseUnitEntity.IsDirectlyControllable && baseUnitEntity2.IsDirectlyControllable)
		{
			PFLog.Default.Error("Can't start combat between directly controllable units");
			return;
		}
		if (baseUnitEntity.IsDirectlyControllable)
		{
			BaseUnitEntity baseUnitEntity3 = baseUnitEntity2;
			BaseUnitEntity baseUnitEntity4 = baseUnitEntity;
			baseUnitEntity = baseUnitEntity3;
			baseUnitEntity2 = baseUnitEntity4;
		}
		baseUnitEntity.Faction.AttackFactions.Add(baseUnitEntity2.Faction.Blueprint);
		baseUnitEntity.CombatGroup.UpdateAttackFactionsCache();
		baseUnitEntity2.CombatGroup.UpdateAttackFactionsCache();
		Game.Instance.GetController<UnitCombatJoinController>(includeInactive: true)?.StartScriptedCombat(baseUnitEntity, baseUnitEntity2, AmbushPlayer);
	}

	public override string GetCaption()
	{
		return $"Start combat: {Unit1} vs {Unit2}";
	}
}
