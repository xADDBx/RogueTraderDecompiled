using System;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.View.NetLobby.Base;
using Kingmaker.UI.MVVM.VM.NetRoles;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.NetRoles.Base;

public class NetRolesPlayerBaseView : ViewBase<NetRolesPlayerVM>
{
	[SerializeField]
	private Image m_PlayerAvatar;

	[SerializeField]
	private GameObject m_MeLayer;

	[SerializeField]
	protected GamerTagAndNameBaseView m_GamerTagAndName;

	private IDisposable m_Disposable;

	public GamerTagAndNameBaseView GamerTagAndName => m_GamerTagAndName;

	public virtual void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		m_GamerTagAndName.Bind(base.ViewModel.GamerTagAndNameVM);
		AddDisposable(base.ViewModel.IsMe.Subscribe(delegate(bool value)
		{
			m_MeLayer.Or(null)?.SetActive(value);
		}));
		AddDisposable(base.ViewModel.Portrait.Subscribe(delegate(Sprite value)
		{
			m_PlayerAvatar.sprite = value;
		}));
		AddDisposable(base.ViewModel.Name.Subscribe(delegate(string value)
		{
			m_Disposable?.Dispose();
			m_Disposable = null;
			m_Disposable = m_PlayerAvatar.SetHint(value);
		}));
	}

	protected override void DestroyViewImplementation()
	{
		m_Disposable?.Dispose();
		m_Disposable = null;
		base.gameObject.SetActive(value: false);
	}

	public string GetUserId()
	{
		return base.ViewModel.UserId.Value;
	}
}
