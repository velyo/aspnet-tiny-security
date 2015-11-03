using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Profile;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Velyo.Web.Security.Sample {

    /// <summary>
    /// 
    /// </summary>
    public partial class _Default : System.Web.UI.Page {

        #region Methods ///////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"/> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            var membershipProvider = Membership.Provider;
            var roleProvider = Roles.Provider;
            var profileProvider = ProfileManager.Provider;

            if (membershipProvider != null) {
                ltrMembershipProviderName.Text = membershipProvider.Name;
                ltrMembershipProviderType.Text = membershipProvider.GetType().FullName;
            }
            if (roleProvider != null) {
                ltrRoleProviderName.Text = roleProvider.Name;
                ltrRoleProviderType.Text = roleProvider.GetType().FullName;
            }
            if (profileProvider != null) {
                ltrProfileProviderName.Text = profileProvider.Name;
                ltrProfileProviderType.Text = profileProvider.GetType().FullName;
            }
        }
        #endregion
    }
}
