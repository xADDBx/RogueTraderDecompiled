using System;
using System.Collections.Generic;
using Kingmaker.UI.MVVM.VM.ShipCustomization.Posts;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using Warhammer.SpaceCombat.StarshipLogic.Posts;

namespace Kingmaker.Code.UI.MVVM.VM.ShipCustomization;

public class AbilitiesInfoGroupVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly List<PostAbilityVM> AllAbilities = new List<PostAbilityVM>();

	public readonly List<PostAbilityVM> CurrentAbilities = new List<PostAbilityVM>();

	private readonly Post m_Post;

	public readonly ReactiveCommand UpdateEventsCommand = new ReactiveCommand();

	public AbilitiesInfoGroupVM(Post post)
	{
		m_Post = post;
	}

	protected override void DisposeImplementation()
	{
		Clear();
	}

	public void UpdateAbilities()
	{
		Clear();
		foreach (Ability item2 in m_Post.UnlockedAbilities())
		{
			if (!item2.Hidden && !item2.Blueprint.IsCantrip)
			{
				PostAbilityVM item = new PostAbilityVM(item2.Blueprint, m_Post);
				AllAbilities.Add(item);
				CurrentAbilities.Add(item);
			}
		}
		UpdateEventsCommand.Execute();
	}

	private void Clear()
	{
		AllAbilities.ForEach(delegate(PostAbilityVM slotVm)
		{
			slotVm.Dispose();
		});
		AllAbilities.Clear();
		CurrentAbilities.Clear();
		UpdateEventsCommand.Execute();
	}
}
