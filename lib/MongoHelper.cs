using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using MongoDB.Bson.IO;

namespace NEL.NNS.lib
{
    public class MongoHelper
    {
        public string notify_mongodbConnStr_testnet = string.Empty;
        public string notify_mongodbDatabase_testnet = string.Empty;
        public string notify_mongodbConnStr_mainnet = string.Empty;
        public string notify_mongodbDatabase_mainnet = string.Empty;
        //
        public string isStartRechargeFlag = string.Empty;
        public string isStartApplyGasFlag = string.Empty;
        public string startMonitorFlag = string.Empty;

        public MongoHelper()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection()    //将配置文件的数据加载到内存中
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())   //指定配置文件所在的目录
                .AddJsonFile("mongodbsettings.json", optional: true, reloadOnChange: true)  //指定加载的配置文件
                .Build();    //编译成对象  
            //
            notify_mongodbConnStr_testnet = config["notify_mongodbConnStr_testnet"];
            notify_mongodbDatabase_testnet = config["notify_mongodbDatabase_testnet"];
            notify_mongodbConnStr_mainnet = config["notify_mongodbConnStr_mainnet"];
            notify_mongodbDatabase_mainnet = config["notify_mongodbDatabase_mainnet"];
            
            //
            startMonitorFlag = config["startMonitorFlag"];

        }

        public JArray GetData(string mongodbConnStr, string mongodbDatabase, string coll, string findBson)
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);

            List<BsonDocument> query = collection.Find(BsonDocument.Parse(findBson)).ToList();
            client = null;

            if (query.Count > 0)
            {
                var jsonWriterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.Strict };
                JArray JA = JArray.Parse(query.ToJson(jsonWriterSettings));
                foreach (JObject j in JA)
                {
                    j.Remove("_id");
                }
                return JA;
            }
            else { return new JArray(); }
        }

        public JArray GetDataPages(string mongodbConnStr, string mongodbDatabase, string coll, string sortStr, int pageSize, int pageNum, string findBson = "{}")
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);

            List<BsonDocument> query = collection.Find(BsonDocument.Parse(findBson)).Sort(sortStr).Skip(pageSize * (pageNum - 1)).Limit(pageSize).ToList();
            client = null;

            if (query.Count > 0)
            {
                var jsonWriterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.Strict };
                JArray JA = JArray.Parse(query.ToJson(jsonWriterSettings));
                foreach (JObject j in JA)
                {
                    j.Remove("_id");
                }
                return JA;
            }
            else { return new JArray(); }
        }

        public JArray GetDataWithField(string mongodbConnStr, string mongodbDatabase, string coll, string fieldBson, string findBson = "{}")
        {
            var client = mongodbConnStr == null ? new MongoClient() : new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);

            List<BsonDocument> query = collection.Find(BsonDocument.Parse(findBson)).Project(BsonDocument.Parse(fieldBson)).ToList();
            client = null;

            if (query.Count > 0)
            {
                var jsonWriterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.Strict };
                JArray JA = JArray.Parse(query.ToJson(jsonWriterSettings));
                foreach (JObject j in JA)
                {
                    j.Remove("_id");
                }
                return JA;
            }
            else { return new JArray(); }
        }

        public JArray GetDataPagesWithField(string mongodbConnStr, string mongodbDatabase, string coll, string fieldBson, int pageCount, int pageNum, string sortBson = "{}", string findBson = "{}")
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);

            List<BsonDocument> query = collection.Find(BsonDocument.Parse(findBson)).Project(BsonDocument.Parse(fieldBson)).Sort(sortBson).Skip(pageCount * (pageNum - 1)).Limit(pageCount).ToList();
            client = null;

            if (query.Count > 0)
            {
                var jsonWriterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.Strict };
                JArray JA = JArray.Parse(query.ToJson(jsonWriterSettings));
                foreach (JObject j in JA)
                {
                    j.Remove("_id");
                }
                return JA;
            }
            else { return new JArray(); }
        }

        public long GetDataCount(string mongodbConnStr, string mongodbDatabase, string coll, string findBson = "{}")
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);

            var txCount = collection.Find(BsonDocument.Parse(findBson)).CountDocuments();

            client = null;

            return txCount;
        }

        public string InsertData(string mongodbConnStr, string mongodbDatabase, string coll, string insertBson)
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);
            try
            {
                var query = collection.Find(BsonDocument.Parse(insertBson)).ToList();
                if (query.Count == 0)
                {
                    BsonDocument bson = BsonDocument.Parse(insertBson);
                    collection.InsertOne(bson);
                }
                client = null;
                return "suc";
            }
            catch (Exception e)
            {
                return e.ToString();
            }

        }

        public string DeleteData(string mongodbConnStr, string mongodbDatabase, string coll, string deleteBson)
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);
            try
            {
                var query = collection.Find(BsonDocument.Parse(deleteBson)).ToList();
                if (query.Count != 0)
                {
                    BsonDocument bson = BsonDocument.Parse(deleteBson);
                    collection.DeleteOne(bson);
                }
                client = null;
                return "suc";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public void UpdateData(string mongodbConnStr, string mongodbDatabase, string coll, string updateBson, string findBson)
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);
            try
            {


                collection.UpdateOne(BsonDocument.Parse(findBson), BsonDocument.Parse(updateBson));
            }
            catch (Exception e)
            {
                Console.Write("" + e);
            }
            //collection.UpdateOne(BsonDocument.Parse(Jcondition), BsonDocument.Parse(Jdata));

            client = null;
        }

        public string ReplaceData(string mongodbConnStr, string mongodbDatabase, string coll, string replaceBson, string whereBson)
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);
            try
            {
                List<BsonDocument> query = collection.Find(whereBson).ToList();
                if (query.Count > 0)
                {
                    collection.ReplaceOne(BsonDocument.Parse(whereBson), BsonDocument.Parse(replaceBson));
                    client = null;
                    return "suc";
                }
                else
                {
                    client = null;
                    return "no data";
                }
            }
            catch (Exception e)
            {
                client = null;
                return e.ToString();
            }
        }

        public List<string> listCollection(string mongodbConnStr, string mongodbDatabase)
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            List<string> list = database.ListCollectionNames().ToList();
            client = null;

            return list;
        }


        public JArray GetDataNew(string mongodbConnStr, string mongodbDatabase, string coll, string findBson, string fieldBson = "{'_id':0}")
        {
            return GetDataOrigin(mongodbConnStr, mongodbDatabase, coll, findBson, "{}", 0, 0, fieldBson);
        }
        public JArray GetDataNewPages(string mongodbConnStr, string mongodbDatabase, string coll, string findBson, string sortBson, int skip, int limit, string fieldBson = "{'_id':0}")
        {
            return GetDataOrigin(mongodbConnStr, mongodbDatabase, coll, findBson, sortBson, skip, limit, fieldBson);
        }
        private JArray GetDataOrigin(string mongodbConnStr, string mongodbDatabase, string coll, string findBson, string sortBson, int skip, int limit, string fieldBson)
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);

            List<BsonDocument> query = null;
            if (limit == 0)
            {
                query = collection.Find(BsonDocument.Parse(findBson)).Project(BsonDocument.Parse(fieldBson)).ToList();
            }
            else
            {
                query = collection.Find(BsonDocument.Parse(findBson)).Project(BsonDocument.Parse(fieldBson)).Sort(BsonDocument.Parse(sortBson)).Skip(skip).Limit(limit).ToList();
            }
            client = null;

            if (query.Count > 0)
            {
                return JArray.Parse(query.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.Strict }));
            }
            else { return new JArray(); }
        }

        //
        private string countFilterStr = new JObject { { "$group", new JObject { { "_id", 1 }, { "sum", new JObject { { "$sum", 1 } } } } } }.ToString();

        public long AggregateCount(string mongodbConnStr, string mongodbDatabase, string coll, IEnumerable<string> collection)
        {
            var res = Aggregate(mongodbConnStr, mongodbDatabase, coll, collection, true);
            if (res != null && res.Count > 0)
            {
                return long.Parse(res[0]["sum"].ToString());
            }
            return 0;
        }

        public JArray Aggregate(string mongodbConnStr, string mongodbDatabase, string coll, IEnumerable<string> collection, bool isGetCount = false)
        {
            IList<IPipelineStageDefinition> stages = new List<IPipelineStageDefinition>();
            foreach (var item in collection)
            {
                stages.Add(new JsonPipelineStageDefinition<BsonDocument, BsonDocument>(item));
            }
            if (isGetCount)
            {
                stages.Add(new JsonPipelineStageDefinition<BsonDocument, BsonDocument>(countFilterStr));
            }
            PipelineDefinition<BsonDocument, BsonDocument> pipeline = new PipelineStagePipelineDefinition<BsonDocument, BsonDocument>(stages);
            var queryRes = Aggregate(mongodbConnStr, mongodbDatabase, coll, pipeline);
            if (queryRes != null && queryRes.Count > 0)
            {
                return JArray.Parse(queryRes.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.Strict }));
            }
            return new JArray { };
        }

        public List<BsonDocument> Aggregate(string mongodbConnStr, string mongodbDatabase, string coll, PipelineDefinition<BsonDocument, BsonDocument> pipeline)
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);
            var query = collection.Aggregate(pipeline).ToList();

            client = null;
            return query;
        }

    }
}
