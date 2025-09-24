using System;

namespace Owlcat.QA.Validation;

[AttributeUsage(AttributeTargets.Class)]
public class ExcludeFromValidation : Attribute
{
}
