using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameCommands;
using Kingmaker.Utility.GameConst;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.ShipCustomization.Posts;

public class PostOfficerSelectorVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	private readonly ReactiveCollection<PostOfficerVM> m_InfoPosts = new ReactiveCollection<PostOfficerVM>();

	private readonly ReactiveProperty<PostEntityVM> m_CurrentSelectedPost;

	public readonly SelectionGroupRadioVM<PostOfficerVM> Selector;

	private readonly ReactiveProperty<PostOfficerVM> m_CurrentSelectedOfficer = new ReactiveProperty<PostOfficerVM>();

	public PostOfficerSelectorVM(ReactiveProperty<PostEntityVM> currentSelectedPost)
	{
		m_CurrentSelectedPost = currentSelectedPost;
		UpdateOfficers();
		AddDisposable(Selector = new SelectionGroupRadioVM<PostOfficerVM>(m_InfoPosts, m_CurrentSelectedOfficer));
		foreach (PostOfficerVM item in Selector.EntitiesCollection)
		{
			item.TryUpdateSelect();
		}
	}

	protected override void DisposeImplementation()
	{
		m_InfoPosts?.Clear();
		m_InfoPosts?.Dispose();
	}

	private void UpdateOfficers()
	{
		m_InfoPosts.Clear();
		m_InfoPosts.Dispose();
		List<BaseUnitEntity> list = new List<BaseUnitEntity>();
		list.Add(Game.Instance.Player.MainCharacterEntity);
		list.AddRange(Game.Instance.Player.ActiveCompanions);
		list.AddRange(Game.Instance.Player.RemoteCompanions);
		foreach (BaseUnitEntity unit in list)
		{
			if (!unit.IsPet)
			{
				PostOfficerVM postOfficerVM = new PostOfficerVM(unit, m_CurrentSelectedPost, delegate
				{
					AppointOnCurrentPost(unit);
				}, delegate
				{
					AppointOnCurrentPost(null);
				});
				AddDisposable(postOfficerVM);
				m_InfoPosts.Add(postOfficerVM);
			}
		}
		int num = UIConsts.MinConsoleLootSlotsInSingleObj - m_InfoPosts.Count;
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				PostOfficerVM postOfficerVM2 = new PostOfficerVM(null, m_CurrentSelectedPost, null, null);
				AddDisposable(postOfficerVM2);
				m_InfoPosts.Add(postOfficerVM2);
			}
		}
	}

	private void AppointOnCurrentPost(BaseUnitEntity unit)
	{
		if (m_CurrentSelectedPost.Value != null)
		{
			Game.Instance.GameCommandQueue.SetUnitOnPost(unit, m_CurrentSelectedPost.Value.Post.PostType, Game.Instance.Player.PlayerShip);
		}
	}
}
