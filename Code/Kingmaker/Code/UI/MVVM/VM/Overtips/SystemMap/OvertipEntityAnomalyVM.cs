using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Exploration;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.SystemMap;

public class OvertipEntityAnomalyVM : OvertipEntityVM, IAnomalyResearchHandler, ISubscriber<AnomalyEntityData>, ISubscriber, IGameModeHandler, IAreaLoadingStagesHandler
{
	public readonly MapObjectEntity SystemMapObject;

	public readonly ReactiveProperty<string> AnomalyName = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<bool> IsExplored = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> HasQuest = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<string> QuestObjectiveName = new ReactiveProperty<string>();

	public readonly AnomalyView AnomalyView;

	protected override Vector3 GetEntityPosition()
	{
		return SystemMapObject.Position;
	}

	public OvertipEntityAnomalyVM(MapObjectEntity systemMapObjectData)
	{
		AddDisposable(EventBus.Subscribe(this));
		SystemMapObject = systemMapObjectData;
		AnomalyView = SystemMapObject.View.GetComponent<AnomalyView>();
		UpdateAnomalyInfo();
	}

	private void UpdateAnomalyInfo()
	{
		if (!(AnomalyView == null))
		{
			AnomalyEntityData data = AnomalyView.Data;
			if (!string.IsNullOrEmpty(data.Blueprint.Name))
			{
				AnomalyName.Value = data.Blueprint.Name;
			}
			IsExplored.Value = data.IsInteracted;
			SetPlanetIconsState();
			EventBus.RaiseEvent(delegate(IAnomalyUIHandler h)
			{
				h.UpdateAnomalyScreen(AnomalyView);
			});
		}
	}

	public void OpenAnomalyInfo()
	{
		EventBus.RaiseEvent(delegate(IAnomalyUIHandler h)
		{
			h.OpenAnomalyScreen(AnomalyView);
		});
	}

	public void HandleAnomalyStartResearch()
	{
		if (EventInvokerExtensions.Entity as AnomalyEntityData == AnomalyView.Data)
		{
			OpenAnomalyInfo();
		}
	}

	public void HandleAnomalyResearched(BaseUnitEntity unit, RulePerformSkillCheck skillCheck)
	{
		if (EventInvokerExtensions.Entity as AnomalyEntityData == AnomalyView.Data)
		{
			UpdateAnomalyInfo();
		}
	}

	public void RequestVisit()
	{
		VisitPlanet();
	}

	private void VisitPlanet()
	{
		Game.Instance.GameCommandQueue.MoveShip(AnomalyView, MoveShipGameCommand.VisitType.MovePlayerShip);
	}

	private void SetPlanetIconsState()
	{
		List<QuestObjective> questsForAnomaly = UIUtilitySpaceQuests.GetQuestsForAnomaly(AnomalyView.Data.Blueprint);
		HasQuest.Value = questsForAnomaly != null && !questsForAnomaly.Empty();
		if (HasQuest.Value)
		{
			List<string> list = questsForAnomaly?.Where((QuestObjective quest) => !string.IsNullOrWhiteSpace(quest.Blueprint.GetTitile())).Select((QuestObjective quest, int index) => $"{index + 1}. " + quest.Blueprint.GetTitile()).ToList();
			if (list != null && list.Any())
			{
				QuestObjectiveName.Value = string.Join(Environment.NewLine, list);
			}
		}
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (Game.Instance.CurrentMode == GameModeType.StarSystem)
		{
			UpdateAnomalyInfo();
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		if (Game.Instance.CurrentMode == GameModeType.StarSystem)
		{
			UpdateAnomalyInfo();
		}
	}

	public void OnAreaScenesLoaded()
	{
	}

	public void OnAreaLoadingComplete()
	{
		if (Game.Instance.CurrentMode == GameModeType.StarSystem)
		{
			UpdateAnomalyInfo();
		}
	}
}
