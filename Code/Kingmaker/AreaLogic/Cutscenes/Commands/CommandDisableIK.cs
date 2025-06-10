using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Visual.Animation;
using RootMotion.FinalIK;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("0270eeda4a791aa40be3d4b54cd211c6")]
public class CommandDisableIK : CommandBase
{
	[SerializeField]
	public bool Disable = true;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
		{
			if (partyAndPet?.View == null)
			{
				continue;
			}
			IKController componentInChildren = partyAndPet.View.GetComponentInChildren<IKController>();
			if (componentInChildren != null)
			{
				componentInChildren.EnableIK = !Disable;
				componentInChildren.enabled = !Disable;
				if (componentInChildren.GrounderIK != null)
				{
					componentInChildren.GrounderIK.enabled = !Disable;
				}
				if (componentInChildren.BipedIk != null)
				{
					componentInChildren.BipedIk.enabled = !Disable;
				}
				if (componentInChildren.GrounderIk != null)
				{
					componentInChildren.GrounderIk.enabled = !Disable;
				}
				continue;
			}
			GrounderBipedIK componentInChildren2 = partyAndPet.View.GetComponentInChildren<GrounderBipedIK>();
			if (componentInChildren2 != null)
			{
				componentInChildren2.enabled = !Disable;
			}
			FullBodyBipedIK componentInChildren3 = partyAndPet.View.GetComponentInChildren<FullBodyBipedIK>();
			if (componentInChildren3 != null)
			{
				componentInChildren3.enabled = !Disable;
			}
			GrounderFBBIK componentInChildren4 = partyAndPet.View.GetComponentInChildren<GrounderFBBIK>();
			if (componentInChildren4 != null)
			{
				componentInChildren4.enabled = !Disable;
			}
		}
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return true;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}

	public override string GetCaption()
	{
		return "<b>" + (Disable ? "Disable" : "Enable") + "</b> IK for party";
	}
}
