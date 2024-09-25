using System.Reflection;

namespace Owlcat.QA.Validation;

public class ValidateNotNullAttribute : ValidatingFieldAttribute
{
	public override void ValidateField(object obj, FieldInfo field, ValidationContext context, int parentIndex)
	{
	}
}
