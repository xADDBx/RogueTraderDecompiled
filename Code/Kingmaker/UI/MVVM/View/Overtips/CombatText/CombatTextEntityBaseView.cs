using System;
using DG.Tweening;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.UI.MVVM.VM.Overtips.CombatText;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Overtips.CombatText;

public abstract class CombatTextEntityBaseView<TCombatMessege> : MonoBehaviour where TCombatMessege : CombatMessageBase
{
	[SerializeField]
	public float Duration = 3f;

	private TimeSpan m_TargetTime;

	[SerializeField]
	protected float ShowFadeTime = 0.3f;

	private bool m_DisposeSoon;

	private Action m_Endcallback;

	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	public CanvasGroup CanvasGroup => m_CanvasGroup;

	public RectTransform Rect => base.transform as RectTransform;

	public Vector2 Size => Rect.sizeDelta;

	public float XPos => GetXPos();

	protected abstract float GetXPos();

	public abstract string GetText();

	public virtual void Initialize()
	{
		m_CanvasGroup.alpha = 0f;
	}

	public void SetData(TCombatMessege combatMessege, Action endcallback = null)
	{
		m_DisposeSoon = false;
		DoData(combatMessege);
		DoShow();
		m_TargetTime = Game.Instance.TimeController.GameTime + Duration.Seconds();
		m_Endcallback = endcallback;
	}

	protected abstract void DoData(TCombatMessege combatMessage);

	protected virtual void DoShow()
	{
		if (Game.Instance.IsPaused)
		{
			CanvasGroup.alpha = 1f;
			return;
		}
		CanvasGroup.alpha = 0f;
		CanvasGroup.DOFade(1f, ShowFadeTime).SetUpdate(isIndependentUpdate: true);
	}

	[UsedImplicitly]
	private void Update()
	{
		if (!LoadingProcess.Instance.IsLoadingInProcess && !m_DisposeSoon && m_TargetTime <= Game.Instance.TimeController.GameTime)
		{
			CanvasGroup.DOFade(0f, ShowFadeTime).OnComplete(Dispose);
			m_DisposeSoon = true;
		}
	}

	public virtual void Dispose()
	{
		m_Endcallback?.Invoke();
		m_Endcallback = null;
	}
}
