using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.SpaceCombat.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic.Posts;

namespace Kingmaker.UI.MVVM.VM.ShipCustomization.Posts;

public class PostAbilityVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IHasTooltipTemplates, IOnPostAbilityChangeHandler, ISubscriber
{
	public readonly ReactiveProperty<Sprite> Icon = new ReactiveProperty<Sprite>(null);

	public readonly BlueprintAbility Ability;

	private readonly Post m_Post;

	public BlueprintAbility AttuneAbility;

	private bool m_IsUltimate;

	private bool m_CanUseUltimate;

	public int UltimateDuration = -1;

	public bool IsUnlocked;

	public bool HasCooldown;

	public int Cooldown;

	public bool IsAlreadyAttuned;

	public bool IsEnoughScrapForAttune;

	public bool IsUsed;

	public bool IsFullHP;

	public TooltipTemplateShipAbility TooltipTemplateAbility;

	public TooltipTemplateShipAbility TooltipTemplateAttunedAbility;

	private readonly StarshipCompanionsOnPostLogic m_PostLogic;

	public string DisplayName => Ability.Name;

	public bool HasDuration
	{
		get
		{
			if (m_IsUltimate)
			{
				return m_CanUseUltimate;
			}
			return false;
		}
	}

	public bool IsAttunable => AttuneAbility != null;

	public int ScrapRequired => BlueprintWarhammerRoot.Instance.BlueprintScrapRoot.ScrapToAttunePostAbility;

	public string LockedReason
	{
		get
		{
			if (m_Post?.CurrentUnit == null)
			{
				return UIStrings.Instance.ShipCustomization.NoCharacterOnPost.Text;
			}
			return LocalizedTexts.Instance.Stats.GetText(m_Post.PostData.AssociatedSkill) + " " + m_Post.CurrentSkillValue;
		}
	}

	public string CooldownReason
	{
		get
		{
			if (m_Post?.CurrentUnit == null)
			{
				return UIStrings.Instance.ShipCustomization.NoCharacterOnPost.Text;
			}
			return UIStrings.Instance.ShipCustomization.HasPenalty.Text;
		}
	}

	public bool CanAttune
	{
		get
		{
			if (IsAttunable && !IsAlreadyAttuned && IsEnoughScrapForAttune && IsFullHP)
			{
				return IsUsed;
			}
			return false;
		}
	}

	public PostAbilityVM(BlueprintAbility ability, Post post)
	{
		if (ability != null)
		{
			m_Post = post;
			Icon.Value = ability.Icon;
			Ability = ability;
			m_PostLogic = Game.Instance.Player.PlayerShip.Facts.GetComponents<StarshipCompanionsOnPostLogic>().FirstOrDefault();
			try
			{
				TooltipTemplateAbility = new TooltipTemplateShipAbility(ability);
			}
			catch (Exception value)
			{
				System.Console.WriteLine(value);
			}
			UpdateParameters();
			AddDisposable(EventBus.Subscribe(this));
		}
	}

	private void UpdateParameters()
	{
		m_IsUltimate = Ability.AbilityTag == AbilityTag.StarshipUltimateAbility;
		m_CanUseUltimate = m_IsUltimate && m_PostLogic != null && m_PostLogic.CanUseUltimate(Game.Instance.Player.PlayerShip, Ability);
		IsUnlocked = !m_IsUltimate || m_CanUseUltimate;
		HasCooldown = m_Post != null && m_Post.HasPenalty && !m_IsUltimate;
		Cooldown = m_PostLogic?.AddToAbilityCooldown(Game.Instance.Player.PlayerShip, Ability) ?? 0;
		UltimateDuration = m_PostLogic?.GetUltimateBuffDuration(Game.Instance.Player.PlayerShip, Ability) ?? (-1);
		AttuneAbility = GetAttuneAbility();
		if ((bool)AttuneAbility)
		{
			try
			{
				TooltipTemplateAttunedAbility = new TooltipTemplateShipAbility(AttuneAbility);
			}
			catch (Exception value)
			{
				System.Console.WriteLine(value);
			}
		}
		else
		{
			TooltipTemplateAttunedAbility = null;
		}
		IsAlreadyAttuned = m_Post?.IsAlreadyAttuned(Ability) ?? false;
		IsUsed = m_Post?.IsAbilityUsed(Ability) ?? false;
		IsEnoughScrapForAttune = m_Post?.IsEnoughScrapToAttune() ?? false;
		IsFullHP = m_Post?.ShipHasFullHealth() ?? false;
	}

	private BlueprintAbility GetAttuneAbility()
	{
		IEnumerable<BlueprintShipPostExpertise> enumerable = m_Post.UnitExpertises(m_Post.CurrentUnit);
		if (enumerable != null && enumerable.Any())
		{
			foreach (BlueprintShipPostExpertise item in enumerable)
			{
				if (item.DefaultPostAbility == Ability)
				{
					return item.ChangedPostAbility;
				}
			}
		}
		return null;
	}

	public void TryAttune()
	{
		if (m_Post != null)
		{
			Game.Instance.GameCommandQueue.AttuneAbilityForPost(m_Post, Ability);
		}
	}

	protected override void DisposeImplementation()
	{
		Icon.Value = null;
	}

	void IOnPostAbilityChangeHandler.HandlePostAbilityChange(int postType)
	{
		if (m_Post.PostType == (PostType)postType)
		{
			UpdateParameters();
		}
	}

	public List<TooltipBaseTemplate> TooltipTemplates()
	{
		List<TooltipBaseTemplate> list = new List<TooltipBaseTemplate>();
		list.Add(TooltipTemplateAbility);
		if (TooltipTemplateAttunedAbility != null)
		{
			list.Add(TooltipTemplateAttunedAbility);
		}
		return list;
	}
}
