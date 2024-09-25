using System.Reflection;

namespace Owlcat.QA.Validation;

public class ValidateHasAutoDestroyAttribute : ValidatingFieldAttribute
{
	public override void ValidateField(object obj, FieldInfo field, ValidationContext context, int parentIndex)
	{
		field.GetValue(obj);
	}
}
