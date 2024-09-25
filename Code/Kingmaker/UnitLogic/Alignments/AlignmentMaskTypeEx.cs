namespace Kingmaker.UnitLogic.Alignments;

public static class AlignmentMaskTypeEx
{
	public static string ShortestDesc(this AlignmentMaskType al)
	{
		switch (al)
		{
		case AlignmentMaskType.Any:
			return "Any";
		case AlignmentMaskType.None:
			return "None";
		default:
		{
			string text = al.ToString();
			string text2 = "not " + (~al & AlignmentMaskType.Any);
			if (text.Length >= text2.Length)
			{
				return text2;
			}
			return text;
		}
		}
	}
}
