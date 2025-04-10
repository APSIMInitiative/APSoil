# API Usage

Every soil has a unique name, called a _fullname_.

To get all _fullnames_ for soils in Africa:
```https://apsoil.apsim.info/search?country=Africa```

To get all _fullnames_ for soils that have _loam_ in their soiltype:

```https://apsoil.apsim.info/search?soiltype=loam```

To get all _fullnames_ (ordered by closeness to a latitude/longitude pair) for soils that have _loam_ in their soiltype

```https://apsoil.apsim.info/search?soiltype=loam&latitude=-27&longitude=150```

To get 10 _fullnames_ (ordered by closeness to a latitude/longitude pair) for soils that have _loam_ in their soiltype

```https://apsoil.apsim.info/search?soiltype=loam&latitude=-27&longitude=150&numToReturn=10```

Get 10 _fullnames_ (ordered by closeness to a latitude/longitude pair and a wheat pawc of 150mm down to 1000mm of soil depth) of soils that have _loam_ in their soiltype

```https://apsoil.apsim.info/search?soiltype=loam&latitude=-27&longitude=150&pawc=150&thickness=1000&cropname=wheat&numToReturn=10```


To get the XML of the soil with the full name __Soils/Africa/Generic/Clay_Deep_HF_200mm (No866-Generic)__
```https://apsoil.apsim.info/get?fullnames=Soils/Africa/Generic/Clay_Deep_HF_200mm (No866-Generic)```

To get the XML information of the soil with the full name __Soils/Africa/Generic/Clay_Deep_HF_200mm (No866-Generic)__
```https://apsoil.apsim.info/info?fullname=Soils/Africa/Generic/Clay_Deep_HF_200mm (No866-Generic)```

To get a graph of the soil with the full name  __Soils/Africa/Generic/Clay_Deep_HF_200mm (No866-Generic)__

```https://apsoil.apsim.info/graph?fullname=Soils/Africa/Generic/Clay_Deep_HF_200mm (No866-Generic)```

To get a Google Earth KMZ spatial layer of soils for a country:

```https://apsoil.apsim.info/xml/search?country=Australia&output=KML```