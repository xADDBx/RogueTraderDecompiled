using System;
using System.Collections.Generic;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;
using Warhammer.SpaceCombat.StarshipLogic;
using Warhammer.SpaceCombat.StarshipLogic.Posts;

namespace Kingmaker.UI.MVVM.VM.ShipCustomization.Posts;

public class PostSelectorVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly List<PostEntityVM> Posts = new List<PostEntityVM>();

	public readonly ReactiveProperty<PostEntityVM> CurrentSelectedPost;

	public SelectionGroupRadioVM<PostEntityVM> Selector;

	public PostSelectorVM(ReactiveProperty<PostEntityVM> currentSelectedPost)
	{
		CurrentSelectedPost = currentSelectedPost;
		List<Post> posts = Game.Instance.Player.PlayerShip.GetHull().Posts;
		for (int i = 0; i < posts.Count; i++)
		{
			Posts.Add(new PostEntityVM(i, posts[i]));
		}
		Selector = new SelectionGroupRadioVM<PostEntityVM>(Posts, CurrentSelectedPost);
		Selector.SelectNextValidEntity();
	}

	protected override void DisposeImplementation()
	{
		Posts.ForEach(delegate(PostEntityVM postVm)
		{
			postVm.Dispose();
		});
		Posts.Clear();
	}
}
