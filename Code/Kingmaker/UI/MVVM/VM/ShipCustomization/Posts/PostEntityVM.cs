using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.ShipCustomization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;
using UnityEngine;
using Warhammer.SpaceCombat;
using Warhammer.SpaceCombat.StarshipLogic.Posts;

namespace Kingmaker.UI.MVVM.VM.ShipCustomization.Posts;

public class PostEntityVM : SelectionGroupEntityVM, IStarshipPostHandler, ISubscriber, IOnNewUnitOnPostHandler
{
	public readonly int Index;

	public Sprite Portrait;

	public readonly AbilitiesInfoGroupVM AbilitiesGroup;

	public readonly ReactiveProperty<bool> IsPostBlocked = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<string> BlockDuration = new ReactiveProperty<string>();

	public readonly ReactiveProperty<bool> FXActivated = new ReactiveProperty<bool>();

	public readonly Post Post;

	public ReactiveCommand OnPostUpdate = new ReactiveCommand();

	public PostEntityVM(int index, Post post)
		: base(allowSwitchOff: false)
	{
		Index = index;
		Post = post;
		Portrait = ((!(post?.Portrait?.SmallPortrait)) ? BlueprintRoot.Instance.UIConfig.Portraits.LeaderPlaceholderPortrait.SmallPortrait : post?.Portrait?.SmallPortrait);
		IsPostBlocked.Value = false;
		BlockDuration.Value = "0";
		FXActivated.Value = false;
		AddDisposable(AbilitiesGroup = new AbilitiesInfoGroupVM(Post));
		UpdateUnitOnPost();
		AddDisposable(EventBus.Subscribe(this));
	}

	public void UpdateUnitOnPost()
	{
		Portrait = ((!(Post?.CurrentUnit?.Portrait?.SmallPortrait)) ? BlueprintRoot.Instance.UIConfig.Portraits.LeaderPlaceholderPortrait.SmallPortrait : Post?.CurrentUnit?.Portrait?.SmallPortrait);
		AbilitiesGroup.UpdateAbilities();
		OnPostUpdate?.Execute();
	}

	protected override void DoSelectMe()
	{
	}

	protected override void DisposeImplementation()
	{
	}

	public void HandlePostBlocked(Post post)
	{
	}

	public void HandleBuffDidAdded(Post post, Buff buff)
	{
	}

	public void HandleBuffDidRemoved(Post post, Buff buff)
	{
	}

	public void HandleNewUnit()
	{
		UpdateUnitOnPost();
	}
}
