using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("eb3243f248c346be85d64832d7aae8ee")]
public class TutorialWindowTrigger : TutorialTrigger, ITutorialWindowClosedHandler, ISubscriber, IHashable
{
	[SerializeField]
	private ActionList m_ActionsOnHide;

	private bool m_ActionsDone;

	public void HandleHideTutorial(TutorialData data)
	{
		if (base.Fact.Blueprint != data.Blueprint || !m_ActionsOnHide.HasActions || m_ActionsDone)
		{
			return;
		}
		using (ContextData<TutorialIsActiveContext>.Request())
		{
			m_ActionsOnHide.Run();
			m_ActionsDone = true;
		}
	}

	protected override void OnActivateOrPostLoad()
	{
		m_ActionsDone = false;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
