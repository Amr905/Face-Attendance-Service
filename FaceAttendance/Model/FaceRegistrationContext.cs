using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceAttendance.Model
{
    public class FaceRegistrationContext
    {
        public string AppId { get; set; }

        public Context Context { get; set; }

        public List<User> Users { get; set; }

        public FaceRegistrationContext(string appID, Context context)
        {
            this.AppId = appID;
            this.Context = context;
            Users = new List<User>();
        }
    }
}
