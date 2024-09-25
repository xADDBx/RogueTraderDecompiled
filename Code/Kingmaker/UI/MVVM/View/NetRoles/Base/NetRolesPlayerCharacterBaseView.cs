using DG.Tweening;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.DragNDrop;
using Kingmaker.UI.MVVM.VM.NetRoles;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.NetRoles.Base;

public class NetRolesPlayerCharacterBaseView : ViewBase<NetRolesPlayerCharacterVM>, INetRoleHighlight, ISubscriber
{
	[SerializeField]
	protected Image m_Portrait;

	public PhotonActorNumber PlayerOwner => base.ViewModel.PlayerOwner;

	public UnitReference Character => base.ViewModel.Character;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(base.ViewModel.PlayerRoleMe.Subscribe(m_Portrait.gameObject.SetActive));
		AddDisposable(base.ViewModel.Portrait.Subscribe(delegate(Sprite value)
		{
			base.gameObject.SetActive(value != null);
			if ((bool)value)
			{
				m_Portrait.sprite = value;
			}
		}));
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
		DragNDropManager.Instance.Or(null)?.CancelDrag();
	}

	public void HandleRoleHighlight(UnitReference unit, bool highlight)
	{
		CanvasGroup target = this.EnsureComponent<CanvasGroup>();
		if (highlight)
		{
			target.DOFade(unit.Equals(Character) ? 1f : 0.5f, 0.2f);
		}
		else
		{
			target.DOFade(1f, 0.2f);
		}
	}
}
