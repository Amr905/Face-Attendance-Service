using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FaceAttendance.Model
{
    
    public class Error 
    {
        public string code { get; set; }

        public string message { get; set; }
        public Error() { }
    }
    
    public class ErrorMsg : Exception, ISerializable

    {
        public Error error { set; get; }
        public ErrorMsg(SerializationInfo info, StreamingContext context) { }
        public ErrorMsg()
        {
            error = new Error();
        }
        public ErrorMsg(string code ,string message)
        {
            error = new Error();
            error.code = code;
            error.message = message;
        }
        public ErrorMsg(string message)
        : base(message)
        {

        }

        public ErrorMsg(string message, Exception inner)
         : base(message, inner)
        {
        }
    }
}
