using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Quests;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Globalmap.SectorMap;

public class SectorMapRumourEntity : Entity, IQuestObjectiveHandler, ISubscriber, IHashable
{
	[JsonProperty]
	private BlueprintQuestObjective m_BlueprintQuestObjective;

	[JsonProperty]
	public bool IsQuestObjectiveActive;

	public new SectorMapRumourView View => (SectorMapRumourView)base.View;

	public SectorMapRumourEntity(string uniqueId, bool isInGame)
		: base(uniqueId, isInGame)
	{
	}

	public SectorMapRumourEntity(SectorMapRumourView view)
		: this(view.UniqueId, view.IsInGameBySettings)
	{
		m_BlueprintQuestObjective = view.Blueprint;
	}

	public SectorMapRumourEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	protected override IEntityViewBase CreateViewForData()
	{
		return null;
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		UpdateState(m_BlueprintQuestObjective);
	}

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		UpdateState(m_BlueprintQuestObjective);
	}

	private void UpdateState(BlueprintQuestObjective objective)
	{
		if (m_BlueprintQuestObjective == objective)
		{
			QuestObjectiveState objectiveState = Game.Instance.Player.QuestBook.GetObjectiveState(m_BlueprintQuestObjective);
			IsQuestObjectiveActive = objectiveState == QuestObjectiveState.Started;
			EventBus.RaiseEvent(delegate(IRumourObjectiveStateHandler h)
			{
				h.HandleRumourObjectiveActiveStateChanged(objective, IsQuestObjectiveActive);
			});
			View.Or(null)?.UpdateVisibility();
		}
	}

	public void HandleQuestObjectiveStarted(QuestObjective objective, bool silentStart = false)
	{
		UpdateState(objective.Blueprint);
	}

	public void HandleQuestObjectiveBecameVisible(QuestObjective objective, bool silentStart = false)
	{
		UpdateState(objective.Blueprint);
	}

	public void HandleQuestObjectiveCompleted(QuestObjective objective)
	{
		UpdateState(objective.Blueprint);
	}

	public void HandleQuestObjectiveFailed(QuestObjective objective)
	{
		UpdateState(objective.Blueprint);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(m_BlueprintQuestObjective);
		result.Append(ref val2);
		result.Append(ref IsQuestObjectiveActive);
		return result;
	}
}
