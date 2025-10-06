// list vms see Notepad v1
public class ProductRowVM { public int Id; public string Title = ""; public string Category = ""; public decimal Price; }
public class ProductListVM { public IEnumerable<ProductRowVM> Items { get; set; } = new List<ProductRowVM>(); public string? Search { get; set; } }

