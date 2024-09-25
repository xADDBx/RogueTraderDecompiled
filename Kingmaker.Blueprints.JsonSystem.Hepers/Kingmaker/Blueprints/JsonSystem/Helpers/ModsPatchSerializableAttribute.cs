using System;
using System.Runtime.InteropServices;

namespace Kingmaker.Blueprints.JsonSystem.Helpers;

[AttributeUsage(AttributeTargets.Field, Inherited = true)]
[ComVisible(true)]
public class ModsPatchSerializableAttribute : Attribute
{
}
