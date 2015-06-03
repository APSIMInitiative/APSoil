<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FindClosest.aspx.cs" Inherits="Apsoil.FindClosest" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
        <asp:Panel ID="Panel1" runat="server">
            <asp:Label ID="Label2" runat="server" Text="Thickness (cm)"></asp:Label>
            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; PAWC (mm)</asp:Panel>
        <asp:TextBox ID="Thickness1" runat="server"></asp:TextBox>
        <asp:TextBox ID="PAWC1" runat="server"></asp:TextBox>
        <br />
        <asp:TextBox ID="Thickness2" runat="server"></asp:TextBox>
        <asp:TextBox ID="PAWC2" runat="server"></asp:TextBox>
        <br />
        <asp:TextBox ID="Thickness3" runat="server"></asp:TextBox>
        <asp:TextBox ID="PAWC3" runat="server"></asp:TextBox>
        <br />
        <asp:TextBox ID="Thickness4" runat="server"></asp:TextBox>
        <asp:TextBox ID="PAWC4" runat="server"></asp:TextBox>
        <br />
        <asp:TextBox ID="Thickness5" runat="server"></asp:TextBox>
        <asp:TextBox ID="PAWC5" runat="server"></asp:TextBox>
        <br />
        <asp:TextBox ID="Thickness6" runat="server"></asp:TextBox>
        <asp:TextBox ID="PAWC6" runat="server"></asp:TextBox>
        <br />
        <asp:TextBox ID="Thickness7" runat="server"></asp:TextBox>
        <asp:TextBox ID="PAWC7" runat="server"></asp:TextBox>
        <br />
        <asp:TextBox ID="Thickness8" runat="server"></asp:TextBox>
        <asp:TextBox ID="PAWC8" runat="server"></asp:TextBox>
        <br />
        <asp:TextBox ID="Thickness9" runat="server"></asp:TextBox>
        <asp:TextBox ID="PAWC9" runat="server"></asp:TextBox>
        <br />
        <asp:TextBox ID="Thickness10" runat="server"></asp:TextBox>
        <asp:TextBox ID="PAWC10" runat="server"></asp:TextBox>
        <br />
        <asp:Button ID="Button1" runat="server" Text="Find" onclick="OnFindClick" />
    
    </div>
    </form>
</body>
</html>
