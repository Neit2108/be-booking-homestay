using QRCoder;
using System;
using System.Text;

namespace HomestayBookingAPI.Utils
{
    public class VietQRGenerator
    {
        // Các hằng số định dạng VietQR
        private const string ID_PAYLOAD_FORMAT = "00";
        private const string ID_POI_METHOD = "01";
        private const string ID_MERCHANT_INFORMATION_BOT = "26";
        private const string ID_GUID = "00";
        private const string ID_MERCHANT_ID = "01";
        private const string ID_SERVICE_CODE = "02";
        private const string ID_TRANSACTION_CURRENCY = "53";
        private const string ID_TRANSACTION_AMOUNT = "54";
        private const string ID_COUNTRY_CODE = "58";
        private const string ID_ADDITIONAL_DATA = "62";
        private const string ID_ADDITIONAL_DATA_BILL_NUMBER = "01";
        private const string ID_CRC = "63";

        /// <summary>
        /// Tạo chuỗi nội dung QR code theo chuẩn VietQR
        /// </summary>
        /// <param name="bankId">Mã ngân hàng (VNPayQR hoặc mã BIN ngân hàng)</param>
        /// <param name="accountNo">Số tài khoản</param>
        /// <param name="accountName">Tên người thụ hưởng (tùy chọn)</param>
        /// <param name="amount">Số tiền (tùy chọn)</param>
        /// <param name="description">Nội dung thanh toán (tùy chọn)</param>
        /// <returns>Chuỗi QR theo chuẩn VietQR</returns>
        public static string GenerateVietQRContent(
            string bankId,
            string accountNo,
            string accountName = null,
            decimal? amount = null,
            string description = null)
        {
            var sb = new StringBuilder();

            // Thông tin định dạng payload - 00 02 01 (chuẩn EMVCo QR)
            sb.Append(GenerateField(ID_PAYLOAD_FORMAT, "01"));

            // Phương thức khởi tạo - 01 02 11 (Static QR)
            sb.Append(GenerateField(ID_POI_METHOD, "11"));

            // Thông tin merchant - 26
            var merchantInfo = new StringBuilder();
            // ID định danh toàn cầu - 00 (VN)
            merchantInfo.Append(GenerateField(ID_GUID, "A000000727"));
            // ID merchant - 01
            merchantInfo.Append(GenerateField(ID_MERCHANT_ID, $"{bankId}{accountNo}"));
            // Service code - 02
            merchantInfo.Append(GenerateField(ID_SERVICE_CODE, "QRIBFTTA"));

            sb.Append(GenerateField(ID_MERCHANT_INFORMATION_BOT, merchantInfo.ToString()));

            // Mã tiền tệ - 53 03 704 (VND)
            sb.Append(GenerateField(ID_TRANSACTION_CURRENCY, "704"));

            // Số tiền giao dịch - 54 (nếu có)
            if (amount.HasValue && amount.Value > 0)
            {
                sb.Append(GenerateField(ID_TRANSACTION_AMOUNT, amount.Value.ToString("0")));
            }

            // Mã quốc gia - 58 02 VN
            sb.Append(GenerateField(ID_COUNTRY_CODE, "VN"));

            // Thông tin bổ sung - 62
            if (!string.IsNullOrEmpty(description))
            {
                var additionalData = new StringBuilder();
                // Mã hóa đơn - 01
                additionalData.Append(GenerateField(ID_ADDITIONAL_DATA_BILL_NUMBER, description));
                sb.Append(GenerateField(ID_ADDITIONAL_DATA, additionalData.ToString()));
            }

            // Thêm CRC (cyclic redundancy check) - 63 04 XXXX
            var crc = CalculateCRC(sb.ToString());
            sb.Append(GenerateField(ID_CRC, crc));

            return sb.ToString();
        }

        /// <summary>
        /// Tạo QR code VietQR dưới dạng base64
        /// </summary>
        /// <param name="bankId">Mã ngân hàng</param>
        /// <param name="accountNo">Số tài khoản</param>
        /// <param name="accountName">Tên người thụ hưởng</param>
        /// <param name="amount">Số tiền</param>
        /// <param name="description">Nội dung thanh toán</param>
        /// <returns>Chuỗi base64 của hình ảnh QR</returns>
        public static string GenerateVietQRBase64(
            string bankId,
            string accountNo,
            string accountName = null,
            decimal? amount = null,
            string description = null)
        {
            string qrContent = GenerateVietQRContent(bankId, accountNo, accountName, amount, description);

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.M);
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeBytes = qrCode.GetGraphic(20);

            return Convert.ToBase64String(qrCodeBytes);
        }

        /// <summary>
        /// Tạo trường dữ liệu QR theo định dạng ID + Length + Value
        /// </summary>
        private static string GenerateField(string id, string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            string valueLength = value.Length.ToString("00");
            return $"{id}{valueLength}{value}";
        }

        /// <summary>
        /// Tính toán CRC-16/CCITT-FALSE cho nội dung QR
        /// </summary>
        private static string CalculateCRC(string content)
        {
            // Thêm các byte 0 bổ sung để tính CRC
            content += "6304";

            int crc = 0xFFFF; // Giá trị ban đầu

            foreach (char c in content)
            {
                crc ^= (c << 8);
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x8000) != 0)
                        crc = (crc << 1) ^ 0x1021;
                    else
                        crc <<= 1;
                }
            }

            crc &= 0xFFFF; // 16 bit cuối
            return crc.ToString("X4"); // Hex 4 ký tự
        }
    }
}