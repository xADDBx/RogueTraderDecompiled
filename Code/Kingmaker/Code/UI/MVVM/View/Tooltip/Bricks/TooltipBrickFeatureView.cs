using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickFeatureView : TooltipBaseBrickView<TooltipBrickFeatureVM>, IUICultAmbushVisibilityChangeHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IUpdateContainerElements
{
	[SerializeField]
	protected TextMeshProUGUI m_Label;

	[SerializeField]
	protected GameObject m_Background;

	[SerializeField]
	private HorizontalLayoutGroup m_HorizontalLayoutGroup;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	protected GameObject m_IconBlock;

	[Header("Acronym")]
	[SerializeField]
	private TextMeshProUGUI m_Acronym;

	[SerializeField]
	private TalentGroupView m_TalentGroupView;

	[SerializeField]
	private Color m_DefaultAcronymColor;

	[Header("Hidden")]
	[SerializeField]
	private bool m_HasHiddenPart;

	[ConditionalShow("m_HasHiddenPart")]
	[SerializeField]
	private CanvasGroup m_HiddenBlock;

	[ConditionalShow("m_HasHiddenPart")]
	[SerializeField]
	private CanvasGroup m_DefaultBlock;

	[SerializeField]
	private float m_DefaultIconSize = 62f;

	private AccessibilityTextHelper m_TextHelper;

	protected override void BindViewImplementation()
	{
		m_TextHelper = new AccessibilityTextHelper(m_Label);
		base.BindViewImplementation();
		m_Label.text = base.ViewModel.Name;
		m_IconBlock.SetActive((bool)base.ViewModel.Icon || base.ViewModel.Acronym != null);
		if ((bool)base.ViewModel.Icon || base.ViewModel.Acronym != null)
		{
			m_Icon.sprite = base.ViewModel.Icon;
			m_Icon.color = base.ViewModel.IconColor;
			m_HorizontalLayoutGroup.childAlignment = TextAnchor.MiddleLeft;
		}
		else
		{
			m_HorizontalLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
		}
		m_Acronym.text = base.ViewModel.Acronym;
		TextMeshProUGUI acronym = m_Acronym;
		TalentIconInfo talentIconsInfo = base.ViewModel.TalentIconsInfo;
		acronym.color = ((talentIconsInfo != null && talentIconsInfo.HasGroups) ? UIConfig.Instance.GroupAcronymColor : m_DefaultAcronymColor);
		if (!base.ViewModel.IsHidden)
		{
			SetTooltip();
		}
		if ((bool)m_Background)
		{
			m_Background.SetActive(base.ViewModel.AvailableBackground);
		}
		m_TalentGroupView.Or(null)?.SetupView(base.ViewModel.TalentIconsInfo);
		m_TextHelper.UpdateTextSize();
		if (m_HasHiddenPart)
		{
			SwitchBlockVisibility(m_HiddenBlock, base.ViewModel.IsHidden, immediate: true);
			SwitchBlockVisibility(m_DefaultBlock, !base.ViewModel.IsHidden, immediate: true);
		}
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_TextHelper.Dispose();
		UpdateElements(m_DefaultIconSize);
	}

	private void SetTooltip()
	{
		AddDisposable(this.SetTooltip(base.ViewModel.Tooltip, new TooltipConfig
		{
			PriorityPivots = new List<Vector2>
			{
				new Vector2(1f, 0.5f),
				new Vector2(0f, 0.5f)
			}
		}));
	}

	private void SwitchBlockVisibility(CanvasGroup block, bool isVisible, bool immediate = false)
	{
		if (immediate)
		{
			block.alpha = (isVisible ? 1f : 0f);
		}
		else
		{
			block.DOFade(isVisible ? 1f : 0f, 1f);
		}
	}

	public void HandleCultAmbushVisibilityChange()
	{
		if (!m_HasHiddenPart)
		{
			return;
		}
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null && baseUnitEntity == base.ViewModel.Caster)
		{
			bool flag = true;
			if (base.ViewModel.HasAbility)
			{
				flag = base.ViewModel.Ability.CultAmbushVisibility(baseUnitEntity) == UnitPartCultAmbush.VisibilityStatuses.NotVisible;
			}
			else if (base.ViewModel.HasFeature)
			{
				flag = base.ViewModel.Feature.CultAmbushVisibility(baseUnitEntity) == UnitPartCultAmbush.VisibilityStatuses.NotVisible;
			}
			SwitchBlockVisibility(m_HiddenBlock, flag);
			SwitchBlockVisibility(m_DefaultBlock, !flag);
			if (!flag)
			{
				SetTooltip();
				base.ViewModel.UpdateCultAmbushVisibility();
			}
		}
	}

	public void UpdateElements(float size)
	{
		RectTransform labelTransform = m_Label.GetComponent<RectTransform>();
		labelTransform.anchoredPosition = new Vector2(size, 0f);
		DelayedInvoker.InvokeInFrames(delegate
		{
			float width = GetComponent<RectTransform>().rect.width;
			labelTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width - size);
		}, 1);
	}
}
