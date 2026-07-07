using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace TravelProject.Services
{
    public class VnpayService
    {
        private readonly string _tmnCode = "COV9584Z"; // Public Sandbox Merchant ID
        private readonly string _hashSecret = "UX8V7N30BFLW2D30S2R6H2U7S8G9J8D8"; // Public Sandbox Hash Secret
        private readonly string _paymentUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";

        public string CreatePaymentUrl(HttpContext httpContext, string bookingType, int bookingId, decimal amount, string ipAddress, string returnUrl)
        {
            var vnpayData = new SortedDictionary<string, string>();
            vnpayData.Add("vnp_Version", "2.1.0");
            vnpayData.Add("vnp_Command", "pay");
            vnpayData.Add("vnp_TmnCode", _tmnCode);
            vnpayData.Add("vnp_Amount", ((long)(amount * 100)).ToString()); // Amount must be multiplied by 100
            vnpayData.Add("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpayData.Add("vnp_CurrCode", "VND");
            vnpayData.Add("vnp_IpAddr", ipAddress);
            vnpayData.Add("vnp_Locale", "vn");
            vnpayData.Add("vnp_OrderInfo", $"Thanh toan {bookingType} ID {bookingId}");
            vnpayData.Add("vnp_OrderType", "other");
            vnpayData.Add("vnp_ReturnUrl", returnUrl);
            vnpayData.Add("vnp_TxnRef", $"{bookingType}_{bookingId}_{DateTime.Now.Ticks}"); // Unique transaction reference

            var rawData = new StringBuilder();
            var queryData = new StringBuilder();
            foreach (var kv in vnpayData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    rawData.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                    queryData.Append(kv.Key + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }
            
            // Remove the trailing &
            if (rawData.Length > 0) rawData.Length--;
            if (queryData.Length > 0) queryData.Length--;

            string secureHash = HmacSha512(_hashSecret, rawData.ToString());
            string paymentUrl = _paymentUrl + "?" + queryData.ToString() + "&vnp_SecureHash=" + secureHash;
            return paymentUrl;
        }

        public bool ValidateSignature(IQueryCollection query, out string bookingType, out int bookingId, out string responseCode)
        {
            bookingType = "";
            bookingId = 0;
            responseCode = "";

            string vnp_SecureHash = "";
            var vnpayData = new SortedDictionary<string, string>();

            foreach (var key in query.Keys)
            {
                if (key.StartsWith("vnp_"))
                {
                    if (key == "vnp_SecureHash")
                    {
                        vnp_SecureHash = query[key]!;
                    }
                    else
                    {
                        vnpayData.Add(key, query[key]!);
                    }
                }
            }

            var rawData = new StringBuilder();
            foreach (var kv in vnpayData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    rawData.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }
            if (rawData.Length > 0) rawData.Length--;

            string calculatedHash = HmacSha512(_hashSecret, rawData.ToString());
            if (!calculatedHash.Equals(vnp_SecureHash, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            // Extract info from txn ref or order info
            string orderInfo = query["vnp_OrderInfo"]!; // e.g. "Thanh toan Tour ID 123"
            responseCode = query["vnp_ResponseCode"]!;  // "00" is success

            if (!string.IsNullOrEmpty(orderInfo))
            {
                var parts = orderInfo.Split(' ');
                if (parts.Length >= 5)
                {
                    bookingType = parts[2];
                    int.TryParse(parts[4], out bookingId);
                }
            }

            return true;
        }

        private static string HmacSha512(string key, string inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }
            return hash.ToString();
        }
    }
}
