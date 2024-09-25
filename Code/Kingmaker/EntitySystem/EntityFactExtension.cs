namespace Kingmaker.EntitySystem;

public static class EntityFactExtension
{
	public static bool IsNullOrDisposed(this EntityFact fact)
	{
		if (fact == null)
		{
			return true;
		}
		return !fact.IsAttached;
	}
}
