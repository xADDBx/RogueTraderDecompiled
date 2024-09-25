namespace Kingmaker.Visual.Sound;

public class BarkWrapper
{
	public readonly UnitAsksComponent.Bark Bark;

	public readonly UnitBarksManager UnitBarksManager;

	public float LastPlayTime = -100f;

	public bool IsPlaying;

	public bool IsOnCooldown
	{
		get
		{
			if (!IsPlaying)
			{
				return Game.Instance.TimeController.RealTime.TotalSeconds < (double)(LastPlayTime + Bark.GetCooldown());
			}
			return true;
		}
	}

	public bool HasBarks
	{
		get
		{
			if (Bark.Chance > 0f)
			{
				UnitAsksComponent.BarkEntry[] entries = Bark.Entries;
				if (entries != null)
				{
					return entries.Length > 0;
				}
				return false;
			}
			return false;
		}
	}

	public BarkWrapper(UnitAsksComponent.Bark bark, UnitBarksManager barksManager)
	{
		Bark = bark;
		UnitBarksManager = barksManager;
	}
}
