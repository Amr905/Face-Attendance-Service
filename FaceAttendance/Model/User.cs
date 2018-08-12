using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceAttendance.Model
{
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Face Face { get; set; }

        public User(string id, string name)
        {
            this.Id = id;
            this.Name = name;

        }

    }
}
