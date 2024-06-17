using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Kingmaker.Visual.Particles;

public class FxAttachmentSlot : MonoBehaviour
{
	private readonly List<FxAttachment> m_Attachments = new List<FxAttachment>();

	public IReadOnlyList<FxAttachment> Attachments => m_Attachments;

	public event EventHandler<FxAttachment> AttachmentAdd;

	public event EventHandler<FxAttachment> AttachmentRemove;

	public void AddAttachment(FxAttachment attachment)
	{
		m_Attachments.Add(attachment);
		this.AttachmentAdd?.Invoke(this, attachment);
	}

	public void RemoveAttachment(FxAttachment attachment)
	{
		int num = m_Attachments.IndexOf(attachment);
		if (num < 0)
		{
			return;
		}
		try
		{
			this.AttachmentRemove?.Invoke(this, attachment);
		}
		finally
		{
			m_Attachments.RemoveAtSwapBack(num);
		}
	}
}
