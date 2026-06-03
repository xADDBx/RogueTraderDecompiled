using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Combat;

public class AbilityImmunityLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventImmuneToAbility>
{
	public void HandleEvent(GameLogEventImmuneToAbility evt)
	{
		if (evt.Target.Entity is BaseUnitEntity baseUnitEntity && baseUnitEntity.HasAbilityImmunity(evt.Ability))
		{
			GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)evt.Target.ToBaseUnitEntity();
			CombatLogMessage message = LogThreadBase.Strings.ImmuneToAbility.CreateCombatLogMessage();
			AddMessage(new CombatLogMessage(message, new TooltipTemplateAbility(evt.Ability), hasTooltip: true, baseUnitEntity));
		}
	}
}
