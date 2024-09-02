using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[RequireComponent(typeof(UnitSpawnerBase))]
[DisallowMultipleComponent]
[KnowledgeDatabaseID("d6f2645cc07c4b578a5163423b9ffee3")]
public class SpawnerCustomCutscene : EntityPartComponent<SpawnerCustomCutscene.Part>
{
	public class Part : ViewBasedPart, IUnitInitializer, IAreaActivationHandler, ISubscriber, IHashable
	{
		private EntityRef<CutscenePlayerData> m_Cutscene;

		public new SpawnerCustomCutscene Source => (SpawnerCustomCutscene)base.Source;

		public void OnSpawn(AbstractUnitEntity unit)
		{
			if (m_Cutscene != null || Source.Cutscene?.Get() == null)
			{
				return;
			}
			using (ContextData<SpawnedUnitData>.Request().Setup(unit, base.ConcreteOwner.HoldingState))
			{
				CutscenePlayerView cutscenePlayerView = CutscenePlayerView.Play(Source.Cutscene.Get(), new ParametrizedContextSetter
				{
					AdditionalParams = { 
					{
						"Spawned",
						(object)unit.FromAbstractUnitEntity()
					} }
				}, queued: false, base.ConcreteOwner.HoldingState);
				m_Cutscene = cutscenePlayerView.PlayerData;
				cutscenePlayerView.PlayerData.TickScene();
			}
		}

		public void OnInitialize(AbstractUnitEntity unit)
		{
			if (m_Cutscene != null && m_Cutscene.Entity != null)
			{
				m_Cutscene.Entity.SetPaused(value: false, CutscenePauseReason.UnitSpawnerDoesNotControlAnyUnit);
			}
		}

		public void OnDispose(AbstractUnitEntity unit)
		{
			if (m_Cutscene != null && m_Cutscene.Entity != null)
			{
				m_Cutscene.Entity.SetPaused(value: true, CutscenePauseReason.UnitSpawnerDoesNotControlAnyUnit);
			}
		}

		public void OnAreaActivated()
		{
			if (Source.m_RestartIfAbsent)
			{
				EntityRef<AbstractUnitEntity> spawnedUnit = ((UnitSpawnerBase.MyData)base.ConcreteOwner).SpawnedUnit;
				if (spawnedUnit != null)
				{
					OnSpawn(spawnedUnit);
				}
			}
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}
	}

	[ValidateNotNull]
	public CutsceneReference Cutscene;

	[SerializeField]
	private bool m_RestartIfAbsent;
}
