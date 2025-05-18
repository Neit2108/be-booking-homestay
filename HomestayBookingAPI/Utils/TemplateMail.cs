using System.Threading.Tasks;
using HomestayBookingAPI.Data;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Models.Enum;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace HomestayBookingAPI.Utils
{
    public class TemplateMail
    {
        public static string OTPTwoFactor(string otp)
        {
            return $@"
        <html>
        <head>
            <style>
                body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #f4f6f8; color: #222; }}
                .container {{ max-width: 500px; margin: 40px auto; padding: 30px 30px 24px 30px; background: #fff; border-radius: 10px; box-shadow: 0 3px 12px rgba(0,0,0,0.09); }}
                h2 {{ color: #007bff; margin-bottom: 14px; }}
                .password-block {{ font-size: 24px; padding: 14px 0; margin-bottom: 18px; background: #f0f7ff; border-radius: 5px; text-align: center; letter-spacing: 1px; font-weight: bold; color: #0056b3; }}
                .btn {{ display: inline-block; margin-top: 18px; padding: 10px 22px; background: #007bff; color: #fff; text-decoration: none; border-radius: 5px; font-weight: 500; }}
                p {{ margin-bottom: 12px; }}
            </style>
        </head>
        <body>
            <div class=""container"">
                <h2>Xác thực đăng nhập</h2>
                <p>Xin chào Quý khách,</p>
                <div class=""password-block"">Mã xác thực đăng nhập: <span style=""font-family:monospace;"">{otp}</span></div>
                <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email.</p>
                <p style=""color:#888; font-size:13px;"">Trân trọng,<br>Đội ngũ hỗ trợ</p>
            </div>
        </body>
        </html>
    ";
        }

        public static string OTPEnableTwoFactor(string otp)
        {
            return $@"
        <html>
        <head>
            <style>
                body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #f4f6f8; color: #222; }}
                .container {{ max-width: 500px; margin: 40px auto; padding: 30px 30px 24px 30px; background: #fff; border-radius: 10px; box-shadow: 0 3px 12px rgba(0,0,0,0.09); }}
                h2 {{ color: #007bff; margin-bottom: 14px; }}
                .password-block {{ font-size: 24px; padding: 14px 0; margin-bottom: 18px; background: #f0f7ff; border-radius: 5px; text-align: center; letter-spacing: 1px; font-weight: bold; color: #0056b3; }}
                .btn {{ display: inline-block; margin-top: 18px; padding: 10px 22px; background: #007bff; color: #fff; text-decoration: none; border-radius: 5px; font-weight: 500; }}
                p {{ margin-bottom: 12px; }}
            </style>
        </head>
        <body>
            <div class=""container"">
                <h2>Mã xác thực bật bảo mật 2 lớp</h2>
                <p>Xin chào Quý khách,</p>
                <div class=""password-block"">Mã xác thực : <span style=""font-family:monospace;"">{otp}</span></div>
                <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email.</p>
                <p style=""color:#888; font-size:13px;"">Trân trọng,<br>Đội ngũ hỗ trợ</p>
            </div>
        </body>
        </html>
    ";
        }


        public static string ForgotPasswordEmail(string password, string fullName, string url)
        {
            return $@"
        <html>
        <head>
            <style>
                body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #f4f6f8; color: #222; }}
                .container {{ max-width: 500px; margin: 40px auto; padding: 30px 30px 24px 30px; background: #fff; border-radius: 10px; box-shadow: 0 3px 12px rgba(0,0,0,0.09); }}
                h2 {{ color: #007bff; margin-bottom: 14px; }}
                .password-block {{ font-size: 18px; padding: 14px 0; margin-bottom: 18px; background: #f0f7ff; border-radius: 5px; text-align: center; letter-spacing: 1px; font-weight: bold; color: #0056b3; }}
                .btn {{ display: inline-block; margin-top: 18px; padding: 10px 22px; background: #007bff; color: #fff; text-decoration: none; border-radius: 5px; font-weight: 500; }}
                p {{ margin-bottom: 12px; }}
            </style>
        </head>
        <body>
            <div class=""container"">
                <h2>Khôi phục mật khẩu thành công</h2>
                <p>Xin chào {fullName ?? "Quý khách"},</p>
                <p>Bạn vừa yêu cầu khôi phục mật khẩu cho tài khoản của mình.</p>
                <div class=""password-block"">Mật khẩu mới của bạn: <span style=""font-family:monospace;"">{password}</span></div>
                <p>Vui lòng <a href=""{url}"" class=""btn"">Đăng nhập ngay</a> và đổi lại mật khẩu để bảo mật tài khoản.</p>
                <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email.</p>
                <p style=""color:#888; font-size:13px;"">Trân trọng,<br>Đội ngũ hỗ trợ</p>
            </div>
        </body>
        </html>
    ";
        }
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

        public static string DepositSuccessEmail(WalletTransaction walletTransaction, string url)
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
            <h2>Thông báo nạp tiền thành công</h2>
            <p>Chào {walletTransaction.Wallet?.User?.FullName ?? "Quý khách"},</p>
            <p>Giao dịch nạp tiền <strong>{walletTransaction.Id}</strong> thành công.</p>
            <div class=""row""><span class=""label"">Mã giao dịch:</span> {walletTransaction.Payment.TransactionId}</div>
            <div class=""row""><span class=""label"">Nội dung:</span> {walletTransaction.Description}</div>
            <div class=""row""><span class=""label"">Ngày tạo:</span> {walletTransaction.CreatedAt}</div>
            <div class=""row""><span class=""label"">Tổng tiền:</span> <span class=""price"">{walletTransaction.Amount:N0} VND</span></div>
            <div class=""row""><span class=""label"">Số dư ví:</span> <span class=""price"">{walletTransaction.Wallet?.Balance:N0} VND</span></div>
            <div class=""row""><span class=""label"">Trạng thái:</span> Đã nạp thành công</div>
            <p>Vui lòng truy cập <a href=""{url}"">vào đây</a> để xem chi tiết đặt phòng.</p>
            <p>Trân trọng,<br>Đội ngũ HomestayBooking</p>
        </div>
    </body>
    </html>
";
        }

    }

}
