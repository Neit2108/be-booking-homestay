﻿namespace HomestayBookingAPI.DTOs.Password
{
    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
        
    }
}
