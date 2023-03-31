using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Ron.NET;
using RonGen;

string path = args[0];

AssemblyDependencyResolver resolver = new AssemblyDependencyResolver(path);

AssemblyLoadContext.Default.Resolving += (context, name) =>
{
    string pth = resolver.ResolveAssemblyToPath(name);
    if (pth != null)
        return context.LoadFromAssemblyPath(pth);
    return null;
};

Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
assembly.GetReferencedAssemblies();

foreach (Type type in assembly.GetTypes().Where(type => type.GetCustomAttribute(typeof(RonGenAttribute)) != null))
{
    Console.WriteLine(Generator.GenerateDeserializerMethodForType(type));
}