<%@ Page Title=" ASP.NET Tiny Membership - Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="false" CodeBehind="Default.aspx.cs" Inherits="Velyo.Web.Security.Sample._Default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h2>Welcome to ASP.NET Tiny Membership Samples Website
    </h2>
    <p>
        To learn more about ASP.NET Tiny Membership visit 
        <a href="https://github.com/velyo/aspnet-tiny-security" title="Source on GitHub">https://github.com/velyo/aspnet-tiny-security</a>.
    </p>
    <div>
        <asp:LoginView ID="LoginView" runat="server" EnableViewState="false">
            <AnonymousTemplate>
                Please, <a href="~/Account/Login.aspx" id="LoginLink" runat="server">Log In</a> to start using XML Membership providers. 
                <br />
                <a href="~/Account/Register.aspx" id="RegisterLink" runat="server">Register</a> if you don't have an account.
            </AnonymousTemplate>
            <LoggedInTemplate>
                You can go now to <a href="/Members">Members Area</a> to check out you user/roles
                access rights.
            </LoggedInTemplate>
        </asp:LoginView>
    </div>
    <p>
        Current membership providers are:
    </p>
    <ul>
        <li><b>MembershipProvider</b> - <i>
            <asp:Literal ID="ltrMembershipProviderName" runat="server"></asp:Literal></i> of
            type <i>
                <asp:Literal ID="ltrMembershipProviderType" runat="server"></asp:Literal></i>
        </li>
        <li><b>RoleProvider</b> - <i>
            <asp:Literal ID="ltrRoleProviderName" runat="server"></asp:Literal></i> of type
            <i>
                <asp:Literal ID="ltrRoleProviderType" runat="server"></asp:Literal></i>
        </li>
        <li><b>ProfileProvider</b> - <i>
            <asp:Literal ID="ltrProfileProviderName" runat="server"></asp:Literal></i> of type
            <i>
                <asp:Literal ID="ltrProfileProviderType" runat="server"></asp:Literal></i>
        </li>
    </ul>
</asp:Content>
