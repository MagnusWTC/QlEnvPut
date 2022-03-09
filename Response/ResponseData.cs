using System;

namespace QLEnvPut.Response
{
    public class ResponseData<T>where T:class
    {

        public int code { get; set; }


        public T data { get; set; }
    }


    public class Token {
        public string token { get; set; }
        public string token_type { get; set; }
        public long expiration { get; set; }

    }

    public class Envs
    {
        public int id { get; set; }
        public string value { get; set; }
        public string timestamp { get; set; }
        public int status { get; set; }
        public float position { get; set; }
        public string name { get; set; }
        public string remarks { get; set; }
        public long created { get; set; }


    }


    public class TokenResponse : ResponseData<Token>
    { 
    
    }

    public class EnvsResponse : ResponseData<Envs>
    { 
        public new Envs[] data { get; set; }
    }
}
