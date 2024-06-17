using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;

namespace Kingmaker.Visual.Decals;

public class GUIDecal : ScreenSpaceDecal
{
	[SerializeField]
	[UsedImplicitly]
	private Color m_NormalColor = Color.yellow;

	[SerializeField]
	[UsedImplicitly]
	private Color m_SelfColor = Color.green;

	private Tweener m_AppearAnimation;

	private Tweener m_DisappearAnimation;

	private bool m_isInit;

	private bool m_isShowed;

	public override DecalType Type => DecalType.GUI;

	private void InitAnimator()
	{
		if (!m_isInit)
		{
			m_AppearAnimation = base.MaterialProperties.DOFade(1f, 0.2f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: false);
			m_DisappearAnimation = base.MaterialProperties.DOFade(0f, 0.2f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: false);
			m_isInit = true;
			m_isShowed = false;
		}
	}

	public void SetSpellRangeScale(float meters, bool dir = true)
	{
		float num = meters - 0.5f;
		if (dir)
		{
			base.transform.localScale = new Vector3(meters, base.transform.localScale.y, meters);
			base.transform.DOScale(new Vector3(num, base.transform.localScale.y, num), 0.2f).From().SetUpdate(isIndependentUpdate: true);
		}
		else
		{
			base.transform.DOScale(new Vector3(num, base.transform.localScale.y, num), 0.2f).SetUpdate(isIndependentUpdate: true);
		}
	}

	public void SetRangeColor(bool isSelf, bool show, float alpha)
	{
		InitAnimator();
		if (m_isShowed == show)
		{
			return;
		}
		m_isShowed = show;
		if (m_isShowed)
		{
			base.gameObject.SetActive(value: true);
			Color color = (isSelf ? m_SelfColor : m_NormalColor);
			color.a = 0f;
			base.MaterialProperties.SetBaseColor(color);
			m_DisappearAnimation.Pause();
			m_DisappearAnimation.Rewind();
			m_AppearAnimation.ChangeEndValue(alpha);
			m_AppearAnimation.Play();
		}
		else
		{
			m_AppearAnimation.Pause();
			m_AppearAnimation.Rewind();
			m_DisappearAnimation.Play().OnComplete(delegate
			{
				base.gameObject.SetActive(value: false);
				m_isShowed = false;
			});
		}
	}

	public void SetPosition(Vector3 targetPosition)
	{
		base.transform.position = targetPosition;
	}
}
