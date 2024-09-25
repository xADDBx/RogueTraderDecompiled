using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.State;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.Interfaces;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.DialogSystem.Blueprints;

[NonOverridable]
[TypeId("87378395f04018e47a470f3f5486a545")]
public abstract class BlueprintCueBase : BlueprintScriptableObject, IConditionDebugContext, IEditorCommentHolder
{
	public bool ShowOnce;

	[ShowIf("ShowOnce")]
	public bool ShowOnceCurrentDialog;

	public ConditionsChecker Conditions;

	[HideInInspector]
	[SerializeField]
	private EditorCommentHolder m_EditorComment;

	public EditorCommentHolder EditorComment
	{
		get
		{
			return m_EditorComment;
		}
		set
		{
			m_EditorComment = value;
		}
	}

	public virtual bool CanShow()
	{
		if (!this)
		{
			return false;
		}
		DialogState dialog = Game.Instance.Player.Dialog;
		DialogController dialogController = Game.Instance.DialogController;
		if (ShowOnce)
		{
			if (ShowOnceCurrentDialog)
			{
				if (dialogController.LocalShownCues.Contains(this))
				{
					DialogDebug.Add(this, "(show once) was shown before (current dialog)", Color.red);
					return false;
				}
			}
			else if (dialog.ShownCues.Contains(this))
			{
				DialogDebug.Add(this, "(show once) was shown before (global)", Color.red);
				return false;
			}
		}
		if (!Conditions.Check(this))
		{
			DialogDebug.Add(this, "condtions failed", Color.red);
			return false;
		}
		return true;
	}

	public void AddConditionDebugMessage(object element, bool result, string messageFormat, object[] @params)
	{
		DialogDebug.AddCondition(this, result, messageFormat, @params);
	}
}
