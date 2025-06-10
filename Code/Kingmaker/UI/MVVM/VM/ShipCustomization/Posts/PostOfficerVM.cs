using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.SpaceCombat.Blueprints;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.SelectionGroup;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic.Posts;

namespace Kingmaker.UI.MVVM.VM.ShipCustomization.Posts;

public class PostOfficerVM : SelectionGroupEntityVM, IOnNewUnitOnPostHandler, ISubscriber, IHasTooltipTemplate
{
	public readonly BaseUnitEntity Unit;

	public readonly Sprite Portrait;

	public PostAbilityVM UnitAbility;

	public int SkillValue;

	public string SkillName;

	public Sprite PostSprite;

	public UIUtilityUnit.SkillRecommendationEnum SkillRecommendation;

	private readonly ReactiveProperty<Post> m_AttachedPost = new ReactiveProperty<Post>();

	private readonly ReactiveProperty<PostEntityVM> m_SelectedPost;

	public readonly ReactiveCommand DataUpdated = new ReactiveCommand();

	private readonly Action m_OnSelect;

	private readonly Action m_OnDeselect;

	private readonly TooltipTemplateUnitOnPost m_TooltipTemplateUnitOnPost;

	public PostOfficerVM(BaseUnitEntity unit, ReactiveProperty<PostEntityVM> selectedPost, Action onSelect, Action onDeselect)
		: base(allowSwitchOff: true)
	{
		m_OnSelect = onSelect;
		m_OnDeselect = onDeselect;
		m_SelectedPost = selectedPost;
		Portrait = (unit?.Portrait.SmallPortrait ? unit.Portrait.SmallPortrait : null);
		Unit = unit;
		if (unit == null)
		{
			UpdatePostData();
			TryUpdateSelect();
			return;
		}
		AddDisposable(m_SelectedPost.Subscribe(delegate
		{
			UpdatePostData();
			TryUpdateSelect();
		}));
		try
		{
			m_TooltipTemplateUnitOnPost = new TooltipTemplateUnitOnPost(Unit, m_SelectedPost);
		}
		catch (Exception value)
		{
			System.Console.WriteLine(value);
		}
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		UnitAbility?.Dispose();
	}

	private void UpdatePostData()
	{
		if (m_SelectedPost.Value == null || Unit == null || Unit.IsPet)
		{
			UnitAbility = null;
			SkillValue = 0;
			PostSprite = null;
			DataUpdated.Execute();
			return;
		}
		m_AttachedPost.Value = Game.Instance.Player.PlayerShip.Hull.Posts.FirstOrDefault((Post x) => x.CurrentUnit == Unit);
		ModifiableValue statOptional = Unit.GetStatOptional((m_SelectedPost?.Value?.Post?.PostData?.AssociatedSkill).GetValueOrDefault());
		SkillValue = statOptional?.ModifiedValue ?? 0;
		SkillName = LocalizedTexts.Instance.Stats.GetText(statOptional?.Type ?? StatType.Unknown);
		SkillRecommendation = UIUtilityUnit.GetSkillGraduation((m_SelectedPost?.Value?.Post?.PostData?.AssociatedSkill).GetValueOrDefault(), Unit, Game.Instance.Player.AllCharacters);
		IEnumerable<BlueprintShipPostExpertise> enumerable = m_SelectedPost.Value.Post?.UnitExpertises(Unit);
		UnitAbility?.Dispose();
		UnitAbility = null;
		if (enumerable != null && m_SelectedPost?.Value?.Post?.DefaultAbilities != null)
		{
			foreach (BlueprintShipPostExpertise item in enumerable)
			{
				foreach (BlueprintAbility item2 in m_SelectedPost?.Value?.Post?.DefaultAbilities)
				{
					if (item.DefaultPostAbility == item2)
					{
						AddDisposable(UnitAbility = new PostAbilityVM(item2, m_SelectedPost?.Value?.Post));
					}
				}
			}
		}
		PostSprite = m_AttachedPost.Value?.PostData?.PostSpriteHolographic;
		DataUpdated.Execute();
	}

	public void TryUpdateSelect()
	{
		TryUpdateSelect(m_SelectedPost.Value.Post == m_AttachedPost.Value);
	}

	private void TryUpdateSelect(bool state)
	{
		if (IsSelected.Value != state)
		{
			SetSelected(state);
		}
	}

	public void DoSelect()
	{
		m_OnSelect?.Invoke();
	}

	public void DoUnselect()
	{
		m_OnDeselect?.Invoke();
	}

	protected override void DoSelectMe()
	{
	}

	public void HandleNewUnit()
	{
		UpdatePostData();
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return m_TooltipTemplateUnitOnPost;
	}
}
