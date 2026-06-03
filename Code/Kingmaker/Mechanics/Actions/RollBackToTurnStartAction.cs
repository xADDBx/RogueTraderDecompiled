using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Mechanics.Actions;

[TypeId("92a69cf32a6d1f941accb821f4956cbb")]
public class RollBackToTurnStartAction : ContextAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public bool RollBackPosition;

	public bool RollBackHp;

	public bool RollBackTemporaryHp;

	public bool RollBackAp;

	public bool RollBackMp;

	public bool RollBackCooldowns;

	public override string GetCaption()
	{
		return $"Roll back {Unit} to the start of its turn";
	}

	protected override void RunAction()
	{
		UnitRollBackTrackerPart orCreate = Unit.GetValue().GetOrCreate<UnitRollBackTrackerPart>();
		if (orCreate.HasSavedData)
		{
			if (RollBackPosition)
			{
				orCreate.RollBackPosition();
			}
			if (RollBackHp)
			{
				orCreate.RollBackHp();
			}
			if (RollBackTemporaryHp)
			{
				orCreate.RollBackTHP();
			}
			if (RollBackAp)
			{
				orCreate.RollBackAp();
			}
			if (RollBackMp)
			{
				orCreate.RollBackMp();
			}
			if (RollBackCooldowns)
			{
				orCreate.RollBackCooldowns();
			}
		}
	}
}
