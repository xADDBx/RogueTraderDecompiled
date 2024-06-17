using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Enums;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Combat;

public class UnitMissedTurnLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventUnitMissedTurn>
{
	public void HandleEvent(GameLogEventUnitMissedTurn evt)
	{
		if (!(evt.Actor.Entity is BaseUnitEntity baseUnitEntity))
		{
			return;
		}
		List<FeatureCountableFlag.BuffList.Element> list = baseUnitEntity.GetMechanicFeature(MechanicsFeatureType.CantAct).AssociatedBuffs.Buffs.ToList();
		UnitCondition[] array = new UnitCondition[3]
		{
			UnitCondition.Stunned,
			UnitCondition.Prone,
			UnitCondition.Sleeping
		};
		foreach (UnitCondition condition in array)
		{
			if (!baseUnitEntity.State.TryGetConditionSource(condition, out var result) || result == null)
			{
				continue;
			}
			foreach (EntityFact item in result)
			{
				if (item is Buff buff)
				{
					list.Add(new FeatureCountableFlag.BuffList.Element(buff));
				}
			}
		}
		GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)baseUnitEntity;
		CombatLogMessage combatLogMessage = LogThreadBase.Strings.UnitMissedTurn.CreateCombatLogMessage();
		if (list.Count > 0 && combatLogMessage?.Tooltip is TooltipTemplateCombatLogMessage tooltipTemplateCombatLogMessage)
		{
			tooltipTemplateCombatLogMessage.ExtraTooltipBricks = CollectExtraBricks(list).ToArray();
			tooltipTemplateCombatLogMessage.ExtraInfoBricks = tooltipTemplateCombatLogMessage.ExtraTooltipBricks;
		}
		AddMessage(combatLogMessage);
	}

	private IEnumerable<ITooltipBrick> CollectExtraBricks(List<FeatureCountableFlag.BuffList.Element> buffs)
	{
		yield return new TooltipBrickIconTextValue(LogThreadBase.Strings.TooltipBrickStrings.Reasons.Text, "", 1, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
		foreach (FeatureCountableFlag.BuffList.Element buff in buffs)
		{
			yield return new TooltipBrickTextValue(buff.BuffInformation.Name, "", 1);
		}
	}
}
