using Owlcat.Runtime.UI.VirtualListSystem;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using UnityEngine;

namespace Owlcat.Runtime.UI.MVVM;

[RequireComponent(typeof(RectTransform))]
public abstract class VirtualListElementViewBase<TViewModel> : ViewBase<TViewModel>, IVirtualListElementView where TViewModel : VirtualListElementVMBase
{
	private RectTransform m_RectTransform;

	private Vector2 m_RectSizes;

	public RectTransform RectTransform => m_RectTransform ?? (m_RectTransform = GetComponent<RectTransform>());

	public virtual VirtualListLayoutElementSettings LayoutSettings => VirtualListLayoutElementSettings.None;

	public bool NeedRebuildToGetSize => true;

	public void BindVirtualList(IVirtualListElementData data)
	{
		if (data is TViewModel viewModel)
		{
			Bind(viewModel);
			base.ViewModel.HasView = true;
		}
	}

	public void UnbindVirtualList()
	{
		if (base.ViewModel != null)
		{
			base.ViewModel.HasView = false;
		}
		Unbind();
	}

	public IVirtualListElementView Instantiate()
	{
		return Object.Instantiate(this);
	}

	public void Update()
	{
		if (LayoutSettings.OverrideType == VirtualListLayoutElementSettings.LayoutOverrideType.UnityLayout && !m_RectSizes.Equals(RectTransform.rect.size))
		{
			LayoutSettings.SetDirty();
			m_RectSizes = RectTransform.rect.size;
		}
	}
}
