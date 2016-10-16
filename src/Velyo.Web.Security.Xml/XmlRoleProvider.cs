using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Linq;
using System.Web.Hosting;
using Velyo.Web.Security.Models;
using Velyo.Web.Security.Resources;
using Velyo.Web.Security.Store;

namespace Velyo.Web.Security
{
    /// <summary>
    /// Custom XML implementation of <c>System.Web.Security.RoleProvider</c>
    /// </summary>
    public class XmlRoleProvider : RoleProviderBase, IDisposable
    {
        private string _file;
        private XmlRoleStore _store;


        ~XmlRoleProvider()
        {
            Dispose(false);
        }


        /// <summary>
        /// Gets the roles.
        /// </summary>
        /// <value>The roles.</value>
        protected List<Role> Roles { get { return Store.Roles; } }

        /// <summary>
        /// Gets the role store.
        /// </summary>
        /// <value>The role store.</value>
        protected XmlRoleStore Store
        {
            get
            {
                return _store ?? (_store = new XmlRoleStore(_file));
            }
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
        /// Adds the specified user names to the specified roles for the configured applicationName.
        /// </summary>
        /// <param name="usernames">A string array of user names to be added to the specified roles.</param>
        /// <param name="roleNames">A string array of the role names to add the specified user names to.</param>
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            if (usernames == null) throw new ArgumentNullException(nameof(usernames));
            if (roleNames == null) throw new ArgumentNullException(nameof(roleNames));

            var comparer = Comparer;
            lock (SyncRoot)
            {
                foreach (string rolename in roleNames)
                {
                    Role role = GetRole(rolename);
                    if (role != null)
                    {
                        foreach (string username in usernames)
                        {
                            if (!role.Users.Contains(username, comparer))
                                role.Users.Add(username);
                        }
                    }
                }
                Store.Save();
            }
        }

        /// <summary>
        /// Adds a new role to the data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to create.</param>
        public override void CreateRole(string roleName)
        {
            if (roleName == null) throw new ArgumentNullException(nameof(roleName));
            if (roleName.IndexOf(',') > 0) throw new ArgumentException(Messages.RoleCannotContainCommas);

            Role role = GetRole(roleName);
            if (role == null)
            {
                role = new Role
                {
                    Name = roleName,
                    Users = new List<string>()
                };
                lock (SyncRoot)
                {
                    Store.Roles.Add(role);
                    Store.Save();
                }
            }
            else
            {
                throw new ProviderException(string.Format(Messages.RoleExists, roleName));
            }
        }

        /// <summary>
        /// Removes a role from the data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to delete.</param>
        /// <param name="throwOnPopulatedRole">If true, throw an exception if roleName has one or more members and do not delete roleName.</param>
        /// <returns>
        /// true if the role was successfully deleted; otherwise, false.
        /// </returns>
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            if (roleName == null) throw new ArgumentNullException(nameof(roleName));

            lock (SyncRoot)
            {
                Role role = GetRole(roleName);
                if (role != null)
                {
                    if (throwOnPopulatedRole && (role.Users.Count > 0))
                        throw new ProviderException(Messages.CannotDeletePopulatedRole);
                    Store.Roles.Remove(role);
                    Store.Save();

                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Gets an array of user names in a role where the user name contains the specified user name to match.
        /// </summary>
        /// <param name="roleName">The role to search in.</param>
        /// <param name="usernameToMatch">The user name to search for.</param>
        /// <returns>
        /// A string array containing the names of all the users where the user name matches usernameToMatch and the user is a member of the specified role.
        /// </returns>
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            if (roleName == null) throw new ArgumentNullException(nameof(roleName));
            if (usernameToMatch == null) throw new ArgumentNullException(nameof(usernameToMatch));

            var comparison = Comparison;
            var query = from role in Roles.AsQueryable()
                        from user in role.Users
                        where (user.IndexOf(usernameToMatch, comparison) >= 0)
                                && role.Name.Equals(roleName, comparison)
                        select user;
            lock (SyncRoot)
            {
                return query.ToArray();
            }
        }

        /// <summary>
        /// Gets a list of all the roles for the configured applicationName.
        /// </summary>
        /// <returns>
        /// A string array containing the names of all the roles stored in the data source for the configured applicationName.
        /// </returns>
        public override string[] GetAllRoles()
        {
            var query = from r in Roles
                        select r.Name;
            lock (SyncRoot)
            {
                return query.ToArray();
            }
        }

        /// <summary>
        /// Gets the role.
        /// </summary>
        /// <param name="roleName">The name.</param>
        /// <returns></returns>
        public Role GetRole(string roleName)
        {
            if (roleName == null) throw new ArgumentNullException(nameof(roleName));

            var query = from r in Roles
                        where r.Name.Equals(roleName, Comparison)
                        select r;
            lock (SyncRoot)
            {
                return query.FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets a list of the roles that a specified user is in for the configured applicationName.
        /// </summary>
        /// <param name="username">The user to return a list of roles for.</param>
        /// <returns>
        /// A string array containing the names of all the roles that the specified user is in for the configured applicationName.
        /// </returns>
        public override string[] GetRolesForUser(string username)
        {
            if (username == null) throw new ArgumentNullException(nameof(username));
            var query = from r in Roles
                        where r.Users.Contains(username, Comparer)
                        select r.Name;
            lock (SyncRoot)
            {
                return query.ToArray();
            }
        }

        /// <summary>
        /// Gets a list of users in the specified role for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to get the list of users for.</param>
        /// <returns>
        /// A string array containing the names of all the users who are members of the specified role for the configured applicationName.
        /// </returns>
        public override string[] GetUsersInRole(string roleName)
        {
            Role role = GetRole(roleName);
            if (role != null)
            {
                lock (SyncRoot)
                {
                    return role.Users.ToArray();
                }
            }
            else
            {
                throw new ProviderException(string.Format(Messages.RoleNotExists, roleName));
            }
        }

        /// <summary>
        /// Gets a value indicating whether the specified user is in the specified role for the configured applicationName.
        /// </summary>
        /// <param name="username">The user name to search for.</param>
        /// <param name="roleName">The role to search in.</param>
        /// <returns>
        /// true if the specified user is in the specified role for the configured applicationName; otherwise, false.
        /// </returns>
        public override bool IsUserInRole(string username, string roleName)
        {
            if (username == null) throw new ArgumentNullException(nameof(username));

            Role role = GetRole(roleName);
            if (role != null)
            {
                lock (SyncRoot)
                {
                    return role.Users.Contains(username, Comparer);
                }
            }
            else
            {
                throw new ProviderException(string.Format(Messages.RoleNotExists, roleName));
            }
        }

        /// <summary>
        /// Removes the specified user names from the specified roles for the configured applicationName.
        /// </summary>
        /// <param name="usernames">A string array of user names to be removed from the specified roles.</param>
        /// <param name="roleNames">A string array of role names to remove the specified user names from.</param>
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            if (usernames == null) throw new ArgumentNullException(nameof(usernames));
            if (roleNames == null) throw new ArgumentNullException(nameof(roleNames));

            var comparer = Comparer;
            var query = from r in Roles
                        where roleNames.Contains(r.Name, comparer)
                        select r;
            lock (SyncRoot)
            {
                foreach (Role role in query)
                {
                    foreach (string username in usernames)
                    {
                        role.Users.Remove(username);
                    }
                }
                Store.Save();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the specified role name already exists in the role data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to search for in the data source.</param>
        /// <returns>
        /// true if the role name already exists in the data source for the configured applicationName; otherwise, false.
        /// </returns>
        public override bool RoleExists(string roleName)
        {
            return GetRole(roleName) != null;
        }

        #region - Initialize -

        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
        /// <exception cref="T:System.ArgumentNullException">The name of the provider is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"></see> on a provider after the provider has already been initialized.</exception>
        /// <exception cref="T:System.ArgumentException">The name of the provider has a length of zero.</exception>
        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            // prerequisite
            if (name.IsNullOrWhiteSpace())
            {
                name = "XmlRoleProvider";
            }
            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "XML Role Provider");
            }

            // initialize the base class
            base.Initialize(name, config);

            // initialize provider fields
            string fileName = config.GetString("fileName", "Roles.xml");
            string folder = config.GetString("folder", "~/App_Data/");

            if (!folder.EndsWith("/")) folder += "/";
            _file = HostingEnvironment.MapPath(string.Format("{0}{1}", folder, fileName));
        }
        #endregion
    }
}