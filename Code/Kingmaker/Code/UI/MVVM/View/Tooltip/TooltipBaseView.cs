using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.Code.UI.MVVM.VM.Tooltip;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip;

public abstract class TooltipBaseView : InfoBaseView<TooltipVM>, IWidgetView
{
	[SerializeField]
	private List<Image> m_Backgrounds;

	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	protected LayoutElement m_LayoutElement;

	[SerializeField]
	protected LayoutElement m_BodyLayoutElement;

	[SerializeField]
	protected ScrollRectExtended m_BodyScroll;

	[SerializeField]
	private GameObject m_Separator;

	[SerializeField]
	protected float m_MaxHeight = 710f;

	[SerializeField]
	protected VerticalLayoutGroup m_ContentVerticalLayoutGroup;

	protected Tweener m_ShowTween;

	protected bool IsShowed;

	protected CanvasGroup CanvasGroup => m_CanvasGroup ?? (m_CanvasGroup = base.gameObject.EnsureComponent<CanvasGroup>());

	public MonoBehaviour MonoBehaviour => this;

	public virtual void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if (base.ViewModel != null)
		{
			m_Separator.SetActive(base.ViewModel.HeaderBricks.Any() && base.ViewModel.BodyBricks.Any());
			base.gameObject.SetActive(value: true);
			CanvasGroup.alpha = (base.ViewModel.IsComparative ? 1 : 0);
			SetBackground();
			SetupInteractionHint();
			if (m_ContentVerticalLayoutGroup != null)
			{
				m_ContentVerticalLayoutGroup.spacing = base.ViewModel.ContentSpacing;
			}
			if (m_BodyScroll != null)
			{
				bool active = base.ViewModel.HasScroll && m_BodyContainer.gameObject.activeSelf;
				m_BodyScroll.enabled = active;
				m_BodyScroll.verticalScrollbar.gameObject.SetActive(active);
			}
			Show();
		}
	}

	protected virtual void SetupInteractionHint()
	{
	}

	protected virtual void SetHeight()
	{
		float num = CalculateBodyHeight();
		if (base.ViewModel.PreferredHeight > 0)
		{
			m_LayoutElement.preferredHeight = base.ViewModel.PreferredHeight;
			m_BodyLayoutElement.preferredHeight = num;
			return;
		}
		float num2 = ((RectTransform)base.transform).rect.height + num;
		float num3 = ((base.ViewModel.MaxHeight > 0) ? ((float)base.ViewModel.MaxHeight) : m_MaxHeight);
		if (num3 > 0f && num2 > num3)
		{
			m_LayoutElement.preferredHeight = num3;
			float num4 = num2 - num3;
			float preferredHeight = num - num4;
			m_BodyLayoutElement.preferredHeight = preferredHeight;
		}
		else
		{
			m_BodyLayoutElement.preferredHeight = num;
		}
	}

	protected virtual string GetInteractionHint()
	{
		return string.Empty;
	}

	protected abstract void Show();

	protected float CalculateBodyHeight()
	{
		return CalculateHeight(m_BodyContainer);
	}

	protected float CalculateHeight(RectTransform parent)
	{
		float num = 0f;
		VerticalLayoutGroup component = parent.GetComponent<VerticalLayoutGroup>();
		float num2 = (component ? component.spacing : 0f);
		foreach (RectTransform item in parent)
		{
			if (item.gameObject.activeSelf)
			{
				LayoutElement component2 = item.GetComponent<LayoutElement>();
				if ((object)component2 == null || !component2.ignoreLayout)
				{
					num += item.rect.height + num2;
				}
			}
		}
		return num;
	}

	private void SetBackground()
	{
		if (m_Backgrounds == null || !m_Backgrounds.Any())
		{
			return;
		}
		Color32 color = base.ViewModel.Background switch
		{
			TooltipBackground.White => Game.Instance.BlueprintRoot.UIConfig.TooltipColors.WhiteBackground, 
			TooltipBackground.Red => Game.Instance.BlueprintRoot.UIConfig.TooltipColors.WhiteBackground, 
			TooltipBackground.Yellow => Game.Instance.BlueprintRoot.UIConfig.TooltipColors.YellowBackground, 
			TooltipBackground.Gray => Game.Instance.BlueprintRoot.UIConfig.TooltipColors.GrayBackground, 
			_ => Color.white, 
		};
		foreach (Image background in m_Backgrounds)
		{
			if (!(background == null))
			{
				background.color = color;
			}
		}
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		CanvasGroup.alpha = 0f;
		m_LayoutElement.preferredHeight = -1f;
		if ((bool)m_BodyLayoutElement)
		{
			m_BodyLayoutElement.preferredHeight = -1f;
		}
		base.gameObject.SetActive(value: false);
		m_ShowTween?.Kill();
		m_ShowTween = null;
	}

	public void Scroll(InputActionEventData obj, float value)
	{
		if (!(m_BodyScroll == null))
		{
			m_BodyScroll.Scroll(value * m_BodyScroll.scrollSensitivity, smooth: true);
		}
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as TooltipVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is TooltipVM;
	}
}
