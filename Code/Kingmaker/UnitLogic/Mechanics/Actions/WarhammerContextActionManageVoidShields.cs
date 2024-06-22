using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("1441c1c4053751d4ab4d4fa0c107aaef")]
public class WarhammerContextActionManageVoidShields : ContextAction
{
	private enum ActionType
	{
		Reinforce,
		RestoreWeakest
	}

	[SerializeField]
	private ActionType actionType;

	[SerializeField]
	[HideIf("IsRestoreWeakest")]
	private StarshipSectorShieldsType shieldsSector;

	[SerializeField]
	[HideIf("IsRestoreWeakest")]
	private bool IsUpgraded;

	[SerializeField]
	[ShowIf("IsRestoreWeakest")]
	private int pctOfMaxStrength;

	private bool IsRestoreWeakest => actionType == ActionType.RestoreWeakest;

	public override string GetCaption()
	{
		return actionType switch
		{
			ActionType.Reinforce => string.Format("Reinforce{0} {1} shields", IsUpgraded ? " (upgraded)" : "", shieldsSector), 
			ActionType.RestoreWeakest => $"Restore up to {pctOfMaxStrength}% of weakest shield sector", 
			_ => "Unknown shield operation", 
		};
	}

	protected override void RunAction()
	{
		StarshipEntity starshipEntity = (StarshipEntity)base.Context.MainTarget.Entity;
		if (starshipEntity != null)
		{
			switch (actionType)
			{
			case ActionType.Reinforce:
				starshipEntity.Shields.ReinforceShield(shieldsSector, (IsUpgraded && PFStatefulRandom.SpaceCombat.Range(0, 100) < 50) ? new StarshipSectorShieldsType?(GetRandomOtherSector(shieldsSector)) : null);
				break;
			case ActionType.RestoreWeakest:
				starshipEntity.Shields.RestoreWeakestSector(pctOfMaxStrength);
				break;
			}
		}
	}

	private StarshipSectorShieldsType GetRandomOtherSector(StarshipSectorShieldsType sector)
	{
		StarshipSectorShieldsType[] array = sector switch
		{
			StarshipSectorShieldsType.Fore => new StarshipSectorShieldsType[3]
			{
				StarshipSectorShieldsType.Aft,
				StarshipSectorShieldsType.Port,
				StarshipSectorShieldsType.Starboard
			}, 
			StarshipSectorShieldsType.Aft => new StarshipSectorShieldsType[3]
			{
				StarshipSectorShieldsType.Fore,
				StarshipSectorShieldsType.Port,
				StarshipSectorShieldsType.Starboard
			}, 
			StarshipSectorShieldsType.Port => new StarshipSectorShieldsType[3]
			{
				StarshipSectorShieldsType.Fore,
				StarshipSectorShieldsType.Aft,
				StarshipSectorShieldsType.Starboard
			}, 
			StarshipSectorShieldsType.Starboard => new StarshipSectorShieldsType[3]
			{
				StarshipSectorShieldsType.Fore,
				StarshipSectorShieldsType.Aft,
				StarshipSectorShieldsType.Port
			}, 
			_ => throw new ArgumentOutOfRangeException("sector", sector, null), 
		};
		int num = PFStatefulRandom.SpaceCombat.Range(0, array.Length);
		return array[num];
	}
}
