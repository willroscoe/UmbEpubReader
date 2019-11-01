## <img src="assets/epub-logo200.png " style="margin-bottom: 5px" align="middle" alt="Logo" title="Umbraco Epub Reader" width="100"> Umbraco Epub Reader Plugin
[![Build status](https://ci.appveyor.com/api/projects/status/g47qga5j21yoeaq9?svg=true)](https://ci.appveyor.com/project/willroscoe/umbepubreader)
[![NuGet release](https://img.shields.io/nuget/v/UmbEpubReader.svg)](https://www.nuget.org/packages/UmbEpubReader)
[![Our Umbraco project page](https://img.shields.io/badge/our-umbraco-orange.svg)](https://our.umbraco.org/projects/website-utilities/epub-reader/)

This plugin allows you to display readable versions of (non-DRM) Epub books, navigable via the internal chapters, on your Umbraco site. The Epub file format is basically a .zip archive with embeded html, css, font & image files.

**Please note this plugin will only work for Umbraco version 7.6 and above.**

Using the provided example 'Book' & 'Books' Document Types and Templates (included in the installable package - not the nuget package) and Setup (see 'Setting Up' below), you can view/read your epub books by simply navigating to:
### /books
...to view a list of all your books. Uses the 'Books' template.

### /books/{NAME OF BOOK}
...to see the book info page. This page uses the 'Body Text' and 'Book Cover Image' from the related 'Book' content node. Uses the 'Book' template.

![Book info page](/assets/front-book.png?raw=true "Book info page")


### /books/{NAME OF BOOK}/read
...to actually read the book. This page (which uses the view: [**/Views/UmbEpubReader_Read.cshtml**][UmbEpubReader_Read]) displays the 'Book' node Cover Image as well as the actual epub chapter content and Table of Contents. This View can be ammended as needed. It has access to the [**'EpubDisplayModel'**][EpubDisplayModel] model from which you can access any of the model's properties, some of which are: Title, Authors, Cover Image, Chapter Html, Table of Contents (List&lt;[**EpubLink**][EpubLink]&gt;), an html &lt;UL&gt; formatted verion of Table of Contents, Previous/Next Chapter [**EpubLink**][EpubLink] navigation links. By default the first chapter will be displayed, unless the 'Start at Chapter' Book property has been set, in which case, that chapter will be the default chapter displayed. 

[EpubLink]: https://github.com/willroscoe/UmbEpubReader/blob/master/Wr.UmbEpubReader/Models/EpubLink.cs
[EpubDisplayModel]: https://github.com/willroscoe/UmbEpubReader/blob/master/Wr.UmbEpubReader/Models/EpubDisplayModel.cs
[UmbEpubReader_Read]: https://github.com/willroscoe/UmbEpubReader/blob/master/Wr.UmbEpubReader/Views/UmbEpubReader_Read.cshtml

![Read book page](/assets/front-read.png?raw=true "Read book page")

### Previous / Next Chapter Navigation
![Previous/Next Nav](/assets/front-bottom-nav.png?raw=true "Previous/Next Nav")


**Note: The provided templates/views are only basic examples of the plugin functionality. You can adapt them to your requirements.**


Epub books have embeded files such as images and fonts, and the plugin allows these file to be served/accessed via custom book routing i.e. /books/{NAME OF BOOK}/read/images/cover.jpg.
Embeded book css styles are added automatically to the outputted html to speed up rendering. See the Security section below.

Please note this plugin has been developed for a specific project and as such may not provided an exhaustive list of functionality or deal with all possible eventualities when encountering different epub books. You can, however, use the plugin source code as a basis for your own requirements.

This documentation/plugin assumes your site already has a basic setup e.g a Homepage Doc type and Master template or 'Umbraco Starter Kit' or similar has been installed.

Any feedback/comments/question are welcome - please use the Umbraco Project Forum [**https://our.umbraco.com/projects/website-utilities/epub-reader/issues-and-feedback/**][UmbracoProjectForum].

[UmbracoProjectForum]: https://our.umbraco.com/projects/website-utilities/epub-reader/issues-and-feedback/

## Installation
Install the package through the Umbraco backoffice (Note: This is the preferred method as it includes the required Document Types/Templates):
- In Developer -> Packages backoffice. Search for 'Epub Reader', in 'Website Utilities'
- or download the package as a zip from [**https://our.umbraco.org/projects/website-utilities/epub-reader/**][OurUmbraco]. Manually install via 'Install local' in your Umbraco package backoffice.
- or download the package as a zip from this project's GitHub page [**https://github.com/willroscoe/UmbEpubReader/releases**][GitHubRelease]. Manually install via 'Install local' in your Umbraco package backoffice.

or via NuGet (Note: Not preferred, as doesn't include the example Document Types/Templates - see above):
- ```PM> Install-Package UmbEpubReader```
- or manually download the [**NuGet Package**][NuGetPackage]. Install the NuGet package in your Visual Studio project.

[NuGetPackage]: https://www.nuget.org/packages/UmbEpubReader
[OurUmbraco]: https://our.umbraco.org/projects/website-utilities/epub-reader
[GitHubRelease]: https://github.com/willroscoe/UmbEpubReader/releases


## Setting up
To use the provided example 'Book/Books' Document Types and Templates.

Once the package has been installed, there is a small amount of setting up to do before being able to add books. In Admin:
1. Settings -> Templates - Make sure the 'Books' and 'Book' templates are children of the 'Master' template
2. Settings -> Document Types -> Home -> Permissions -> Add Child -> Select the 'Books' Document Type -> Save
3. Content -> Home -> Create an Item under Home -> Books -> Titled 'Books' -> Using the 'Books' template -> Save and Publish (or Save)

## Adding books
Once setup is complete you can add book pages to your site:
1. Content -> Home -> Books -> Create Book -> Add the book items.

The path to the books will be /books/{NAME OF BOOK}. If you want a different path e.g. /ebooks/{NAME OF BOOK} you should rename the 'Books' item in 'Setting up' point 3 (above) to the desired name i.e. 'Ebooks'.
In order for the plugin routing to work correctly for the '/read' route you will also need to add/update the 'UmbEpubReader.BooksPathSegment' app setting in your Web.Config. See 'Web.Config Settings'

![Books list in Admin](/assets/admin-books.png?raw=true "Books list in Admin")

![Book content in Admin](/assets/admin-book.png?raw=true "Book content in Admin")

## Web.Config App Settings
Optional appSettings:
```
<appSettings>
...
<add key="UmbEpubReader.BooksPathSegment" value="books"/>
<add key="UmbEpubReader.ReadPathSegment" value="read"/>
...
</appSettings>
```
The key 'UmbEpubReader.BooksPathSegment' allows you to override the /**books**/{NAME OF BOOK} path segment (however, you must also update the 'Books' Content node name to the same value). e.g. setting it to 'ebooks' will allow the following path /ebooks/{NAME OF BOOK} to work.

The key 'UmbEpubReader.ReadPathSegment' allows you to override the /books/{NAME OF BOOK}/**read** path segment. e.g. setting it to 'readonline' will allow the following path /books/{NAME OF BOOK}/**readonline** to work

## Security
Please bear in mind that Epub books could potentially contain malicious embeded files which could be served by the plugin. As a precaution the plugin currently only permits the following file extensions to be served: .html, .htm, .xhtml, .jpg, .gif, .png, .css, .ttf, .otf, .woff, .woff2, .svg

It is advisable to vet/check any epub files before using. This can be done by changing the .epub extension to .zip and uncompressing the archive in order to inspect all the contained files.


## Dependencies
This project references the following additional projects:
- [**Vers One EpubReader**][VersOneEpubReader]
- [**dotless** ][dotless]

[VersOneEpubReader]: https://github.com/vers-one/EpubReader
[dotless]: https://github.com/dotless/dotless


## License
Copyright © 2018 Will Roscoe

Licensed under the [**MIT License**][MITLicense]

[MITLicense]: https://github.com/willroscoe/UmbEpubReader/blob/master/LICENSE.md


## Version history
- 0.1.0
    - Initial release
- 0.1.1 
     - Fixed TOC UL output
- 8.0.1
	- Updated for Umbraco v8
