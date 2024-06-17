using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Persistence;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("96c32a6355a5cbe419f21098bb7591d4")]
public class CommandTeleport : CommandBase
{
	[InfoBox("If this command is used to teleport to a different area, the cutscene would be stopped completely.\nDo not do this unless this is the last command in the cutscene.")]
	public BlueprintAreaEnterPointReference TargetPoint;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		if (TargetPoint.Get().Area == Game.Instance.CurrentlyLoadedArea)
		{
			Game.Instance.Teleport(TargetPoint.Get());
			return;
		}
		player.Stop();
		Game.Instance.LoadArea(TargetPoint, AutoSaveMode.None);
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return !LoadingProcess.Instance.IsLoadingInProcess;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}

	protected override void OnStop(CutscenePlayerData player)
	{
	}

	public override string GetCaption()
	{
		return "Teleport to (" + TargetPoint.NameSafe() + ")";
	}
}
