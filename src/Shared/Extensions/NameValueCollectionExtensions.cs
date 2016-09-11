using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Collections.Specialized {

    /// <summary>
    /// 
    /// </summary>
    static class XtsNameValueCollection {

        #region Static Methods ////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the bool.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">if set to <c>true</c> [default value].</param>
        /// <returns></returns>
        public static bool GetBool(this NameValueCollection collection, string key, bool defaultValue) {

            bool result = defaultValue;
            string value = collection[key];
            if (!string.IsNullOrEmpty(value))
                bool.TryParse(value, out result);
            return result;
        }

        /// <summary>
        /// Gets the bool.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static bool GetBool(this NameValueCollection collection, string key) {
            return GetBool(collection, key, false);
        }

        /// <summary>
        /// Gets the date time.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static DateTime GetDate(this NameValueCollection collection, string key, DateTime defaultValue) {

            DateTime result = defaultValue;
            string value = collection[key];
            if (!string.IsNullOrEmpty(value))
                DateTime.TryParse(value, out result);
            return result;
        }

        /// <summary>
        /// Gets the date time.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static DateTime GetDate(this NameValueCollection collection, string key) {
            return GetDate(collection, key, DateTime.MinValue);
        }

        /// <summary>
        /// Gets the enum.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static T GetEnum<T>(this NameValueCollection collection, string key, T defaultValue) {

            T result = defaultValue;
            string value = collection[key];
            if (!string.IsNullOrEmpty(value))
                result = (T)Enum.Parse(typeof(T), value, true);
            return result;
        }

        /// <summary>
        /// Gets the enum.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static T GetEnum<T>(this NameValueCollection collection, string key) {
            return GetEnum<T>(collection, key, default(T));
        }

        /// <summary>
        /// Gets the int.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static int GetInt(this NameValueCollection collection, string key, int defaultValue) {

            int result = defaultValue;
            string value = collection[key];
            if (!string.IsNullOrEmpty(value))
                int.TryParse(value, out result);
            return result;
        }

        /// <summary>
        /// Gets the int.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static int GetInt(this NameValueCollection collection, string key) {
            return GetInt(collection, key, 0);
        }

        /// <summary>
        /// Gets the string.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static string GetString(this NameValueCollection collection, string key, string defaultValue) {
            return collection[key] ?? defaultValue;
        }

        /// <summary>
        /// Gets the string.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string GetString(this NameValueCollection collection, string key) {
            return GetString(collection, key, null);
        }
        #endregion
    }
}
