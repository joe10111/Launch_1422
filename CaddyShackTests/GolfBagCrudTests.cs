using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System.Net;
using CaddyShackMVC.Models;
using CaddyShackMVC.DataAccess;
using CaddyShackTests.FeatureTests;

namespace CaddyShackTests
{
    [Collection("Controller Tests")]
    public class GolfBagCrudTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public GolfBagCrudTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        private CaddyShackContext GetDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<CaddyShackContext>();
            optionsBuilder.UseInMemoryDatabase("TestDatabase");

            var context = new CaddyShackContext(optionsBuilder.Options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            return context;
        }

        [Fact]
        public async Task Test_Index_ReturnsViewWithGolfBags()
        {
            // Arange
            var context = GetDbContext();
            context.GolfBags.Add(new GolfBag { Player = "Joe", Capacity = 10 });
            context.GolfBags.Add(new GolfBag { Player = "Jim", Capacity = 8 });
            context.SaveChanges();

            // Act
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/golfbags");
            var html = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains("Joe", html);
            Assert.Contains("10", html);

            Assert.Contains("Jim", html);
            Assert.Contains("8", html);

            Assert.DoesNotContain("James", html);
        }

        [Fact]
        public async Task Test_New_ReturnsFormView()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/golfbags/new");
            var html = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Contains("Add a Golf Bag", html);
            Assert.Contains("form method=\"post\" action=\"/golfbags\"", html);
        }

        [Fact]
        public async Task Test_AddGolfBag_ReturnsRedirect_ToShow()
        {
            var context = GetDbContext();

            // Arrange
            var client = _factory.CreateClient();
            var formData = new Dictionary<string, string>
            {
                { "Player", "Joe" },
            };

            // Act
            var response = await client.PostAsync("/golfbags", new FormUrlEncodedContent(formData));
            var html = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("GolfBag Details", html);
            Assert.Contains("Player: Joe", html);
        }

        [Fact]
        public async Task Test_Edit_ReturnsFormView_PrePopulated()
        {
            // Arrange
            var context = GetDbContext();
            var client = _factory.CreateClient();

            var testedGolfBag = new GolfBag { Player = "Joe", Capacity = 10 };

            context.GolfBags.Add(testedGolfBag);
            context.SaveChanges();

            // Act
            var response = await client.GetAsync($"/golfbags/{testedGolfBag.Id}/edit");
            var html = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains("Edit GolfBag", html);
            Assert.Contains(testedGolfBag.Player, html);
            Assert.Contains(testedGolfBag.Capacity.ToString(), html);
        }

        [Fact]
        public async Task Test_Update_SavesChangesTo_GolfBag()
        {
            // Arrange
            var context = GetDbContext();
            var client = _factory.CreateClient();

            var testedGolfBag = new GolfBag { Player = "Joe", Capacity = 10 };

            context.GolfBags.Add(testedGolfBag);
            context.SaveChanges();

            var formData = new Dictionary<string, string>
            {
                { "Player", "Joe" },
            };

            // Act
            var response = await client.PostAsync(
                $"/golfbags/{testedGolfBag.Id}",
                new FormUrlEncodedContent(formData)
            );
            var html = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();

            Assert.Contains("Joe", html);
            Assert.DoesNotContain("Jim", html);
        }

        [Fact]
        public async Task Test_Delete_RemovesGolfBag_FromIndexPage()
        {
            // Arrange
            var context = GetDbContext();
            var client = _factory.CreateClient();

            var testedGolfBag = new GolfBag { Player = "Joe", Capacity = 10 };
            context.GolfBags.Add(testedGolfBag);
            context.SaveChanges();

            // Act
            var response = await client.PostAsync($"/golfbags/delete/{testedGolfBag.Id}", null);
            var html = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.DoesNotContain("Joe", html);
        }
    }
}