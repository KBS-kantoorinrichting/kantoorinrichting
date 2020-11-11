using System;
using System.Collections.Generic;
using System.Text;

namespace OfficeDesigner.Model
{
    public class Institution
    {
        public int InstitutionId { get; set; }
        public string Name { get; set; }

        public List<User> Users { get; set; }

        public List<Space> Spaces { get; set; }
    }
}
