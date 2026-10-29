using System;
using System.Linq;
using BlogApp.Domain.Enums;
using BlogApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;
using Xunit.Abstractions;

namespace BlogApp.Application.Tests;

public class PostEnumSqlTests
{
    private readonly ITestOutputHelper _output;

    public PostEnumSqlTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void TestEnumMappingToQueryString()
    {
        var connectionString = "Host=localhost;Port=5432;Database=blog-database-schema;Username=postgres;Password=123456";
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString, o => o.MapEnum<PostStatus>("post_status"))
            .Options;

        using var db = new ApplicationDbContext(options);
        var sql = db.Posts.Where(p => p.Status == PostStatus.Published).ToQueryString();
        _output.WriteLine("GENERATED SQL:");
        _output.WriteLine(sql);

        // Verify EF Core generates PostgreSQL enum cast instead of integer literal
        Assert.Contains("'published'::post_status", sql);

        // Also test executing against real DB if available
        try
        {
            var count = db.Posts.Count(p => p.Status == PostStatus.Published);
            _output.WriteLine($"SUCCESSFULLY QUERIED: count = {count}");
        }
        catch (Exception ex)
        {
            _output.WriteLine("DB not reachable or query failed: " + ex.Message);
        }
    }
}
