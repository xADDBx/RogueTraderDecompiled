using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.Interfaces;
using UnityEngine;

namespace Kingmaker.DialogSystem.Blueprints;

[TypeId("6091ddca960a4d1459ac23e917c21d7d")]
public class BlueprintSequenceExit : BlueprintScriptableObject, IEditorCommentHolder
{
	[HideInInspector]
	[SerializeField]
	private EditorCommentHolder m_EditorComment;

	public List<BlueprintAnswerBaseReference> Answers = new List<BlueprintAnswerBaseReference>();

	public CueSelection Continue;

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
}
