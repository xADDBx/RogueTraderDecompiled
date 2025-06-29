using System;

namespace UnityModManagerNet;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field, AllowMultiple = false)]
public class HorizontalAttribute : Attribute
{
}
