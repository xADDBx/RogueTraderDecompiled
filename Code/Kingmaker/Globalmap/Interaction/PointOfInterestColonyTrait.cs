using System;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.UI.Common;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Globalmap.Interaction;

[Serializable]
public class PointOfInterestColonyTrait : BasePointOfInterest, IHashable
{
	public new BlueprintPointOfInterestColonyTrait Blueprint => (BlueprintPointOfInterestColonyTrait)base.Blueprint;

	public PointOfInterestColonyTrait(BlueprintPointOfInterestColonyTrait blueprint)
		: base(blueprint)
	{
	}

	public PointOfInterestColonyTrait(JsonConstructorMark _)
		: base(_)
	{
	}

	public override void Interact(StarSystemObjectEntity entity)
	{
		base.Interact(entity);
		UIUtility.ShowMessageBox(Blueprint.Name + " \n\n " + Blueprint.Description, DialogMessageBoxBase.BoxType.Message, delegate
		{
		});
		ChangeStatusToInteracted();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
