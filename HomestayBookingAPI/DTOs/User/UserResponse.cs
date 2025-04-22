namespace HomestayBookingAPI.DTOs.User
{
    public class UserResponse
    {
        public string FullName { get; set; }
        public string IdentityCard {  get; set; }
        public string HomeAddress { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public List<string> Role { get; set; }

    }
}
