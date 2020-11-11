using OfficeDesigner.Model;
using System.Data.Entity;

namespace OfficeDesigner.ViewModel
{
    public class SampleViewModel : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public SampleViewModel()
        {
            // Do your thing
        }
    }
}
