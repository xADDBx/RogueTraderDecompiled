using System.Linq;
using Kingmaker.AreaLogic.SummonPool;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("ccb0fb0efcf797442840443865ee40bd")]
public class Summon : GameAction
{
	[ValidateNotNull]
	[SerializeField]
	private BlueprintUnitReference m_Unit;

	[SerializeField]
	private BlueprintSummonPoolReference m_SummonPool;

	[ShowIf("HasSummonPool")]
	public bool GroupBySummonPool;

	[ValidateNotNull]
	[SerializeReference]
	public TransformEvaluator Transform;

	public Vector3 Offset;

	public ActionList OnSummmon;

	private bool HasSummonPool => SummonPool;

	public BlueprintUnit Unit => m_Unit?.Get();

	public BlueprintSummonPool SummonPool => m_SummonPool?.Get();

	public override string GetCaption()
	{
		return $"Summon ({Unit})";
	}

	protected override void RunAction()
	{
		SceneEntitiesState mainState = Game.Instance.State.LoadedAreaState.MainState;
		BaseUnitEntity baseUnitEntity = ((!SummonPool) ? Game.Instance.EntitySpawner.SpawnUnit(Unit, Transform.GetValue().position + Offset, Transform.GetValue().rotation, mainState) : Game.Instance.SummonPools.Summon(SummonPool, Unit, Transform.GetValue(), Offset));
		if (baseUnitEntity == null)
		{
			return;
		}
		if (SummonPool != null && GroupBySummonPool)
		{
			ISummonPool summonPool = Game.Instance.SummonPools.Get(SummonPool);
			if (summonPool != null && summonPool.Count > 1)
			{
				baseUnitEntity.CombatGroup.Id = summonPool.Units.FirstOrDefault()?.GetCombatGroupOptional()?.Id ?? baseUnitEntity.CombatGroup.Id;
			}
		}
		if (OnSummmon.HasActions)
		{
			using (ContextData<SpawnedUnitData>.Request().Setup(baseUnitEntity, mainState))
			{
				OnSummmon.Run();
			}
		}
	}
}
