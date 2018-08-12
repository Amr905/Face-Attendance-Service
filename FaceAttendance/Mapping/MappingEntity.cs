using Microsoft.WindowsAzure.Storage.Table;

namespace FaceAttendance.Mapping
{
    public class MappingEntity : TableEntity
    {
        public MappingEntity(string partition, string row)
        {
            this.PartitionKey = partition;
            this.RowKey = row;
        }
        public string Value { get; set; }
        public MappingEntity() { }
    }
}
