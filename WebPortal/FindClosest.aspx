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
            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; PAWC (mm)&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Grav. water (mm/mm)</asp:Panel>
        <asp:TextBox ID="Thickness1" runat="server"></asp:TextBox>
        <asp:TextBox ID="PAWC1" runat="server"></asp:TextBox>
        <asp:TextBox ID="Grav1" runat="server"></asp:TextBox>
        <br />
        <asp:TextBox ID="Thickness2" runat="server"></asp:TextBox>
        <asp:TextBox ID="PAWC2" runat="server"></asp:TextBox>
        <asp:TextBox ID="Grav2" runat="server"></asp:TextBox>
        <br />
        <asp:TextBox ID="Thickness3" runat="server"></asp:TextBox>
        <asp:TextBox ID="PAWC3" runat="server"></asp:TextBox>
        <asp:TextBox ID="Grav3" runat="server"></asp:TextBox>
        <br />
        <asp:TextBox ID="Thickness4" runat="server"></asp:TextBox>
        <asp:TextBox ID="PAWC4" runat="server"></asp:TextBox>
        <asp:TextBox ID="Grav4" runat="server"></asp:TextBox>
        <br />
        <asp:TextBox ID="Thickness5" runat="server"></asp:TextBox>
        <asp:TextBox ID="PAWC5" runat="server"></asp:TextBox>
        <asp:TextBox ID="Grav5" runat="server"></asp:TextBox>
        <br />
        <asp:TextBox ID="Thickness6" runat="server"></asp:TextBox>
        <asp:TextBox ID="PAWC6" runat="server"></asp:TextBox>
        <asp:TextBox ID="Grav6" runat="server"></asp:TextBox>
        <br />
        <asp:TextBox ID="Thickness7" runat="server"></asp:TextBox>
        <asp:TextBox ID="PAWC7" runat="server"></asp:TextBox>
        <asp:TextBox ID="Grav7" runat="server"></asp:TextBox>
        <br />
        <asp:TextBox ID="Thickness8" runat="server"></asp:TextBox>
        <asp:TextBox ID="PAWC8" runat="server"></asp:TextBox>
        <asp:TextBox ID="Grav8" runat="server"></asp:TextBox>
        <br />
        <asp:TextBox ID="Thickness9" runat="server"></asp:TextBox>
        <asp:TextBox ID="PAWC9" runat="server"></asp:TextBox>
        <asp:TextBox ID="Grav9" runat="server"></asp:TextBox>
        <br />
        <asp:TextBox ID="Thickness10" runat="server"></asp:TextBox>
        <asp:TextBox ID="PAWC10" runat="server"></asp:TextBox>
        <asp:TextBox ID="Grav10" runat="server"></asp:TextBox>
        <br />
        <asp:CheckBox ID="CheckBox1" runat="server" Text="Gravimetrics are at lower limit?" />
        <br />
        <asp:Button ID="Button1" runat="server" Text="Find" onclick="OnFindClick" />
        
    </div>
    </form>
</body>
</html>
