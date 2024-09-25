using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View;
using Owlcat.QA.Validation;
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

	protected override void RunAction()
	{
		if (!UnitCopyTo.TryGetValue(out var value) || !UnitCopyFrom.TryGetValue(out var value2))
		{
			Element.LogError("Can't copy view from {0} to {1} since one of them is missing", UnitCopyFrom, UnitCopyTo);
			return;
		}
		if (!(value is BaseUnitEntity baseUnitEntity))
		{
			Element.LogError("[IS NOT BASE UNIT ENTITY] Game action {0}, {1} is not BaseUnitEntity", this, UnitCopyTo);
			return;
		}
		if (!(value2 is BaseUnitEntity baseUnitEntity2))
		{
			Element.LogError("[IS NOT BASE UNIT ENTITY] Game action {0}, {1} is not BaseUnitEntity", this, UnitCopyFrom);
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
