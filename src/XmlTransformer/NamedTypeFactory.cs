using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace XmlTransformer
{
    internal class NamedTypeFactory
    {
        private List<NamedTypeFactory.Registration> registrations = new List<NamedTypeFactory.Registration>();
        private string relativePathRoot;

        internal NamedTypeFactory(string relativePathRoot)
        {
            this.relativePathRoot = relativePathRoot;
            this.CreateDefaultRegistrations();
        }

        private void CreateDefaultRegistrations()
        {
            this.AddAssemblyRegistration(base.GetType().Assembly, base.GetType().Namespace);
        }

        internal void AddAssemblyRegistration(Assembly assembly, string nameSpace)
        {
            this.registrations.Add(new NamedTypeFactory.Registration(assembly, nameSpace));
        }

        internal void AddAssemblyRegistration(string assemblyName, string nameSpace)
        {
            this.registrations.Add((NamedTypeFactory.Registration)new NamedTypeFactory.AssemblyNameRegistration(assemblyName, nameSpace));
        }

        internal void AddPathRegistration(string path, string nameSpace)
        {
            if (!Path.IsPathRooted(path))
                path = Path.Combine(Path.GetDirectoryName(this.relativePathRoot), path);
            this.registrations.Add((NamedTypeFactory.Registration)new NamedTypeFactory.PathRegistration(path, nameSpace));
        }

        internal ObjectType Construct<ObjectType>(string typeName) where ObjectType : class
        {
            if (string.IsNullOrEmpty(typeName))
                return default(ObjectType);
            Type type = this.GetType(typeName);
            if (type == (Type)null)
                throw new XmlTransformationException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "Could not resolve '{0}' as a type of {1}", new object[2] { (object) typeName, (object) typeof (ObjectType).Name }));
            else if (!type.IsSubclassOf(typeof(ObjectType)))
            {
                throw new XmlTransformationException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "'{0}' is not a type of {1}", new object[2] { (object) type.FullName, (object) typeof (ObjectType).Name }));
            }
            else
            {
                ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
                if (!(constructor == (ConstructorInfo)null))
                    return constructor.Invoke(new object[0]) as ObjectType;
                throw new XmlTransformationException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "Type '{0}' must have a no-argument constructor to be instantiated by the transformation engine", new object[1] { (object) type.FullName }));
            }
        }

        private Type GetType(string typeName)
        {
            Type type1 = (Type)null;
            foreach (NamedTypeFactory.Registration registration in this.registrations)
            {
                if (registration.IsValid)
                {
                    Type type2 = registration.Assembly.GetType(registration.NameSpace + "." + typeName);
                    if (type2 != (Type)null)
                    {
                        if (type1 == (Type)null)
                            type1 = type2;
                        else
                            throw new XmlTransformationException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "Type '{0}' was found in more than one assembly", new object[1] { (object) typeName }));
                    }
                }
            }
            return type1;
        }

        private class Registration
        {
            private Assembly assembly;
            private string nameSpace;

            public bool IsValid
            {
                get
                {
                    return this.assembly != (Assembly)null;
                }
            }

            public string NameSpace
            {
                get
                {
                    return this.nameSpace;
                }
            }

            public Assembly Assembly
            {
                get
                {
                    return this.assembly;
                }
            }

            public Registration(Assembly assembly, string nameSpace)
            {
                this.assembly = assembly;
                this.nameSpace = nameSpace;
            }
        }

        private class AssemblyNameRegistration : NamedTypeFactory.Registration
        {
            public AssemblyNameRegistration(string assemblyName, string nameSpace)
                : base(Assembly.Load(assemblyName), nameSpace)
            {
            }
        }

        private class PathRegistration : NamedTypeFactory.Registration
        {
            public PathRegistration(string path, string nameSpace)
                : base(Assembly.LoadFile(path), nameSpace)
            {
            }
        }
    }
}
