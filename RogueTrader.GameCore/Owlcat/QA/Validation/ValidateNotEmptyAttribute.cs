using System.Collections;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Localization;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.QA.Validation;

public class ValidateNotEmptyAttribute : ValidatingFieldAttribute
{
	public override void ValidateField(object obj, FieldInfo field, ValidationContext context, int parentIndex)
	{
		if (field.FieldType.IsUnityCollection())
		{
			ValidateList(obj, field, parentIndex, context);
		}
		else if (field.FieldType == typeof(LocalizedString))
		{
			if (!(field.GetValue(obj) is LocalizedString localizedString) || !localizedString.IsSet())
			{
				context.AddError(ErrorLevel.Normal, field.Name + " text is not set");
			}
		}
		else if (!(field.FieldType != typeof(EntityReference)) && (!(field.GetValue(obj) is EntityReference entityReference) || string.IsNullOrEmpty(entityReference.UniqueId)))
		{
			context.AddError(ErrorLevel.Normal, field.Name + " reference is not set");
		}
	}

	private void ValidateList(object obj, FieldInfo field, int parentIndex, ValidationContext context)
	{
		if (field.GetValue(obj) is ICollection { Count: 0 })
		{
			context.AddError(ErrorLevel.Normal, field.Name + " array is empty");
		}
	}
}
