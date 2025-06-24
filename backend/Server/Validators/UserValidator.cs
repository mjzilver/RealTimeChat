using RealTimeChatServer.Models;

namespace RealTimeChatServer.Validators;

public class UserValidator
{
	public static (bool IsValid, string ErrorMessage) ValidateNewUser(User user)
	{
		(bool IsValid, string ErrorMessage) nameResult = SharedValidator.ValidateName(user.Name);
		if (!nameResult.IsValid)
		{
			return nameResult;
		}

		if (user.Password == null)
		{
			return (false, "Password cannot be null");
		}

		(bool IsValid, string ErrorMessage) passwordResult = SharedValidator.ValidatePassword(user.Password);

		if (!passwordResult.IsValid)
		{
			return passwordResult;
		}

		return (true, "");
	}

	public static (bool IsValid, string ErrorMessage) ValidateUpdatedUser(User user, User oldUser)
	{
		if (user.Id != oldUser.Id || user.Name != oldUser.Name)
		{
			return (false, "User Name cannot be changed.");
		}

		return (true, "");
	}
}

