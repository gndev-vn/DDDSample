using System.Reflection;
using CatalogAPI.Controllers;
using IdentityAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using OrderingAPI.Controllers;
using PaymentAPI.Controllers;

namespace DDDSample.Tests.Controllers;

public sealed class ProducesResponseTypeContractTests
{
    public static TheoryData<Type> ControllerTypes =>
    [
        typeof(CategoriesController),
        typeof(ProductsController),
        typeof(ProductVariantsController),
        typeof(AuthController),
        typeof(RolesController),
        typeof(UsersController),
        typeof(OrdersController),
        typeof(PaymentsController)
    ];

    [Theory]
    [MemberData(nameof(ControllerTypes))]
    public void Public_Actions_Define_ProducesResponseType_Metadata(Type controllerType)
    {
        var actions = controllerType
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Where(method => typeof(IActionResult).IsAssignableFrom(method.ReturnType) ||
                             typeof(Task<IActionResult>).IsAssignableFrom(method.ReturnType))
            .ToList();

        Assert.NotEmpty(actions);

        foreach (var action in actions)
        {
            var attributes = action.GetCustomAttributes<ProducesResponseTypeAttribute>(inherit: true).ToList();
            Assert.True(attributes.Count > 0,
                $"Expected {controllerType.Name}.{action.Name} to define at least one ProducesResponseTypeAttribute.");
        }
    }
}
