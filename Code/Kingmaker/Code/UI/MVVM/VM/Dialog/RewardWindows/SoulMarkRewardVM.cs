using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Dialog.RewardWindows;

public class SoulMarkRewardVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, INetLobbyRequest, ISubscriber
{
	public readonly string FeatureName;

	public readonly Sprite FeatureIcon;

	public readonly TooltipBaseTemplate Tooltip;

	private readonly Action m_OnClose;

	public SoulMarkRewardVM(SoulMarkDirection direction, int rankAdded, Action onClose)
	{
		m_OnClose = onClose;
		BlueprintFeature soulMarkFeature = SoulMarkTooltipExtensions.GetSoulMarkFeature(SoulMarkShiftExtension.GetBaseSoulMarkFor(direction), rankAdded);
		if (soulMarkFeature != null)
		{
			FeatureName = soulMarkFeature.Name;
			FeatureIcon = soulMarkFeature.Icon;
			BaseUnitEntity mainCharacterEntity = Game.Instance.Player.MainCharacterEntity;
			Tooltip = new TooltipTemplateSoulMarkFeature(mainCharacterEntity, direction, rankAdded, null);
		}
	}

	protected override void DisposeImplementation()
	{
	}

	public void HandleNetLobbyRequest(bool isMainMenu = false)
	{
	}

	public void HandleNetLobbyClose()
	{
	}

	public void OnAcceptPressed()
	{
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleOpenCharacterInfo();
		});
		m_OnClose?.Invoke();
	}

	public void OnDeclinePressed()
	{
		m_OnClose?.Invoke();
	}
}
