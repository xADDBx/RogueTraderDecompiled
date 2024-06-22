using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Dialog;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.Interfaces;
using Kingmaker.Localization;
using MemoryPack;
using UnityEngine;

namespace Kingmaker.DialogSystem.Blueprints;

[NonOverridable]
[TypeId("c8ff73feae580b142a9f43e0c61d7f32")]
[MemoryPackable(GenerateType.NoGenerate)]
public class BlueprintDialog : BlueprintScriptableObject, IConditionDebugContext, IEditorCommentHolder
{
	public CueSelection FirstCue;

	[CanBeNull]
	[SerializeReference]
	public PositionEvaluator StartPosition;

	public ConditionsChecker Conditions = new ConditionsChecker();

	public ActionList StartActions = new ActionList();

	public ActionList FinishActions = new ActionList();

	public ActionList ReplaceActions = new ActionList();

	[Tooltip("Если галка стоит, то любое изменение диалога будет проверяться хуком CheckDialogFeatureFreezed")]
	public bool FeatureFreezed;

	public bool TurnPlayer = true;

	public bool TurnFirstSpeaker = true;

	[Tooltip("Отключает вращение камеры игроком на время диалога")]
	public bool IsLockCameraRotationButtons;

	public DialogType Type;

	public LocalizedString Description;

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

	[NotNull]
	public BlueprintAnswer GetContinueAnswer()
	{
		if (Type == DialogType.Epilog)
		{
			return Game.Instance.BlueprintRoot.Dialog.InterchapterContinueAnswer;
		}
		return Game.Instance.BlueprintRoot.Dialog.ContinueAnswer;
	}

	[NotNull]
	public BlueprintAnswer GetExitAnswer()
	{
		if (Type == DialogType.Epilog)
		{
			return Game.Instance.BlueprintRoot.Dialog.InterchapterExitAnswer;
		}
		return Game.Instance.BlueprintRoot.Dialog.ExitAnswer;
	}

	public void AddConditionDebugMessage(object element, bool result, string messageFormat, object[] @params)
	{
		DialogDebug.AddCondition(this, result, messageFormat, @params);
	}
}
