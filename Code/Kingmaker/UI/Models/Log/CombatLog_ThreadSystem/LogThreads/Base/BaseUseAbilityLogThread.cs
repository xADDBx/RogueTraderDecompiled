using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.Utility.UnityExtensions;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Base;

public abstract class BaseUseAbilityLogThread : LogThreadBase
{
	protected void HandleUseAbility(AbilityData ability, [CanBeNull] RulePerformAbility rule)
	{
		if (ability.Blueprint.DisableLog || (rule != null && rule.Context.DisableLog))
		{
			return;
		}
		bool flag = !ability.Name.IsNullOrEmpty();
		ItemEntity sourceItem = ability.SourceItem;
		bool flag2 = sourceItem != null && !sourceItem.Name.IsNullOrEmpty() && sourceItem.Name != ability.Name;
		if (!flag && !flag2)
		{
			return;
		}
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			if (rule != null && rule.Context.ExecutionFromPsychicPhenomena)
			{
				GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)rule.Context.Caster;
				GameLogContext.Text = rule.Context.AbilityBlueprint.Name;
				CombatLogMessage newMessage = LogThreadBase.Strings.PerilsOfTheWarp.CreateCombatLogMessage();
				AddMessage(newMessage);
				return;
			}
			MechanicEntity mechanicEntity = rule?.ConcreteInitiator ?? ability.Caster;
			MechanicEntity mechanicEntity2 = rule?.SpellTarget.Entity;
			GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)mechanicEntity;
			GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)((mechanicEntity2 == mechanicEntity) ? null : mechanicEntity2);
			GameLogContext.Text = ((flag && flag2) ? (sourceItem.Name + " / " + ability.Name) : (flag ? ability.Name : sourceItem.Name));
			GameLogContext.Description = ability.Description;
			if (sourceItem != null)
			{
				ItemEntity itemEntity = ItemsEntityFactory.CreateItemCopy(ability.SourceItem, 1);
				GameLogContext.Tooltip = itemEntity;
				CombatLogMessage message = LogThreadBase.Strings.UseItem.CreateCombatLogMessage();
				AddMessage(new CombatLogMessage(message, new TooltipTemplateItem(itemEntity), hasTooltip: true, mechanicEntity));
			}
			else
			{
				GameLogContext.Tooltip = ability;
				CombatLogMessage message2 = ((GameLogContext.TargetEntity.Value != null) ? LogThreadBase.Strings.UseAbilityOnTarget : LogThreadBase.Strings.UseAbility).CreateCombatLogMessage();
				AddMessage(new CombatLogMessage(message2, new TooltipTemplateAbility(ability), hasTooltip: true, mechanicEntity));
			}
			LogThreadBase.IsPreviousMessageUseSomething = true;
		}
	}
}
