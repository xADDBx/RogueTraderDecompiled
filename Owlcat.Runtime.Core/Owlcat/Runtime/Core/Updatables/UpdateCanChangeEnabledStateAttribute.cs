using System;

namespace Owlcat.Runtime.Core.Updatables;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class UpdateCanChangeEnabledStateAttribute : Attribute
{
}
