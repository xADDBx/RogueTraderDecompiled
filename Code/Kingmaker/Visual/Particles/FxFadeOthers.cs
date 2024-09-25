using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.Particles;

public class FxFadeOthers : MonoBehaviour, FxFadeOut.IOpacitySource
{
	[SerializeField]
	private float m_OpacityDelay;

	[SerializeField]
	private float m_OpacityDuration = 1f;

	[SerializeField]
	private AnimationCurve m_OpacityCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

	private FxAttachment m_SelfAttachment;

	private float m_AttachTime;

	private readonly List<FxFadeOut> m_AffectedObjects = new List<FxFadeOut>();

	private void Start()
	{
		if (TryGetComponent<FxAttachment>(out m_SelfAttachment))
		{
			if (m_SelfAttachment.AttachmentSlot != null)
			{
				OnAttachToSlot(m_SelfAttachment, m_SelfAttachment.AttachmentSlot);
			}
			m_SelfAttachment.AttachToSlot += OnAttachToSlot;
			m_SelfAttachment.DetachFromSlot += OnDetachFromSlot;
		}
	}

	private void OnAttachToSlot(object sender, FxAttachmentSlot attachmentSlot)
	{
		m_AttachTime = Time.unscaledTime;
		foreach (FxAttachment attachment in attachmentSlot.Attachments)
		{
			if (attachment != m_SelfAttachment)
			{
				Register(attachment);
			}
		}
		attachmentSlot.AttachmentAdd += OnSlotAttachmentAdd;
		attachmentSlot.AttachmentRemove += OnSlotAttachmentRemove;
	}

	private void OnDetachFromSlot(object sender, FxAttachmentSlot attachmentSlot)
	{
		attachmentSlot.AttachmentAdd -= OnSlotAttachmentAdd;
		attachmentSlot.AttachmentRemove -= OnSlotAttachmentRemove;
		foreach (FxFadeOut affectedObject in m_AffectedObjects)
		{
			affectedObject.RemoveOpacitySource(this);
		}
		m_AffectedObjects.Clear();
	}

	private void OnSlotAttachmentAdd(object sender, FxAttachment attachment)
	{
		if (attachment != m_SelfAttachment)
		{
			Register(attachment);
		}
	}

	private void OnSlotAttachmentRemove(object sender, FxAttachment attachment)
	{
		if (attachment != m_SelfAttachment)
		{
			Unregister(attachment);
		}
	}

	private void Register(FxAttachment attachment)
	{
		if (!attachment.TryGetComponent<FxFadeOut>(out var component))
		{
			component = attachment.gameObject.AddComponent<FxFadeOut>();
			component.hideFlags |= HideFlags.DontSave;
		}
		component.AddOpacitySource(this);
		m_AffectedObjects.Add(component);
	}

	private void Unregister(FxAttachment attachment)
	{
		if (attachment.TryGetComponent<FxFadeOut>(out var component))
		{
			m_AffectedObjects.Remove(component);
			component.RemoveOpacitySource(this);
		}
	}

	public float GetOpacity()
	{
		if (m_OpacityDuration > 0f)
		{
			float time = Mathf.Clamp01((Time.unscaledTime - m_AttachTime - m_OpacityDelay) / m_OpacityDuration);
			return m_OpacityCurve.Evaluate(time);
		}
		return m_OpacityCurve.Evaluate(1f);
	}
}
