using System;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[AllowMultipleComponents]
[TypeId("55fafc5a15cd40e386986c7a9583cb8f")]
public class HardReloadMechanic : GameAction
{
	[SerializeField]
	private bool _clearFx = true;

	[SerializeField]
	private SceneReference _mechanicsScene;

	[SerializeField]
	private ActionsHolderReference _actionsAfterReload;

	[SerializeField]
	private bool _needLoadingScreen;

	private bool IsMechanicsSceneDefined => _mechanicsScene?.IsDefined ?? false;

	protected override void RunAction()
	{
		BlueprintArea currentlyLoadedArea = Game.Instance.CurrentlyLoadedArea;
		if (!currentlyLoadedArea.GetActiveDynamicScenes().Contains(_mechanicsScene) || !IsMechanicsSceneDefined)
		{
			PFLog.Default.Error($"Current area {currentlyLoadedArea} doesn't have {_mechanicsScene} " + "scene as one of mechanics scenes.");
		}
		else
		{
			Game.ReloadAreaMechanic(_mechanicsScene, callback: (_actionsAfterReload != null && !_actionsAfterReload.IsEmpty()) ? new Action(_actionsAfterReload.Get().Run) : null, clearFx: _clearFx, loadingScreen: _needLoadingScreen);
		}
	}

	public override string GetCaption()
	{
		StringBuilder stringBuilder = new StringBuilder("Hard reload mechanic scenes");
		if (IsMechanicsSceneDefined)
		{
			stringBuilder.AppendFormat(" to {0}", _mechanicsScene);
		}
		stringBuilder.Append(".");
		return stringBuilder.ToString();
	}
}
