using Microsoft.AspNetCore.Mvc;
using TripPlanner.Domain.Entities;
using TripPlanner.Web.Controllers;

namespace TripPlanner.Tests.Controllers;

public class CitiesControllerTests
{
    // GET: повертає всі міста
    [Fact]
    public async Task GetCities_ReturnsAllCities()
    {
        var context = TestDbHelper.CreateContext("GetCities_All");
        TestDbHelper.SeedBasicData(context);
        var controller = new CitiesController(context);

        var result = await controller.GetCities(null);

        var cities = Assert.IsAssignableFrom<IEnumerable<City>>(result.Value);
        Assert.Equal(2, cities.Count());
    }

    // GET: фільтрація за категорією
    [Fact]
    public async Task GetCities_FilterByCategory_ReturnsFiltered()
    {
        var context = TestDbHelper.CreateContext("GetCities_Filter");
        TestDbHelper.SeedBasicData(context);
        var controller = new CitiesController(context);

        var result = await controller.GetCities("Столиця");

        var cities = Assert.IsAssignableFrom<IEnumerable<City>>(result.Value);
        Assert.Single(cities);
        Assert.Equal("Київ", cities.First().Name);
    }

    // GET by id: повертає місто
    [Fact]
    public async Task GetCity_ExistingId_ReturnsCity()
    {
        var context = TestDbHelper.CreateContext("GetCity_Exists");
        TestDbHelper.SeedBasicData(context);
        var controller = new CitiesController(context);

        var result = await controller.GetCity(1);

        Assert.Equal("Київ", result.Value.Name);
    }

    // GET by id: неіснуючий id повертає 404
    [Fact]
    public async Task GetCity_NonExistingId_ReturnsNotFound()
    {
        var context = TestDbHelper.CreateContext("GetCity_NotFound");
        TestDbHelper.SeedBasicData(context);
        var controller = new CitiesController(context);

        var result = await controller.GetCity(999);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    // POST: створює нове місто
    [Fact]
    public async Task PostCity_ValidData_ReturnsCreated()
    {
        var context = TestDbHelper.CreateContext("PostCity_Valid");
        var controller = new CitiesController(context);

        var city = new City { Name = "Одеса", Description = "Перлина моря", Category = "Курортне" };
        var result = await controller.PostCity(city);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedCity = Assert.IsType<City>(created.Value);
        Assert.Equal("Одеса", returnedCity.Name);
        Assert.Equal(1, context.Cities.Count());
    }

    // POST: дублікат назви повертає 409
    [Fact]
    public async Task PostCity_DuplicateName_ReturnsConflict()
    {
        var context = TestDbHelper.CreateContext("PostCity_Duplicate");
        TestDbHelper.SeedBasicData(context);
        var controller = new CitiesController(context);

        var city = new City { Name = "Київ", Description = "Дублікат" };
        var result = await controller.PostCity(city);

        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    // DELETE: неіснуючий id повертає 404
    [Fact]
    public async Task DeleteCity_NonExistingId_ReturnsNotFound()
    {
        var context = TestDbHelper.CreateContext("DeleteCity_NotFound");
        TestDbHelper.SeedBasicData(context);
        var controller = new CitiesController(context);

        var result = await controller.DeleteCity(999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    // DELETE: місто з локаціями повертає 400
    [Fact]
    public async Task DeleteCity_WithLocations_ReturnsBadRequest()
    {
        var context = TestDbHelper.CreateContext("DeleteCity_HasLocations");
        TestDbHelper.SeedBasicData(context);
        var controller = new CitiesController(context);

        var result = await controller.DeleteCity(1); // Київ має локацію Хрещатик

        Assert.IsType<BadRequestObjectResult>(result);
    }
}