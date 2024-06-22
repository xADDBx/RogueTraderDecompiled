using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(false)]
[TypeId("152c73a9f599c524d9b59f391034e57e")]
public class MarkCuesSeen : GameAction
{
	[ValidateNoNullEntries]
	[SerializeField]
	[FormerlySerializedAs("Cues")]
	private BlueprintCueBaseReference[] m_Cues;

	public ReferenceArrayProxy<BlueprintCueBase> Cues
	{
		get
		{
			BlueprintReference<BlueprintCueBase>[] cues = m_Cues;
			return cues;
		}
	}

	public override string GetDescription()
	{
		return "Помечает указанные реплики в диалоге как выбранные.";
	}

	public override string GetCaption()
	{
		string text = "";
		for (int i = 0; i < Cues.Length; i++)
		{
			if (i > 0)
			{
				text += ", ";
			}
			text += Cues[i].ToString();
		}
		return "Mark Cues Seen(" + text + ")";
	}

	protected override void RunAction()
	{
		foreach (BlueprintCueBase cue in Cues)
		{
			Game.Instance.Player.Dialog.ShownCues.Add(cue);
		}
	}
}
