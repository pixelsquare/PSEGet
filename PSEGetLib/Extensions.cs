using System;
using System.Globalization;
using System.Collections.Specialized;

namespace PSEGetLib
{
    public static class MyExtensions
    {
        public static DateTime UnixTimestampToDateTime(this ulong unixTimestamp)
        {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
            dt = dt.AddSeconds(unixTimestamp);
            return dt;
        }

        public static ulong ToUnixTimestamp(this DateTime dateTime)
        {
            DateTime now = new DateTime(2006, 3, 22, 23, 12, 23);
            TimeSpan t = (now - new DateTime(1970, 1, 1).ToLocalTime());
            ulong timestamp = (ulong)t.TotalSeconds;
            return timestamp;
        }

        public static int IndexOfString(this StringCollection sc, string substr)
        {
            int index = -1;
            foreach (string s in sc)
            {
                if (s.Contains(substr))
                {
                    return index + 1;
                }

                index++;
            }

            return -1;
        }
		
		
        /// <summary>
        /// Returns the first occurance of a string convertible to a double
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Nullable<double> ParseDouble(this string s)
        {
            string[] row = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            double d;
            //bool isNegative = false;
            NumberFormatInfo nfi = new NumberFormatInfo();	
            nfi.NumberNegativePattern = 0;
            nfi.NumberGroupSeparator = ",";
            nfi.NumberDecimalSeparator = ".";
			nfi.NegativeSign ="-";
			
		
            for (int i = 0; i < row.Length; i++)
            {
                if (row[i].Trim().Length == 0)
                    continue;

				string stringNumber = row[i].Trim().Replace(",", string.Empty); 
				stringNumber = stringNumber.Replace("(", "-").Replace(")", string.Empty);
                if (double.TryParse(stringNumber, NumberStyles.Any, nfi, out d))
                {
                    return d;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the first occurance of a string convertible to a int
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Nullable<int> ParseInt(this string s)
        {
            string[] row = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int u;
            //bool isNegative = false;
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberNegativePattern = 0;
            nfi.NumberGroupSeparator = ",";
            nfi.NumberDecimalSeparator = ".";


            for (int i = 0; i < row.Length; i++)
            {
                if (row[i].Trim().Length == 0)
                    continue;

				string stringNumber = row[i].Trim().Replace(",", string.Empty); 
				stringNumber = stringNumber.Replace("(", "-").Replace(")", string.Empty);
                if (int.TryParse(stringNumber, NumberStyles.Any, nfi, out u))
                {
                    return u;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the first occurance of a string covertible to a ulong
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Nullable<ulong> ParseUlong(this string s)
        {
            string[] row = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            ulong u;
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberNegativePattern = 0;
            nfi.NumberGroupSeparator = ",";
            nfi.NumberDecimalSeparator = ".";
            
            for (int i = 0; i < row.Length; i++)
            {
                //string stringNumber = row[i].Trim().Replace(",", string.Empty);
                //stringNumber = stringNumber.Replace("(", "-").Replace(")", string.Empty);
                if (ulong.TryParse(row[i].Trim(), NumberStyles.Any, nfi, out u))
                {
                    return u;
                }
            }

            return null;
        }

        public static Nullable<long> ParseLong(this string s)
        {
            string[] row = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            long u;
            //bool isNegative = false;
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberNegativePattern = 0;
            nfi.NumberGroupSeparator = ",";
            nfi.NumberDecimalSeparator = ".";

            for (int i = 0; i < row.Length; i++)
            {
                if (row[i].Trim().Length == 0)
                    continue;

                //string stringNumber = row[i].Trim().Replace(",", string.Empty);
                //stringNumber = stringNumber.Replace("(", "-").Replace(")", string.Empty);
                if (long.TryParse(row[i].Trim(), NumberStyles.Any, nfi, out u))
                {

                    return u;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns true if the NameValueCollection object's Value field contains the specified string
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool ContainsValue(this NameValueCollection nv, string s)
        {
            foreach (string key in nv.Keys)
            {
                foreach (string value in nv.GetValues(key))
                {
                    if (value == s)
                        return true;
                }
            }

            return false;
        }

        public static string GetKey(this NameValueCollection c, string s)
        {
            foreach (string key in c.Keys)
            {
                foreach (string value in c.GetValues(key))
                {
                    if (value == s)
                        return key;
                }
            }

            return null;
        }
    }
}
