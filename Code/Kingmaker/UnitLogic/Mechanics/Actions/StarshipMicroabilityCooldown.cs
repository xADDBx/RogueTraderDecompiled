using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Code.UI.MVVM.VM.Bark;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("49ad6acf6b67a114da523f8e353e1c25")]
public class StarshipMicroabilityCooldown : ContextAction
{
	[SerializeField]
	private bool m_UpgradedCooldown;

	[SerializeField]
	private BlueprintFeatureReference m_CostReduction1;

	[SerializeField]
	private BlueprintFeatureReference m_CostReduction2;

	public override string GetCaption()
	{
		return "Start random cooldown for starship microability";
	}

	protected override void RunAction()
	{
		if (!(base.Caster is StarshipEntity))
		{
			Element.LogError(this, "Caster is missing");
			return;
		}
		int num = ((!m_UpgradedCooldown) ? 30 : 0);
		int num2 = (m_UpgradedCooldown ? 40 : 50);
		int num3 = PFStatefulRandom.SpaceCombat.Range(0, 100);
		int num4 = ((num3 < num) ? 2 : ((num3 < num + num2) ? 1 : 0));
		int num5 = 0;
		BlueprintFeatureReference blueprintFeatureReference = null;
		BlueprintFeatureReference blueprintFeatureReference2 = null;
		if (base.Caster.Facts.Contains(m_CostReduction2?.Get()))
		{
			num5 = 50;
			blueprintFeatureReference = m_CostReduction2;
		}
		else if (base.Caster.Facts.Contains(m_CostReduction1?.Get()))
		{
			num5 = 25;
			blueprintFeatureReference = m_CostReduction1;
		}
		if (PFStatefulRandom.SpaceCombat.Range(0, 100) < num5)
		{
			int num6 = Math.Max(num4 - 1, 0);
			if (num6 != num4)
			{
				blueprintFeatureReference2 = blueprintFeatureReference;
			}
			num4 = num6;
		}
		StarshipCompanionsOnPostLogic starshipCompanionsOnPostLogic = base.Caster.Facts.GetComponents<StarshipCompanionsOnPostLogic>().FirstOrDefault();
		if (starshipCompanionsOnPostLogic != null)
		{
			num4 += starshipCompanionsOnPostLogic.AddToAbilityCooldown(base.Caster as StarshipEntity, base.Context.SourceAbility);
		}
		base.Caster.GetAbilityCooldownsOptional()?.StartAutonomousCooldown(base.AbilityContext.AbilityBlueprint, num4 + 1);
		if (blueprintFeatureReference2 == null)
		{
			return;
		}
		using (GameLogContext.Scope)
		{
			GameLogContext.Text = blueprintFeatureReference2.Get().Name;
			CombatLogMessage combatLogMessage = GameLogStrings.Instance.WarhammerStarshipSteadyHandActivation.CreateCombatLogMessage();
			BarkPlayer.Bark(base.Caster, combatLogMessage.Message, 0f).InterruptBark();
		}
	}
}
