using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Persistence;
using UnityEngine;

namespace Kingmaker.UI.Canvases;

public class CanvasAnimation : MonoBehaviour, ILoadingScreen
{
	[Serializable]
	public class CanvasEntry
	{
		public CanvasGroup Canvas;

		public float FadeTime;

		public float UnfadeTime;

		public float FadeDelay;

		public float UnfadeDelay;
	}

	[SerializeField]
	[UsedImplicitly]
	public CanvasEntry[] Canvases;

	private LoadingScreenState m_State;

	[NotNull]
	private readonly List<Tween> m_RunningTweens = new List<Tween>();

	public void ShowLoadingScreen()
	{
		if (m_State != LoadingScreenState.Shown && m_State != LoadingScreenState.ShowAnimation)
		{
			m_RunningTweens.Clear();
			m_State = LoadingScreenState.ShowAnimation;
			CanvasEntry[] canvases = Canvases;
			foreach (CanvasEntry canvasEntry in canvases)
			{
				Tweener item = canvasEntry.Canvas.DOFade(1f, canvasEntry.FadeTime).SetDelay(canvasEntry.FadeDelay).SetUpdate(isIndependentUpdate: true);
				m_RunningTweens.Add(item);
			}
		}
	}

	public void HideLoadingScreen()
	{
		if (m_State != 0 && m_State != LoadingScreenState.HideAnimation)
		{
			m_RunningTweens.Clear();
			m_State = LoadingScreenState.HideAnimation;
			CanvasEntry[] canvases = Canvases;
			foreach (CanvasEntry canvasEntry in canvases)
			{
				Tweener item = canvasEntry.Canvas.DOFade(0f, canvasEntry.UnfadeTime).SetDelay(canvasEntry.UnfadeDelay).SetUpdate(isIndependentUpdate: true);
				m_RunningTweens.Add(item);
			}
		}
	}

	private void Update()
	{
		if (m_State == LoadingScreenState.ShowAnimation && m_RunningTweens.All((Tween t) => !t.IsActive()))
		{
			m_State = LoadingScreenState.Shown;
		}
		if (m_State == LoadingScreenState.HideAnimation && m_RunningTweens.All((Tween t) => !t.IsActive()))
		{
			m_State = LoadingScreenState.Hidden;
		}
	}

	public LoadingScreenState GetLoadingScreenState()
	{
		return m_State;
	}
}
