using System;
using System.Collections;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.VM.SpaceCombat.Components;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Warhammer.SpaceCombat.StarshipLogic.Posts;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.Base;

public class ShipPostBaseView : ViewBase<ShipPostVM>
{
	[Serializable]
	public struct ShipPostTypeIcon
	{
		public PostType Type;

		public Sprite Icon;
	}

	[Header("Common")]
	[SerializeField]
	protected Image m_Portrait;

	[SerializeField]
	protected Image m_PostIcon;

	[SerializeField]
	protected ShipPostTypeIcon[] m_PostTypeIcons;

	[SerializeField]
	private CanvasGroup m_PostBlock;

	[SerializeField]
	private TextMeshProUGUI m_BlockDuration;

	[SerializeField]
	private GameObject m_PostBlockFX;

	private Coroutine m_FXRoutine;

	private Tweener m_PostBlockTw;

	protected override void BindViewImplementation()
	{
		FadePost(show: false, 1f);
		m_PostBlockFX.SetActive(value: false);
		m_Portrait.sprite = base.ViewModel.Portrait;
		SetPostTypeIcon();
		AddDisposable(base.ViewModel.IsPostBlocked.Subscribe(ShowPostBlocked));
		AddDisposable(base.ViewModel.BlockDuration.Subscribe(delegate(string val)
		{
			m_BlockDuration.text = val;
		}));
		AddDisposable(base.ViewModel.FXActivated.Subscribe(StartFX));
	}

	protected override void DestroyViewImplementation()
	{
		m_PostBlockTw?.Kill();
		if (m_FXRoutine != null)
		{
			StopCoroutine(m_FXRoutine);
		}
	}

	private void ShowPostBlocked(bool val)
	{
		FadePost(val, 2f);
		StartFX(val);
	}

	private void FadePost(bool show, float duration)
	{
		m_PostBlockTw?.Kill();
		m_PostBlock.gameObject.SetActive(show);
		m_PostBlock.DOFade(show ? 1f : 0f, duration);
	}

	private void StartFX(bool value)
	{
		if (value)
		{
			m_FXRoutine = StartCoroutine(BlockFX());
		}
	}

	private IEnumerator BlockFX()
	{
		m_PostBlockFX.SetActive(value: true);
		yield return new WaitForSeconds(2f);
		m_PostBlockFX.SetActive(value: false);
	}

	private void SetPostTypeIcon()
	{
		Sprite postIconByPostType = GetPostIconByPostType(base.ViewModel.PostType);
		m_PostIcon.sprite = postIconByPostType;
		m_PostIcon.enabled = postIconByPostType != null;
	}

	private Sprite GetPostIconByPostType(PostType type)
	{
		ShipPostTypeIcon[] postTypeIcons = m_PostTypeIcons;
		for (int i = 0; i < postTypeIcons.Length; i++)
		{
			ShipPostTypeIcon shipPostTypeIcon = postTypeIcons[i];
			if (shipPostTypeIcon.Type == type)
			{
				return shipPostTypeIcon.Icon;
			}
		}
		return null;
	}
}
