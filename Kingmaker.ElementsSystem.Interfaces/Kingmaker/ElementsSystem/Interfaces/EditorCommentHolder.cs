using System;
using UnityEngine;

namespace Kingmaker.ElementsSystem.Interfaces;

[Serializable]
public class EditorCommentHolder
{
	[HideInInspector]
	[SerializeField]
	private string m_EditorComment;

	[HideInInspector]
	[SerializeField]
	private bool m_Foldout;

	public string EditorComment
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

	public bool Foldout
	{
		get
		{
			return m_Foldout;
		}
		set
		{
			m_Foldout = value;
		}
	}
}
