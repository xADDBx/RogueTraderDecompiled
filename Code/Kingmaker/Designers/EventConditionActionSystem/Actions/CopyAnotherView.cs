using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA;
using Kingmaker.View;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("a587e9fbbab348f386eae364b1fb6fa9")]
public class CopyAnotherView : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator UnitCopyTo;

	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator UnitCopyFrom;

	public bool CopyPortrait;

	public bool CopyEquipmentView;

	public override void RunAction()
	{
		if (!UnitCopyTo.TryGetValue(out var value) || !UnitCopyFrom.TryGetValue(out var value2))
		{
			PFLog.Default.Error("Can't copy view from " + UnitCopyFrom?.name + " to " + UnitCopyTo?.name + " since one of them is missing");
			return;
		}
		if (!(value is BaseUnitEntity baseUnitEntity))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {UnitCopyTo} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
			return;
		}
		if (!(value2 is BaseUnitEntity baseUnitEntity2))
		{
			string message2 = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {UnitCopyFrom} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message2))
			{
				UberDebug.LogError(message2);
			}
			return;
		}
		UnitEntityView unitEntityView = baseUnitEntity2.CreateView();
		UnitEntityView view = baseUnitEntity.View;
		if (CopyPortrait)
		{
			baseUnitEntity.UISettings.SetPortrait(baseUnitEntity2.Portrait);
		}
		baseUnitEntity.DetachView();
		view.DestroyViewObject();
		baseUnitEntity.AttachView(unitEntityView);
		if (CopyEquipmentView)
		{
			unitEntityView.CharacterAvatar?.AddEquipmentEntities(baseUnitEntity2.View.CharacterAvatar?.EquipmentEntities);
		}
	}

	public override string GetCaption()
	{
		return $"Copy view from {UnitCopyFrom} to {UnitCopyTo}";
	}
}
