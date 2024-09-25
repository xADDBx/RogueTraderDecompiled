using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("0231c8c2024742d6934ce8f5af4e6341")]
public class MarkCuesUnSeen : PlayerUpgraderOnlyAction
{
	[ValidateNoNullEntries]
	[SerializeField]
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
		return "Снимает с указанных реплик пометку о том, что они были выбраны в диалоге.";
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
		return "Mark Cues Unseen(" + text + ")";
	}

	protected override void RunActionOverride()
	{
		foreach (BlueprintCueBase cue in Cues)
		{
			Game.Instance.Player.Dialog.ShownCues.Remove(cue);
		}
	}
}
