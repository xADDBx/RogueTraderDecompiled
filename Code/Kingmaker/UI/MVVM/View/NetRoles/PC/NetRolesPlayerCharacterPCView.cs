using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.DragNDrop;
using Kingmaker.UI.MVVM.View.NetRoles.Base;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.UI.MVVM.View.NetRoles.PC;

public class NetRolesPlayerCharacterPCView : NetRolesPlayerCharacterBaseView, IDraggableElement
{
	[SerializeField]
	private OwlcatButton m_MoveRoleUp;

	[SerializeField]
	private OwlcatButton m_MoveRoleDown;

	[SerializeField]
	private RectTransform m_MoveRoleUpBackground;

	[SerializeField]
	private RectTransform m_MoveRoleDownBackground;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.CanUp.Subscribe(delegate(bool value)
		{
			m_MoveRoleUp.gameObject.SetActive(value);
			m_MoveRoleUpBackground.gameObject.SetActive(value);
		}));
		AddDisposable(base.ViewModel.CanDown.Subscribe(delegate(bool value)
		{
			m_MoveRoleDown.gameObject.SetActive(value);
			m_MoveRoleDownBackground.gameObject.SetActive(value);
		}));
		AddDisposable(m_MoveRoleUp.OnLeftClickAsObservable().Subscribe(base.ViewModel.MoveRoleCharacterUp));
		AddDisposable(m_MoveRoleDown.OnLeftClickAsObservable().Subscribe(base.ViewModel.MoveRoleCharacterDown));
		if (PhotonManager.Instance.IsRoomOwner)
		{
			AddDisposable(this.OnBeginDragAsObservable().Subscribe(OnBeginDrag));
			AddDisposable(this.OnDragAsObservable().Subscribe(OnDrag));
			AddDisposable(this.OnEndDragAsObservable().Subscribe(OnEndDrag));
			AddDisposable(this.OnDropAsObservable().Subscribe(OnDrop));
		}
	}

	private void MoveCharacter(PhotonActorNumber player)
	{
		base.ViewModel.MoveCharacter(player);
	}

	private void OnBeginDrag(PointerEventData eventData)
	{
		EventBus.RaiseEvent(delegate(IDragDropEventUIHandler h)
		{
			h.OnBeginDrag(eventData, base.gameObject);
		});
	}

	private void OnDrag(PointerEventData eventData)
	{
		EventBus.RaiseEvent(delegate(IDragDropEventUIHandler h)
		{
			h.OnDrag(eventData);
		});
	}

	private void OnEndDrag(PointerEventData eventData)
	{
		EventBus.RaiseEvent(delegate(IDragDropEventUIHandler h)
		{
			h.OnEndDrag(eventData);
		});
	}

	private void OnDrop(PointerEventData eventData)
	{
		EventBus.RaiseEvent(delegate(IDragDropEventUIHandler h)
		{
			h.OnDrop(eventData, base.gameObject);
		});
	}

	public void StartDrag()
	{
		EventBus.RaiseEvent(delegate(INetRoleHighlight h)
		{
			h.HandleRoleHighlight(base.Character, highlight: true);
		});
	}

	public void EndDrag(PointerEventData eventData)
	{
		EventBus.RaiseEvent(delegate(INetRoleHighlight h)
		{
			h.HandleRoleHighlight(base.Character, highlight: false);
		});
		GameObject dropTarget = DragNDropManager.DropTarget;
		if (!(dropTarget == null))
		{
			NetRolesPlayerCharacterBaseView component = dropTarget.GetComponent<NetRolesPlayerCharacterBaseView>();
			if (component != null && component.Character.Equals(base.Character))
			{
				MoveCharacter(component.PlayerOwner);
			}
		}
	}

	public bool SetDragSlot(DragNDropManager slot)
	{
		if (!base.ViewModel.PlayerRoleMe.Value)
		{
			return false;
		}
		slot.Icon.sprite = base.ViewModel.Portrait.Value;
		slot.Count.text = string.Empty;
		slot.OverideSize = new Vector2(81f, 106f);
		return true;
	}

	public void CancelDrag()
	{
	}
}
