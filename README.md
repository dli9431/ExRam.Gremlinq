<p align="center">
  <img src="https://raw.githubusercontent.com/ExRam/ExRam.Gremlinq/main/assets/Logo.png" alt="Gremlinq mascot" title="Gremlinq mascot" width=40% />
</p>


ExRam.Gremlinq is a .NET object-graph-mapper for [Apache TinkerPop™](http://tinkerpop.apache.org/) Gremlin enabled databases.

![License](https://img.shields.io/github/license/ExRam/ExRam.Gremlinq?style=flat-square)
![Downloads](https://img.shields.io/nuget/dt/ExRam.Gremlinq.Core?style=flat-square&logo=nuget)
[![#](https://img.shields.io/nuget/v/ExRam.Gremlinq.Core?style=flat-square&logo=nuget)](https://www.nuget.org/packages/ExRam.Gremlinq.Core)
[![#](https://img.shields.io/nuget/vpre/ExRam.Gremlinq.Core?style=flat-square&logo=nuget)](https://www.nuget.org/packages/ExRam.Gremlinq.Core)
![Build status](https://img.shields.io/github/actions/workflow/status/ExRam/ExRam.Gremlinq/pack.yml?style=flat-square&logo=github)
[![codecov](https://img.shields.io/codecov/c/github/ExRam/ExRam.Gremlinq/branch/main?token=YrGnIo6XyH&logo=codecov&style=flat-square)](https://codecov.io/github/ExRam/ExRam.Gremlinq)
[![#](https://img.shields.io/github/sponsors/danielcweber?logo=githubsponsors&style=flat-square)](https://github.com/sponsors/danielcweber)

# Getting started

Install the Gremlinq templates package:

   ```sh
   > dotnet new install ExRam.Gremlinq.Templates
   ```

There are two `dotnet new` templates included in the package: one for a simple console app and one that shows how to get things running on Asp.NET Core. Currently, there is out-of-the-box support for the generic Gremlin Server, AWS Neptune, Azure CosmosDb and JanusGraph.

   ```sh
   > dotnet new gremlinq-console|gremlinq-aspnet --name GettingStartedWithGremlinq --provider GremlinServer|Neptune|CosmosDb|JanusGraph
   ```

# Paid support

Schedule a video session with [@danielcweber](https://github.com/danielcweber) to get assistance with Gremlinq setup and configuration, query writing, debugging or review.

[![#](https://img.shields.io/badge/Schedule%20a%20call-blue)](https://calendly.com/danielcweber/generalgremlinqsupport)

# Features

The following snippets are part of the sample project mentioned above. They showcase
some of the many features of ExRam.Gremlinq.

### Easily create vertices and edges
``` csharp
    var marko = await _g
        .AddV(new Person { Name = "Marko", Age = 29 })
        .FirstAsync();

    var vadas = await _g
        .AddV(new Person { Name = "Vadas", Age = 27 })
        .FirstAsync();
            
    var josh = await _g
        .AddV(new Person { Name = "Josh", Age = 32 })
        .FirstAsync();

    var peter = await _g
        .AddV(new Person { Name = "Peter", Age = 35 })
        .FirstAsync();

    var daniel = await _g
        .AddV(new Person
        {
            Name = "Daniel",
            Age = 37,
            PhoneNumbers = new []
            {
                "+491234567",
                "+492345678"
            }
        })
        .FirstAsync();

    var charlie = await _g
        .AddV(new Dog { Name = "Charlie", Age = 2 })
        .FirstAsync();

    var catmanJohn = await _g
        .AddV(new Cat { Name = "Catman John", Age = 5 })
        .FirstAsync();

    var luna = await _g
        .AddV(new Cat { Name = "Luna", Age = 9 })
        .FirstAsync();

    var lop = await _g
        .AddV(new Software { Name = "Lop", Language = ProgrammingLanguage.Java })
        .FirstAsync();

    var ripple = await _g
        .AddV(new Software { Name = "Ripple", Language = ProgrammingLanguage.Java })
        .FirstAsync();

    await _g
        .V(_marko.Id)
        .AddE<Knows>()
        .To(__ => __
            .V(vadas.Id))
        .FirstAsync();

    await _g
        .V(_marko.Id)
        .AddE<Knows>()
        .To(__ => __
            .V(josh.Id))
        .FirstAsync();

    await _g
        .V(_marko.Id)
        .AddE<Created>()
        .To(__ => __
            .V(lop.Id))
        .FirstAsync();

    await _g
        .V(josh.Id)
        .AddE<Created>()
        .To(__ => __
            .V(ripple.Id))
        .FirstAsync();

    await _g
        .V(josh.Id)
        .AddE<Created>()
        .To(__ => __
            .V(lop.Id))
        .FirstAsync();

    await _g
        .V(peter.Id)
        .AddE<Created>()
        .To(__ => __
            .V(lop.Id))
        .FirstAsync();

    await _g
        .V(josh.Id)
        .AddE<Owns>()
        .To(__ => __
            .V(charlie.Id))
        .FirstAsync();

    await _g
        .V(josh.Id)
        .AddE<Owns>()
        .To(__ => __
            .V(luna.Id))
        .FirstAsync();

    await _g
        .V(daniel.Id)
        .AddE<Owns>()
        .To(__ => __
            .V(catmanJohn.Id))
        .FirstAsync();

    // Add Persons and and edge in between in one single query!
    await _g
        .AddV(new Person { Name = "Bob", Age = 36 })
        .AddE<Knows>()
        .To(__ => __
            .AddV(new Person { Name = "Jeff", Age = 27 }))
        .FirstAsync();
```

### Build nice queries
From `marko`, walk all the `Knows` edges to all the `Persons`
that he knows and order them by their names:

``` csharp
    var peopleKnownToMarko = await _g
        .V(marko.Id)
        .Out<Knows>()
        .OfType<Person>()
        .Order(_ => _
            .By(x => x.Name))
        .Values(x => x.Name)
        .ToArrayAsync();
```
\
Gremlinq supports boolean expressions like you're used to use them
in your Linq-queries. Under the hood, they will be translated to the
corresponding Gremlin-expressions, like

```
g.V().hasLabel('Person').has('Age', gt(30))
```
in this case:

``` csharp
    var peopleOlderThan30 = await _g
        .V<Person>()
        .Where(x => x.Age > 30)
        .ToArrayAsync();
```
\
Even an expression like 'StartsWith' on a string will be recognized by ExRam.Gremlinq and translated to proper Gremlin syntax:
``` csharp
    var nameStartsWithB = await _g
        .V<Person>()
        .Where(x => x.Name.Value.StartsWith("B"))
        .ToArrayAsync();
```
\
Here, we demonstrate how to deal with Gremlin step labels. Instead of
dealing with raw strings, ExRam.Gremlinq uses a dedicated `StepLabel`-type
for these. And you don't even need to declare them upfront, as the
As-operator of ExRam.Gremlinq will put them in scope for you, along
with a continuation-query that you can further build upon! Also, ExRam.Gremlinq's `Select` operators will not leave you with
raw dictionaries (or maps, as Java calls them). Instead, you'll get
nice `ValueTuples`!
``` csharp
    var friendTuples = await _g
        .V<Person>()
        .As((__, person) => __
            .Out<Knows>()
            .OfType<Person>()
            .As((__, friend) => __
                .Select(person, friend)));
```
\
ExRam.Gremlinq supports inheritance! Below query will find all the dogs
and all the cats and instantiate the right type.
``` csharp
    var pets = await _g
        .V<Pet>();
```
\
This sample demonstrates how to fluently build projections with
ExRam.Gremlinq. It can project to a `ValueTuple` or to a `dynamic`.
In the latter case, the user may specify the name of each projection.
``` csharp
    var dynamics = await _g
        .V<Person>()
        .Project(b => b
            .ToDynamic()
            .By(person => person.Name)
            .By(
                "count",
                __ => __
                    .Cast<object>()
                    .Out<Owns>()
                    .OfType<Pet>()
                    .Count()));
```
\
ExRam.Gremlinq supports multi-properties! And since these are
represented on the POCOs as arrays (in this case PhoneNumbers),
you want to call things like `Contain` on them! ExRam.Gremlinq
recognizes these expressions!
``` csharp
    var personWithThatPhoneNumber = await _g
        .V<Person>()
        .Where(person => person
            .PhoneNumbers
            .Contains("+491234567"))
        .FirstOrDefaultAsync();
```
\
`Group` also has a beautiful fluent interface!
``` csharp
    var entityGroups = await _g
       .V()
       .Group(g => g
           .ByKey(__ => __.Label())
           .ByValue(__ => __.Count()))
       .FirstAsync();
```
\
This showcases the power of the fluent interface of ExRam.Gremlinq.
Once we go from a `Person` to the `Created` edge, the entity we
came from is actually encoded in the interface, so on calling `OutV`,
ExRam.Gremlinq remembers that we're now on a `Person` again.
``` csharp
    var creators = await _g
        .V<Person>()
        .OutE<Created>()
        .OutV()
        .Dedup();
```
\
ExRam.Gremlinq even defines extension methods on `StepLabels` so
you can ask question like the following: Which persons have an age
that's within a previously collected set of ages, referenced by a step label?
So first, for simplicity, we inject 3 values (29, 30, 31), fold them
and store them in a step label 'ages'. Note that these values 29, 30 and 31
don't need to be hard coded but can come from an ordinary traversal.
Then, we ask for all the persons whose age is contained within the array
that the 'ages' step label references.
``` csharp
    var personsWithSpecificAges = await _g
        .Inject(29, 30, 31)
        .Fold()
        .As((_, ages) => _
            .V<Person>()
            .Where(person => ages.Contains(person.Age)));
```
\
Finally, we demonstrate setting and retrieving properties on vertex properties.
Furthermore, we show how to dynamically avoid queries if the
underlying graph database provider doesn't support them. The following code will not run on
AWS Neptune since it doesn't support meta properties.
``` csharp
    if (_g.Environment.FeatureSet.Supports(VertexFeatures.MetaProperties))
    {
        await _g
            .V<Person>(_marko.Id)
            .Properties(x => x.Name)
            .Property(x => x.Creator, "Stephen")
            .Property(x => x.Date, DateTimeOffset.Now)
            .ToArrayAsync();

        var metaProperties = await _g
            .V()
            .Properties()
            .Properties()
            .ToArrayAsync();
    }
```

# Azure Cosmos DB for Apache Gremlin Configuration

Example configuration, use Azure Key Vault, env variables or user secrets for sensitive values.

``` csharp
Program.cs

using ExRam.Gremlinq.Providers.CosmosDb;
using ExRam.Gremlinq.Providers.Core;
using ExRam.Gremlinq.Support.NewtonsoftJson;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGremlinq(setup => setup.UseCosmosDb<Vertex, Edge>().Configure((configurator, providersection) // vertex and edge here refer to the Azure Cosmos template classes in GremlinqTest/Elements
    => configurator
        .At(new Uri("wss://your-endpoint.gremlin.cosmos.azure.com:443/"))
        .AuthenticateBy("your-azure-gremlin-accountKey")
        .OnDatabase("your-gremlin-db")
        .OnGraph("your-gremlin-collection")
        .WithPartitionKey(vertex => vertex.PartitionKey)
        .UseNewtonsoftJson())
).AddControllers();

```
