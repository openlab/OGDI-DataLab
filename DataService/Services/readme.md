This project is provided to add the ability to split the Data Service from the Data Browser for scalability purposes.

To do this, add the Services.ccproj back to the Ogdi.sln and Remove the following section from the ServiceDefinition.csdef & ServiceConfiguration.cscfg.

### csdef

```xml
<Site name="Data" physicalDirectory="..\..\DataService\Services_WebRole">
     <Bindings>
          <Binding name="DataServiceIn" endpointName="DataServiceIn"/>
     </Bindings>
</Site>
```

```xml
      <Setting name="RootServiceNamespace" />
      <Setting name="TableStorageBaseUrl" />
      <Setting name="BlobStorageBaseUrl" />
```

```xml
      <InputEndpoint name="DataServiceIn" protocol="http" port="8080"/>
```
### cscfg

```xml
      <!-- Data Service Configurations -->
      <Setting name="RootServiceNamespace" value="OGDI" />
      <Setting name="TableStorageBaseUrl" value="https://{0}.table.core.windows.net/" />
      <Setting name="BlobStorageBaseUrl" value="https://{0}.blob.core.windows.net/" />
```
