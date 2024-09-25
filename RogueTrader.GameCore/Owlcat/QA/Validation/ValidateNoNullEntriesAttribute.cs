using System.Collections;
using System.Reflection;
using Kingmaker.Blueprints;

namespace Owlcat.QA.Validation;

public class ValidateNoNullEntriesAttribute : ValidatingFieldAttribute
{
	public override void ValidateField(object obj, FieldInfo field, ValidationContext context, int parentIndex)
	{
		if (!(field.GetValue(obj) is IEnumerable enumerable))
		{
			return;
		}
		int num = 0;
		foreach (object item in enumerable)
		{
			if (item is BlueprintReferenceBase blueprintReferenceBase && (blueprintReferenceBase.IsEmpty() || blueprintReferenceBase.GetBlueprint() == null))
			{
				context.AddError(ErrorLevel.Critical, "Unknown", "{0}[{1}] is NULL!", field.Name, num);
			}
			else if (item == null)
			{
				context.AddError(ErrorLevel.Critical, "Unknown", "{0}[{1}] is NULL!", field.Name, num);
			}
			num++;
		}
	}
}
