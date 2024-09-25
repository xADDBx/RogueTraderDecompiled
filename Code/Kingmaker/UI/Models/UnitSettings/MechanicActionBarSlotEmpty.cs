using Kingmaker.UI.Sound;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UI.Models.UnitSettings;

public class MechanicActionBarSlotEmpty : MechanicActionBarSlot, IHashable
{
	public override bool IsPossibleActive => false;

	public override void OnClick()
	{
		UISounds.Instance.Sounds.Combat.ActionBarCanNotSlotClick.Play();
	}

	public override int GetResource()
	{
		return -1;
	}

	public override int GetResourceCost()
	{
		return -1;
	}

	public override int GetResourceAmount()
	{
		return -1;
	}

	public override Sprite GetIcon()
	{
		return null;
	}

	public override bool NeedUpdate()
	{
		return false;
	}

	public override string GetTitle()
	{
		return string.Empty;
	}

	public override string GetDescription()
	{
		return string.Empty;
	}

	public override bool IsCasting()
	{
		return false;
	}

	public override object GetContentData()
	{
		return null;
	}

	protected override bool CanUseIfTurnBased()
	{
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
