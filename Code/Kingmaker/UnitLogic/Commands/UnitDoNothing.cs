using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Visual.Animation.Kingmaker;

namespace Kingmaker.UnitLogic.Commands;

public sealed class UnitDoNothing : UnitCommand<UnitDoNothingParams>
{
	public override bool IsMoveUnit => false;

	public UnitDoNothing(UnitDoNothingParams @params)
		: base(@params)
	{
	}

	protected override void TriggerAnimation()
	{
		StartAnimation(UnitAnimationType.Stunned);
	}

	protected override void OnTick()
	{
		base.OnTick();
		if (base.TimeSinceStart >= base.Params.DoNothingTime)
		{
			ForceFinish(ResultType.Success);
		}
	}

	protected override ResultType OnAction()
	{
		return ResultType.None;
	}
}
