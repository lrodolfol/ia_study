//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using System.Net;
using System.Net.Http.Json;
using Application.DTOs.Categories;
using Application.DTOs.Common;

namespace pigMoney.Tests.Integration;

public class CategoriesIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CategoriesIntegrationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostCategory_ShouldReturn201()
    {
        var request = new CreateCategoryRequest("Food", "Groceries");

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/categories", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        CategoryResponse? created = await response.Content.ReadFromJsonAsync<CategoryResponse>();
        Assert.NotNull(created);
        Assert.Equal("Food", created.Name);
    }

    [Fact]
    public async Task GetCategoryById_WhenNotFound_ShouldReturn404()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/v1/categories/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCategories_ShouldReturn200WithPagination()
    {
        var request = new CreateCategoryRequest("Transport", "Bus and metro");
        await _client.PostAsJsonAsync("/api/v1/categories", request);

        HttpResponseMessage response = await _client.GetAsync("/api/v1/categories?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        PaginatedList<CategoryResponse>? result = await response.Content.ReadFromJsonAsync<PaginatedList<CategoryResponse>>();
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task PutCategory_ShouldReturn200()
    {
        var createRequest = new CreateCategoryRequest("Original", "Old desc");
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/v1/categories", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        CategoryResponse? created = await createResponse.Content.ReadFromJsonAsync<CategoryResponse>();

        var updateRequest = new UpdateCategoryRequest("Updated", "New desc");
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/categories/{created!.Id}", updateRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        CategoryResponse? updated = await response.Content.ReadFromJsonAsync<CategoryResponse>();
        Assert.Equal("Updated", updated!.Name);
    }

    [Fact]
    public async Task DeleteCategory_WhenNoDependents_ShouldReturn204()
    {
        var createRequest = new CreateCategoryRequest("ToDelete", "Remove me");
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/v1/categories", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        CategoryResponse? created = await createResponse.Content.ReadFromJsonAsync<CategoryResponse>();

        HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/categories/{created!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
