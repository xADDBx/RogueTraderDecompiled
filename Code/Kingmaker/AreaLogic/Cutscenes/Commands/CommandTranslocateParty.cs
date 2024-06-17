using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("d1b13a2a9ce49e645a228792a62998f5")]
public class CommandTranslocateParty : CommandBase
{
	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("Cutscene");

	[SerializeField]
	private Player.CharactersList m_UnitsList;

	[AllowedEntityType(typeof(LocatorView))]
	[ValidateNotEmpty]
	[ValidateNoNullEntries]
	public EntityReference[] Targets;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		BlueprintAreaPart blueprintAreaPart = null;
		foreach (var item3 in Game.Instance.Player.GetCharactersList(m_UnitsList).Select((BaseUnitEntity v, int i) => (v: v, i: i)))
		{
			BaseUnitEntity item = item3.v;
			int item2 = item3.i;
			int num = item2 % Targets.Length;
			EntityReference entityReference = Targets[num];
			IEntityViewBase entityViewBase = entityReference.FindView();
			if (entityViewBase == null)
			{
				Logger.Error($"{this}: Can't find locator #{num}, {entityReference} for teleport unit");
				continue;
			}
			if (item2 < Targets.Length && item.IsCustomCompanion() && !item.IsViewActive)
			{
				item.IsInGame = true;
				item.Parts.GetOptional<UnitPartCompanion>()?.SetState(CompanionState.InParty);
			}
			item.Position = entityViewBase.ViewTransform.position;
			item.SetOrientation(entityViewBase.ViewTransform.rotation.y);
			blueprintAreaPart = AreaService.FindMechanicBoundsContainsPoint(item.Position);
		}
		if (blueprintAreaPart != null && blueprintAreaPart != Game.Instance.CurrentlyLoadedAreaPart)
		{
			Logger.Warning($"{this}: translocating party from {Game.Instance.CurrentlyLoadedAreaPart} to {blueprintAreaPart}. Starting fix process.");
			LoadingProcess.Instance.StartLoadingProcess(Game.Instance.SceneLoader.SwitchToAreaPartCoroutine(blueprintAreaPart));
		}
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return true;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}

	protected override void OnStop(CutscenePlayerData player)
	{
	}

	public override string GetCaption()
	{
		return $"Teleport party ({m_UnitsList})";
	}
}
