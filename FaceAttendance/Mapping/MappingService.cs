using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount  
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types  
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FaceAttendance.Mapping
{
    public static class MappingService
    {
        public static async Task
            CreateTable()
        {
            //connect to storage account name ,key
            CloudStorageAccount StorageAccount = GetCloudStorageAccount();
            CloudTableClient TableClient = StorageAccount.CreateCloudTableClient();
            CloudTable MappingTable = TableClient.GetTableReference("mappingTable");
            await MappingTable.CreateIfNotExistsAsync();

        }
        public static CloudStorageAccount GetCloudStorageAccount()
        {
            return new CloudStorageAccount(
                new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
                "functionapp778fe8", "zf7JXe/C4NZMoyqsmiGeGKzeLBwl+vNo1GI8KqxUg2h7eDbJl/YdaQpGe/Y3EfIibXvmBsWGVF3RAOa10mvpQw=="), true);
        }

        public static async Task InsertNewPersonGroup(string AppID, string ContextID, string PersonGroupID)
        {
            TableBatchOperation BatchOperation = new TableBatchOperation();
            MappingEntity ContextMapping = new MappingEntity(AppID, ContextID);
            ContextMapping.Value = PersonGroupID;

            MappingEntity PersonGroupMapping = new MappingEntity(AppID, PersonGroupID);
            PersonGroupMapping.Value = ContextID;


            BatchOperation.Insert(ContextMapping);
            BatchOperation.Insert(PersonGroupMapping);

            CloudStorageAccount StorageAccount = GetCloudStorageAccount();
            CloudTableClient TableClient = StorageAccount.CreateCloudTableClient();
            CloudTable MappingTable = TableClient.GetTableReference("mappingTable");//table name 
            MappingTable.CreateIfNotExists();
            await MappingTable.ExecuteBatchAsync(BatchOperation);

        }
        public static async Task<IList<TableResult>> InsertNewPerson(string AppID, string ContextID, string PersonID, string UserID)
        {
            TableBatchOperation BatchOperation = new TableBatchOperation();
            MappingEntity UserMapping = new MappingEntity(AppID + "-" + ContextID, UserID);
            UserMapping.Value = PersonID;

            MappingEntity PersonGroupPersonMapping = new MappingEntity(AppID + "-" + ContextID, PersonID);
            PersonGroupPersonMapping.Value = UserID;

            BatchOperation.Insert(UserMapping);
            BatchOperation.Insert(PersonGroupPersonMapping);

            CloudStorageAccount StorageAccount = GetCloudStorageAccount();
            CloudTableClient TableClient = StorageAccount.CreateCloudTableClient();
            CloudTable MappingTable = TableClient.GetTableReference("mappingTable");//table name 
            MappingTable.CreateIfNotExists();
           return await MappingTable.ExecuteBatchAsync(BatchOperation);

        }

        public static Boolean IsPersonGroupExist(string AppID, string ContexID)
        {
            //if true then it exist
            string Result = GetFromMapping(AppID, ContexID);

            if (Result == "")
                return false;
            return true;
        }

        public static string GetFromMapping(string Partition, string Row)
        {
            CloudStorageAccount storageAccount = GetCloudStorageAccount();
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            //CloudTable mappingTable = tableClient.GetTableReference("mappingTable");//table name
            //TableOperation retrieveOperation = TableOperation.Retrieve<MappingEntity>(Partition, Row);
            //TableResult retrievedResult = mappingTable.Execute(retrieveOperation);
            //MappingEntity ResultEntity = (MappingEntity)retrievedResult.Result;

            CloudTable MappingTable = tableClient.GetTableReference("mappingTable");

            TableOperation retrieveOperation = TableOperation.Retrieve<MappingEntity>(Partition, Row);
            MappingTable.CreateIfNotExists();
            TableResult Query = MappingTable.Execute(retrieveOperation);
            MappingEntity ResultEntity = (MappingEntity)Query.Result;
            string Result = "";
            if (ResultEntity != null)
                Result = ResultEntity.Value;
            return Result;
        }
        public static async Task CreateBlobContainer()
        {
            // Create storagecredentials object by reading the values from the configuration (appsettings.json)
            //   StorageCredentials storageCredentials = new StorageCredentials(_storageConfig.AccountName, _storageConfig.AccountKey);

            // Create cloudstorage account by passing the storagecredentials
            CloudStorageAccount storageAccount = GetCloudStorageAccount();// new CloudStorageAccount(storageCredentials, true);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get a reference to a container named "my-new-container."
            CloudBlobContainer container = blobClient.GetContainerReference("groupimages");

            // If "mycontainer" doesn't exist, create it.
            await container.CreateIfNotExistsAsync();

        }

    }
}

