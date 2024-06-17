using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View.Covers;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("10c8c11acea64f0b8b1daa0b91d92d68")]
public class ContextConditionIsInCover : ContextCondition
{
	public bool UseBestShootingPosition;

	public LosCalculations.CoverType MinimalCover;

	protected override string GetConditionCaption()
	{
		return "Check if target is in cover";
	}

	protected override bool CheckCondition()
	{
		MechanicEntity maybeCaster = base.Context.MaybeCaster;
		MechanicEntity entity = base.Target.Entity;
		if (maybeCaster == null || entity == null)
		{
			return false;
		}
		LosCalculations.CoverType coverType = ((!UseBestShootingPosition) ? LosCalculations.GetWarhammerLos(maybeCaster, entity).CoverType : LosCalculations.GetWarhammerLos(LosCalculations.GetBestShootingPosition(maybeCaster, entity), maybeCaster.SizeRect, entity.Position, entity.SizeRect).CoverType);
		bool num = MinimalCover == LosCalculations.CoverType.Invisible && coverType.HasFlag(LosCalculations.CoverType.Invisible);
		bool flag = MinimalCover == LosCalculations.CoverType.Full && (coverType.HasFlag(LosCalculations.CoverType.Invisible) || coverType.HasFlag(LosCalculations.CoverType.Full));
		bool flag2 = MinimalCover == LosCalculations.CoverType.Half && (coverType.HasFlag(LosCalculations.CoverType.Invisible) || coverType.HasFlag(LosCalculations.CoverType.Full) || coverType.HasFlag(LosCalculations.CoverType.Half));
		return num || flag || flag2;
	}
}
