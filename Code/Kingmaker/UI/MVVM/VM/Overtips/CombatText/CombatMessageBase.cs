using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Overtips.CombatText;

public abstract class CombatMessageBase
{
	public abstract string GetText();

	public virtual Color? GetColor()
	{
		return null;
	}

	public virtual bool GetAttention()
	{
		return false;
	}

	public virtual Sprite GetSprite()
	{
		return null;
	}
}
