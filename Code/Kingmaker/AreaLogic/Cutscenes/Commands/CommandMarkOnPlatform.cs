using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("ee8188ea95c9412da826a7d098a3885e")]
public class CommandMarkOnPlatform : CommandBase
{
	private class Data
	{
		[CanBeNull]
		public AbstractUnitEntity Unit;

		public PlatformObjectEntity Platform;
	}

	[AllowedEntityType(typeof(PlatformObjectView))]
	[ValidateNotEmpty]
	public EntityReference PlatformReference;

	[SerializeReference]
	public AbstractUnitEvaluator UnitEvaluator;

	public override bool IsContinuous => true;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Unit = UnitEvaluator.GetValue();
		commandData.Platform = PlatformReference.FindData() as PlatformObjectEntity;
		commandData.Unit.GetOrCreate<EntityPartStayOnPlatform>().SetOnPlatform(commandData.Platform);
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Unit?.GetOrCreate<EntityPartStayOnPlatform>().ReleaseFromPlatform(commandData.Platform);
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return false;
	}

	public override string GetCaption()
	{
		return "Mark " + UnitEvaluator?.GetCaptionShort() + " <b>on platform</b>";
	}
}
