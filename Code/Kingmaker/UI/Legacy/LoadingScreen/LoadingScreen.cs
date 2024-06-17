using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Utility.Random;
using RogueTrader.Code.ShaderConsts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.Legacy.LoadingScreen;

public class LoadingScreen : MonoBehaviour, ILoadingScreen
{
	[Serializable]
	public class SettingTypeScreens
	{
		public BlueprintArea.SettingType Type;

		public List<Sprite> Sprites;
	}

	public LoadingScreenHints Hints;

	public TextMeshProUGUI Hint;

	[UsedImplicitly]
	[SerializeField]
	private List<Image> m_Points;

	[Header("Content")]
	public GameObject MapContainer;

	public Image Picture;

	public Sprite TempLoadingPicture;

	public List<SettingTypeScreens> SettingTypeScreensList;

	[Header("Character")]
	public Image CharacterPortrait;

	public TextMeshProUGUI CharacterNameText;

	public TextMeshProUGUI CharacterDesctiptionText;

	[Header("Variables")]
	public float FadeTime = 1f;

	public float LoadingDissolveTime = 2f;

	public float HidingDissolveTime = 0.5f;

	public float MinDissolveStep = 0.02f;

	public float MaxDissolveStep = 0.1f;

	public float HidingMaxDissolveStep = 0.3f;

	private bool m_ShowDissolve;

	private float m_CurrentThreshold;

	private Material m_PictureMaterial;

	private float m_CurrentTime;

	private LoadingScreenState m_State;

	private CanvasGroup m_CanvasGroup;

	[CanBeNull]
	private Tween m_HideTween;

	private Sequence m_PointSequence;

	private CanvasGroup CanvasGroup
	{
		get
		{
			if (!m_CanvasGroup)
			{
				return m_CanvasGroup = GetComponent<CanvasGroup>();
			}
			return m_CanvasGroup;
		}
	}

	[UsedImplicitly]
	private void Awake()
	{
		CanvasGroup.blocksRaycasts = false;
		CanvasGroup.alpha = 0f;
		m_PictureMaterial = Picture.material;
		m_State = LoadingScreenState.Hidden;
	}

	public void ShowLoadingScreen()
	{
		CanvasGroup.blocksRaycasts = true;
		CanvasGroup.alpha = 1f;
		Hint.text = Hints.TakeHint(LoadingScreenHints.LocationEnum.MainMenuHints, PFStatefulRandom.NonDeterministic);
		m_State = LoadingScreenState.ShowAnimation;
		StartCoroutine(ShowCoroutine());
		int num = 1;
		m_PointSequence = DOTween.Sequence();
		foreach (Image point in m_Points)
		{
			Color color = point.color;
			color.a = 0f;
			point.color = color;
			m_PointSequence.Append(point.DOFade(1f, 0.4f).SetUpdate(isIndependentUpdate: true).SetDelay(0.4f * (float)num++));
		}
		m_PointSequence.Play().SetUpdate(isIndependentUpdate: true).SetLoops(-1, LoopType.Restart);
		m_CurrentThreshold = 1f;
		if (!m_ShowDissolve)
		{
			m_CurrentThreshold = 1f;
			m_PictureMaterial.SetFloat(ShaderProps._Threshold, m_CurrentThreshold);
			MapContainer.SetActive(value: false);
			Picture.gameObject.SetActive(value: false);
		}
	}

	public void HideLoadingScreen()
	{
		m_State = LoadingScreenState.HideAnimation;
		if (!m_ShowDissolve)
		{
			m_HideTween = CanvasGroup.DOFade(0f, FadeTime).OnComplete(DoHide).SetUpdate(isIndependentUpdate: true);
		}
	}

	public void Update()
	{
		if (!m_ShowDissolve || m_State < LoadingScreenState.Shown)
		{
			return;
		}
		if (Mathf.Abs(m_CurrentThreshold) < 1E-06f)
		{
			if (m_State == LoadingScreenState.HideAnimation && m_HideTween == null)
			{
				m_HideTween = CanvasGroup.DOFade(0f, FadeTime).OnComplete(DoHide).SetUpdate(isIndependentUpdate: true);
			}
			return;
		}
		m_CurrentTime += Time.unscaledDeltaTime;
		float num = ((m_State == LoadingScreenState.HideAnimation) ? HidingDissolveTime : LoadingDissolveTime);
		if (m_CurrentTime >= num * MinDissolveStep)
		{
			float minDissolveStep = MinDissolveStep;
			float max = ((m_State == LoadingScreenState.HideAnimation) ? HidingMaxDissolveStep : MaxDissolveStep);
			float num2 = Mathf.Clamp(m_CurrentTime / num, minDissolveStep, max);
			m_CurrentThreshold -= num2;
			m_CurrentThreshold = Mathf.Clamp01(m_CurrentThreshold);
			m_PictureMaterial.SetFloat(ShaderProps._Threshold, m_CurrentThreshold);
			m_CurrentTime = 0f;
		}
	}

	private void DoHide()
	{
		m_CurrentTime = 0f;
		m_ShowDissolve = false;
		m_HideTween.Kill();
		m_HideTween = null;
		m_PointSequence.Kill();
		m_PointSequence = null;
		m_State = LoadingScreenState.Hidden;
		CanvasGroup.blocksRaycasts = false;
	}

	private IEnumerator ShowCoroutine()
	{
		yield return null;
		m_State = LoadingScreenState.Shown;
	}

	public LoadingScreenState GetLoadingScreenState()
	{
		return m_State;
	}
}
