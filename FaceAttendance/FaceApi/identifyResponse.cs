using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceAttendance.FaceApi
{
    public class identifyResponse
    {
        public string FaceId;
        public List<Candidate> Candidates;
        public identifyResponse()
        {
            FaceId = "";
            Candidates = new List<Candidate>();
        }

    }
}
