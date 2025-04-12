using HomestayBookingAPI.Data;
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

    }

}
