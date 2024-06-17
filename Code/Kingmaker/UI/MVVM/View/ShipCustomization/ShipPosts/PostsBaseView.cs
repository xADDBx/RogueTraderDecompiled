using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.MVVM.VM.ShipCustomization.Posts;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.ShipCustomization.ShipPosts;

public class PostsBaseView : ViewBase<ShipPostsVM>
{
	[SerializeField]
	protected FadeAnimator m_FadeAnimator;

	[SerializeField]
	public PostSelectorView PostSelectorView;

	[SerializeField]
	public PostOfficerSelectorView PostOfficerSelector;

	[Header("Post Description")]
	[Header("Titles")]
	[SerializeField]
	protected Image m_PostImage;

	[SerializeField]
	protected TextMeshProUGUI m_PostHeader;

	[SerializeField]
	protected TextMeshProUGUI m_PostHeaderName;

	[SerializeField]
	protected TextMeshProUGUI m_PostDescription;

	[SerializeField]
	protected TextMeshProUGUI m_PostDescriptionSkillHeader;

	[SerializeField]
	protected TextMeshProUGUI m_PostDescriptionSkill;

	[Header("Officer")]
	[SerializeField]
	protected TextMeshProUGUI m_PostOfficerHeader;

	[SerializeField]
	protected TextMeshProUGUI m_PostOfficerHeaderName;

	[SerializeField]
	protected Image m_SkillHintPlace;

	[SerializeField]
	protected TextMeshProUGUI m_PostOfficerHeaderSkillValue;

	[SerializeField]
	protected TextMeshProUGUI m_PostOfficerHeaderSkillName;

	[SerializeField]
	protected Image m_OfficerPortrait;

	[SerializeField]
	protected GameObject m_NoOfficerPortrait;

	[Header("Effects Block")]
	[SerializeField]
	protected PostAbilitiesGroupDetailedBaseView m_PostAbilitiesGroupDetailedBaseView;

	[SerializeField]
	protected GameObject m_LockBackground;

	protected GridConsoleNavigationBehaviour Navigation;

	private readonly ReactiveCommand m_OnPostUpdated = new ReactiveCommand();

	public void Initialize()
	{
		m_FadeAnimator.Initialize();
	}

	protected override void BindViewImplementation()
	{
		ShowWindow();
		AddDisposable(Navigation = new GridConsoleNavigationBehaviour());
		PostSelectorView.Bind(base.ViewModel.PostsSelectorVM);
		PostOfficerSelector.Bind(base.ViewModel.PostOfficerSelectorVM);
		AddDisposable(base.ViewModel.OnPostUpdated.Subscribe(delegate
		{
			SetPostDescription(base.ViewModel.CurrentSelectedPost.Value);
		}));
		AddDisposable(base.ViewModel.CurrentSelectedPost.Subscribe(SetPostDescription));
		AddDisposable(base.ViewModel.IsLocked.Subscribe(m_LockBackground.SetActive));
	}

	protected override void DestroyViewImplementation()
	{
		HideWindow();
	}

	protected void SetPostDescription(PostEntityVM post)
	{
		if (post != null)
		{
			UIShipCustomization shipCustomization = UIStrings.Instance.ShipCustomization;
			UISpaceCombatTexts.SpacePostStrings postStrings = UIStrings.Instance.SpaceCombatTexts.GetPostStrings(post.Index);
			m_PostDescription.text = postStrings.Description.Text;
			m_PostHeader.text = shipCustomization.Post;
			m_PostHeaderName.text = postStrings.Title.Text;
			m_PostDescriptionSkill.text = LocalizedTexts.Instance.Stats.GetText(post.Post.PostData.AssociatedSkill);
			m_PostDescriptionSkillHeader.text = shipCustomization.SkillRequired;
			string text = post.Post.CurrentUnit?.CharacterName ?? ((string)shipCustomization.NoOfficer);
			m_PostOfficerHeaderName.text = text;
			m_PostOfficerHeader.text = shipCustomization.Officer;
			m_PostOfficerHeaderSkillName.text = LocalizedTexts.Instance.Stats.GetText(post.Post.PostData.AssociatedSkill);
			m_PostOfficerHeaderSkillValue.text = post.Post.CurrentSkillValue.ToString();
			AddDisposable(m_SkillHintPlace.SetHint(UIStrings.Instance.ShipCustomization.SkillRequired.Text));
			m_OfficerPortrait.sprite = ((!(post?.Post.Portrait?.SmallPortrait)) ? null : post?.Post.Portrait?.SmallPortrait);
			m_OfficerPortrait.gameObject.SetActive(post?.Post.Portrait?.SmallPortrait != null);
			m_NoOfficerPortrait.gameObject.SetActive(!m_OfficerPortrait.gameObject.activeSelf);
			m_PostImage.sprite = post?.Post.PostData.PostSpriteHolographic;
			m_PostAbilitiesGroupDetailedBaseView.Bind(base.ViewModel.CurrentSelectedPost.Value?.AbilitiesGroup);
			m_OnPostUpdated.Execute();
		}
	}

	private void ShowWindow()
	{
		base.gameObject.SetActive(value: true);
		m_FadeAnimator.AppearAnimation();
	}

	private void HideWindow()
	{
		m_FadeAnimator.DisappearAnimation(OnDisappearEnd);
	}

	private void OnDisappearEnd()
	{
		base.gameObject.SetActive(value: false);
	}

	public ConsoleNavigationBehaviour GetNavigationBehaviour()
	{
		if (!base.ViewModel.IsLocked.Value)
		{
			Navigation.AddRow<ConsoleNavigationBehaviour>(PostSelectorView.GetNavigationBehaviour());
			Navigation.AddRow(new List<IConsoleNavigationEntity>
			{
				PostOfficerSelector.GetNavigationBehaviour(),
				m_PostAbilitiesGroupDetailedBaseView.GetNavigationBehaviour()
			});
			return Navigation;
		}
		return null;
	}
}
