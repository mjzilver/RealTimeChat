namespace RealTimeChatServer.Validators;

public class SharedValidator
{
	private static readonly string[] AllowedColors =
	[
		"red",
		"green",
		"blue",
		"yellow",
		"purple",
		"orange",
		"pink",
		"brown",
		"white",
		"grey"
	];

	public static (bool IsValid, string ErrorMessage) ValidateColor(string color)
	{
		return (AllowedColors.Contains(color), "color must be one of the following: " + string.Join(", ", AllowedColors));
	}

	public static (bool IsValid, string ErrorMessage) ValidateName(string name)
	{
		return (name.Length is >= 5 and <= 50, "name must be more than 5 and less than 50 characters");
	}

	public static (bool IsValid, string ErrorMessage) ValidatePassword(string password)
	{
		return (password.Length is >= 5 and <= 50, "password must be more than 5 and less than 50 characters");
	}
}
