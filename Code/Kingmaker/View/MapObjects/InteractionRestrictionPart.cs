using Kingmaker.Code.UI.MVVM.VM.Bark;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

public abstract class InteractionRestrictionPart : ViewBasedPart, IHashable
{
	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public bool IsDisabled;

	[StringCreateWindow(StringCreateWindowAttribute.StringType.Bark, GetNameFromAsset = true)]
	public SharedStringAsset RestrictedBark;

	[StringCreateWindow(StringCreateWindowAttribute.StringType.Bark, GetNameFromAsset = true)]
	public SharedStringAsset AllowedBark;

	public abstract int GetUserPriority(BaseUnitEntity user);

	public abstract bool CheckRestriction(BaseUnitEntity user);

	public virtual void OnDidInteract(BaseUnitEntity user)
	{
	}

	public virtual void OnFailedInteract(BaseUnitEntity user)
	{
	}

	protected virtual string GetDefaultBark(BaseUnitEntity baseUnitEntity, bool restricted)
	{
		return restricted ? Game.Instance.BlueprintRoot.LocalizedTexts.LockedContainer : Game.Instance.BlueprintRoot.LocalizedTexts.UnlockedContainer;
	}

	public void ShowRestrictionBark(BaseUnitEntity user)
	{
		ShowBarkInternal(user, restricted: true);
	}

	public void ShowSuccessBark(BaseUnitEntity user)
	{
		ShowBarkInternal(user, restricted: false);
	}

	private void ShowBarkInternal(BaseUnitEntity user, bool restricted)
	{
		LocalizedString localizedString = (restricted ? RestrictedBark : AllowedBark)?.String;
		string text = ((localizedString != null) ? ((string)localizedString) : GetDefaultBark(user, restricted));
		if (!string.IsNullOrEmpty(text))
		{
			BarkPlayer.Bark(user, text, -1f, null, user);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref IsDisabled);
		return result;
	}
}
public abstract class InteractionRestrictionPart<TSettings> : InteractionRestrictionPart, IHashable where TSettings : class, new()
{
	public TSettings Settings { get; private set; } = new TSettings();


	public override void SetSource(IAbstractEntityPartComponent source)
	{
		IAbstractEntityPartComponent source2 = base.Source;
		base.SetSource(source);
		Settings = source.GetSettings() as TSettings;
		OnSettingsDidSet(source2 != source);
	}

	protected virtual void OnSettingsDidSet(bool isNewSettings)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
