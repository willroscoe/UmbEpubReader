
    tools\UmbracoTools\Wr.UmbracoTools.Packager.exe -set umbraco\package_settings.json -out "..\..\..\testing\UmbEpubReader-{version}.zip"

	tools\nuget.exe pack  ..\Wr.UmbEpubReader\Wr.UmbEpubReader.csproj -OutputDirectory ..\..\..\testing\