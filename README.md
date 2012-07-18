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

```
  /s[ourceFile]:<fileName> 		The file that you want to apply the transformation to

  /t[ransformFile]:<fileName> 	The name of the file that contains the transformations
					
  /k[ind]:Merge					Use the merge method
```

#### Assembly Reference

There are 3 static methods on the `XmlTransformer` class for merging xml files:  

```static XElement TransformXml(XElement sourceDocument, XElement transformDocument);```  
```static XmlElement TransformXml(XmlElement sourceDocument, XmlElement transformDocument);```
```static void TransformFile(string sourceFile, string transformPath);```  

The `TransformFile` method will actually save the merged document to the original file, the others simply returned the modified xml object.