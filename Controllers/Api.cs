using NEL.NNS.lib;
using NEL_FutureDao_API.RPC;
using NEL_FutureDao_API.Service;
using Newtonsoft.Json.Linq;
using System;

namespace NEL_FutureDao_API.Controllers
{
    public class Api
    {
        private string netnode { get; set; }
        // services
        private DaoService daoService;

        //
        private HttpHelper hh = new HttpHelper();
        private MongoHelper mh = new MongoHelper();
        //
        private static Api testApi = new Api("testnet");
        private static Api mainApi = new Api("mainnet");
        public static Api getTestApi() { return testApi; }
        public static Api getMainApi() { return mainApi; }
        private Monitor monitor;

        public Api(string node) {
            netnode = node;
            switch (netnode)
            {
                case "testnet":
                    daoService = new DaoService
                    {
                        mh = mh,
                        mongodbConnStr = mh.notify_mongodbConnStr_testnet,
                        mongodbDatabase = mh.notify_mongodbDatabase_testnet
                    };
                    break;
                case "mainnet":
                    daoService = new DaoService
                    {
                        mh = mh,
                        mongodbConnStr = mh.notify_mongodbConnStr_mainnet,
                        mongodbDatabase = mh.notify_mongodbDatabase_mainnet
                    };
                    break;
            }

            initMonitor();
        }

        public object getRes(JsonRPCrequest req, string reqAddr)
        {
            JArray result = new JArray();
            string resultStr = string.Empty;
            string findFliter = string.Empty;
            string sortStr = string.Empty;
            try
            {
                point(req.method);
                switch (req.method)
                {
                    // 获取服务列表
                    case "getServiceList":
                        result = daoService.getServiceList(int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()));
                        break;
                    // 获取治理信息列表(治理)
                    case "getVoteTxHistList":
                        result = daoService.getVoteTxHistList(req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()));
                        break;
                    // 获取治理信息(治理)
                    case "getVoteInfo":
                        result = daoService.getVoteInfo(req.@params[0].ToString());
                        break;
                    // 获取项目交易历史列表(众筹)
                    case "getProjTxHistList":
                        result = daoService.getProjTxHistList(req.@params[0].ToString(),int.Parse(req.@params[1].ToString()),int.Parse(req.@params[2].ToString()));
                        break;
                    // 获取项目信息(众筹)
                    case "getProjInfo":
                        result = daoService.getProjInfo(req.@params[0].ToString());
                        break;
                    // 存储治理信息(治理)
                    case "storeVoteInfo":
                        result = daoService.storeVoteInfo(req.@params[0].ToString(), req.@params[1].ToString(), req.@params[2].ToString(), req.@params[3].ToString(), req.@params[4].ToString(), req.@params[5].ToString(), req.@params[6].ToString());
                        break;
                    // 存储项目信息(众筹)
                    case "storeProjInfo":
                        result = daoService.storeProjInfo(req.@params[0].ToString(), req.@params[1].ToString(), req.@params[2].ToString(), req.@params[3].ToString(), req.@params[4].ToString(), req.@params[5].ToString(), req.@params[5].ToString());
                        break;
                    
                    // test
                    case "getnodetype":
                        result = new JArray { new JObject { { "nodeType", netnode } } };
                        break;
                }
                if (result.Count == 0)
                {
                    JsonPRCresponse_Error resE = new JsonPRCresponse_Error(req.id, -1, "No Data", "Data does not exist");
                    return resE;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("errMsg:{0},errStack:{1}", e.Message, e.StackTrace);
                JsonPRCresponse_Error resE = new JsonPRCresponse_Error(req.id, -100, "Parameter Error", e.Message);
                return resE;
            }

            JsonPRCresponse res = new JsonPRCresponse();
            res.jsonrpc = req.jsonrpc;
            res.id = req.id;
            res.result = result;

            return res;
        }

        private void initMonitor()
        {
            string startMonitorFlag = mh.startMonitorFlag;
            if (startMonitorFlag == "1")
            {
                monitor = new Monitor();
            }
        }
        private void point(string method)
        {
            if (monitor != null)
            {
                monitor.point(netnode, method);
            }
        }
    }
}
