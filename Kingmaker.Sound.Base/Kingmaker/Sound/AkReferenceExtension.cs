namespace Kingmaker.Sound;

public static class AkReferenceExtension
{
	public static bool IsValid(this AkReferenceBase akReference)
	{
		return !string.IsNullOrWhiteSpace(akReference?.Value);
	}

	public static bool IsValid(this AkReferenceWithGroupBase akReferenceWithGroupBase)
	{
		if (!string.IsNullOrWhiteSpace(akReferenceWithGroupBase?.Group))
		{
			return ((AkReferenceBase)akReferenceWithGroupBase).IsValid();
		}
		return false;
	}
}
