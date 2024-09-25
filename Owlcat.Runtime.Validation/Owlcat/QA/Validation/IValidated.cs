namespace Owlcat.QA.Validation;

public interface IValidated
{
	void Validate(ValidationContext context, int parentIndex);
}
