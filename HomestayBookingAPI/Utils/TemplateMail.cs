﻿using HomestayBookingAPI.Data;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Models.Enum;

namespace HomestayBookingAPI.Utils
{
    public class TemplateMail
    {
        public static string BookingConfirmationForCustomer(Booking booking, string url)
        {
            return $@"
                    <html>
                    <head>
                        <style>
                            body {{ font-family: 'Segoe UI', sans-serif; background-color: #f9f9f9; color: #333; }}
                            .container {{ max-width: 600px; margin: auto; padding: 20px; background: white; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }}
                            h2 {{ color: #4CAF50; }}
                            .row {{ margin-bottom: 10px; }}
                            .label {{ font-weight: bold; }}
                        </style>
                    </head>
                    <body>
                        <div class=""container"">
                            <h2>Xác nhận đặt chỗ thành công</h2>
                            <p>Chào {booking.User?.FullName ?? "Quý khách"},</p>
                            <p>Bạn đã đặt chỗ thành công tại <strong>{booking.Place?.Name ?? "Địa điểm không xác định"}</strong>.</p>
                            <div class=""row""><span class=""label"">Ngày nhận phòng:</span> {booking.StartDate:dd/MM/yyyy}</div>
                            <div class=""row""><span class=""label"">Ngày trả phòng:</span> {booking.EndDate:dd/MM/yyyy}</div>
                            <div class=""row""><span class=""label"">Số khách:</span> {booking.NumberOfGuests}</div>
                            <div class=""row""><span class=""label"">Tổng tiền:</span> {booking.TotalPrice:N0} VND</div>
                            <div class=""row""><span class=""label"">Trạng thái thanh toán:</span> {booking.PaymentStatus}</div>
                            <p>Vui lòng truy cập <a href=""{url}"" class=""btn""> vào đây </a> để xem chi tiết đặt chỗ của bạn.</p>
                            <p>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi.</p>
                        </div>
                    </body>
                    </html>";
        }

        public static string BookingRequestForLanlord(Booking booking, string url)
        {
            return $@"
                    <html>
                    <head>
                        <style>
                            body {{ font-family: 'Segoe UI', sans-serif; background-color: #f9f9f9; color: #333; }}
                            .container {{ max-width: 600px; margin: auto; padding: 20px; background: white; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }}
                            h2 {{ color: #2196F3; }}
                            .row {{ margin-bottom: 10px; }}
                            .label {{ font-weight: bold; }}
                            .btn {{
                                display: inline-block;
                                padding: 10px 20px;
                                background-color: #4CAF50;
                                color: white;
                                text-decoration: none;
                                border-radius: 4px;
                                margin-top: 15px;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class=""container"">
                            <h2>Thông báo đặt chỗ mới</h2>
                            <p>Có một khách hàng đã đặt chỗ tại <strong>{booking.Place?.Name ?? "Chỗ ở của bạn"}</strong>.</p>
                            <div class=""row""><span class=""label"">Khách hàng:</span> {booking.User?.FullName ?? booking.UserId}</div>
                            <div class=""row""><span class=""label"">Ngày nhận phòng:</span> {booking.StartDate:dd/MM/yyyy}</div>
                            <div class=""row""><span class=""label"">Ngày trả phòng:</span> {booking.EndDate:dd/MM/yyyy}</div>
                            <div class=""row""><span class=""label"">Số khách:</span> {booking.NumberOfGuests}</div>
                            <div class=""row""><span class=""label"">Tổng tiền:</span> {booking.TotalPrice:N0} VND</div>
                            <div class=""row""><span class=""label"">Trạng thái:</span> {booking.Status}</div>

                            <a href=""{url}"" class=""btn"">Xem & Duyệt Booking</a>
                        </div>
                    </body>
                    </html>";
        }

        public static string BookingStatusChangeEmail(Booking booking, string url, bool isAccepted, string rejectReason = "Không xác định")
        {
            return isAccepted ? $@"
                    <html>
                    <head>
                        <style>
                            body {{ font-family: 'Segoe UI', sans-serif; background-color: #f9f9f9; color: #333; }}
                            .container {{ max-width: 600px; margin: auto; padding: 20px; background: white; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }}
                            h2 {{ color: {(isAccepted ? "#4CAF50" : "#F44336")}; }}
                            .row {{ margin-bottom: 10px; }}
                            .label {{ font-weight: bold; }}
                        </style>
                    </head>
                    <body>
                        <div class=""container"">
                            <h2>Đặt chỗ thành công</h2>
                            <p>Chào {booking.User?.FullName ?? "Quý khách"},</p>
                            <p>Yêu cầu đặt chỗ tại <strong>{booking.Place?.Name ?? "Địa điểm không xác định"}</strong> đã được xác nhận.</p>
                            <div class=""row""><span class=""label"">Ngày nhận phòng:</span> {booking.StartDate:dd/MM/yyyy}</div>
                            <div class=""row""><span class=""label"">Ngày trả phòng:</span> {booking.EndDate:dd/MM/yyyy}</div>
                            <div class=""row""><span class=""label"">Số khách:</span> {booking.NumberOfGuests}</div>
                            <div class=""row""><span class=""label"">Tổng tiền:</span> {booking.TotalPrice:N0} VND</div>
                            <div class=""row""><span class=""label"">Trạng thái thanh toán:</span> {booking.PaymentStatus}</div>
                            <p>Vui lòng truy cập <a href=""{url}"" class=""btn""> vào đây </a> để thanh toán.</p>
                            <p>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi.</p>
                        </div>
                    </body>
                    </html>" 
                    :
                    $@"
                    <html>
                    <head>
                        <style>
                            body {{ font-family: 'Segoe UI', sans-serif; background-color: #f9f9f9; color: #333; }}
                            .container {{ max-width: 600px; margin: auto; padding: 20px; background: white; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }}
                            h2 {{ color: #4CAF50; }}
                            .row {{ margin-bottom: 10px; }}
                            .label {{ font-weight: bold; }}
                        </style>
                    </head>
                    <body>
                        <div class=""container"">
                            <h2>Đặt chỗ không thành thành công</h2>
                            <p>Chào {booking.User?.FullName ?? "Quý khách"},</p>
                            <p>Yêu cầu đặt chỗ tại <strong>{booking.Place?.Name ?? "Địa điểm không xác định"}</strong> đã bị từ chối.</p>
                            <div class=""row""><span class=""label"">Ngày nhận phòng:</span> {booking.StartDate:dd/MM/yyyy}</div>
                            <div class=""row""><span class=""label"">Ngày trả phòng:</span> {booking.EndDate:dd/MM/yyyy}</div>
                            <div class=""row""><span class=""label"">Số khách:</span> {booking.NumberOfGuests}</div>
                            <div class=""row""><span class=""label"">Tổng tiền:</span> {booking.TotalPrice:N0} VND</div>
                            <div class=""row""><span class=""label"">Trạng thái thanh toán:</span> {booking.PaymentStatus}</div>
                            <div class=""row""><span class=""label"">Lí do từ chỗi:</span> {rejectReason}</div>
                            <p>Vui lòng truy cập <a href=""{url}"" class=""btn""> vào đây </a> để xem chi tiết.</p>
                            <p>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi.</p>
                        </div>
                    </body>
                    </html>";
        }

        // Utils/TemplateMail.cs - Thêm các template email cho thanh toán
        public static string PaymentSuccessEmail(Booking booking, string url)
        {
            return $@"
    <html>
    <head>
        <style>
            body {{ font-family: 'Segoe UI', sans-serif; background-color: #f9f9f9; color: #333; }}
            .container {{ max-width: 600px; margin: auto; padding: 20px; background: white; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }}
            h2 {{ color: #4CAF50; }}
            .row {{ margin-bottom: 10px; }}
            .label {{ font-weight: bold; }}
            .price {{ color: #4CAF50; font-weight: bold; }}
        </style>
    </head>
    <body>
        <div class=""container"">
            <h2>Thanh toán thành công</h2>
            <p>Chào {booking.User?.FullName ?? "Quý khách"},</p>
            <p>Cảm ơn bạn đã thanh toán đặt phòng tại <strong>{booking.Place?.Name ?? "Địa điểm không xác định"}</strong>.</p>
            <div class=""row""><span class=""label"">Ngày nhận phòng:</span> {booking.StartDate:dd/MM/yyyy}</div>
            <div class=""row""><span class=""label"">Ngày trả phòng:</span> {booking.EndDate:dd/MM/yyyy}</div>
            <div class=""row""><span class=""label"">Số khách:</span> {booking.NumberOfGuests}</div>
            <div class=""row""><span class=""label"">Tổng tiền:</span> <span class=""price"">{booking.TotalPrice:N0} VND</span></div>
            <div class=""row""><span class=""label"">Trạng thái:</span> Đã thanh toán</div>
            <p>Vui lòng truy cập <a href=""{url}"">vào đây</a> để xem chi tiết đặt phòng của bạn.</p>
            <p>Chúc bạn có một kỳ nghỉ vui vẻ!</p>
            <p>Trân trọng,<br>Đội ngũ HomestayBooking</p>
        </div>
    </body>
    </html>";
        }

        public static string PaymentFailureEmail(Booking booking, string url)
        {
            return $@"
    <html>
    <head>
        <style>
            body {{ font-family: 'Segoe UI', sans-serif; background-color: #f9f9f9; color: #333; }}
            .container {{ max-width: 600px; margin: auto; padding: 20px; background: white; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }}
            h2 {{ color: #F44336; }}
            .row {{ margin-bottom: 10px; }}
            .label {{ font-weight: bold; }}
            .retry-btn {{ display: inline-block; padding: 10px 20px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 4px; }}
        </style>
    </head>
    <body>
        <div class=""container"">
            <h2>Thanh toán không thành công</h2>
            <p>Chào {booking.User?.FullName ?? "Quý khách"},</p>
            <p>Rất tiếc, giao dịch thanh toán đặt phòng tại <strong>{booking.Place?.Name ?? "Địa điểm không xác định"}</strong> không thành công.</p>
            <div class=""row""><span class=""label"">Ngày nhận phòng:</span> {booking.StartDate:dd/MM/yyyy}</div>
            <div class=""row""><span class=""label"">Ngày trả phòng:</span> {booking.EndDate:dd/MM/yyyy}</div>
            <div class=""row""><span class=""label"">Số khách:</span> {booking.NumberOfGuests}</div>
            <div class=""row""><span class=""label"">Tổng tiền:</span> {booking.TotalPrice:N0} VND</div>
            <p>Vui lòng <a href=""{url}"" class=""retry-btn"">thử lại thanh toán</a> để hoàn tất đặt phòng của bạn.</p>
            <p>Nếu bạn gặp khó khăn trong quá trình thanh toán, vui lòng liên hệ với chúng tôi qua email support@homestaybooking.com hoặc hotline 1900xxxx.</p>
            <p>Trân trọng,<br>Đội ngũ HomestayBooking</p>
        </div>
    </body>
    </html>";
        }

        public static string LandlordPaymentNotificationEmail(Booking booking, string url)
        {
            return $@"
    <html>
    <head>
        <style>
            body {{ font-family: 'Segoe UI', sans-serif; background-color: #f9f9f9; color: #333; }}
            .container {{ max-width: 600px; margin: auto; padding: 20px; background: white; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }}
            h2 {{ color: #2196F3; }}
            .row {{ margin-bottom: 10px; }}
            .label {{ font-weight: bold; }}
            .price {{ color: #4CAF50; font-weight: bold; }}
        </style>
    </head>
    <body>
        <div class=""container"">
            <h2>Thông báo thanh toán đặt phòng</h2>
            <p>Chào {booking.Place?.Owner?.FullName ?? "Quý chủ nhà"},</p>
            <p>Khách hàng <strong>{booking.User?.FullName ?? "Khách hàng"}</strong> đã thanh toán thành công cho đặt phòng tại <strong>{booking.Place?.Name ?? "Chỗ ở của bạn"}</strong>.</p>
            <div class=""row""><span class=""label"">Ngày nhận phòng:</span> {booking.StartDate:dd/MM/yyyy}</div>
            <div class=""row""><span class=""label"">Ngày trả phòng:</span> {booking.EndDate:dd/MM/yyyy}</div>
            <div class=""row""><span class=""label"">Số khách:</span> {booking.NumberOfGuests}</div>
            <div class=""row""><span class=""label"">Tổng tiền:</span> <span class=""price"">{booking.TotalPrice:N0} VND</span></div>
            <div class=""row""><span class=""label"">Trạng thái:</span> Đã thanh toán</div>
            <p>Vui lòng truy cập <a href=""{url}"">vào đây</a> để xem chi tiết đặt phòng.</p>
            <p>Trân trọng,<br>Đội ngũ HomestayBooking</p>
        </div>
    </body>
    </html>";
        }

    }

}
