using B4mServer.Models;

namespace B4mServer.Validators;

public class ChannelValidator
{
	public static (bool IsValid, string ErrorMessage) ValidateUpdatedChannel(Channel channel, Channel oldChannel)
	{
		// not allowed to change id, name
		if (channel.Id != oldChannel.Id || channel.Name != oldChannel.Name)
		{
			return (false, "Invalid channel object");
		}

		var nameResult = SharedValidator.ValidateName(channel.Name);

		// you can change color
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

		// password is optional
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
