<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Apsoil.aspx.cs" Inherits="Apsoil.WebForm1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        .wrap { white-space: normal; width: 100px; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>

        <asp:Button ID="Button9" runat="server" onclick="OnShowAllClick" 
            Text="Show all soils"
             Height="72px" Width="80px" CssClass="wrap" />
        <asp:Button ID="Button10" runat="server" onclick="OnFindClosestClick" 
            Text="Find soils with closest PAWC"
            Height="72px" Width="120px" CssClass="wrap" />
        <asp:Button ID="Button4" runat="server" onclick="UploadClick" 
            Text="Upload APSOIL .soils file"
            Height="72px" Width="120px" CssClass="wrap" />
        <asp:Button ID="Button14" runat="server" onclick="KMLClick" 
            Text="Get KML for all soils"
            Height="72px" Width="120px" CssClass="wrap" />

   
        <asp:Label ID="Label1" runat="server" Text="Filter:"></asp:Label>
        <asp:TextBox ID="FilterTextBox" runat="server" 
            ontextchanged="OnFilterTextBoxChanged"></asp:TextBox>
    </div>
    <div>
        <asp:Label ID="Label2" runat="server" Text="Soil paths:"></asp:Label>
    </div>
    <asp:ListBox ID="ListBox" runat="server" Height="410px" Width="776px" 
        SelectionMode="Multiple">
    </asp:ListBox>
    <div>
    <asp:Label ID="Label" runat="server" Text="Number of soils: "></asp:Label>
    </div>
    <div>
    <asp:Button ID="Button" runat="server" onclick="ShowXMLClick" 
       Text="Show XML" />
    <asp:Button ID="Button6" runat="server" onclick="ShowJSONClick" 
       Text="Show JSON" style="margin-bottom: 0px" />
    <asp:Button ID="Button13" runat="server" onclick="OnShowPathsClick" 
       Text="Show paths" />
    <asp:Button ID="Button7" runat="server" onclick="OnSoilChart" 
       Text="Soil chart" style="margin-bottom: 0px" />
    <asp:Button ID="Button12" runat="server" onclick="OnUploadSoilClick" 
       Text="Upload user soils" />
    <asp:Button ID="Button15" runat="server" onclick="OnSoilChartFromJson" 
       Text="Soil chart from JSON" style="margin-bottom: 0px" />
    <asp:Button ID="Button16" runat="server" onclick="OnCheckSoilsClick" 
       Text="Check selected soils" style="margin-bottom: 0px" />
    <asp:Button ID="Button1" runat="server" onclick="OnDeleteClick" 
       Text="Delete selected soils" style="margin-bottom: 0px" />
    <p>
       <asp:Label ID="InfoLabel" runat="server" Text=" "></asp:Label>
    <asp:Label ID="label3" runat="server"></asp:Label>
    </p>
    </div>
    </form>
</body>
</html>
