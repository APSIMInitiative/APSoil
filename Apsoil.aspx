<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Apsoil.aspx.cs" Inherits="Apsoil.WebForm1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
       <asp:Label ID="Label" runat="server" Text="Number of soils: "></asp:Label>
    
    </div>
    <asp:ListBox ID="ListBox" runat="server" Height="410px" Width="776px">
    </asp:ListBox>
    <asp:Button ID="Button4" runat="server" onclick="Button4_Click" 
       Text="Import from .soils" />
    <asp:Button ID="Button" runat="server" onclick="Button_Click" 
       Text="Show selected soil" />
    <asp:Button ID="Button2" runat="server" onclick="Button2_Click" 
       Text="ShowInfo" />
    <asp:Button ID="Button3" runat="server" onclick="Button3_Click" 
       Text="GetKML for all soils" />
    <p>
       <asp:Label ID="InfoLabel" runat="server" Text=" "></asp:Label>
    </p>
    </form>
</body>
</html>
