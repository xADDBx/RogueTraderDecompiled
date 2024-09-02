namespace Kingmaker.QA;

public interface ISpamDetectionStrategy
{
	(bool, SpamDetectionResult?) Check(RegistrationService<LogItem> registrationService);
}
