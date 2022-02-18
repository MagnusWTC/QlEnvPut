using Microsoft.Extensions.Configuration;
using QLEnvPut.Response;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using static QLEnvPut.Pages.Home;

namespace QLEnvPut.Service
{
    public class CommitService
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory clientFactory;

        public CommitService(IConfiguration configuration,IHttpClientFactory httpClientFactory)
        {
            this.configuration = configuration;
            this.clientFactory = httpClientFactory;
        }
        public bool UpdateCK(Model model)
        {
            
            try
            {
                var pin = "pt_pin=.+?;";
                var key = "pt_key=.+?;";
                bool res = false;
                string mapCkStr = "";
                var matchPin = Regex.Match(model.Env, pin);
                var matchKey = Regex.Match(model.Env, key);
                if (!matchPin.Success || !matchPin.Success) {
                    throw new Exception("ck格式不正确，需包含pt_pin和pt_key");
                }
                mapCkStr = matchKey.Value + matchPin.Value;
                var ClientId = configuration.GetSection("QlConfig:ClientId").Value;
                var ClientSecret = configuration.GetSection("QlConfig:ClientSecret").Value;
                var Qlhost = configuration.GetSection("QlConfig:QlHost").Value;
                using (var client = clientFactory.CreateClient())
                {
                    var token = "";
                    var tokenUrl = $"{Qlhost}/open/auth/token?client_id={ClientId}&client_secret={ClientSecret}";
                    var data = client.GetFromJsonAsync<TokenResponse>(tokenUrl).ConfigureAwait(false).GetAwaiter().GetResult();
                    if (data.code == 200)
                    {
                        token = data.data.token;
                    }
                    //获取到token 获取环境变量
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                    var dd = client.GetStringAsync($"{Qlhost}/open/envs").ConfigureAwait(false).GetAwaiter().GetResult();
                    var envs = client.GetFromJsonAsync<EnvsResponse>($"{Qlhost}/open/envs").ConfigureAwait(false).GetAwaiter().GetResult();
                    if (envs.code == 200)
                    {
                        string[] values = mapCkStr.Split(';');
                        var pt_pin = values.FirstOrDefault(m => m.Contains("pt_pin"));
                        var id = GetEnvByPtIn(pt_pin, envs.data);
                        var needUpdateEnv = envs.data.FirstOrDefault(x => x._id == id);
                        if (needUpdateEnv != null)
                        {
                            needUpdateEnv.value = mapCkStr.Trim();
                            needUpdateEnv.status = 0;
                        }
                        //update 
                        var responseMessage = client.PutAsJsonAsync($"{Qlhost}/open/envs", new { value = needUpdateEnv.value, name = needUpdateEnv.name, remarks = needUpdateEnv.remarks, _id = needUpdateEnv._id }).ConfigureAwait(false).GetAwaiter().GetResult();
                        if (responseMessage.IsSuccessStatusCode)
                        {
                            //启用账号
                            var response = client.PutAsJsonAsync($"{Qlhost}/open/envs/enable", new string[] { needUpdateEnv._id }).ConfigureAwait(false).GetAwaiter().GetResult();
                            if (response.IsSuccessStatusCode)
                            {
                                res = true;
                            }

                        } 
                    }

                }
                return res;

            }
            catch (Exception ex)
            {
                throw;
            }

        }


        /// <summary>
        /// 获取ck id
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        private string GetEnvByPtIn(string pt_pin, Envs[] envs)
        {
            if (envs != null)
            {

                foreach (var item in envs)
                {
                    if (item.value.Contains(pt_pin))
                    {
                        return item._id;
                    }

                }
            }

            return null;

        }

    }
}
