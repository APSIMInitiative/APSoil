# Description

Web portal and web service for serving APSoil soils.
This is used in the APSIM NG download soils function among other places.

# Example API endpoint

```
http://apsimdev.apsim.info/ApsoilWebService/Service.asmx/SearchSoilsReturnInfo?latitude=-33.2815802&longitude=149.0862242&radius=100&SoilType=
```

# How to update web service
1. Remote desktop into apsimdev.apsim.info
2. Stop all web services in 'Internet Information Services Manager (IIS)'. 
    1. This located on the far right once you've clicked: `APSIMDEV2-APSIM > Sites > Default Web Site`.
3. Open file explorer and navigate to: `Data > Websites > APSIM.Soil.Service`
4. In the root folder copy and replace:
    1. `Apsim.xml`
    2. `packages.config`
    3. `Web.config`
    4. `Service.asmx`
5. Also copy over and replace all files in the `bin` directory.
6. Start the Default Web Site.
7. Done.
