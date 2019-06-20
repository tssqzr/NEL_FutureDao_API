﻿using NEL.NNS.lib;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace NEL_FutureDao_API.Service
{
    public class DaoService
    {
        public MongoHelper mh { get; set; }
        public string mongodbConnStr { get; set; }
        public string mongodbDatabase { get; set; }

        private string projInfoCol = "futureDao_ProjInfo";
        private string voteInfoCol = "futureDao_VoteInfo";
        private string ethPriceStateCol = "ethPriceState";
        private string ethVoteStateCol = "ethVoteState";

        // 发布项目
        public JArray storeProjInfo(string address, string hash, string creator, string projName, string projDetail, string projTeam, string revengePlan)
        {
            string findStr = new JObject { { "address", address}, {"hash", hash },{ "projName", projName} }.ToString();
            if (mh.GetDataCount(mongodbConnStr, mongodbDatabase, projInfoCol, findStr) == 0)
            {
                var newdata = new JObject {
                    { "hash", hash},
                    { "address", address},
                    { "creator", creator},
                    { "projName", projName},
                    { "projDetail", projDetail},
                    { "projTeam", projTeam},
                    { "revengePlan", revengePlan},
                    { "time", TimeHelper.GetTimeStamp()}
                }.ToString();
                mh.InsertData(mongodbConnStr, mongodbDatabase, projInfoCol, newdata);
                return new JArray { new JObject { { "res", true } } };
            }
            return new JArray { new JObject { { "res", false } } };
        }
        // 发布治理
        public JArray storeVoteInfo(string address, string hash, string voter, string name, string summary, string detail)
        {
            string findStr = new JObject { { "address", address }, { "hash", hash }, { "name", name } }.ToString();
            if(mh.GetDataCount(mongodbConnStr, mongodbDatabase, voteInfoCol, findStr) == 0)
            {
                var newdata = new JObject {
                    { "hash", hash},
                    { "address", address},
                    { "name", name},
                    { "summary", summary},
                    { "detail", detail},
                    { "state", VoteState.Voting},
                    { "time", TimeHelper.GetTimeStamp()}
                }.ToString();
                mh.InsertData(mongodbConnStr, mongodbDatabase, voteInfoCol, newdata);
                return new JArray { new JObject { { "res", true } } };
            }
            return new JArray { new JObject { { "res", false } } };
        }
        // 投票

        // 显示
        // 显示.交易详情
        public JArray getProjInfo(string hash)
        {
            //发起人/已筹资金/ 目标资金/ 剩余时间 /总参与人数/ 已售出股份数
            string findStr = new JObject { { "hash", hash } }.ToString();
            string fieldStr = new JObject { { "address",1} }.ToString();
            var queryRes = mh.GetDataWithField(mongodbConnStr, mongodbDatabase, ethPriceStateCol, fieldStr, findStr);
            int joinCount = 0;
            if(queryRes != null && queryRes.Count > 0)
            {
                joinCount = queryRes.Select(p => p["address"].ToString()).Distinct().Count();
            }
            return new JArray { new JObject { { "joinCount", joinCount } } };
        }
        public JArray getProjTxHistList(string hash, int pageNum=1, int pageSize=10)
        {
            //time/txid/height/address/eventName/ethAmount/fndAmount
            string findStr = new JObject { { "hash", hash} }.ToString();
            string sortStr = new JObject { { "blocktime", -1} }.ToString();
            var queryRes = mh.GetDataPages(mongodbConnStr, mongodbDatabase, ethPriceStateCol, sortStr, pageSize, pageNum, findStr);
            if (queryRes == null || queryRes.Count == 0) return new JArray {};

            var count = mh.GetDataCount(mongodbConnStr, mongodbDatabase, ethPriceStateCol, findStr);

            return new JArray { new JObject { { "count", count},{ "list", queryRes} } };
        }

        // 显示.治理详情
        public JArray getVoteInfo(string hash)
        {
            //发起人/已筹资金/ 治理资金/ 启动时间 /总参与人数/ 已售出股份数
            string findStr = new JObject { { "hash", hash } }.ToString();
            string fieldStr = new JObject { { "address", 1 } }.ToString();
            var queryRes = mh.GetDataWithField(mongodbConnStr, mongodbDatabase, ethPriceStateCol, fieldStr, findStr);
            int joinCount = 0;
            if (queryRes != null && queryRes.Count > 0)
            {
                joinCount = queryRes.Select(p => p["address"].ToString()).Distinct().Count();
            }
            return new JArray { new JObject { { "joinCount", joinCount } } };
        }
        public JArray getVoteTxHistList(string hash, int pageNum=1, int pageSize=10)
        {
            //名称/所需时间/地址/状态<投票中>/申请金额/支持数量/反对数量
            string findStr = new JObject { {"hash", hash} }.ToString();
            string sortStr = new JObject { { "startTime", -1 } }.ToString();
            string fieldStr = new JObject { { "_id",0},
                { "proposalName", 1 },
                { "timeConsuming", 1 },
                { "proposer",1 },
                { "proposalState", 1 },
                { "value", 1 },
                { "voteYesCount", 1 },
                { "voteNotCount", 1 }
            }.ToString();
            var queryRes = mh.GetDataNewPages(mongodbConnStr, mongodbDatabase, ethVoteStateCol, findStr, sortStr, (pageNum - 1) * pageSize, pageSize, fieldStr); ;
            if (queryRes == null || queryRes.Count == 0) return new JArray { };

            var count = mh.GetDataCount(mongodbConnStr, mongodbDatabase, ethVoteStateCol, findStr);
            return new JArray { new JObject { {"count", count },{ "list", queryRes} } };
        }

        // 显示.查询服务列表
        public JArray getServiceList(int pageNum=1, int pageSize=10)
        {
            string findStr = "{}";
            string sortStr = new JObject { { "time", -1} }.ToString();
            string fieldStr = new JObject { {"hash", 1},{ "perFrom24h",1 },{ "_id",0} }.ToString();
            var queryRes = mh.GetDataPagesWithField(mongodbConnStr, mongodbDatabase, ethPriceStateCol, fieldStr, pageSize, pageNum, sortStr, findStr);
            if (queryRes == null || queryRes.Count == 0) return new JArray { };

            var count = mh.GetDataCount(mongodbConnStr, mongodbDatabase, ethPriceStateCol, findStr);
            return new JArray { new JObject { { "count", count }, { "list", queryRes } } };
        }
    }

    class VoteState
    {
        public const int Voting = 0;  // 投票中
        public const int Executing = 1;// 执行中
        public const int End = 2; // 结束
    }
}
