using RealTimeChatServer.Models;

namespace RealTimeChatServer.Validators;

public class MessageValidator
{
	public static (bool IsValid, string ErrorMessage) ValidateNewMessage(Message message)
	{
		if (message.Text.Length is < 1 or > 300)
		{
			return (false, "Message text length must be between 1 and 300 characters.");
		}

		return (true, "");
	}
}