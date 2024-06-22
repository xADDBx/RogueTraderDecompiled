using System;

namespace Kingmaker.Blueprints.JsonSystem.Helpers;

public interface IIdAttribute
{
	string GuidString { get; }

	Guid Guid { get; }
}
