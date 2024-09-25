using System;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.VM.NetLobby;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.NetLobby.Base;

public class NetLobbyPlayerBaseView : ViewBase<NetLobbyPlayerVM>
{
	[Space]
	[SerializeField]
	private Image m_Portrait;

	[SerializeField]
	protected GamerTagAndNameBaseView m_GamerTagAndName;

	[SerializeField]
	private _2dxFX_GrayScale m_GrayScale;

	[SerializeField]
	protected Image m_Plus;

	[SerializeField]
	private Image m_MeLayer;

	[SerializeField]
	protected OwlcatButton m_MainButton;

	[SerializeField]
	protected Image m_ProblemsWithPlayerAndHostDlcsMarker;

	public BoolReactiveProperty InviteButtonInteractable = new BoolReactiveProperty();

	public BoolReactiveProperty KickButtonInteractable = new BoolReactiveProperty();

	private IDisposable m_Disposable;

	public GamerTagAndNameBaseView GamerTagAndName => m_GamerTagAndName;

	public void Initialize()
	{
	}

	protected override void BindViewImplementation()
	{
		m_GamerTagAndName.Bind(base.ViewModel.GamerTagAndNameVM);
		AddDisposable(base.ViewModel.IsEmpty.Subscribe(delegate(bool value)
		{
			m_Portrait.gameObject.SetActive(!value);
			if (value)
			{
				m_ProblemsWithPlayerAndHostDlcsMarker.gameObject.SetActive(value: false);
			}
		}));
		AddDisposable(base.ViewModel.IsMeHost.CombineLatest(base.ViewModel.IsEmpty, base.ViewModel.IsMe, (bool host, bool empty, bool me) => new { host, empty, me }).Subscribe(value =>
		{
			InviteButtonInteractable.Value = value.empty;
			KickButtonInteractable.Value = value.host && !value.empty && !value.me;
			m_Plus.gameObject.SetActive(value.empty);
		}));
		AddDisposable(base.ViewModel.IsActive.Subscribe(delegate(bool value)
		{
			if (!(m_GrayScale == null))
			{
				m_GrayScale.EffectAmount = (value ? 0f : 0.8f);
				m_GrayScale.Alpha = (value ? 1f : 0.5f);
			}
		}));
		AddDisposable(base.ViewModel.IsMe.Subscribe(m_MeLayer.gameObject.SetActive));
		AddDisposable(base.ViewModel.Portrait.Subscribe(delegate(Sprite value)
		{
			m_Portrait.sprite = value;
		}));
		AddDisposable(base.ViewModel.Name.Subscribe(delegate(string value)
		{
			bool flag = !string.IsNullOrWhiteSpace(value);
			m_GamerTagAndName.ShowOrHide(flag);
			m_Disposable?.Dispose();
			m_Disposable = null;
			if (flag)
			{
				m_Disposable = m_Portrait.SetHint(value);
			}
		}));
		AddDisposable(base.ViewModel.PlayersDifferentDlcs.Subscribe(CheckProblemsWithPlayerAndHostDlcs));
	}

	protected override void DestroyViewImplementation()
	{
		m_Disposable?.Dispose();
		m_Disposable = null;
	}

	public string GetUserId()
	{
		return base.ViewModel.UserId.Value;
	}

	private void CheckProblemsWithPlayerAndHostDlcs(string dlcList)
	{
		if (!(m_ProblemsWithPlayerAndHostDlcsMarker == null))
		{
			bool active = !string.IsNullOrWhiteSpace(dlcList);
			m_ProblemsWithPlayerAndHostDlcsMarker.gameObject.SetActive(active);
			CheckProblemsWithPlayerAndHostDlcsImpl(dlcList);
		}
	}

	protected virtual void CheckProblemsWithPlayerAndHostDlcsImpl(string dlcList)
	{
	}
}
