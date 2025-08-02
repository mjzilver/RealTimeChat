using RealTimeChatServer.Models;

namespace RealTimeChatServer.Validators;

public class ChannelValidator
{
	public static (bool IsValid, string ErrorMessage) ValidateUpdatedChannel(Channel channel, Channel oldChannel)
	{
		if (channel.Id != oldChannel.Id || channel.Name != oldChannel.Name)
		{
			return (false, "Invalid channel object");
		}

		var nameResult = SharedValidator.ValidateName(channel.Name);

		if (!nameResult.IsValid)
		{
			return nameResult;
		}

		return (true, "");
	}
	public static (bool IsValid, string ErrorMessage) ValidateNewChannel(Channel channel)
	{
		var nameResult = SharedValidator.ValidateName(channel.Name);
		if (!nameResult.IsValid)
		{
			return nameResult;
		}

		var colorResult = SharedValidator.ValidateColor(channel.Color);
		if (!colorResult.IsValid)
		{
			return colorResult;
		}

		if (channel.Password != null)
		{
			var passwordResult = SharedValidator.ValidatePassword(channel.Password);
			if (!passwordResult.IsValid)
			{
				return passwordResult;
			}
		}

		return (true, "");
	}
}
