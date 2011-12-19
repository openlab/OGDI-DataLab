<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ConfigTool._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <p>
            This is a down and dirty / simple config tool. It is NOT designed to be published
            anywhere. It is just a tool to quickly add/delete the configuration data stored
            in http://ogdiconfig.table.core.windows.net</p>
        <h1>
            Enabled storage accounts:</h1>
        <span>Alias</span><asp:TextBox ID="AliasBox" runat="server" Text="" /><br />
        <span>Description</span><asp:TextBox ID="DescriptionBox" runat="server" Text="" /><br />
        <span>TableStorageAccountName</span><asp:TextBox ID="tableStorageAccountNameBox"
            runat="server" />
        <br />
        <span>TableStorageAccountKey</span><asp:TextBox ID="tableStorageAccountKeyBox"
            runat="server" />
        <br />
        <span>Disclaimer<asp:TextBox ID="DisclaimerBox" runat="server" Height="200px" Width="500px" TextMode="MultiLine"></asp:TextBox></span>
        <br />
        <asp:Button ID="submitButton" runat="server" Text="Add" OnClick="SubmitButton_Click" /><br />
        <div>
            <asp:Label ID="status" runat="server" CssClass="error" /></div>
        <asp:ListView ID="messageList" runat="server">
            <LayoutTemplate>
                <ul id="messages">
                    <li><b>alias | description | storageaccountname | storageaccountkey</b></li>
                    <li id="itemPlaceholder" runat="server" />
                </ul>
            </LayoutTemplate>
            <ItemTemplate>
                <li class="even">
                    <%# Eval("alias")%> | 
                    <%# Eval("description")%> | 
                    <%# Eval("storageaccountname")%> | 
                    <%# Eval("storageaccountkey")%>                  
                    <%# Eval("disclaimer")%>                  
                    <asp:LinkButton ID="DeleteButton" CommandArgument='<%# BuildKey(Container.DataItem) %>'
                        Text="DELETE" OnClick="DeleteButton_Click" runat="server" />
                </li>
            </ItemTemplate>
        </asp:ListView>
    </div>
    </form>
</body>
</html>
