using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.GameCommands;
using Kingmaker.UI.Common;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/TeleportParty")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(true)]
[TypeId("8072988edd00cce40bc433869828e6b3")]
public class TeleportParty : GameAction, IAreaEnterPointReference
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("exitPositon")]
	private BlueprintAreaEnterPointReference m_exitPositon;

	public AutoSaveMode AutoSaveMode = AutoSaveMode.BeforeExit;

	public bool ForcePauseAfterTeleport;

	[ShowIf("CanHaveActions")]
	public ActionList AfterTeleport;

	public BlueprintAreaEnterPoint exitPositon => m_exitPositon?.Get();

	private bool CanHaveActions => AutoSaveMode == AutoSaveMode.None;

	protected override void RunAction()
	{
		if (exitPositon.Area == Game.Instance.CurrentlyLoadedArea)
		{
			BaseUnitEntity baseUnitEntity = ContextData<AreaTransitionPartGameCommand.TransitionExecutorEntity>.Current?.EntityRef.Entity;
			if (baseUnitEntity != null && !baseUnitEntity.IsAlsoControlMainCharacterWithWarning())
			{
				return;
			}
			if (AutoSaveMode == AutoSaveMode.BeforeExit)
			{
				Save();
			}
			Game.Instance.Teleport(exitPositon, includeFollowers: true);
			if (AutoSaveMode == AutoSaveMode.AfterEntry)
			{
				Save();
			}
		}
		else
		{
			BaseUnitEntity baseUnitEntity2 = ContextData<AreaTransitionPartGameCommand.TransitionExecutorEntity>.Current?.EntityRef.Entity;
			if (baseUnitEntity2 != null && !baseUnitEntity2.IsAlsoControlMainCharacterWithWarning())
			{
				return;
			}
			Game.Instance.LoadArea(exitPositon, AutoSaveMode);
		}
		LoadingProcess.Instance.StartLoadingProcess(delegate
		{
			if (ForcePauseAfterTeleport)
			{
				Game.Instance.PauseOnLoadPending = true;
			}
			if (CanHaveActions)
			{
				AfterTeleport.Run();
			}
		}, LoadingProcessTag.TeleportParty);
	}

	private void Save()
	{
		LoadingProcess.Instance.StartLoadingProcess(Game.Instance.SaveManager.SaveRoutine(Game.Instance.SaveManager.GetNextAutoslot()));
	}

	public override string GetCaption()
	{
		return $"Teleport Party ({exitPositon})";
	}

	public bool GetUsagesFor(BlueprintAreaEnterPoint point)
	{
		return point == exitPositon;
	}
}
