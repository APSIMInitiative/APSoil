<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UploadApsoilSoilsFile.aspx.cs" Inherits="Apsoil.UploadApsoilSoilsFile" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server"  >
    <div>
    
       <asp:Label ID="Label2" runat="server" 
          Text="This form allows you to upload soils to apsimdev.apsim.info. They will then be available in Google Earth."></asp:Label>
    
    </div>
    <asp:Label ID="Label1" runat="server" 
       Text="Locate the .soils file you wish to upload : "></asp:Label>
    <asp:FileUpload ID="File1" runat="server" />
    <p>
       <asp:Button ID="UploadButton" runat="server" onclick="UploadButton_Click" 
          Text="Upload" />
    </p>
    <p>
       <asp:Label ID="SuccessLabel" runat="server" Font-Bold="True" 
          ForeColor="#FF3300" Text="All soils successfully uploaded." Visible="False"></asp:Label>
    </p>
    </form>
</body>
</html>
