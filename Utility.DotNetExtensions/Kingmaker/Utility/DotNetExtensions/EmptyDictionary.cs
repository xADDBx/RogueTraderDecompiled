using System.Collections.Generic;

namespace Kingmaker.Utility.DotNetExtensions;

public static class EmptyDictionary<TKey, TValue>
{
	public static readonly IReadOnlyDictionary<TKey, TValue> Instance = new Dictionary<TKey, TValue>();
}
