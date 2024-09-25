using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Levelup;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("a15a0bef1e2e41bc8a0b3c95e256451c")]
public class TutorialTriggerUnitLevelUp : TutorialTrigger, ILevelUpManagerUIHandler, ISubscriber, IHashable
{
	[SerializeField]
	private BlueprintUnitReference m_Unit;

	[SerializeField]
	private int m_Level;

	private BlueprintUnit Unit => m_Unit.Get();

	public void HandleCreateLevelUpManager(LevelUpManager manager)
	{
		BaseUnitEntity unit = manager.TargetUnit;
		PartUnitProgression required = unit.Parts.GetRequired<PartUnitProgression>();
		if (unit.Blueprint == Unit && required.ExperienceLevel >= m_Level)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SolutionUnit = unit;
			});
		}
	}

	public void HandleDestroyLevelUpManager()
	{
	}

	public void HandleUISelectCareerPath()
	{
	}

	public void HandleUICommitChanges()
	{
	}

	public void HandleUISelectionChanged()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
