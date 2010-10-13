<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Log in required
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Please log in</h2>
    
    <%= Html.ValidationSummary() %>
    <% using(Html.BeginForm()) { %>        
        <%= Html.AntiForgeryToken() %>
        
        <p>Username: <%= Html.TextBox("username") %></p>
        <p>Password: <%= Html.Password("password") %></p>
        
        <input type="submit" value="Log in" />
    <% } %>
</asp:Content>
