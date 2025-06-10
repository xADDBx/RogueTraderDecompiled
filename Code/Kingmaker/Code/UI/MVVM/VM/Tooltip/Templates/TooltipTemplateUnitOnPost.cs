using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.SpaceCombat.Blueprints;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.ShipCustomization.Posts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic;
using Warhammer.SpaceCombat.StarshipLogic.Posts;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateUnitOnPost : TooltipBaseTemplate
{
	private readonly BaseUnitEntity m_Unit;

	private readonly StarshipCompanionsOnPostLogic m_PostLogic;

	private readonly List<Post> m_Posts;

	private readonly ReactiveProperty<PostEntityVM> m_SelectedPost;

	public TooltipTemplateUnitOnPost(BaseUnitEntity unit, ReactiveProperty<PostEntityVM> selectedPost)
	{
		try
		{
			if (unit != null)
			{
				m_Unit = unit;
				m_PostLogic = Game.Instance.Player.PlayerShip.Facts.GetComponents<StarshipCompanionsOnPostLogic>().FirstOrDefault();
				m_Posts = Game.Instance.Player.PlayerShip.GetHull().Posts;
				m_SelectedPost = selectedPost;
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't create TooltipTemplate for: {unit?.Blueprint?.name}: {arg}");
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		if (m_Unit == null)
		{
			yield return new TooltipBrickText(UIStrings.Instance.Tooltips.UnitIsNotInspected);
			yield break;
		}
		Sprite surfaceCombatStandardPortrait = UIUtilityUnit.GetSurfaceCombatStandardPortrait(m_Unit, UIUtilityUnit.PortraitCombatSize.Small);
		yield return new TooltipBrickPortraitAndName(surfaceCombatStandardPortrait, m_Unit.CharacterName);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (m_Unit == null)
		{
			return list;
		}
		if (m_Posts == null)
		{
			return list;
		}
		list.Add(new TooltipBrickTitle(UIStrings.Instance.Inspect.AbilitiesTitle, TooltipTitleType.H2));
		List<ITooltipBrick> list2 = new List<ITooltipBrick>();
		for (int i = 0; i < m_Posts.Count; i++)
		{
			Post post = m_Posts[i];
			IEnumerable<BlueprintShipPostExpertise> enumerable = post.UnitExpertises(m_Unit);
			if (enumerable == null || !enumerable.Any() || post.DefaultAbilities == null || !post.DefaultAbilities.Any())
			{
				continue;
			}
			foreach (BlueprintShipPostExpertise item in enumerable)
			{
				foreach (BlueprintAbility defaultAbility in post.DefaultAbilities)
				{
					if (item?.DefaultPostAbility == defaultAbility && post == m_SelectedPost?.Value?.Post)
					{
						list2.Add(new TooltipBrickTitle(UIStrings.Instance.SpaceCombatTexts.GetPostStrings(i).Title, TooltipTitleType.H4));
						ModifiableValue statOptional = m_Unit.GetStatOptional(post.PostData.AssociatedSkill);
						list2.Add(new TooltipBrickFeature(defaultAbility));
						list2.Add(new TooltipBrickIconStatValue(LocalizedTexts.Instance.Stats.GetText(statOptional?.Type ?? StatType.Unknown), statOptional?.ModifiedValue.ToString()));
					}
				}
			}
		}
		if (!list2.Any())
		{
			list.Add(new TooltipBrickText(UIStrings.Instance.Inspect.NoAbilities.Text, TooltipTextType.Simple | TooltipTextType.BrightColor));
		}
		else
		{
			list.AddRange(list2);
		}
		return list;
	}
}
