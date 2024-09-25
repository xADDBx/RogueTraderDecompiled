namespace Kingmaker.Blueprints.Root;

public static class DialogFormats
{
	public static readonly string AnswerFormatWithColorName = "<b><color=#{0}>{1}</color></b>: {2}";

	public static readonly string AnswerFormatWithoutName = "{0}";

	public static readonly string SpeakerFormatWithColorName = "<b><color=#{1}>{2}</color></b>: <color=#{4}>{0}</color> {3}";

	public static readonly string SpeakerFormatWithoutName = "{1}{0}";

	public static readonly string NarratorsTextFormat = "<i><color=#{0}>{1}</color><i>";
}
