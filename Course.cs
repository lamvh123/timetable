using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    class Course
    {
        public int classId { get; set; }
        public List<int> dayOfWeek { get; set; }
        public String startDate { get; set; }
        public int slot { get; set; }
    }
}
