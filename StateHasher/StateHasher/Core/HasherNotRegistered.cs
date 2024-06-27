using System;

namespace StateHasher.Core;

public class HasherNotRegistered : Exception
{
	private readonly Type _type;

	public override string Message => ToString();

	public HasherNotRegistered(Type type)
	{
		_type = type;
	}

	public override string ToString()
	{
		return "Type " + _type.FullName + " not registered";
	}
}
