using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceAttendance.Model
{
    public class Face
    {
        public string FaceID { get; set; }
        public Location Location;
        public Face(string faceID, int top, int left, int width, int height)
        {
            FaceID = faceID;
            Location = new Location(top, left, width, height);
        }

    }
}
