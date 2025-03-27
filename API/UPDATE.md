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

Old Response:
```xml
```

New URL:
New Response:
```xml
```

## GetSoilAnalysisInfo

Old Response:
```xml
```

New URL:
New Response:
```xml
```

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

Old Response:
```xml
```

New URL:
New Response:
```xml
```

##  PAWC

Old Response:
```xml
```

New URL:
New Response:
```xml
```

## PAWCJson   Is this needed?

Old Response:
```xml
```

New URL:
New Response:
```
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
Old Response:
```xml
```

New URL: https://apsoil.apsim.info/xml/graph?fullname=Soils/Africa/Generic/Clay_Deep_HF_200mm%20(No866-Generic)
New Response:
```xml
```

##  SoilChartPNGFromXML    Is this needed?

Old Response:
```xml
```

New URL:
New Response:
```xml
```

## SoilXML

Old Response:
```xml
```

New URL:
New Response:
```xml
```

## SearchSoilsReturnInfo
Old Response:
```xml
```

New URL:
New Response:
```xml
```

##  SoilChartPNG
Old Response:
```xml
```

New URL:
New Response:
```xml
```

##  UpdateUserSoilXml
Old Response:
```xml
```

New URL:
New Response:
```xml
```


