namespace B4mServer.Validators;

public class SharedValidator
{
	// array of all color strings allowed
	private static readonly string[] AllowedColors = new string[]
	{
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
	};

	public static (bool IsValid, string ErrorMessage) ValidateColor(string color)
	{
		return (AllowedColors.Contains(color), "color must be one of the following: " + string.Join(", ", AllowedColors));
	}

	public static (bool IsValid, string ErrorMessage) ValidateName(string name)
	{
		return (name.Length >= 5 && name.Length <= 50, "name must be more than 5 and less than 50 characters");
	}

	public static (bool IsValid, string ErrorMessage) ValidatePassword(string password)
	{
		return (password.Length >= 5 && password.Length <= 50, "password must be more than 5 and less than 50 characters");
	}
}
