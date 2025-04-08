# How to update from old SOAP APSOIL API to new REST API


## AllSoilNames

Old Response:
```xml
<AllSoilNamesResult>
    <string>Soils/Australia/Queensland/Darling Downs and Granite Belt/Red Chromosol (Billa Billa No066)</string>
    ...
</AllSoilNamesResult>
```

New URL: https://apsoil.apsim.info/xml/search?output=Names
New Response:
```xml
<ArrayOfString xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
    <string>Soils/Africa/Generic/Clay_Deep_HF_200mm (No866-Generic)</string>
    ...
</ArrayOfString>
```

## AllAustralianSoils

Old Response:
```xml
<AllAustralianSoilsResult>
    <SoilBasicInfo>
        <Name>Soils/Australia/Queensland/Darling Downs and Granite Belt/Red Chromosol (Billa Billa No066)</Name>
        <Latitude>-28.165095</Latitude>
        <Longitude>150.201197</Longitude>
    </SoilBasicInfo>
    ...
</AllAustralianSoilsResult>
```

New URL: https://apsoil.apsim.info/xml/search?country=Australia&output=BasicInfo
New Response:
```xml
<ArrayOfBasicInfo xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
    <BasicInfo>
        <Name>Soils/Australia/Queensland/Darling Downs and Granite Belt/Red Chromosol (Billa Billa No066)</Name>
        <Latitude>-28.162</Latitude>
        <Longitude>150.201</Longitude>
    </BasicInfo>
    ...
```

## ClosestMatchingSoils

Old Request:
```xml
<ClosestMatchingSoils xmlns="http://www.apsim.info/">
    <thickness>
        <double>100</double>
        <double>500</double>
    </thickness>
    <sw>
        <double>5</double>
        <double>10</double>
    </sw>
    <cropName>wheat</cropName>
    <numSoilsToReturn>2</numSoilsToReturn>
    <swAtCLL>true</swAtCLL>
    <swIsGrav>false</swIsGrav>
</ClosestMatchingSoils>
```
Old Response:
```xml
<ClosestMatchingSoilsResponse xmlns="http://www.apsim.info/">
    <ClosestMatchingSoilsResult>
        <string>Soils/Australia/South Australia/Mid North/Black Cracking Clay (Alma)(CL908)</string>
        <string>Soils/Australia/South Australia/Yorke Peninsula/Deep Calcareous Sand (Coonarie)(CY020)</string>
    </ClosestMatchingSoilsResult>
</ClosestMatchingSoilsResponse>
```

New URL: http://localhost/xml/search?cropname=wheat&thickness=100,500&cll=0.3,0.2&numToReturn=2
New Response:
```xml
<?xml version="1.0" encoding="utf-16"?>
<ArrayOfString xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
    <string>Soils/Australia/Tasmania/Central North/Medium Clay (Longford No651)</string>
    <string>Soils/New Zealand/Silt clay loam (Havelock North No1338)</string>
</ArrayOfString>
```

## GetSoilAnalysisInfo

Same as GetSoilInfo below.

## GetSoilInfo

Old Response:
```xml
<GetSoilInfoResponse xmlns="http://www.apsim.info/">
    <GetSoilInfoResult>
        <Name>Clay_Deep_HF_200mm (No866-Generic)</Name>
        <Description>Dr. John P Dimes, ICRISAT, Bulawayo, Zimbabwe J.Dimes@cgiar.org</Description>
        <SoilType>Other</SoilType>
        <Latitude>-20.074444</Latitude>
        <Longitude>30.832778</Longitude>
        <Distance>0</Distance>
        <ASCOrder />
        <ASCSubOrder />
        <Site>Generic</Site>
        <Region />
        <NearestTown />
    </GetSoilInfoResult>
</GetSoilInfoResponse>
```

New URL: http://localhost/xml/search?fullname=Soils/Africa/Generic/Clay_Deep_HF_200mm%20(No866-Generic)&output=ExtendedInfo
New Response:
```xml
<?xml version="1.0" encoding="utf-16"?>
<ArrayOfSoilInfo xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
    <SoilInfo>
        <Name>Soils/Africa/Generic/Clay_Deep_HF_200mm (No866-Generic)</Name>
        <Description>Dr. John P Dimes, ICRISAT, Bulawayo, Zimbabwe
J.Dimes@cgiar.org</Description>
        <SoilType>Other</SoilType>
        <Latitude>-20.074444</Latitude>
        <Longitude>30.832778</Longitude>
        <Distance>0</Distance>
        <ASCOrder />
        <ASCSubOrder />
        <Site>Generic</Site>
        <Region />
        <NearestTown />
        <Thickness>
            <double>100</double>
            ...
        </Thickness>
        <Texture />
        <EC />
        <PH>
            <double>6.5</double>
            ...
        </PH>
        <CL />
        <ESP />
        <LL15>
            <double>0.21</double>
            ...
        </LL15>
        <DUL>
            <double>0.39</double>
            ...
        </DUL>
        <Crops>
            <SoilCropInfo>
                <Name>maize</Name>
                <LL>
                    <double>0.21</double>
                    ...
                </LL>
                <KL>
                    <double>0.08</double>
                    ...
                </KL>
                <XF>
                    <double>1</double>
                    ...
                </XF>
            </SoilCropInfo>
            ...
        </Crops>
    </SoilInfo>
```

## PAW

Old Request:
```xml
<PAW xmlns="http://www.apsim.info/">
    <SoilName>Soils/Australia/Generic/Ferrosol (Nth Au No1032-Generic)</SoilName>
    <Thickness>
    <double>100</double>
    <double>500</double>
    </Thickness>
    <SW>
    <double>0.4</double>
    <double>0.3</double>
    </SW>
    <IsGravimetric>false</IsGravimetric>
    <CropName>wheat</CropName>
</PAW>
```

Old Response:
```xml
<PAWResponse xmlns="http://www.apsim.info/">
    <PAWResult>24.062205301138675</PAWResult>
</PAWResponse>
```

New URL: http://localhost/xml/paw?fullname=Soils/Australia/Generic/Ferrosol (Nth Au No1032-Generic)&cropName=wheat&thickness=100,500&sw=0.5,0.4&swIsGrav=false
New Response:
```
24.062205301138675
```

##  PAWC

Old Request:
```xml
<PAWC xmlns="http://www.apsim.info/">
    <SoilName>Soils/Australia/Generic/Ferrosol (Nth Au No1032-Generic)</SoilName>
    <CropName>wheat</CropName>
</PAWC>
```

Old Response:
```xml
<PAWCResponse xmlns="http://www.apsim.info/">
    <PAWCResult>121.86453640103461</PAWCResult>
</PAWCResponse>
```

New URL: http://localhost/xml/pawc?fullname=Soils/Australia/Generic/Ferrosol (Nth Au No1032-Generic)&cropName=wheat
New Response:
```
121.86453640103461
```

## PAWCJson

No equivalent. Is this needed?

## SearchSoilsReturnInfo

Old Request:
```xml
    <SearchSoilsReturnInfo xmlns="http://www.apsim.info/">
      <Latitude>-27</Latitude>
      <Longitude>150</Longitude>
      <Radius>30</Radius>
      <SoilType>Clay</SoilType>
    </SearchSoilsReturnInfo>
```
Old Response:
```xml
<SearchSoilsReturnInfoResult>
    <SoilInfo>
        <Name>Soils/Australia/Queensland/Darling Downs and Granite Belt/Grey Vertosol-Kupunn (Condamine No105)</Name>
        <Description>Clay - </Description>
        <SoilType>Clay</SoilType>
        <Latitude>-27.0942323</Latitude>
        <Longitude>149.9419239</Longitude>
        <Distance>11.95230938092892</Distance>
        <ASCOrder>Vertosol</ASCOrder>
        <ASCSubOrder>Grey</ASCSubOrder>
        <Site>Condamine</Site>
        <Region>Darling Downs and Granite Belt</Region>
        <NearestTown>Condamine, Q 4416</NearestTown>
    </SoilInfo>
    ...
</SearchSoilsReturnInfoResponse>
```

New URL: http://localhost/xml/search?latitude=-27&longitude=150&radius=30&soiltype=Clay&output=ExtendedInfo
New Response:
```xml
<ArrayOfSoilInfo xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
    <SoilInfo>
        <Name>Soils/Australia/Queensland/Darling Downs and Granite Belt/Grey Vertosol-Kupunn (Condamine No105)</Name>
        <Description>CSIRO Sustainable Ecosystems, Toowoomba</Description>
        <SoilType>Clay</SoilType>
        <Latitude>-27.091</Latitude>
        <Longitude>149.942</Longitude>
        <Distance>0</Distance>
        <ASCOrder>Vertosol</ASCOrder>
        <ASCSubOrder>Grey</ASCSubOrder>
        <Site>Condamine</Site>
        <Region>Darling Downs and Granite Belt</Region>
        <NearestTown>Condamine, Q 4416</NearestTown>
        ...
    </SoilInfo>
</ArrayOfSoilInfo>
```

## SoilChartWithSamplePNG

Old Response:
```xml
```

New URL:
New Response:
```xml
```

## SoilChartPNG

New URL: https://apsoil.apsim.info/xml/graph?fullname=Soils/Africa/Generic/Clay_Deep_HF_200mm%20(No866-Generic)

##  SoilChartPNGFromXML

No equivalent. Is this needed?

## SoilXML

Old Response:
```xml
<folder name="Soils">
  <Soil name="Ferrosol (Nth Au No1032-Generic)">
    <RecordNumber>944</RecordNumber>
    <ASCOrder>Ferrosol</ASCOrder>
    <ASCSubOrder />
    <SoilType>Other</SoilType>
    <LocalName />
    <Site>Northern GRDC region</Site>
    <NearestTown />
    <Region />
    <State>Generic</State>
    <Country>Australia</Country>
    <NaturalVegetation />
    <ApsoilNumber>1032-GENERIC</ApsoilNumber>
    <Latitude>-27.161725
    ...
  </Soil>
</folder>
```

New URL: http://localhost/xml/search?fullname=Soils/Australia/Generic/Ferrosol%20(Nth%20Au%20No1032-Generic)&output=FullSoil
New Response:
```xml
<Folder xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" name="Soils">
    <Soil name="Ferrosol (Nth Au No1032-Generic)">
        <RecordNumber>944</RecordNumber>
        <ASCOrder>Ferrosol</ASCOrder>
        <ASCSubOrder />
        <SoilType>Other</SoilType>
        <LocalName />
        <Site>Northern GRDC region</Site>
        <NearestTown />
        <Region />
        <State>Generic</State>
        <Country>Australia</Country>
        <NaturalVegetation />
        <ApsoilNumber>1032-GENERIC</ApsoilNumber>
        <Latitude>-27.161725</Latitude>
        ...
    </Soil>
</Folder/>
```

##  UpdateUserSoilXml

New POST URL: http://localhost/xml/add
Body is the XML of the soil to add / update
