using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceAttendance.Model
{
    public class FaceIdentificationContext
    {
        public string AppID { get; set; }

        public Context Context { get; set; }

        public string ImageUrl { get; set; }

        public FaceIdentificationContext(string appID, Context context)
        {
            this.AppID = appID;
            this.Context = context;
        }
    }
}
