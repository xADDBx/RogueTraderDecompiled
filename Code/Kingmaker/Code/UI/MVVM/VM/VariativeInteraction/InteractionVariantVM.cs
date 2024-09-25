using System;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Interaction;
using Kingmaker.UI.Common;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.VariativeInteraction;

public class InteractionVariantVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	private readonly EntityViewBase m_View;

	private readonly IInteractionVariantActor m_InteractionActor;

	public readonly Sprite ImageNormal;

	public readonly Sprite ImageHighlighted;

	public readonly Sprite ImagePressed;

	public readonly Sprite ImageDisabled;

	public StringReactiveProperty InteractionName = new StringReactiveProperty();

	public int? RequiredResourceCount;

	public int? ResourceCount;

	public int? UnitCount;

	public bool OnlyOnceCheck;

	public string ResourceName;

	private readonly Action m_OnInteract;

	public bool Disabled;

	private IDisposable m_SelectedUnitsSubscription;

	public bool LimitedUnitsCheck => UnitCount.HasValue;

	public InteractionVariantVM(EntityViewBase view, IInteractionVariantActor interactionActor, string resourceName, int? resourceCount, int? requiredResourceCount, int? unitCount, Action onInteract)
	{
		InteractionVariantVM interactionVariantVM = this;
		m_View = view;
		m_InteractionActor = interactionActor;
		m_OnInteract = onInteract;
		InteractionVariantVisualSetEntry set = BlueprintRoot.Instance.UIConfig.InteractionVariantVisualSetsBlueprint.GetSet(m_InteractionActor.Type);
		InteractionSkillCheckPart interactionSkillCheckPart = interactionActor.InteractionPart as InteractionSkillCheckPart;
		ImageNormal = set?.Normal;
		ImageHighlighted = set?.Highlighted;
		ImagePressed = set?.Pressed;
		ImageDisabled = set?.Disable;
		InteractionName.Value = UIUtility.GetInteractionVariantActorText(interactionActor, Game.Instance.SelectionCharacter.SelectedUnits.ToList(), out var needChance);
		RequiredResourceCount = requiredResourceCount;
		ResourceCount = resourceCount;
		UnitCount = unitCount;
		OnlyOnceCheck = interactionActor.CheckOnlyOnce;
		ResourceName = resourceName;
		Disabled = (interactionSkillCheckPart != null && interactionSkillCheckPart.IsFailed && interactionSkillCheckPart.Settings.InteractOnlyWithToolAfterFail && !RequiredResourceCount.HasValue) || resourceCount < requiredResourceCount || !interactionActor.CanUse || (LimitedUnitsCheck && UnitCount <= 0);
		if (needChance)
		{
			m_SelectedUnitsSubscription = UniRxExtensionMethods.Subscribe(Game.Instance.SelectionCharacter.ActualGroupUpdated, delegate
			{
				interactionVariantVM.InteractionName.Value = UIUtility.GetInteractionVariantActorText(interactionActor, Game.Instance.SelectionCharacter.SelectedUnits.ToList(), out needChance);
			});
		}
		if (set == null)
		{
			UberDebug.LogError($"InteractionVariantVisualSets is null for InteractionVariantActor {m_InteractionActor.Type}");
		}
	}

	public void Interact()
	{
		using (ContextData<InteractionVariantData>.Request().Setup(m_InteractionActor))
		{
			ClickMapObjectHandler.TryInteract(m_InteractionActor.InteractionPart, Game.Instance.SelectionCharacter.SelectedUnits.ToList(), muteEvents: false, m_InteractionActor);
		}
		m_OnInteract?.Invoke();
	}

	protected override void DisposeImplementation()
	{
		m_SelectedUnitsSubscription?.Dispose();
	}
}
