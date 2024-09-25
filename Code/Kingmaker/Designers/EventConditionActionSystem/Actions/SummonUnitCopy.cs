using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA;
using Kingmaker.UnitLogic;
using Kingmaker.View;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("73d33ed03a8613342b7b03883782e1dc")]
public class SummonUnitCopy : GameAction
{
	[SerializeReference]
	public AbstractUnitEvaluator CopyFrom;

	[SerializeReference]
	public LocatorEvaluator Locator;

	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("CopyBlueprint")]
	private BlueprintUnitReference m_CopyBlueprint;

	[CanBeNull]
	[SerializeField]
	[FormerlySerializedAs("SummonPool")]
	private BlueprintSummonPoolReference m_SummonPool;

	public bool DoNotCreateItems;

	public ActionList OnSummon;

	public BlueprintUnit CopyBlueprint => m_CopyBlueprint?.Get();

	public BlueprintSummonPool SummonPool => m_SummonPool?.Get();

	public override string GetCaption()
	{
		return $"Sumon unit copy ({CopyFrom} at {Locator})";
	}

	protected override void RunAction()
	{
		if (!(CopyFrom.GetValue() is BaseUnitEntity source))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {CopyFrom} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
			return;
		}
		LocatorEntity value = Locator.GetValue();
		if (CopyBlueprint == null)
		{
			Element.LogError("CopyBlueprint is not set: " + GetCaption());
			return;
		}
		BaseUnitEntity baseUnitEntity = CreateCopy(source, CopyBlueprint, value.HoldingState, DoNotCreateItems);
		baseUnitEntity.Position = value.View.ViewTransform.position;
		baseUnitEntity.SetOrientation(value.View.ViewTransform.rotation.eulerAngles.y);
		if (SummonPool != null)
		{
			Game.Instance.SummonPools.Register(SummonPool, baseUnitEntity);
		}
		if (!OnSummon.HasActions)
		{
			return;
		}
		using (ContextData<SpawnedUnitData>.Request().Setup(baseUnitEntity, value.HoldingState))
		{
			OnSummon.Run();
		}
	}

	public static BaseUnitEntity CreateCopy(BaseUnitEntity source, BlueprintUnit bp, SceneEntitiesState state, bool doNotCreateItems = false)
	{
		return UnitHelper.SummonCopy(source, bp, state, doNotCreateItems);
	}
}
