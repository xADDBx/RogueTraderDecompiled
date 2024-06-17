using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Items;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Slots;

public class ItemSlotView<TViewModel> : VirtualListElementViewBase<TViewModel>, IWidgetView, IItemSlotView where TViewModel : ItemSlotVM
{
	[SerializeField]
	protected UsableSourceType UsableSource;

	[SerializeField]
	protected Image m_Icon;

	[SerializeField]
	private GameObject m_CountBackground;

	[SerializeField]
	private TextMeshProUGUI m_Count;

	[SerializeField]
	private OwlcatMultiSelectable m_ItemStatus;

	[SerializeField]
	private OwlcatMultiSelectable m_ItemGrade;

	[SerializeField]
	private GameObject m_UsableBgr;

	[Header("Raycast Zone")]
	[SerializeField]
	private RectTransform m_RaycastZone;

	[SerializeField]
	private Vector2 m_ExpandRaycastSize;

	[SerializeField]
	protected CanvasGroup m_BlinkMark;

	public ItemEntity Item => base.ViewModel?.Item?.Value;

	public ItemSlotVM SlotVM => base.ViewModel;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Item.Subscribe(delegate
		{
			RefreshItem();
		}));
		AddDisposable(Game.Instance.SelectionCharacter.SelectedUnitInUI.Subscribe(delegate
		{
			RefreshItem();
		}));
		AddDisposable(base.ViewModel.Icon.Subscribe(SetupIcon));
		AddDisposable(base.ViewModel.Count.Subscribe(delegate(int value)
		{
			SetupCount(value);
			SetupContextMenu();
		}));
		AddDisposable(base.ViewModel.IsUsable.Subscribe(SetupUsable));
		AddDisposable(base.ViewModel.ItemGrade.Subscribe(SetupGrade));
		AddDisposable(base.ViewModel.ItemStatus.Subscribe(delegate(ItemStatus value)
		{
			SetupStatus(value);
			SetupContextMenu();
		}));
		SetupDropZoneSize();
	}

	public virtual void RefreshItem()
	{
		SetupContextMenu();
		SetupGrade(base.ViewModel.ItemGrade.Value);
		SetupStatus(base.ViewModel.ItemStatus.Value);
		SetupUsable(base.ViewModel.IsUsable.Value);
	}

	protected virtual void SetupContextMenu()
	{
	}

	protected virtual void SetupIcon(Sprite value)
	{
		m_Icon.gameObject.SetActive(value != null);
		m_Icon.sprite = value;
	}

	private void SetupCount(int value)
	{
		if (!(m_Count == null))
		{
			if (m_CountBackground != null)
			{
				m_CountBackground.SetActive(value > 1);
			}
			m_Count.gameObject.SetActive(value > 1);
			m_Count.text = ((value > 1) ? base.ViewModel.Count.ToString() : string.Empty);
		}
	}

	private void SetupGrade(ItemGrade itemGrade)
	{
		if (!(m_ItemGrade == null))
		{
			m_ItemGrade.gameObject.SetActive(base.ViewModel.HasItem);
			m_ItemGrade.SetActiveLayer(itemGrade.ToString());
		}
	}

	private void SetupStatus(ItemStatus itemStatus)
	{
		if (!(m_ItemStatus == null))
		{
			m_ItemStatus.gameObject.SetActive(base.ViewModel.HasItem);
			m_ItemStatus.SetActiveLayer(itemStatus.ToString());
		}
	}

	private void SetupUsable(bool value)
	{
		if (!(m_UsableBgr == null))
		{
			m_UsableBgr.gameObject.SetActive(value);
		}
	}

	protected virtual void ClearView()
	{
		m_Icon.gameObject.SetActive(value: false);
		m_Icon.sprite = null;
		if (m_Count != null)
		{
			if (m_CountBackground != null)
			{
				m_CountBackground.SetActive(value: false);
			}
			m_Count.gameObject.SetActive(value: false);
			m_Count.text = string.Empty;
		}
	}

	private void SetupDropZoneSize()
	{
		if ((bool)m_RaycastZone)
		{
			m_RaycastZone.sizeDelta = m_ExpandRaycastSize;
		}
	}

	public RectTransform GetParentContainer()
	{
		RectTransform rectTransform = (RectTransform)base.transform;
		float width = rectTransform.rect.width;
		float height = rectTransform.rect.height;
		while (true)
		{
			if (!rectTransform)
			{
				return null;
			}
			if (rectTransform.rect.width > width && rectTransform.rect.height > height)
			{
				break;
			}
			rectTransform = (RectTransform)rectTransform.parent;
		}
		return rectTransform;
	}

	protected override void DestroyViewImplementation()
	{
		ClearView();
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as TViewModel);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel.GetType() == typeof(TViewModel);
	}
}
