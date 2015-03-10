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
    <div>
    <asp:Button ID="Button4" runat="server" onclick="UploadClick" 
       Text="Upload .soils file" />
    <asp:Button ID="Button5" runat="server" onclick="DownloadClick" 
       Text="Download .soils file" />
    <asp:Button ID="Button" runat="server" onclick="ShowXMLClick" 
       Text="Show selected soil" />
    <asp:Button ID="Button2" runat="server" onclick="ShowInfoClick" 
       Text="ShowInfo" />
    <asp:Button ID="Button3" runat="server" onclick="KMLClick" 
       Text="GetKML for all soils" />
    <asp:Button ID="Button6" runat="server" onclick="ShowJSONClick" 
       Text="Show selected soil (json)" style="margin-bottom: 0px" />
    <asp:Button ID="Button7" runat="server" onclick="SoilChart" 
       Text="Soil chart" style="margin-bottom: 0px" />
    <asp:Button ID="Button8" runat="server" onclick="SoilChartFromJson" 
       Text="Soil chart (from json)" style="margin-bottom: 0px" />
    <asp:Button ID="Button9" runat="server" onclick="Button9_Click" 
       Text="All Aust Soils" style="margin-bottom: 0px" />
    <p>
       <asp:Label ID="InfoLabel" runat="server" Text=" "></asp:Label>
    </p>
    </div>
    </form>
</body>
</html>
