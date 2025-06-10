using System.Linq;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap.Utils;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Networking.Serialization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

public class AreaTransitionPart : ViewBasedPart<AreaTransitionSettings>, IUnlockableFlagReference, IAreaEnterPointReference, IUnlockHandler, ISubscriber, IEtudesUpdateHandler, IAreaHandler, ILocalizationHandler, IHashable
{
	[JsonProperty]
	[GameStateIgnore]
	public bool AlreadyUnlocked;

	public BlueprintAreaEnterPoint AreaEnterPoint => base.Settings.AreaEnterPoint;

	public float ProximityDistance => base.Settings.ProximityDistance;

	public BlueprintAreaTransition Blueprint => base.Settings.Blueprint;

	protected override void OnSettingsDidSet(bool isNewSettings)
	{
		if (base.Settings.AddMapMarker)
		{
			LocalMapMarkerPart localMapMarkerPart = base.ConcreteOwner?.GetOrCreate<LocalMapMarkerPart>();
			if (localMapMarkerPart != null)
			{
				localMapMarkerPart.IsRuntimeCreated = true;
				localMapMarkerPart.Settings.Type = LocalMapMarkType.Exit;
				if (AreaEnterPoint != null)
				{
					localMapMarkerPart.NonLocalizedDescription = AreaEnterPoint.Tooltip(base.Settings.TooltipIndex);
				}
			}
		}
		UpdateVisibility();
	}

	public void UpdateVisibility()
	{
		if (base.Settings.VisibilityFlag != null)
		{
			base.Owner.IsInGame = base.Settings.VisibilityFlag.IsUnlocked;
		}
		else if (base.Settings.VisibilityEtude != null)
		{
			Etude etude = Game.Instance.Player.EtudesSystem.Etudes.Get(base.Settings.VisibilityEtude);
			base.Owner.IsInGame = etude?.IsPlaying ?? false;
		}
	}

	public bool CheckRestrictions(BaseUnitEntity user)
	{
		if (AlreadyUnlocked)
		{
			return true;
		}
		InteractionRestrictionPart interactionRestrictionPart = base.ConcreteOwner?.GetAll<InteractionRestrictionPart>()?.FirstOrDefault();
		if (interactionRestrictionPart == null)
		{
			AlreadyUnlocked = true;
			return true;
		}
		if (base.ConcreteOwner != null)
		{
			foreach (InteractionRestrictionPart item in base.ConcreteOwner.Parts.GetAll<InteractionRestrictionPart>())
			{
				if (item.CheckRestriction(user))
				{
					item.ShowSuccessBark(user);
					AlreadyUnlocked = true;
					return true;
				}
			}
		}
		interactionRestrictionPart.ShowRestrictionBark(user);
		return false;
	}

	public UnlockableFlagReferenceType GetUsagesFor(BlueprintUnlockableFlag flag)
	{
		if (flag != base.Settings.VisibilityFlag)
		{
			return UnlockableFlagReferenceType.None;
		}
		return UnlockableFlagReferenceType.Check;
	}

	public bool GetUsagesFor(BlueprintAreaEnterPoint point)
	{
		return point == base.Settings.AreaEnterPoint;
	}

	public void HandleUnlock(BlueprintUnlockableFlag flag)
	{
		UpdateVisibility();
	}

	public void HandleLock(BlueprintUnlockableFlag flag)
	{
		UpdateVisibility();
	}

	public void OnEtudesUpdate()
	{
		UpdateVisibility();
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		UpdateVisibility();
	}

	public void HandleLanguageChanged()
	{
		if (base.Settings.AddMapMarker)
		{
			LocalMapMarkerPart localMapMarkerPart = base.ConcreteOwner?.GetOrCreate<LocalMapMarkerPart>();
			if (AreaEnterPoint != null && localMapMarkerPart != null)
			{
				localMapMarkerPart.NonLocalizedDescription = AreaEnterPoint.Tooltip(base.Settings.TooltipIndex);
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
