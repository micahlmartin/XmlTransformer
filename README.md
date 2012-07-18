# XmlTransformer

Merges XML configuration files the way [NuGet does it](http://docs.nuget.org/docs/creating-packages/configuration-file-and-source-code-transformations) 
or Transforms them the way Microsofts [web.config transformations does it](http://msdn.microsoft.com/en-us/library/dd465326.aspx) but without the dependency on MSBuild.

## Get it

**Chocolatey**

```cinst xmltransformer```

Don't have [chocolately](http://chocolatey.org)? Install it like this:

```
C:\> @powershell -NoProfile -ExecutionPolicy unrestricted -Command "iex 
	((new-object net.webclient).DownloadString('http://bit.ly/psChocInstall'))" 
	&& SET PATH=%PATH%;%systemdrive%\chocolatey\bin
 ```

**NuGet**

```install-package xmltransformer```

## Command-line args

```  
Usage: XmlTransformer /s:[sourceFile] /t:[transformFile] /d:[destinationFile] /k:[ind]

Description:        Transforms xml files

Params:

  /s[ourceFile]:<fileName>

                    The file that you want to apply the transformation to


  /t[ransformFile]:<fileName> 

                    The name of the file that contains the transformations


  /d[estinationFile]:<fileName> (optional)

                    The name of the file where the transformations
                    will be saved to. This will default to the source
                    file if not specified.


  /k[ind]:<Transform,Merge>
                    The type of transformation to apply. Merge will merge 
                    the files and transform will transform them using 
                    the transformation directives.
```
					
## Transform

### How it works

See [here](http://msdn.microsoft.com/en-us/library/dd465326.aspx) for a more detailed explaination.

A transform file is an XML file that specifies how the another XML file should be changed. 
Transformation actions are specified by using XML attributes that are defined in the XML-Document-Transform namespace, which is mapped to the xdt prefix. 
The XML-Document-Transform namespace defines two attributes: Locator and Transform. The Locator attribute specifies the element or set of elements that you want to change in some way. 
The Transform attribute specifies what you want to do to the elements that the Locator attribute finds.

The following example shows the contents of a transform file that changes a connection string and replaces the customErrors element:

```  
<?xml version="1.0"?>
<configuration xmlns:xdt="http://schemas.microsoft.com/XML-Document-Transform">
  <connectionStrings>
    <add name="MyDB" 
      connectionString="value for the deployed Web.config file" 
      xdt:Transform="SetAttributes" xdt:Locator="Match(name)"/>
  </connectionStrings>
  <system.web>
    <customErrors defaultRedirect="GenericError.htm"
      mode="RemoteOnly" xdt:Transform="Replace">
      <error statusCode="500" redirect="InternalError.htm"/>
    </customErrors>
  </system.web>
</configuration>
```  

The root element of a transform file must specify the XML-Document-Transform namespace in its opening tag, as shown in the preceding example. 
The Locator and Transform elements themselves are not reproduced in the transformed file.

You can find a full list of features and syntax at http://msdn.microsoft.com/en-us/library/dd465326.aspx

### Usage

#### Commandline

This will transform the source file and overwrite it:

```
  XmlTransformer.exe /s:c:\myproject\config.xml /t:c:\myproject\newConfig.xml /k:Transform
```  

This will create a new file containing the transformed document:

```  
  XmlTransformer.exe /s:c:\myproject\config.xml /t:c:\myproject\mychanges.xml /d:c:\myproject\newConfig.xml /k:Transform
```

#### Assembly Reference

There are 2 static methods on the `XmlTransformer` class for transforming xml files:  

```public static void TransformXml(string sourceFile, string transformFile);```  
```public static void TransformXml(string sourceFile, string transformFile, string destinationFile);```


## Merge

### How it works
The transformation file should contain XML that looks like a web.config or app.config file, but it includes only the sections that need to be merged into the project's configuration file.

For example, suppose you want to add an item to the modules collection of the web.config file.

**Transformation File**  

```
<configuration>  
	<system.webServer>  
		<modules>  
			<add name="MyNuModule" type="Sample.MyNuModule" />  
		</modules>  
	<system.webServer>  
</configuration>
```

**Original File**  
```
<configuration>
	<system.webServer>
		<modules>
			<add name="MyModule" type="Sample.MyModule" />
		</modules>
	<system.webServer>
</configuration>
```

After the transformation the original file will look like this:
```  
<configuration>
	<system.webServer>
		<modules>
			<add name="MyModule" type="Sample.MyModule" />
			<add name="MyNuModule" type="Sample.MyNuModule" />
		</modules>
	<system.webServer>
</configuration>
```

Notice that modules section wasn't completely replaced, it just merged the new entry into it. When a file is merged, it only adds elements or adds attributes to existing elements in the configuration file; it does not change existing elements or attributes in any other way.

### Usage

#### Commandline

This will merge the two files and overwrite the c:\myproject\config.xml file:

```
  XmlTransformer.exe /s:c:\myproject\config.xml /t:c:\myproject\newConfig.xml /k:Merge
```  

This will create a new file containing the merged document:

```  
  XmlTransformer.exe /s:c:\myproject\config.xml /t:c:\myproject\mychanges.xml /d:c:\myproject\newConfig.xml /k:Merge
```

#### Assembly Reference

There are 3 static methods on the `XmlTransformer` class for merging xml files:  

```static XElement TransformXml(XElement sourceDocument, XElement transformDocument);```  
```static XmlElement TransformXml(XmlElement sourceDocument, XmlElement transformDocument);```
```static void TransformFile(string sourceFile, string transformPath);```  

The `TransformFile` method will actually save the merged document to the original file, the others simply returned the modified xml object.


**Disclaimer**

All credit is given to the guys at NuGet who wrote the merge utility and Microsoft who wrote the transformation utility. 
I have simply just made them available and easier to access with a command-line utility.	