using DG.Tweening;
using Kingmaker.UI.MVVM.VM.Overtips.CombatText;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Overtips.CombatText;

public class CombatTextHitPointsView : CombatTextEntityBaseView<CombatMessageBase>
{
	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private Color m_DamageColor;

	[SerializeField]
	private Color m_CritDamageColor;

	[SerializeField]
	private Color m_HealColor;

	[SerializeField]
	private Color m_MissColor;

	[SerializeField]
	private float m_MoveDelta;

	[SerializeField]
	private float m_DamageScaleDelta;

	[SerializeField]
	private float m_CritScaleDelta;

	[SerializeField]
	private float m_MissScaleDelta;

	[SerializeField]
	private float m_MoveTime;

	[SerializeField]
	private Ease m_MoveEase;

	private Sequence m_AnimatioinSequence;

	private Tweener m_MoveTween;

	private float m_Scale;

	protected override float GetXPos()
	{
		return 0f;
	}

	public override string GetText()
	{
		return m_Text.text;
	}

	protected override void DoData(CombatMessageBase combatMessage)
	{
		m_Text.text = combatMessage.GetText();
		if (!(combatMessage is CombatMessageDamage combatMessageDamage))
		{
			if (!(combatMessage is CombatMessageAttackMiss))
			{
				if (combatMessage is CombatMessageHealing)
				{
					m_Text.color = m_HealColor;
					m_Scale = 1f;
				}
			}
			else
			{
				m_Text.color = m_MissColor;
				m_Scale = m_MissScaleDelta;
			}
		}
		else
		{
			m_Text.color = (combatMessageDamage.IsCritical ? m_CritDamageColor : m_DamageColor);
			m_Scale = (combatMessageDamage.IsCritical ? m_CritScaleDelta : m_DamageScaleDelta);
		}
	}

	protected override void DoShow()
	{
		base.Rect.anchoredPosition = Vector2.zero;
		base.Rect.localScale = Vector3.one;
		base.CanvasGroup.alpha = 0f;
		m_AnimatioinSequence = DOTween.Sequence().SetUpdate(isIndependentUpdate: true).Pause()
			.SetAutoKill(autoKillOnCompletion: true);
		m_AnimatioinSequence.Append(base.CanvasGroup.DOFade(1f, ShowFadeTime).SetUpdate(isIndependentUpdate: true));
		m_AnimatioinSequence.Join(base.Rect.DOScale(m_Scale, ShowFadeTime));
		m_AnimatioinSequence.Append(base.CanvasGroup.DOFade(0f, Duration));
		m_AnimatioinSequence.Join(base.Rect.DOScale(1f, Duration));
	}

	public void SetDirection(Vector2 direction, bool single, bool even)
	{
		Vector2 vector = new Vector2(Random.Range(-15f, 15f), Random.Range(20f, 30f));
		if (!single)
		{
			float x = (even ? Random.Range(-30f, 0f) : Random.Range(0f, 30f));
			vector = new Vector2(x, Random.Range(20f, 30f));
			base.Rect.anchoredPosition = new Vector3(x, Random.Range(-15f, 15f), 0f);
		}
		Vector2 vector2 = base.Rect.anchoredPosition + direction.normalized * m_MoveDelta + vector;
		m_MoveTween = base.Rect.DOLocalMove(vector2, m_MoveTime).SetEase(m_MoveEase).SetUpdate(isIndependentUpdate: true)
			.SetAutoKill(autoKillOnCompletion: true);
	}

	public void PlayAnimation()
	{
		m_AnimatioinSequence.Play();
	}

	public override void Dispose()
	{
		m_AnimatioinSequence.Kill();
		m_AnimatioinSequence = null;
		m_MoveTween.Kill();
		m_MoveTween = null;
		base.Dispose();
	}
}
