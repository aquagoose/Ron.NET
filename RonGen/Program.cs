using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.Loader;
using CommandLine;
using Ron.NET;
using RonGen;

Console.WriteLine($"RonGen {Assembly.GetExecutingAssembly().GetName().Version}\nskyebird189 2023");

CliArgs cli = Parser.Default.ParseArguments<CliArgs>(args).Value;

string path = cli.Path;

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

Console.WriteLine($"Searching for classes and structs in {assembly.FullName} with attribute {typeof(RonGenAttribute)}...");

Type[] types = assembly.GetTypes().Where(type => type.GetCustomAttribute(typeof(RonGenAttribute)) != null).ToArray();

foreach (Type type in types)
    Console.WriteLine($"Found {type.FullName}.");

string output = Generator.GenerateRonGenMethodsClass(types, cli.ClassName, cli.Namespace);

if (cli.Output == null)
    Console.WriteLine(output);
else
    File.WriteAllText(cli.Output, output);

class CliArgs
{
    [Value(0, Required = true)]
    public string Path { get; set; }

    [Option('c', "class")]
    public string ClassName { get; set; } = "RonSerializers";

    [Option('n', "namespace")]
    public string Namespace { get; set; } = "MyNamespace";
    
    [Option('o', "output")]
    public string Output { get; set; }
}