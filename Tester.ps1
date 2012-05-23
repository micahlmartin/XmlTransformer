
$rootPath = Split-Path $MyInvocation.MyCommand.Definition -Parent

[System.Reflection.Assembly]::LoadFrom("$rootPath\release\net40\binaries\XmlTransformer.dll")

$source = [xml]"<configuration><test></test></configuration>"
$transform = [xml]"<configuration><something></something><test att=`"testatt`"></test></configuration>"

$s = [XmlTransformer.XmlTransformer]::TransformXml($source.DocumentElement, $transform.DocumentElement)
$s