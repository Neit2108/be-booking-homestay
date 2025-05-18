namespace HomestayBookingAPI.Utils
{
    public class GeneratePassword
    {
        public static string GenerateStrongPassword(int length = 6)
        {
            if (length < 6) length = 6;

            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string specials = "!@#$%^&*()_-+=<>?";

            var rnd = new Random();

            string password =
                upper[rnd.Next(upper.Length)].ToString() +
                lower[rnd.Next(lower.Length)].ToString() +
                digits[rnd.Next(digits.Length)].ToString() +
                specials[rnd.Next(specials.Length)].ToString();

            string all = upper + lower + digits + specials;
            for (int i = password.Length; i < length; i++)
            {
                password += all[rnd.Next(all.Length)];
            }

            return new string(password.OrderBy(_ => rnd.Next()).ToArray());
        }

    }
}
