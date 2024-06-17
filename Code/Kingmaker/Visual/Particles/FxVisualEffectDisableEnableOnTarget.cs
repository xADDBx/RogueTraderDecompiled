using UnityEngine;
using UnityEngine.VFX;

namespace Kingmaker.Visual.Particles;

public class FxVisualEffectDisableEnableOnTarget : MonoBehaviour
{
	private FxAttachment m_SelfAttachment;

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
		VisualEffect[] componentsInChildren = attachmentSlot.GetComponentsInChildren<VisualEffect>(includeInactive: true);
		if (componentsInChildren != null)
		{
			VisualEffect[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(value: false);
			}
		}
	}

	private void OnDetachFromSlot(object sender, FxAttachmentSlot attachmentSlot)
	{
		VisualEffect[] componentsInChildren = attachmentSlot.GetComponentsInChildren<VisualEffect>(includeInactive: true);
		if (componentsInChildren != null)
		{
			VisualEffect[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(value: true);
			}
		}
	}
}
