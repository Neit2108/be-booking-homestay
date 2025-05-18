namespace HomestayBookingAPI.DTOs
{
    public class LoginReponseDTO
    {
        public string Token { get; set; }
        public string FullName { get; set; }
        public string AvatarUrl { get; set; }
        public bool RequiresTwoFactor { get; set; }
        public string UserId { get; set; }
    }
}
