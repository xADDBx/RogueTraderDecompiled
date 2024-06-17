using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Parts;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/Unrecruit")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("7d6c4f7ff596e5e4086531c0f96ac650")]
public class Unrecruit : GameAction
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("CompanionBlueprint")]
	private BlueprintUnitReference m_CompanionBlueprint;

	public ActionList OnUnrecruit;

	public BlueprintUnit CompanionBlueprint
	{
		get
		{
			return m_CompanionBlueprint?.Get();
		}
		set
		{
			m_CompanionBlueprint = value.ToReference<BlueprintUnitReference>();
		}
	}

	public override void RunAction()
	{
		BaseUnitEntity mainCharacter = Game.Instance.Player.MainCharacterEntity;
		BaseUnitEntity baseUnitEntity = Game.Instance.Player.AllCharacters.Where((BaseUnitEntity u) => u != mainCharacter).FirstOrDefault((BaseUnitEntity unit) => IsCompanion(unit.Blueprint));
		if (baseUnitEntity == null)
		{
			PFLog.Default.Error("No companion unit found when unrecruiting " + CompanionBlueprint);
			return;
		}
		DoUnrecruit(baseUnitEntity);
		EventBus.RaiseEvent((IBaseUnitEntity)baseUnitEntity, (Action<ICompanionChangeHandler>)delegate(ICompanionChangeHandler h)
		{
			h.HandleUnrecruit();
		}, isCheckRuntime: true);
	}

	private void DoUnrecruit(BaseUnitEntity companion)
	{
		UnitPartCompanion optional = companion.GetOptional<UnitPartCompanion>();
		if (optional != null && optional.State == CompanionState.ExCompanion)
		{
			PFLog.Default.Error($"Companion {companion} already lost, cannot unrecruit again.");
		}
		if ((bool)companion.View)
		{
			companion.View.StopMoving();
		}
		Game.Instance.Player.RemoveCompanion(companion);
		companion.GetOrCreate<UnitPartCompanion>().SetState(CompanionState.ExCompanion);
		using (ContextData<RecruitedUnitData>.Request().Setup(companion))
		{
			OnUnrecruit?.Run();
		}
	}

	private bool IsCompanion(BlueprintUnit unit)
	{
		return unit == CompanionBlueprint;
	}

	public override string GetCaption()
	{
		return $"Unrecruit ({CompanionBlueprint})";
	}
}
