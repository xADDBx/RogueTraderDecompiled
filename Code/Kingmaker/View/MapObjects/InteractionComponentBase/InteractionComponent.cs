using Kingmaker.DialogSystem.Blueprints;
using UnityEngine;

namespace Kingmaker.View.MapObjects.InteractionComponentBase;

[RequireComponent(typeof(MapObjectView))]
public abstract class InteractionComponent<TPart, TSettings> : EntityPartComponent<TPart, TSettings>, IInteractionComponent, IDialogReference where TPart : InteractionPart<TSettings>, new() where TSettings : InteractionSettings, new()
{
	InteractionSettings IInteractionComponent.Settings => Settings;

	public DialogReferenceType GetUsagesFor(BlueprintDialog dialog)
	{
		if (dialog != Settings.Dialog)
		{
			return DialogReferenceType.None;
		}
		return DialogReferenceType.Start;
	}
}
