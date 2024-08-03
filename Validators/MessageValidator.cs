using B4mServer.Models;

namespace B4mServer.Validators;

public class MessageValidator
{
	public static (bool IsValid, string ErrorMessage) ValidateNewMessage(Message message)
	{
		if (message.Text.Length < 1 || message.Text.Length > 300)
		{
			return (false, "Message text length must be between 1 and 300 characters.");
		}

		return (true, "");
	}
}