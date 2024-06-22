using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(false)]
[TypeId("330752e882f99ea41a86354e64f8d769")]
public class UnlockCompanionStory : GameAction
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Story")]
	private BlueprintCompanionStoryReference m_Story;

	public BlueprintCompanionStory Story
	{
		get
		{
			return m_Story?.Get();
		}
		set
		{
			m_Story = value.ToReference<BlueprintCompanionStoryReference>();
		}
	}

	public override string GetCaption()
	{
		return "Unlock companion story: " + Story;
	}

	protected override void RunAction()
	{
		if (!Story)
		{
			Element.LogError("Can't unlock story: missing");
		}
		else
		{
			Story.Unlock();
		}
	}
}
