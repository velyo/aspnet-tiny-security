using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using System.Collections.Specialized;

namespace Velyo.Web.Security {

    /// <summary>
    /// 
    /// </summary>
    public abstract class RoleProviderBase : RoleProvider {

        #region Fields  ///////////////////////////////////////////////////////////////////////////

        static readonly object _syncRoot = new object();

        #endregion

        #region Properties  ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the name of the application to store and retrieve role information for.
        /// </summary>
        /// <value></value>
        /// <returns>The name of the application to store and retrieve role information for.</returns>
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
        public static object SyncRoot { get { return _syncRoot; } }

        /// <summary>
        /// Gets a value indicating whether [use universal time].
        /// </summary>
        /// <value><c>true</c> if [use universal time]; otherwise, <c>false</c>.</value>
        protected virtual bool UseUniversalTime { get; set; }

        #endregion

        #region Methods ///////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
        /// <exception cref="T:System.ArgumentNullException">The name of the provider is null.</exception>
        /// <exception cref="T:System.ArgumentException">The name of the provider has a length of zero.</exception>
        /// <exception cref="T:System.InvalidOperationException">An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"/> on a provider after the provider has already been initialized.</exception>
        public override void Initialize(string name, NameValueCollection config) {
            base.Initialize(name, config);

            string defaultAppName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
            this.ApplicationName = config.GetString("applicationName", defaultAppName);

            this.CaseSensitive = config.GetBool("caseSensitive", false);
            this.Comparer = this.CaseSensitive
                ? StringComparer.CurrentCulture : StringComparer.CurrentCultureIgnoreCase;
            this.Comparison = this.CaseSensitive
                    ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
            this.UseUniversalTime = config.GetBool("useUniversalTime", false);
        }
        #endregion
    }
}