# XmlTransformer

Merges XML configuration files.

## Usage
The transformation file should contain XML that looks like a web.config or app.config file, but it includes only the sections that need to be merged into the project's configuration file.

For example, suppose you want to add an item to the modules collection of the web.config file.

**Transformation File**  
`
	<configuration>
		<system.webServer>
			<modules>
				<add name="MyNuModule" type="Sample.MyNuModule" />
			</modules>
		<system.webServer>
	</configuration>
`
**Original File**  
    <configuration>
        <system.webServer>
            <modules>
                <add name="MyModule" type="Sample.MyModule" />
            </modules>
        <system.webServer>
    </configuration>

After the transformation the original file will look like this:  
    <configuration>
        <system.webServer>
            <modules>
                <add name="MyModule" type="Sample.MyModule" />
                <add name="MyNuModule" type="Sample.MyNuModule" />
            </modules>
        <system.webServer>
    </configuration>

Notice that modules section wasn't completely replaced, it just merged the new entry into it. When a file is merged, it only adds elements or adds attributes to existing elements in the configuration file; it does not change existing elements or attributes in any other way.
