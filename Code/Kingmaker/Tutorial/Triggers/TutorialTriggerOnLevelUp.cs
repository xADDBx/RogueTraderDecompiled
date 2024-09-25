using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Levelup;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("f9f07e048c7a40fda6634c1906227542")]
public class TutorialTriggerOnLevelUp : TutorialTrigger, ILevelUpManagerUIHandler, ISubscriber, IHashable
{
	private LevelUpManager m_Manager;

	public void HandleCreateLevelUpManager(LevelUpManager manager)
	{
		m_Manager = manager;
	}

	public void HandleDestroyLevelUpManager()
	{
	}

	public void HandleUISelectCareerPath()
	{
	}

	public void HandleUICommitChanges()
	{
		TryToTrigger(null, delegate(TutorialContext context)
		{
			context.SolutionUnit = m_Manager.TargetUnit;
		});
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
