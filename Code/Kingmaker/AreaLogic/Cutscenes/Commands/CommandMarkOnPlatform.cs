using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Animation;
using Owlcat.QA.Validation;
using RootMotion.FinalIK;
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

		public IKState SavedIKState;
	}

	private class IKState
	{
		public bool IKControllerEnabled;

		public bool IKControllerEnableIK;

		public bool GrounderIKEnabled;

		public bool BipedIkEnabled;

		public bool GrounderFBBIKEnabled;
	}

	[AllowedEntityType(typeof(PlatformObjectView))]
	[ValidateNotEmpty]
	public EntityReference PlatformReference;

	[SerializeReference]
	public AbstractUnitEvaluator UnitEvaluator;

	[SerializeField]
	public bool DisableUnitIK = true;

	public override bool IsContinuous => true;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Unit = UnitEvaluator.GetValue();
		commandData.Platform = PlatformReference.FindData() as PlatformObjectEntity;
		commandData.Unit.GetOrCreate<EntityPartStayOnPlatform>().SetOnPlatform(commandData.Platform);
		if (DisableUnitIK && commandData.Unit is BaseUnitEntity unit)
		{
			DisableUnitIKComponents(commandData, unit);
		}
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Unit?.GetOrCreate<EntityPartStayOnPlatform>().ReleaseFromPlatform(commandData.Platform);
		if (DisableUnitIK && commandData.Unit is BaseUnitEntity unit && commandData.SavedIKState != null)
		{
			RestoreUnitIKComponents(commandData, unit);
		}
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
		string text = (DisableUnitIK ? " + disable IK" : "");
		return "Mark " + UnitEvaluator?.GetCaptionShort() + " <b>on platform</b>" + text;
	}

	private void DisableUnitIKComponents(Data data, BaseUnitEntity unit)
	{
		if (unit?.View == null)
		{
			return;
		}
		IKState iKState = (data.SavedIKState = new IKState());
		IKController componentInChildren = unit.View.GetComponentInChildren<IKController>();
		if (componentInChildren != null)
		{
			iKState.IKControllerEnabled = componentInChildren.enabled;
			iKState.IKControllerEnableIK = componentInChildren.EnableIK;
			componentInChildren.EnableIK = false;
			componentInChildren.enabled = false;
			if (componentInChildren.GrounderIK != null)
			{
				iKState.GrounderIKEnabled = componentInChildren.GrounderIK.enabled;
				componentInChildren.GrounderIK.enabled = false;
			}
			if (componentInChildren.BipedIk != null)
			{
				iKState.BipedIkEnabled = componentInChildren.BipedIk.enabled;
				componentInChildren.BipedIk.enabled = false;
			}
			if (componentInChildren.GrounderIk != null)
			{
				iKState.GrounderFBBIKEnabled = componentInChildren.GrounderIk.enabled;
				componentInChildren.GrounderIk.enabled = false;
			}
		}
		else
		{
			GrounderBipedIK componentInChildren2 = unit.View.GetComponentInChildren<GrounderBipedIK>();
			if (componentInChildren2 != null)
			{
				iKState.GrounderIKEnabled = componentInChildren2.enabled;
				componentInChildren2.enabled = false;
			}
			FullBodyBipedIK componentInChildren3 = unit.View.GetComponentInChildren<FullBodyBipedIK>();
			if (componentInChildren3 != null)
			{
				iKState.BipedIkEnabled = componentInChildren3.enabled;
				componentInChildren3.enabled = false;
			}
			GrounderFBBIK componentInChildren4 = unit.View.GetComponentInChildren<GrounderFBBIK>();
			if (componentInChildren4 != null)
			{
				iKState.GrounderFBBIKEnabled = componentInChildren4.enabled;
				componentInChildren4.enabled = false;
			}
		}
	}

	private void RestoreUnitIKComponents(Data data, BaseUnitEntity unit)
	{
		if (unit?.View == null || data.SavedIKState == null)
		{
			return;
		}
		IKState savedIKState = data.SavedIKState;
		IKController componentInChildren = unit.View.GetComponentInChildren<IKController>();
		if (componentInChildren != null)
		{
			componentInChildren.EnableIK = savedIKState.IKControllerEnableIK;
			componentInChildren.enabled = savedIKState.IKControllerEnabled;
			if (componentInChildren.GrounderIK != null)
			{
				componentInChildren.GrounderIK.enabled = savedIKState.GrounderIKEnabled;
			}
			if (componentInChildren.BipedIk != null)
			{
				componentInChildren.BipedIk.enabled = savedIKState.BipedIkEnabled;
			}
			if (componentInChildren.GrounderIk != null)
			{
				componentInChildren.GrounderIk.enabled = savedIKState.GrounderFBBIKEnabled;
			}
		}
		else
		{
			GrounderBipedIK componentInChildren2 = unit.View.GetComponentInChildren<GrounderBipedIK>();
			if (componentInChildren2 != null)
			{
				componentInChildren2.enabled = savedIKState.GrounderIKEnabled;
			}
			FullBodyBipedIK componentInChildren3 = unit.View.GetComponentInChildren<FullBodyBipedIK>();
			if (componentInChildren3 != null)
			{
				componentInChildren3.enabled = savedIKState.BipedIkEnabled;
			}
			GrounderFBBIK componentInChildren4 = unit.View.GetComponentInChildren<GrounderFBBIK>();
			if (componentInChildren4 != null)
			{
				componentInChildren4.enabled = savedIKState.GrounderFBBIKEnabled;
			}
		}
		data.SavedIKState = null;
	}
}
