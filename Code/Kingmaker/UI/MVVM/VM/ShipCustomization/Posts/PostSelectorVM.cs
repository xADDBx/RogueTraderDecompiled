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
	private readonly List<PostEntityVM> m_Posts = new List<PostEntityVM>();

	public readonly SelectionGroupRadioVM<PostEntityVM> Selector;

	public PostSelectorVM(ReactiveProperty<PostEntityVM> currentSelectedPost)
	{
		List<Post> posts = Game.Instance.Player.PlayerShip.GetHull().Posts;
		for (int i = 0; i < posts.Count; i++)
		{
			m_Posts.Add(new PostEntityVM(i, posts[i]));
		}
		Selector = new SelectionGroupRadioVM<PostEntityVM>(m_Posts, currentSelectedPost);
		Selector.SelectNextValidEntity();
	}

	protected override void DisposeImplementation()
	{
		m_Posts.ForEach(delegate(PostEntityVM postVm)
		{
			postVm.Dispose();
		});
		m_Posts.Clear();
	}
}
