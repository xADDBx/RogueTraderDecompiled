using System;
using Newtonsoft.Json;

namespace Kingmaker.Utility;

public class ContextTypeJsonConverter : JsonConverter<BugContext.ContextType>
{
	public override bool CanWrite => false;

	public override void WriteJson(JsonWriter writer, BugContext.ContextType value, JsonSerializer serializer)
	{
		throw new NotImplementedException("No need to write this");
	}

	public override BugContext.ContextType ReadJson(JsonReader reader, Type objectType, BugContext.ContextType existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (Enum.TryParse(typeof(BugContext.ContextType), reader.Value?.ToString() ?? "", ignoreCase: true, out var result))
		{
			return (BugContext.ContextType)result;
		}
		return BugContext.ContextType.None;
	}
}
