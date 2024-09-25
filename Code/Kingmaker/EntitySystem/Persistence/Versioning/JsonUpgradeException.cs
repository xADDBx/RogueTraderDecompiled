using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Kingmaker.EntitySystem.Persistence.Versioning;

public class JsonUpgradeException : Exception
{
	public JsonUpgradeException()
	{
	}

	public JsonUpgradeException(string message)
		: base(message)
	{
	}

	public JsonUpgradeException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected JsonUpgradeException([NotNull] SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
