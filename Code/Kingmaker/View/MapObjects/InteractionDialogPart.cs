using Kingmaker.Controllers.Dialog;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

public class InteractionDialogPart : InteractionPart<InteractionSettings>, IHashable
{
	protected override UIInteractionType GetDefaultUIType()
	{
		return UIInteractionType.Info;
	}

	protected override void OnInteract(BaseUnitEntity user)
	{
		if (base.View != null)
		{
			DialogData data = DialogController.SetupDialogWithMapObject(base.Settings.Dialog, base.View, null, user);
			Game.Instance.DialogController.StartDialog(data);
		}
	}

	public override bool CanInteract()
	{
		if (base.CanInteract())
		{
			return base.Settings.Dialog != null;
		}
		return false;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
