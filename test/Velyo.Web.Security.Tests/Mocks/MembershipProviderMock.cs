using System;
using System.Collections.Generic;
using System.Web.Security;
using Velyo.Web.Security.Models;

namespace Velyo.Web.Security.Tests.Mocks
{
    class MembershipProviderMock : MembershipProviderBase
    {
        private IList<User> _users = new List<User>();


        public new string DecodePassword(string encodedPassword)
        {
            return base.DecodePassword(encodedPassword);
        }

        public new string EncodePassword(string password, ref string salt)
        {
            return base.EncodePassword(password, ref salt);
        }

        public new bool VerifyPasswordIsValid(string password)
        {
            return base.VerifyPasswordIsValid(password);
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        public override string GetUserNameByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }

        protected override bool TryGetPassword(string username, out string password, out string salt, out string question, out string answer)
        {
            throw new NotImplementedException();
        }

        protected override bool TrySetPassword(string username, string password, string salt, string question, string answer)
        {
            throw new NotImplementedException();
        }

        protected override void UpdateUserInfo(string username, bool valid)
        {
            throw new NotImplementedException();
        }
    }
}
