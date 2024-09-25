using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.Exploration;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.SystemMap;

public class SystemScannerVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IGameModeHandler, ISubscriber, IAreaLoadingStagesHandler, ISystemMapRadarHandler, IAnomalyHandler, ISubscriber<AnomalyEntityData>
{
	public readonly ReactiveProperty<bool> IsOnSystemMap = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsInSpaceCombat = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsSystemChanged = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsAnomalyInteractedChanged = new ReactiveProperty<bool>();

	public List<string> AnomaliesTypesText = new List<string>();

	public readonly ReactiveProperty<bool> ShouldShow = new ReactiveProperty<bool>(initialValue: true);

	public readonly AutoDisposingList<SystemScannerObjectVM> ObjectsList = new AutoDisposingList<SystemScannerObjectVM>();

	public SystemScannerVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		OnGameModeStart(Game.Instance.CurrentMode);
	}

	private void AddObjectsInfo(bool isSystemChanged)
	{
		ObjectsList.Clear();
		AnomaliesTypesText.Clear();
		foreach (AnomalyEntityData item in from o in Game.Instance.State.StarSystemObjects.OfType<AnomalyEntityData>().ToList()
			where !o.IsInteracted && !o.View.BlueprintAnomaly.HideInUI
			select o)
		{
			ObjectsList.Add(new SystemScannerObjectVM(item));
			if (Enum.TryParse<BlueprintAnomaly.AnomalyObjectType>(item.Blueprint.AnomalyType.ToString(), out var result))
			{
				AnomaliesTypesText.Add("<color=#" + ColorUtility.ToHtmlStringRGB(UIConfig.Instance.AnomalyColor.GetAnomalyColor(result)) + ">- " + UIStrings.Instance.ExplorationTexts.GetAnomalyTypeName(result) + " -</color>");
			}
		}
		if (isSystemChanged)
		{
			IsSystemChanged.Value = !IsSystemChanged.Value;
		}
		else
		{
			IsAnomalyInteractedChanged.Value = !IsAnomalyInteractedChanged.Value;
		}
		AnomaliesTypesText = AnomaliesTypesText.Distinct().ToList();
	}

	public void HandleAnomalyInteracted()
	{
		AddObjectsInfo(isSystemChanged: false);
	}

	protected override void DisposeImplementation()
	{
		ObjectsList.Clear();
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		IsOnSystemMap.Value = Game.Instance.CurrentMode == GameModeType.StarSystem;
		IsInSpaceCombat.Value = Game.Instance.CurrentMode == GameModeType.SpaceCombat;
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		IsOnSystemMap.Value = Game.Instance.CurrentMode == GameModeType.StarSystem;
		IsInSpaceCombat.Value = Game.Instance.CurrentMode == GameModeType.SpaceCombat;
	}

	public void OnAreaScenesLoaded()
	{
	}

	public void OnAreaLoadingComplete()
	{
		AddObjectsInfo(isSystemChanged: true);
	}

	public void HandleShowSystemMapRadar()
	{
		AddObjectsInfo(isSystemChanged: true);
	}
}
