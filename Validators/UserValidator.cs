using B4mServer.Models;

namespace B4mServer.Validators;

public class UserValidator
{
	public static (bool IsValid, string ErrorMessage) ValidateNewUser(User user)
	{
		var nameResult = SharedValidator.ValidateName(user.Name);
		if (!nameResult.IsValid)
		{
			return nameResult;
		}

		if (user.Password == null)
		{
			return (false, "Password cannot be null");
		}

		var passwordResult = SharedValidator.ValidatePassword(user.Password);

		if (!passwordResult.IsValid)
		{
			return passwordResult;
		}

		return (true, "");
	}

	public static (bool IsValid, string ErrorMessage) ValidateUpdatedUser(User user, User oldUser)
	{
		// not allowed to change id, name
		if (user.Id != oldUser.Id || user.Name != oldUser.Name)
		{
			return (false, "User Name cannot be changed.");
		}

		return (true, "");
	}
}

