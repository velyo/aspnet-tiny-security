namespace System
{
    /// <summary>
    /// Extension methods for <see cref="System.String"/>
    /// </summary>
    public static class StringExtensions
    {
        #region Static Methods ////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Formats the specified string with params' values.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        public static string F(this string value, params object[] args)
        {
            return (!string.IsNullOrEmpty(value)) ? string.Format(value, args) : value;
        }

        /// <summary>
        /// Determines whether the specified string is null.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// 	<c>true</c> if the specified value is null; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNull(this string value)
        {
            return value == null;
        }

        /// <summary>
        /// Determines whether the specified string is null or empty.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// 	<c>true</c> if [is null or empty] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmpty(this string value)
        {
            return (value == null || value.Length == 0);
        }

        /// <summary>
        /// Determines whether the specified <b>string</b> is not null or empty.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// 	<c>true</c> if [is not null or empty] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNotNullOrEmpty(this string value)
        {
            return !IsNullOrEmpty(value);
        }
        #endregion
    }
}
