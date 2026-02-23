namespace PuppetFestivalProject.Shared.Data;

using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class TemplateServices
{  private List<TemplateItem> _items = new List<TemplateItem>();

    public TemplateServices()
    {
        // For now, just initialize an empty list
        _items = new List<TemplateItem>();
    }

    public IEnumerable<TemplateItem> LoadItems()
    {
        // Return the in-memory list
        return _items;
    }

    public void SaveItems(IEnumerable<TemplateItem> items)
    {
        // Save to memory only for now
        _items = items.ToList();
    }
}