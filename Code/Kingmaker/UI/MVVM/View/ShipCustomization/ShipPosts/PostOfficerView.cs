using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.VM.ShipCustomization.Posts;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup.View;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.ShipCustomization.ShipPosts;

public class PostOfficerView : SelectionGroupEntityView<PostOfficerVM>, IFunc01ClickHandler, IConsoleEntity, IWidgetView
{
	[SerializeField]
	protected PostAbilityView m_UnitBuffForShip;

	[SerializeField]
	protected Image m_Portrait;

	[SerializeField]
	protected GameObject EmptyBlock;

	[SerializeField]
	public GameObject SkillValueBlock;

	[SerializeField]
	protected GameObject m_PostImageBlock;

	[SerializeField]
	protected Image m_PostImageIcon;

	[Header("Skill Value")]
	[SerializeField]
	public TextMeshProUGUI SkillValue;

	[SerializeField]
	public Image SkillHintPlace;

	[SerializeField]
	public OwlcatMultiSelectable SkillValueState;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		SetupEmptyState();
		AddDisposable(base.ViewModel.DataUpdated.Subscribe(delegate
		{
			UpdateOfficerForPost();
		}));
		UpdateOfficerForPost();
	}

	protected override void DestroyViewImplementation()
	{
	}

	protected void SetupEmptyState()
	{
		if (base.ViewModel.Unit == null)
		{
			m_Portrait.gameObject.SetActive(value: false);
			SkillValueBlock.SetActive(value: false);
			EmptyBlock.SetActive(value: true);
			m_PostImageBlock.gameObject.SetActive(value: false);
			m_Portrait.sprite = null;
			m_Button.SetActiveLayer("Empty");
			m_Button.Interactable = false;
		}
		else
		{
			m_Portrait.gameObject.SetActive(value: true);
			SkillValueBlock.SetActive(value: true);
			EmptyBlock.SetActive(value: false);
			m_PostImageBlock.gameObject.SetActive(value: true);
			m_Portrait.sprite = base.ViewModel.Portrait;
			m_Button.SetActiveLayer(base.ViewModel.IsSelected.Value ? "Selected" : "Normal");
			SkillValueState.SetActiveLayer(0);
			m_Button.Interactable = true;
			AddDisposable(this.SetTooltip(base.ViewModel.TooltipTemplate(), new TooltipConfig
			{
				TooltipPlace = (base.transform.parent.transform.parent.transform as RectTransform),
				PriorityPivots = new List<Vector2>
				{
					new Vector2(1f, 0.5f)
				}
			}));
			AddDisposable(SkillHintPlace.SetHint(base.ViewModel.SkillName + " — " + UIStrings.Instance.ShipCustomization.SkillRequired.Text));
		}
	}

	private void UpdateOfficerForPost()
	{
		SkillValue.text = base.ViewModel.SkillValue.ToString();
		m_UnitBuffForShip.Bind(base.ViewModel.UnitAbility);
		m_UnitBuffForShip.gameObject.SetActive(base.ViewModel.UnitAbility?.Ability != null);
		m_PostImageIcon.sprite = base.ViewModel.PostSprite;
		m_PostImageIcon.gameObject.SetActive(base.ViewModel.PostSprite != null);
		SkillValueState.SetActiveLayer((int)base.ViewModel.SkillRecommendation);
	}

	protected override void OnClick()
	{
		base.OnClick();
		if (base.ViewModel.IsSelected.Value)
		{
			base.ViewModel.DoSelect();
		}
		else
		{
			base.ViewModel.DoUnselect();
		}
	}

	protected override string GetConfirmActionName()
	{
		if (!base.ViewModel.IsSelected.Value)
		{
			return UIStrings.Instance.ShipCustomization.AppointOnPost.Text;
		}
		return UIStrings.Instance.ShipCustomization.RemoveFromPost.Text;
	}

	public bool CanFunc01Click()
	{
		return base.ViewModel.Unit != null;
	}

	public void OnFunc01Click()
	{
		TooltipHelper.ShowInfo(base.ViewModel.TooltipTemplate());
	}

	public string GetFunc01ClickHint()
	{
		return UIStrings.Instance.CommonTexts.Information.Text;
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as PostOfficerVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is PostOfficerVM;
	}
}
