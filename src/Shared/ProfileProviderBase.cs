using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Text;
using System.Web.Profile;
using System.Globalization;
using System.Diagnostics;

namespace Velyo.Web.Security
{
    public abstract class ProfileProviderBase : ProfileProvider
    {
        /// <summary>
        /// Gets or sets the name of the currently running application.
        /// </summary>
        /// <value></value>
        /// <returns>A <see cref="T:System.String"/> that contains the application's shortened name, which does not contain a full path or extension, for example, SimpleAppSettings.</returns>
        public override string ApplicationName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [case sensitive].
        /// </summary>
        /// <value><c>true</c> if [case sensitive]; otherwise, <c>false</c>.</value>
        public virtual bool CaseSensitive { get; protected set; }

        /// <summary>
        /// Gets the comparer.
        /// </summary>
        /// <value>The comparer.</value>
        protected virtual StringComparer Comparer { get; set; }

        /// <summary>
        /// Gets the comparison.
        /// </summary>
        /// <value>The comparison.</value>
        protected virtual StringComparison Comparison { get; set; }

        /// <summary>
        /// Gets the sync root.
        /// </summary>
        /// <value>The sync root.</value>
        public static object SyncRoot { get; } = new object();

        /// <summary>
        /// Gets a value indicating whether [use universal time].
        /// </summary>
        /// <value><c>true</c> if [use universal time]; otherwise, <c>false</c>.</value>
        protected virtual bool UseUniversalTime { get; set; }


        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
        /// <exception cref="T:System.ArgumentNullException">The name of the provider is null.</exception>
        /// <exception cref="T:System.ArgumentException">The name of the provider has a length of zero.</exception>
        /// <exception cref="T:System.InvalidOperationException">An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"/> on a provider after the provider has already been initialized.</exception>
        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);

            string defaultAppName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
            ApplicationName = config.GetString("applicationName", defaultAppName);

            CaseSensitive = config.GetBool("caseSensitive", false);
            Comparer = CaseSensitive ? StringComparer.CurrentCulture : StringComparer.CurrentCultureIgnoreCase;
            Comparison = CaseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
            UseUniversalTime = config.GetBool("useUniversalTime", false);
        }

        /// <summary>
        /// Gets the property values.
        /// </summary>
        /// <param name="propertyNames">The property names.</param>
        /// <param name="stringValues">The string values.</param>
        /// <param name="binaryValues">The binary values.</param>
        /// <param name="svc">The SVC.</param>
        protected virtual void GetPropertyValues(
            string propertyNames,
            string stringValues,
            string binaryValues,
            SettingsPropertyValueCollection svc)
        {
            // property names are not provided, then nothing to parse for
            if (propertyNames.IsNullOrEmpty()) return;

            // any values are not provided, then nothing to parse for
            if (stringValues.IsNullOrEmpty() && binaryValues.IsNullOrEmpty()) return;

            /// decode
            Encoding encoding = Encoding.UTF8;
            string[] names = encoding.GetString(Convert.FromBase64String(propertyNames)).Split(':');
            string values = null;
            byte[] binaries = null;

            if (!stringValues.IsNullOrEmpty())
            {
                values = encoding.GetString(Convert.FromBase64String(stringValues));
            }
            if (!binaryValues.IsNullOrEmpty())
            {
                binaries = Convert.FromBase64String(binaryValues);
            }

            ParseProfileData(names, values, binaries, svc);
        }

        /// <summary>
        /// Parses the profile data.
        /// </summary>
        /// <param name="names">The names.</param>
        /// <param name="values">The values.</param>
        /// <param name="binaries">The buffer.</param>
        /// <param name="properties">The properties.</param>
        protected internal virtual void ParseProfileData(
            string[] names, string values, byte[] binaries, SettingsPropertyValueCollection properties)
        {
            try
            {
                for (int num1 = 0; num1 < (names.Length / 4); num1++)
                {
                    string text1 = names[num1 * 4];
                    SettingsPropertyValue value1 = properties[text1];
                    if (value1 != null)
                    {
                        int num2 = int.Parse(names[(num1 * 4) + 2], CultureInfo.InvariantCulture);
                        int num3 = int.Parse(names[(num1 * 4) + 3], CultureInfo.InvariantCulture);
                        if ((num3 == -1) && !value1.Property.PropertyType.IsValueType)
                        {
                            value1.PropertyValue = null;
                            value1.IsDirty = false;
                            value1.Deserialized = true;
                        }
                        if (((names[(num1 * 4) + 1] == "S") && (num2 >= 0)) && ((num3 > 0) && (values.Length >= (num2 + num3))))
                        {
                            value1.SerializedValue = values.Substring(num2, num3);
                        }
                        if (((names[(num1 * 4) + 1] == "B") && (num2 >= 0)) && ((num3 > 0) && (binaries.Length >= (num2 + num3))))
                        {
                            byte[] buffer1 = new byte[num3];
                            Buffer.BlockCopy(binaries, num2, buffer1, 0, num3);
                            value1.SerializedValue = buffer1;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// Prepares the data for saving.
        /// </summary>
        /// <param name="names">All names.</param>
        /// <param name="values">All values.</param>
        /// <param name="binaries">The buf.</param>
        /// <param name="binarySupported">if set to <c>true</c> [binary supported].</param>
        /// <param name="properties">The properties.</param>
        /// <param name="userIsAuthenticated">if set to <c>true</c> [user is authenticated].</param>
        protected virtual void PrepareDataForSaving(
            ref string names,
            ref string values,
            ref byte[] binaries,
            bool binarySupported,
            SettingsPropertyValueCollection properties,
            bool userIsAuthenticated)
        {
            StringBuilder builder1 = new StringBuilder();
            StringBuilder builder2 = new StringBuilder();
            MemoryStream stream = binarySupported ? new MemoryStream() : null;

            try
            {
                bool flag = false;

                foreach (SettingsPropertyValue value1 in properties)
                {
                    if (!value1.IsDirty)
                    {
                        continue;
                    }
                    if (userIsAuthenticated || ((bool)value1.Property.Attributes["AllowAnonymous"]))
                    {
                        flag = true;
                        break;
                    }
                }

                if (!flag) return;

                foreach (SettingsPropertyValue value2 in properties)
                {
                    if (!userIsAuthenticated && !((bool)value2.Property.Attributes["AllowAnonymous"]))
                    {
                        continue;
                    }
                    if (value2.IsDirty || !value2.UsingDefaultValue)
                    {
                        int num1 = 0;
                        int num2 = 0;
                        string text1 = null;
                        if (value2.Deserialized && (value2.PropertyValue == null))
                        {
                            num1 = -1;
                        }
                        else
                        {
                            object obj1 = value2.SerializedValue;
                            if (obj1 == null)
                            {
                                num1 = -1;
                            }
                            else
                            {
                                if (!(obj1 is string) && !binarySupported)
                                {
                                    obj1 = Convert.ToBase64String((byte[])obj1);
                                }
                                if (obj1 is string)
                                {
                                    text1 = (string)obj1;
                                    num1 = text1.Length;
                                    num2 = builder2.Length;
                                }
                                else
                                {
                                    byte[] buffer1 = (byte[])obj1;

                                    num2 = (int)stream.Position;
                                    stream.Write(buffer1, 0, buffer1.Length);
                                    stream.Position = num2 + buffer1.Length;
                                    num1 = buffer1.Length;
                                }
                            }
                        }
                        string[] textArray1 = new string[8] { value2.Name, ":", (text1 != null) ? "S" : "B", ":", num2.ToString(CultureInfo.InvariantCulture), ":", num1.ToString(CultureInfo.InvariantCulture), ":" };
                        builder1.Append(string.Concat(textArray1));
                        if (text1 != null)
                        {
                            builder2.Append(text1);
                        }
                    }
                }
                if (binarySupported)
                {
                    binaries = stream.ToArray();
                }
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream = null;
                }
            }

            names = builder1.ToString();
            values = builder2.ToString();
        }
    }
}
