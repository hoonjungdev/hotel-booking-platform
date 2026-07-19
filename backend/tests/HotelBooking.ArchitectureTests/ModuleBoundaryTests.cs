using System.Reflection;
using HotelBooking.Modules.Booking;
using HotelBooking.Modules.Identity;
using HotelBooking.Modules.Inventory;
using HotelBooking.Modules.Notification;
using HotelBooking.Modules.Payment;
using HotelBooking.Modules.Pricing;
using HotelBooking.Modules.Pricing.Domain.PriceQuotes.ValueObjects;
using HotelBooking.Modules.Pricing.Domain.References;
using HotelBooking.Modules.Property;

namespace HotelBooking.ArchitectureTests;

public class ModuleBoundaryTests
{
    private static readonly Assembly[] ModuleAssemblies =
    [
        typeof(BookingModuleAssembly).Assembly,
        typeof(IdentityModuleAssembly).Assembly,
        typeof(InventoryModuleAssembly).Assembly,
        typeof(NotificationModuleAssembly).Assembly,
        typeof(PaymentModuleAssembly).Assembly,
        typeof(PricingModuleAssembly).Assembly,
        typeof(PropertyModuleAssembly).Assembly
    ];

    [Fact]
    public void Module_assemblies_do_not_reference_another_module_implementation()
    {
        foreach (Assembly moduleAssembly in ModuleAssemblies)
        {
            string[] referencedModuleAssemblies = moduleAssembly
                .GetReferencedAssemblies()
                .Select(reference => reference.Name)
                .Where(name => name is not null && name.StartsWith("HotelBooking.Modules.", StringComparison.Ordinal))
                .Cast<string>()
                .ToArray();

            Assert.True(
                referencedModuleAssemblies.Length == 0,
                $"{moduleAssembly.GetName().Name} references module implementation(s): " +
                string.Join(", ", referencedModuleAssemblies));
        }
    }

    [Fact]
    public void Domain_types_do_not_depend_on_forbidden_layers_or_module_implementations()
    {
        foreach (Assembly moduleAssembly in ModuleAssemblies)
        {
            string moduleAssemblyName = moduleAssembly.GetName().Name!;
            Type[] domainTypes = moduleAssembly
                .GetTypes()
                .Where(type => type.Namespace?.Contains(".Domain", StringComparison.Ordinal) == true)
                .ToArray();

            foreach (Type domainType in domainTypes)
            {
                string[] forbiddenDependencies = GetContractTypes(domainType)
                    .Select(type => type.Assembly.GetName().Name)
                    .Where(name => IsForbiddenDomainDependency(name, moduleAssemblyName))
                    .Distinct(StringComparer.Ordinal)
                    .Cast<string>()
                    .ToArray();

                Assert.True(
                    forbiddenDependencies.Length == 0,
                    $"{domainType.FullName} depends on forbidden assembly(ies): " +
                    string.Join(", ", forbiddenDependencies));
            }
        }
    }

    [Fact]
    public void Hotel_rate_settings_cannot_be_created_outside_pricing()
    {
        MethodInfo? publicFactory = typeof(HotelRateSettings).GetMethod(
            "Create",
            BindingFlags.Public | BindingFlags.Static);

        Assert.Null(publicFactory);
    }

    [Fact]
    public void Nightly_price_has_no_public_creation_interface()
    {
        ConstructorInfo[] publicConstructors = typeof(NightlyPrice).GetConstructors(
            BindingFlags.Public | BindingFlags.Instance);
        MethodInfo[] publicFactories = typeof(NightlyPrice)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(method => method.ReturnType == typeof(NightlyPrice))
            .ToArray();

        Assert.Empty(publicConstructors);
        Assert.Empty(publicFactories);
    }

    [Fact]
    public void Query_and_api_contracts_do_not_expose_domain_types()
    {
        Assembly apiAssembly = typeof(HotelBooking.Api.ApiAssembly).Assembly;
        IEnumerable<Type> contractTypes = ModuleAssemblies
            .Append(apiAssembly)
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type =>
                type.Assembly == apiAssembly ||
                type.Namespace?.Contains(".Queries", StringComparison.Ordinal) == true);

        foreach (Type contractType in contractTypes)
        {
            Type[] exposedDomainTypes = GetPublicContractTypes(contractType)
                .Where(IsDomainType)
                .Distinct()
                .ToArray();

            Assert.True(
                exposedDomainTypes.Length == 0,
                $"{contractType.FullName} exposes domain type(s): " +
                string.Join(", ", exposedDomainTypes.Select(type => type.FullName)));
        }
    }

    private static IEnumerable<Type> GetContractTypes(Type type)
    {
        const BindingFlags allMembers =
            BindingFlags.Instance |
            BindingFlags.Static |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.DeclaredOnly;

        IEnumerable<Type> directTypes =
            type.GetInterfaces()
                .Append(type.BaseType)
                .Concat(type.GetFields(allMembers).Select(field => field.FieldType))
                .Concat(type.GetProperties(allMembers).Select(property => property.PropertyType))
                .Concat(type.GetConstructors(allMembers).SelectMany(constructor =>
                    constructor.GetParameters().Select(parameter => parameter.ParameterType)))
                .Concat(type.GetMethods(allMembers).SelectMany(method =>
                    method.GetParameters()
                        .Select(parameter => parameter.ParameterType)
                        .Append(method.ReturnType)))
                .Where(referencedType => referencedType is not null)
                .Cast<Type>();

        return directTypes.SelectMany(ExpandType);
    }

    private static IEnumerable<Type> GetPublicContractTypes(Type type)
    {
        const BindingFlags publicMembers =
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.DeclaredOnly;

        IEnumerable<Type> directTypes = type
            .GetProperties(publicMembers)
            .Select(property => property.PropertyType)
            .Concat(type.GetMethods(publicMembers).SelectMany(method =>
                method.GetParameters()
                    .Select(parameter => parameter.ParameterType)
                    .Append(method.ReturnType)));

        return directTypes.SelectMany(ExpandType);
    }

    private static IEnumerable<Type> ExpandType(Type type)
    {
        yield return type;

        if (type.HasElementType && type.GetElementType() is { } elementType)
        {
            foreach (Type expandedType in ExpandType(elementType))
            {
                yield return expandedType;
            }
        }

        foreach (Type genericArgument in type.GetGenericArguments())
        {
            foreach (Type expandedType in ExpandType(genericArgument))
            {
                yield return expandedType;
            }
        }
    }

    private static bool IsForbiddenDomainDependency(string? assemblyName, string moduleAssemblyName)
    {
        if (assemblyName is null || assemblyName == moduleAssemblyName)
        {
            return false;
        }

        return assemblyName.StartsWith("HotelBooking.Modules.", StringComparison.Ordinal) ||
               assemblyName is "HotelBooking.Api" or "HotelBooking.Worker" ||
               assemblyName.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal) ||
               assemblyName.StartsWith("MassTransit", StringComparison.Ordinal);
    }

    private static bool IsDomainType(Type type)
    {
        return type.Namespace?.Contains(".Domain", StringComparison.Ordinal) == true;
    }
}
