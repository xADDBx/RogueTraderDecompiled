using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;

namespace Kingmaker.Controllers.Units;

public class UnitForcedTargetController : BaseUnitController
{
	protected override void TickOnUnit(AbstractUnitEntity entity)
	{
		if (!(entity is BaseUnitEntity baseUnitEntity))
		{
			return;
		}
		UnitPartForcedTarget optional = baseUnitEntity.GetOptional<UnitPartForcedTarget>();
		BaseUnitEntity baseUnitEntity2 = optional?.Unit;
		if (baseUnitEntity2 == null)
		{
			return;
		}
		UnitCommandHandle cmdHandle = optional.CmdHandle;
		if (baseUnitEntity.IsDirectlyControllable)
		{
			cmdHandle?.Interrupt();
			optional.CmdHandle = null;
		}
		if (cmdHandle == null || !cmdHandle.IsFinished || !(cmdHandle.Target == baseUnitEntity2))
		{
			Ability ability = baseUnitEntity.Abilities.GetAbility(BlueprintRoot.Instance.SystemMechanics.ChargeAbility);
			if (ability != null && ability.Data.IsAvailable && ability.Data.CanTarget(baseUnitEntity2))
			{
				UnitUseAbilityParams cmdParams = new UnitUseAbilityParams(ability.Data, baseUnitEntity2);
				optional.CmdHandle = baseUnitEntity.Commands.Run(cmdParams);
			}
		}
	}
}
