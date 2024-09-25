using ExitGames.Client.Photon.StructWrapping;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.EntitySystem.Persistence.Versioning.UnitUpgraderOnlyActions;

[TypeId("4bc4d4630e61e3e439f1f4056e32d2e1")]
public class RemoveBrokenSummon : UnitUpgraderOnlyAction
{
	public override string GetCaption()
	{
		return "Removes broken dead summoned monster when save is loaded";
	}

	protected override void RunActionOverride()
	{
		if (base.Target.LifeState.IsFinallyDead && !base.Target.Get<UnitPartSummonedMonster>())
		{
			Game.Instance.EntityDestroyer.Destroy(base.Target);
		}
	}
}
