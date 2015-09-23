<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="false"
    CodeBehind="Default.aspx.cs" Inherits="Alienlab.Web.Security.Sample.Members.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>
        Welcome to Members' Area.
    </h1>
    <p>
        This area can be accessed only by authorized users(members).
    </p>
    <div>
        From here you can test you access rights and try to go to:
       <ul>
        <li><a href="/Members/Clients">Clients Area</a></li>
        <li><a href="/Members/Administrators">Administrators Area</a></li>
       </ul>
    </div>
</asp:Content>
