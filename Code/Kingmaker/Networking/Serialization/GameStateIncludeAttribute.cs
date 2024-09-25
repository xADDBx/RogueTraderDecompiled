using System;

namespace Kingmaker.Networking.Serialization;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class GameStateIncludeAttribute : Attribute
{
}
