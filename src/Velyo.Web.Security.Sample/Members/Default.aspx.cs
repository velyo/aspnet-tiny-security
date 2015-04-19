using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SampleWebSite.Members {

    /// <summary>
    /// 
    /// </summary>
    public partial class Default : System.Web.UI.Page {

        #region Methods ///////////////////////////////////////////////////////////////////////////

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            //var profile = HttpContext.Current.Profile;
            //profile.SetPropertyValue("FirstName", "Test");
            //string firstName = profile["FirstName"] as string;
            //if (firstName == null)
            //    profile["FirstName"] = "Test";
            //this.Context.Profile.F
        } 
        #endregion
    }
}