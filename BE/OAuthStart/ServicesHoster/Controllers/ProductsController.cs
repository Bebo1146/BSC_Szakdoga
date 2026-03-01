using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ServicesHoster.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private static readonly List<ProductDto> Products = new()
        {
            new("p-1001", "Yellow Sweater Set", "Active", "Set",      "15 in stock for 2 variants", "S.laz Store"),
            new("p-1002", "Merraid Top",        "Active", "Tops",     "8 in stock for 2 variants",  "S.laz Store"),
            new("p-1003", "Summer Bag",         "Active", "Handbag",  "25 in stock for 2 variants", "S.laz Store"),
            new("p-1004", "Lizzy Jacket",       "Active", "Jackets",  "5 in stock for 3 variants",  "S.laz Store"),
            new("p-1005", "Stripes Trousers",   "Active", "Trousers", "2 in stock for 2 variants",  "S.laz Store"),
            new("p-1006", "Sunny Sweeter",      "Draft",  "Top",      "5 in stock for 2 variants",  "S.laz Store"),
            new("p-1007", "Linen Shirt",        "Active", "Tops",     "10 in stock for 2 variants", "S.laz Store"),
        };

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(Products);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            var product = Products.FirstOrDefault(p => p.Id == id);
            return product is null ? NotFound() : Ok(product);
        }

        [HttpGet("/health")]
        public IActionResult Health()
        {
            return Ok("OK");
        }
    }

    public record ProductDto(
    string Id,
    string Name,
    string Status,
    string Category,
    string InventoryText,
    string Vendor);
}
