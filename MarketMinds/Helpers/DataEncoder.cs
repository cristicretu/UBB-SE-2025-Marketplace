using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marketplace_SE.Utilities
{
    public class DataEncoder
    {
        public static string Char2Hex(byte data)
        {
            int p = data / 16;
            int q = data % 16;
            string output = string.Empty;

            if (p < 10)
            {
                output += (char)('0' + p);
            }
            else
            {
                output += (char)('a' + (p - 10));
            }
            if (q < 10)
            {
                output += (char)('0' + q);
            }
            else
            {
                output += (char)('a' + (q - 10));
            }
            return output;
        }

        public static byte Hex2Char(string hex)
        {
            int p, q;
            if (hex[0] <= '9')
            {
                p = hex[0] - '0';
            }
            else
            {
                p = hex[0] - 'a' + 10;
            }
            p *= 16;

            if (hex[1] <= '9')
            {
                q = hex[1] - '0';
            }
            else
            {
                q = hex[1] - 'a' + 10;
            }

            return (byte)(p + q);
        }

        public static byte Hex2Char(char[] hex)
        {
            int p, q;
            if (hex[0] <= '9')
            {
                p = hex[0] - '0';
            }
            else
            {
                p = hex[0] - 'a' + 10;
            }
            p *= 16;

            if (hex[1] <= '9')
            {
                q = hex[1] - '0';
            }
            else
            {
                q = hex[1] - 'a' + 10;
            }

            return (byte)(p + q);
        }

        public static string HexEncode(byte[] data)
        {
            string hexString = BitConverter.ToString(data).Replace("-", string.Empty).ToLower();
            return hexString;
            /*
            StringBuilder sb = new StringBuilder();
            for(int i=0;i<data.Length; i++)
            {
                string h = Char2Hex(data[i]);
                sb.Append(h);
            }
            return sb.ToString();
            */
        }

        public static byte[] HexDecode(string data)
        {
            byte[] byteArray = Enumerable.Range(0, data.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(data.Substring(x, 2), 16))
                             .ToArray();
            return byteArray;

            /*
            byte[] output = new byte[data.Length/2];
            for (int i = 0; i < data.Length; i+=2)
            {
                output[i/2] = Hex2Char(new char[]
                {
                    data[i],
                    data[i+1],
                });
            }
            return output;
            */
        }

        public static string ConvertTimestampToDate(ulong timestamp)
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds((long)timestamp);
            return dateTimeOffset.ToString("MM/dd/yyyy");
        }

        public static string ConvertTimestampToLocalDateTime(ulong timestamp)
        {
            DateTimeOffset utcTime = DateTimeOffset.FromUnixTimeMilliseconds((long)timestamp);
            DateTimeOffset localTime = utcTime.ToLocalTime();
            return localTime.ToString("MM/dd/yyyy HH:mm:ss");
        }
    }
}
