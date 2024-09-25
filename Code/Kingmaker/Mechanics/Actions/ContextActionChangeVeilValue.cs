using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace Kingmaker.Mechanics.Actions;

[TypeId("b96d07cc91ca426c9b48bfef6f965788")]
public class ContextActionChangeVeilValue : ContextAction
{
	public ContextValue Value;

	public bool OverrideMinValue;

	public override string GetCaption()
	{
		return $"Changes Veil by {Value}";
	}

	protected override void RunAction()
	{
		Game.Instance.TurnController.VeilThicknessCounter.Value = Rulebook.Trigger(new RuleCalculateVeilCount(Game.Instance.Player.MainCharacterEntity, Value.Calculate(base.Context))
		{
			OverrideMinVeil = OverrideMinValue
		}).Result;
	}
}
