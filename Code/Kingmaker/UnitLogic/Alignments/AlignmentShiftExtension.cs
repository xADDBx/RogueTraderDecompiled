namespace Kingmaker.UnitLogic.Alignments;

public static class AlignmentShiftExtension
{
	public static void ApplyAlignmentShift(this IAlignmentShiftProvider provider)
	{
		if (provider.AlignmentShift.Value >= 1)
		{
			Game.Instance.Player.MainCharacterEntity.Alignment.Shift(provider);
		}
	}
}
