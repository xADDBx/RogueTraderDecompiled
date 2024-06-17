using System;

namespace Kingmaker.Blueprints.JsonSystem.Helpers;

[AttributeUsage(AttributeTargets.Field)]
public class ExcludeFieldFromBuildAttribute : Attribute
{
}
