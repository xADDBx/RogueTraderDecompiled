using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("ab1a60c2f02348d0bbf5e40fdf12076d")]
public class ContextRepeatRedirect : ContextAction
{
	protected override void RunAction()
	{
		if (base.Caster.GetCommandsOptional() != null)
		{
			PartAbilityRedirect orCreate = base.Caster.GetOrCreate<PartAbilityRedirect>();
			if (!orCreate.LastUsedAbility.IsEmpty)
			{
				AbilityExecutionContext context = new AbilityData(orCreate.LastUsedAbility.Fact).CreateExecutionContext(orCreate.LastUsedAbilityTarget.Entity);
				Game.Instance.AbilityExecutor.Execute(context);
			}
		}
	}

	public override string GetCaption()
	{
		return "Repeats last redirected ability";
	}
}
