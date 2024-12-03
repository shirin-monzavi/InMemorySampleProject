using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace InMemorySampleProject.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EmployeeController : ControllerBase
{
    private const string employeeListCacheKey1 = "employeeList1";
    private const string employeeListCacheKey2 = "employeeList2";
    private MyMemoryCache _cache;
    private ILogger<EmployeeController> _logger;
    public EmployeeController(
        MyMemoryCache cache,
        ILogger<EmployeeController> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        List<Employee> employees1 = new List<Employee>();
        List<Employee> employees2 = new List<Employee>();

        if (_cache.Cache.TryGetValue(employeeListCacheKey1, out employees1) ||
            _cache.Cache.TryGetValue(employeeListCacheKey2, out employees2)
            )
        {
            employees1.AddRange(employees2.AsEnumerable());

            return Ok(employees1);
        }

        _logger.Log(LogLevel.Information, "Employee list not found in cache. Fetching from database.");

        employees1 = GetEmployees1();
        employees2 = GetEmployees2();

        var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                .SetPriority(CacheItemPriority.Normal)
                .SetSize(4);

        _cache.Cache.Set(employeeListCacheKey1, employees1, cacheEntryOptions);
        _cache.Cache.Set(employeeListCacheKey2, employees2, cacheEntryOptions);

        employees1.AddRange(employees2.AsEnumerable());

        return Ok(employees1);
    }

    private List<Employee> GetEmployees1()
    {
        return new List<Employee>()
        {
            new Employee(){Id = 1,Name="shirin1"},
            new Employee(){Id = 2,Name="shirin2"}
        };
    }

    private List<Employee> GetEmployees2()
    {
        return new List<Employee>()
        {
            new Employee(){Id = 3,Name="shirin3"},
            new Employee(){Id = 4,Name="shirin4"}
        };
    }
}
