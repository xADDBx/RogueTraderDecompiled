using System;

namespace Owlcat.Runtime.Core.Logging;

[AttributeUsage(AttributeTargets.Method)]
public class StackTraceIgnore : Attribute
{
}
