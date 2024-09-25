using System;
using UnityEngine;

namespace Kingmaker.Visual.Particles;

public class FxAttachment : MonoBehaviour
{
	private FxAttachmentSlot m_AttachmentSlot;

	public FxAttachmentSlot AttachmentSlot => m_AttachmentSlot;

	public event EventHandler<FxAttachmentSlot> AttachToSlot;

	public event EventHandler<FxAttachmentSlot> DetachFromSlot;

	internal void Attach(FxAttachmentSlot attachmentSlot)
	{
		if (!(m_AttachmentSlot == attachmentSlot))
		{
			Detach();
			m_AttachmentSlot = attachmentSlot;
			m_AttachmentSlot.AddAttachment(this);
			this.AttachToSlot?.Invoke(this, m_AttachmentSlot);
		}
	}

	private void Detach()
	{
		if (m_AttachmentSlot == null)
		{
			return;
		}
		try
		{
			this.DetachFromSlot?.Invoke(this, m_AttachmentSlot);
		}
		finally
		{
			m_AttachmentSlot.RemoveAttachment(this);
			m_AttachmentSlot = null;
		}
	}

	private void OnDisable()
	{
		Detach();
	}
}
