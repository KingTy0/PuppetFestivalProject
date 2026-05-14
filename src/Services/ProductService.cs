using Microsoft.EntityFrameworkCore;
using PuppetFestAPP.Web.Data;
//using PuppetFestAPP.Web.Models;

namespace PuppetFestAPP.Web.Services;

public class ProductService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Image)
            .Include(p => p.Category)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Image)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
