using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Exploration;

public class ExplorationResourceVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IMiningUIHandler, ISubscriber
{
	public readonly ReactiveProperty<Sprite> Icon = new ReactiveProperty<Sprite>();

	public readonly ReactiveProperty<string> Name = new ReactiveProperty<string>();

	public readonly ReactiveProperty<int> Count = new ReactiveProperty<int>();

	public readonly ReactiveProperty<string> Description = new ReactiveProperty<string>();

	public readonly ReactiveProperty<bool> IsBeingMined = new ReactiveProperty<bool>();

	private readonly BlueprintResource m_BlueprintResource;

	private readonly StarSystemObjectEntity m_StarSystemObjectEntity;

	private bool m_IsFocused;

	public bool IsFocused => m_IsFocused;

	public ExplorationResourceVM(StarSystemObjectEntity sso, BlueprintResource blueprintResource, int count)
	{
		m_BlueprintResource = blueprintResource;
		m_StarSystemObjectEntity = sso;
		Icon.Value = blueprintResource.Icon;
		Name.Value = blueprintResource.Name;
		Count.Value = count;
		Description.Value = blueprintResource.Description;
		IsBeingMined.Value = m_StarSystemObjectEntity.ResourceMiners.ContainsKey(m_BlueprintResource);
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	public void Interact()
	{
		if (!UINetUtility.IsControlMainCharacterWithWarning())
		{
			return;
		}
		if (IsBeingMined.Value)
		{
			TryRemoveResourceMiner();
		}
		else if (!Game.Instance.ColonizationController.HaveResourceMiner())
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.ExplorationTexts.NotEnoughResourceMiners, addToLog: true, WarningNotificationFormat.Attention);
			});
		}
		else
		{
			TryUseResourceMiner();
		}
	}

	public void SetFocus(bool value)
	{
		m_IsFocused = value;
	}

	private void TryUseResourceMiner()
	{
		using (GameLogContext.Scope)
		{
			GameLogContext.Count = 1;
			GameLogContext.Text = UIStrings.Instance.ExplorationTexts.ResourceMiner.Text;
			GameLogContext.Description = m_BlueprintResource.Name;
			UIUtility.ShowMessageBox(UIStrings.Instance.ExplorationTexts.UseResourceMinerDialogMessage.Text, DialogMessageBoxBase.BoxType.Dialog, OnUseResourceMiner);
		}
	}

	private void OnUseResourceMiner(DialogMessageBoxBase.BoxButton button)
	{
		if (button == DialogMessageBoxBase.BoxButton.Yes)
		{
			Game.Instance.ColonizationController.UseResourceMiner(m_StarSystemObjectEntity, m_BlueprintResource);
		}
	}

	private void TryRemoveResourceMiner()
	{
		using (GameLogContext.Scope)
		{
			GameLogContext.Count = 1;
			GameLogContext.Text = UIStrings.Instance.ExplorationTexts.ResourceMiner.Text;
			UIUtility.ShowMessageBox(UIStrings.Instance.ExplorationTexts.RemoveResourceMinerDialogMessage.Text, DialogMessageBoxBase.BoxType.Dialog, OnRemoveResourceMiner);
		}
	}

	private void OnRemoveResourceMiner(DialogMessageBoxBase.BoxButton button)
	{
		if (button == DialogMessageBoxBase.BoxButton.Yes)
		{
			Game.Instance.ColonizationController.RemoveResourceMiner(m_StarSystemObjectEntity, m_BlueprintResource);
		}
	}

	void IMiningUIHandler.HandleStartMining(StarSystemObjectEntity starSystemObjectEntity, BlueprintResource blueprintResource)
	{
		if (m_StarSystemObjectEntity == starSystemObjectEntity && m_BlueprintResource == blueprintResource)
		{
			UISounds.Instance.Sounds.SpaceExploration.ExtractuimSet.Play();
			IsBeingMined.Value = true;
		}
	}

	void IMiningUIHandler.HandleStopMining(StarSystemObjectEntity starSystemObjectEntity, BlueprintResource blueprintResource)
	{
		if (m_StarSystemObjectEntity == starSystemObjectEntity && m_BlueprintResource == blueprintResource)
		{
			IsBeingMined.Value = false;
		}
	}
}
