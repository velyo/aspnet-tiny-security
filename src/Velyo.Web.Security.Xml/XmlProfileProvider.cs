using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using System.Web.Profile;
using Velyo.Web.Security.Store;

namespace Velyo.Web.Security
{
    /// <summary>
    /// Summary description for XmlProfileProvider
    /// TODO: implement some of the methods with respect of ProfileAuthenticationOption and result paging
    /// </summary>
    public class XmlProfileProvider : ProfileProviderBase, IDisposable
    {
        private string _file;
        private XmlProfileStore _store;


        ~XmlProfileProvider()
        {
            Dispose(false);
        }


        /// <summary>
        /// Gets the profiles.
        /// </summary>
        /// <value>The profiles.</value>
        private List<XmlProfile> Profiles { get { return Store.Profiles; } }

        /// <summary>
        /// Gets the profiles store.
        /// </summary>
        /// <value>The store.</value>
        protected XmlProfileStore Store
        {
            get { return _store ?? (_store = new XmlProfileStore(_file)); }
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                if (_store != null)
                {
                    _store.Dispose();
                    _store = null;
                }
            }
        }

        /// <summary>
        /// When overridden in a derived class, deletes all user-profile data for profiles in which the last activity date occurred before the specified date.
        /// </summary>
        /// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"/> values, specifying whether anonymous, authenticated, or both types of profiles are deleted.</param>
        /// <param name="userInactiveSinceDate">A <see cref="T:System.DateTime"/> that identifies which user profiles are considered inactive. If the <see cref="P:System.Web.Profile.ProfileInfo.LastActivityDate"/>  value of a user profile occurs on or before this date and time, the profile is considered inactive.</param>
        /// <returns>
        /// The number of profiles deleted from the data source.
        /// </returns>
        public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            int count;
            var query = from p in Profiles
                        where (p.LastUpdated <= userInactiveSinceDate)
                        select p;

            if (authenticationOption != ProfileAuthenticationOption.All)
            {
                bool authenticated = (authenticationOption == ProfileAuthenticationOption.Authenticated);
                query = query.Where(p => p.Authenticated == authenticated);
            }

            lock (SyncRoot)
            {
                var profilesToDelete = query.ToArray();
                count = profilesToDelete.Count(p => Profiles.Remove(p));
                Store.Save();
            }

            return count;
        }

        /// <summary>
        /// When overridden in a derived class, deletes profile properties and information for profiles that match the supplied list of user names.
        /// </summary>
        /// <param name="usernames">A string array of user names for profiles to be deleted.</param>
        /// <returns>
        /// The number of profiles deleted from the data source.
        /// </returns>
        public override int DeleteProfiles(string[] usernames)
        {
            if (usernames == null) throw new ArgumentNullException(nameof(usernames));

            int count;
            var query = from p in Profiles
                        where usernames.Contains(p.UserName, Comparer)
                        select p;

            lock (SyncRoot)
            {
                var profilesToDelete = query.ToArray();
                count = profilesToDelete.Count(p => Profiles.Remove(p));
                Store.Save();
            }

            return count;
        }

        /// <summary>
        /// When overridden in a derived class, deletes profile properties and information for the supplied list of profiles.
        /// </summary>
        /// <param name="profiles">A <see cref="T:System.Web.Profile.ProfileInfoCollection"></see>  of information about profiles that are to be deleted.</param>
        /// <returns>
        /// The number of profiles deleted from the data source.
        /// </returns>
        public override int DeleteProfiles(ProfileInfoCollection profiles)
        {
            if (profiles == null) throw new ArgumentNullException(nameof(profiles));

            IEnumerable<string> profilesToDelete = profiles.Cast<ProfileInfo>().Select(p => p.UserName);
            return DeleteProfiles(profilesToDelete.ToArray());
        }

        /// <summary>
        /// When overridden in a derived class, retrieves profile information for profiles in which the last activity date occurred on or before the specified date and the user name matches the specified user name.
        /// </summary>
        /// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"></see> values, specifying whether anonymous, authenticated, or both types of profiles are returned.</param>
        /// <param name="usernameToMatch">The user name to search for.</param>
        /// <param name="userInactiveSinceDate">A <see cref="T:System.DateTime"></see> that identifies which user profiles are considered inactive. If the <see cref="P:System.Web.Profile.ProfileInfo.LastActivityDate"></see> value of a user profile occurs on or before this date and time, the profile is considered inactive.</param>
        /// <param name="pageIndex">The index of the page of results to return.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">When this method returns, contains the total number of profiles.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Profile.ProfileInfoCollection"></see> containing user profile information for inactive profiles where the user name matches the supplied usernameToMatch parameter.
        /// </returns>
        public override ProfileInfoCollection FindInactiveProfilesByUserName(
            ProfileAuthenticationOption authenticationOption,
            string usernameToMatch,
            DateTime userInactiveSinceDate,
            int pageIndex,
            int pageSize,
            out int totalRecords)
        {
            XmlProfile[] profiles;
            int pageOffset = pageIndex * pageSize;

            var query = from p in Profiles
                        where (p.LastUpdated <= userInactiveSinceDate) &&
                            (p.UserName.IndexOf(usernameToMatch, Comparison) >= 0)
                        select p;

            if (authenticationOption != ProfileAuthenticationOption.All)
            {
                bool authenticated = (authenticationOption == ProfileAuthenticationOption.Authenticated);
                query = query.Where(p => p.Authenticated == authenticated);
            }

            lock (SyncRoot)
            {
                var results = query.ToArray();
                totalRecords = results.Length;
                profiles = results.Skip(pageOffset).Take(pageSize).ToArray();
            }

            return CreateProfileInfoCollection(profiles);
        }

        /// <summary>
        /// When overridden in a derived class, retrieves profile information for profiles in which the user name matches the specified user names.
        /// </summary>
        /// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"/> values, specifying whether anonymous, authenticated, or both types of profiles are returned.</param>
        /// <param name="usernameToMatch">The user name to search for.</param>
        /// <param name="pageIndex">The index of the page of results to return.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">When this method returns, contains the total number of profiles.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Profile.ProfileInfoCollection"/> containing user-profile information for profiles where the user name matches the supplied <paramref name="usernameToMatch"/> parameter.
        /// </returns>
        public override ProfileInfoCollection FindProfilesByUserName(
            ProfileAuthenticationOption authenticationOption,
            string usernameToMatch,
            int pageIndex,
            int pageSize,
            out int totalRecords)
        {
            XmlProfile[] profiles;
            int pageOffset = pageIndex * pageSize;

            var query = from p in Profiles
                        where (p.UserName.IndexOf(usernameToMatch, Comparison) >= 0)
                        select p;

            if (authenticationOption != ProfileAuthenticationOption.All)
            {
                bool authenticated = (authenticationOption == ProfileAuthenticationOption.Authenticated);
                query = query.Where(p => p.Authenticated == authenticated);
            }

            lock (SyncRoot)
            {
                var queryResults = query.ToArray();
                totalRecords = queryResults.Length;
                profiles = queryResults.Skip(pageOffset).Take(pageSize).ToArray();
            }

            return CreateProfileInfoCollection(profiles);
        }

        /// <summary>
        /// When overridden in a derived class, retrieves user-profile data from the data source for profiles in which the last activity date occurred on or before the specified date.
        /// </summary>
        /// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"/> values, specifying whether anonymous, authenticated, or both types of profiles are returned.</param>
        /// <param name="userInactiveSinceDate">A <see cref="T:System.DateTime"/> that identifies which user profiles are considered inactive. If the <see cref="P:System.Web.Profile.ProfileInfo.LastActivityDate"/>  of a user profile occurs on or before this date and time, the profile is considered inactive.</param>
        /// <param name="pageIndex">The index of the page of results to return.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">When this method returns, contains the total number of profiles.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Profile.ProfileInfoCollection"/> containing user-profile information about the inactive profiles.
        /// </returns>
        public override ProfileInfoCollection GetAllInactiveProfiles(
            ProfileAuthenticationOption authenticationOption,
            DateTime userInactiveSinceDate,
            int pageIndex,
            int pageSize,
            out int totalRecords)
        {
            XmlProfile[] profiles;
            int pageOffset = pageIndex * pageSize;

            var query = from p in Profiles
                        where (p.LastUpdated <= userInactiveSinceDate)
                        select p;

            if (authenticationOption != ProfileAuthenticationOption.All)
            {
                bool authenticated = (authenticationOption == ProfileAuthenticationOption.Authenticated);
                query = query.Where(p => p.Authenticated == authenticated);
            }

            lock (SyncRoot)
            {
                var queryResults = query.ToArray();
                totalRecords = queryResults.Length;
                profiles = queryResults.Skip(pageOffset).Take(pageSize).ToArray();
            }

            return CreateProfileInfoCollection(profiles);
        }

        /// <summary>
        /// When overridden in a derived class, retrieves user profile data for all profiles in the data source.
        /// </summary>
        /// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"/> values, specifying whether anonymous, authenticated, or both types of profiles are returned.</param>
        /// <param name="pageIndex">The index of the page of results to return.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">When this method returns, contains the total number of profiles.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Profile.ProfileInfoCollection"/> containing user-profile information for all profiles in the data source.
        /// </returns>
        public override ProfileInfoCollection GetAllProfiles(
            ProfileAuthenticationOption authenticationOption,
            int pageIndex,
            int pageSize,
            out int totalRecords)
        {
            XmlProfile[] profiles;
            int pageOffset = pageIndex * pageSize;

            IEnumerable<XmlProfile> query = Profiles;

            if (authenticationOption != ProfileAuthenticationOption.All)
            {
                bool authenticated = (authenticationOption == ProfileAuthenticationOption.Authenticated);
                query = query.Where(p => p.Authenticated == authenticated);
            }

            lock (SyncRoot)
            {
                var queryResilts = query.ToArray();
                totalRecords = queryResilts.Length;
                profiles = queryResilts.Skip(pageOffset).Take(pageSize).ToArray();
            }

            return CreateProfileInfoCollection(profiles);
        }

        /// <summary>
        /// When overridden in a derived class, returns the number of profiles in which the last activity date occurred on or before the specified date.
        /// </summary>
        /// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"/> values, specifying whether anonymous, authenticated, or both types of profiles are returned.</param>
        /// <param name="userInactiveSinceDate">A <see cref="T:System.DateTime"/> that identifies which user profiles are considered inactive. If the <see cref="P:System.Web.Profile.ProfileInfo.LastActivityDate"/>  of a user profile occurs on or before this date and time, the profile is considered inactive.</param>
        /// <returns>
        /// The number of profiles in which the last activity date occurred on or before the specified date.
        /// </returns>
        public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            var query = from p in Profiles
                        where (p.LastUpdated <= userInactiveSinceDate)
                        select p;

            if (authenticationOption != ProfileAuthenticationOption.All)
            {
                bool authenticated = (authenticationOption == ProfileAuthenticationOption.Authenticated);
                query = query.Where(p => p.Authenticated == authenticated);
            }

            lock (SyncRoot)
            {
                return query.Count();
            }
        }

        /// <summary>
        /// Returns the collection of settings property values for the specified application instance and settings property group.
        /// </summary>
        /// <param name="context">A <see cref="T:System.Configuration.SettingsContext"/> describing the current application use.</param>
        /// <param name="collection">A <see cref="T:System.Configuration.SettingsPropertyCollection"/> containing the settings property group whose values are to be retrieved.</param>
        /// <returns>
        /// A <see cref="T:System.Configuration.SettingsPropertyValueCollection"/> containing the values for the specified settings property group.
        /// </returns>
        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
        {
            SettingsPropertyValueCollection coll = new SettingsPropertyValueCollection();

            if (collection.Count > 0)
            {
                string username = context["UserName"] as string;
                if (!string.IsNullOrEmpty(username))
                {
                    XmlProfile profile = GetProfile(username);

                    foreach (SettingsProperty prop in collection)
                    {
                        if (prop.SerializeAs == SettingsSerializeAs.ProviderSpecific)
                        {
                            if (prop.PropertyType.IsPrimitive || (prop.PropertyType == typeof(string)))
                            {
                                prop.SerializeAs = SettingsSerializeAs.String;
                            }
                            else
                            {
                                prop.SerializeAs = SettingsSerializeAs.Xml;
                            }
                        }
                        coll.Add(new SettingsPropertyValue(prop));
                    }

                    if (profile != null)
                    {
                        GetPropertyValues(profile.Names, profile.ValuesString, profile.ValuesBinary, coll);
                    }
                }
            }

            return coll;
        }

        /// <summary>
        /// Sets the values of the specified group of property settings.
        /// </summary>
        /// <param name="context">A <see cref="T:System.Configuration.SettingsContext"/> describing the current application usage.</param>
        /// <param name="collection">A <see cref="T:System.Configuration.SettingsPropertyValueCollection"/> representing the group of property settings to set.</param>
        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            if (collection.Count > 0)
            {
                string username = context["UserName"] as string;

                // TODO ensure username for anonymous users is not null
                if (!string.IsNullOrEmpty(username))
                {
                    string names = string.Empty;
                    string valuesString = string.Empty;
                    byte[] valuesBinary = null;
                    bool isAuthenticated = Convert.ToBoolean(context["IsAuthenticated"]);

                    // prepare data for saving
                    PrepareDataForSaving(ref names, ref valuesString, ref valuesBinary, true, collection, isAuthenticated);

                    // save data
                    if (!string.IsNullOrWhiteSpace(valuesString) || (valuesBinary != null))
                    {
                        Encoding encoding = Encoding.UTF8;

                        lock (SyncRoot)
                        {
                            XmlProfile profile = GetProfile(username);

                            if (profile == null)
                            {
                                profile = new XmlProfile { UserName = username };
                                Profiles.Add(profile);
                            }

                            profile.Names = Convert.ToBase64String(encoding.GetBytes(names));
                            profile.ValuesBinary = (valuesBinary != null) ? Convert.ToBase64String(valuesBinary) : "";
                            profile.ValuesString = Convert.ToBase64String(encoding.GetBytes(valuesString));
                            profile.LastUpdated = DateTime.Now;
                            profile.Authenticated = isAuthenticated;

                            Store.Save();
                        }
                    }
                }
            }
        }

#region - Helpers -

        /// <summary>
        /// Creates the profile info collection.
        /// </summary>
        /// <param name="profiles">The profiles.</param>
        /// <returns></returns>
        protected internal ProfileInfoCollection CreateProfileInfoCollection(IEnumerable<XmlProfile> profiles)
        {
            ProfileInfoCollection collection = new ProfileInfoCollection();

            foreach (var p in profiles)
            {
                int size = p.Names.Length + p.ValuesString.Length + p.ValuesBinary.Length;
                collection.Add(new ProfileInfo(
                    p.UserName, !p.Authenticated, p.LastUpdated, p.LastUpdated, size));
            }

            return collection;
        }

        /// <summary>
        /// Gets the profile.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns></returns>
        protected XmlProfile GetProfile(string username)
        {
            var query = from p in Profiles
                        where p.UserName.Equals(username, Comparison)
                        select p;

            lock (SyncRoot)
            {
                return query.SingleOrDefault();
            }
        }

#endregion

#region - Initialize -

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
            if (config == null) throw new ArgumentNullException(nameof(config));

            // prerequisites
            if (string.IsNullOrEmpty(name))
            {
                name = "XmlProfileProvider";
            }
            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "XML Profile Provider");
            }

            // initialize the base class
            base.Initialize(name, config);

            // initialize provider fields
            string fileName = config.GetString("fileName", "Profiles.xml");
            string folder = config.GetString("folder", "~/App_Data/");

            if (!folder.EndsWith("/")) folder += "/";
            _file = HostingEnvironment.MapPath(string.Format("{0}{1}", folder, fileName));
        }
#endregion
    }
}