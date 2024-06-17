using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models;
using Kingmaker.UI.Sound;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Pause;

public class PauseNotification : MonoBehaviour, IPauseHandler, ISubscriber, IAreaHandler, IFullScreenUIHandler
{
	[SerializeField]
	private CanvasGroup m_PauseBlock;

	private Tweener m_Animation;

	[SerializeField]
	private TextMeshProUGUI m_PauseText;

	private bool m_CombatIsAlreadyStarted;

	private bool IsPaused
	{
		get
		{
			if (Game.Instance.IsPaused)
			{
				return !Game.Instance.PauseController.IsPausedByPlayers;
			}
			return false;
		}
	}

	public void OnPauseToggled()
	{
		PlayPause(IsPaused);
	}

	public void PlayPause(bool state)
	{
		if (state)
		{
			UISounds.Instance.Sounds.Systems.PauseSound.Play();
		}
		m_Animation?.Kill();
		m_Animation = m_PauseBlock.DOFade(state ? 1f : 0f, 0.2f).SetUpdate(isIndependentUpdate: true).SetDelay(state ? 0.2f : 0.0001f);
	}

	public void Initialize()
	{
		EventBus.Subscribe(this);
		m_PauseBlock.alpha = (IsPaused ? 1f : 0f);
		m_PauseText.text = UIStrings.Instance.CommonTexts.Pause;
	}

	public void Dispose()
	{
		EventBus.Unsubscribe(this);
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		m_PauseBlock.alpha = (IsPaused ? 1f : 0f);
	}

	public void OnDestroy()
	{
		Dispose();
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		m_PauseBlock.alpha = ((state && IsPaused) ? 0f : (IsPaused ? 1f : 0f));
	}
}
