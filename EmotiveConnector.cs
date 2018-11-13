﻿using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
   
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.WebSockets;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

using UnityEngine.UI;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;

public class EmotiveConnector : MonoBehaviour {
    public static TextMesh tm;

    private static object consoleLock = new object();
    private const int sendChunckSize = 1024;
    private const int recieveChunckSize = 1024;
    private const bool verbose = true;
    private static readonly TimeSpan delay = TimeSpan.FromMilliseconds(3000);
    private static ClientWebSocket wsc = null;
    private static string auth;
    private static string session;
    private static string userid = "";
    private static string id = "";
    private static string lastMethod = "";
    private static string currentHeadset = "";
    
    static UTF8Encoding encoder = new UTF8Encoding();



    // Use this for initialization and retrive auth, sessio, headset, 
   void Start() {
        // System.Diagnostics.Process.Start("mozroots.exe", "--import");
        System.Net.ServicePointManager.CertificatePolicy = new NoCheckCertificatePolicy();
        tm = GameObject.FindObjectOfType<TextMesh>();// (typeof(Text));
        connect("wss://emotivcortex.com:54321"); //.ConfigureAwait(false);



        //setSimulatorText("init");
        // getUserLogin();
        Task.Run(async () => { await Authorize(); }).Wait();
       
        
       // setSimulatorText("authorize");
      // Task queryHeadset();
        Task.Run(async () => { await queryHeadset(); }).Wait();
        // setSimulatorText("query headset");
        // createSession();
        Task.Run(async () => { await createSession(); }).Wait();
        setSimulatorText("create session");
        Task.Run(async () => { await getSessions(); }).Wait();
        Subscribe();
        //setSimulatorText("subdcribe");
    }
  public static  void setSimulatorText(String message) {
       // tm.text = message;
       // Debug.Log(message);
    }
	// Update is called once per frame
	void Update () {
		
	}
    async Task subscription() {
        try
        {
            byte[] buffer = new byte[recieveChunckSize];
            var mstream = new MemoryStream();


                while (webSocket.State == WebSocketState.Open)
                {

                    Console.WriteLine("Recieve:");
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.EndOfMessage)
                    {
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, String.Empty, CancellationToken.None);
                        }
                        else if (result.MessageType == WebSocketMessageType.Text)
                        {

                            String resString = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            setSimulatorText(resString);

                            if (result.EndOfMessage)
                            {
                                JObject rj = JObject.Parse(resString);
                                Console.WriteLine("Recieved:" + resString);



                                if (rj["error"] != null)
                                {
                                    Console.WriteLine("Error in request");
                                    setSimulatorText("recieve: Error in request");
                                }
                                else
                                {
                                    String queryVal = "";
                                    if (rj["result"] != null)
                                    {
                                        setSimulatorText("Recieve: Result:..");
                                        //dynamic resArr = rj["result"];
                                        /*Dictionary<string, string> jreso = resArr.ToObject<Dictionary<string, string>>();
                                        */

                                        {
                                            JObject res = (JObject)rj["result"];
                                            var itm = res["_auth"].ToObject<String>();

                                            lastMethod = "";
                                            auth = itm;
                                            Console.WriteLine(auth);
                                        }
                                        /*  if (rj.ContainsKey("id"))
                                          {
                                              //  userid = rj["id"].ToString();
                                          }
                                          */

                                        if (lastMethod == "queryHeadsets")
                                        {
                                            JArray res = (JArray)rj["result"];
                                            var itm = from c in res["id"].Values<string>()
                                                      group c by c into g
                                                      select new { idents = g.Key };
                                            foreach (var id in itm)
                                            {
                                                Console.WriteLine(id.idents);
                                                currentHeadset = id.idents;
                                            }
                                            lastMethod = "";
                                        }

                                    }
                                }
                            }
                            else
                            {
                                Debug.LogError("Recieved: " + result.ToString());
                                Console.WriteLine("Status: Recieved..:" + result.ToString());

                            }
                        }
                    }
                }
            
          
        }
        catch (Exception e) { }
    }

    //Function to send and recieve. 
    async Task SendRecieve(String JSonSendString) {
        // var jobs = new List<Task>();
        try
        {
            ArraySegment<byte> recievesegment = new ArraySegment<byte>(new byte[8096]);
            MemoryStream memStream = new MemoryStream();
            
            byte[] buffer = encoder.GetBytes(JSonSendString);
            await wsc.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None).ConfigureAwait(false);
            WebSocketReceiveResult wsrr;// = new WebSocketReceiveResult();
            do
            {
                
                wsrr = await wsc.ReceiveAsync(recievesegment, CancellationToken.None);
                memStream.Write(recievesegment.Array, recievesegment.Offset, recievesegment.Count);
                // Task.Delay(1000);
            } while (!wsrr.EndOfMessage);
            if (memStream.Length > 0 && wsrr.MessageType == WebSocketMessageType.Text) {
                memStream.Seek(0, SeekOrigin.Begin);
                String tresult = new StreamReader(memStream, Encoding.UTF8).ReadToEnd(); ;
                int ind = tresult.IndexOf('\0');
                tresult = tresult.Substring(0, ind);
                parseRecievedInitJson(tresult);

            }

        }
        catch (Exception ea) {
            Debug.Log("SendRecieve:" +ea.Message+"...."+ea.StackTrace); }
    }

    void parseRecievedInitJson(String recieved)
    {
        JObject recievedObj = JObject.Parse(recieved);


        if (recievedObj["result"] != null)
        {

            JProperty jpres = null;
            foreach (JProperty jpr in recievedObj.Properties())
            {
                if (jpr.Name == "result")
                {
                    jpres = jpr;
                    break;

                }

            }

            JProperty jpro = null;
            foreach (JToken jp in jpres.ToList<JToken>())
            {

                if (jp.First != null)
                {
                    switch (jp.First.Type.ToString())
                    {
                        case "Object": { break; }
                        case "Property":
                            {
                                jpro = jp.First.ToObject<JProperty>();
                                break;
                            }


                    }


                    if (jpro != null)
                    {
                        if (jpro.Name == "_auth")
                        {
                            auth = jpro.Value.ToString();
                        }
                        else if (jpro.Name == "method") {
                            Debug.unityLogger.Log("method");
                        }
                    }


                }


            }
        }
    }
    // Setup a websocket 
    //public static async Task connect(string uri)
    public void connect(string uri)
    {
        try
        {
            wsc = new ClientWebSocket();
            setSimulatorText("Connecting");
            wsc.ConnectAsync(new Uri(uri), CancellationToken.None).ConfigureAwait(true);
            Debug.Log("Connected" + wsc.State.ToString());
            //Wait for the websocket to change state into open
            while (wsc.State == WebSocketState.Connecting) { Task.Delay(100); }
            //    { 
            /*await*/
            //Task.WhenAll(Receive(wsc), Send(wsc)).ConfigureAwait(true);
            //  }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString() + e.Message);
            Debug.Log("Send: " + e.ToString() + e.Message);
        }
        finally
        {
            if (wsc != null && (wsc.State == WebSocketState.CloseReceived || wsc.State == WebSocketState.CloseSent))
            {
                wsc.Dispose();
                lock (consoleLock) { Console.WriteLine("WebSocket closed"); }
            }
        }
        }
        private static async Task Send(ClientWebSocket webSocket)
        {
        try
        {
            byte[] buffer = new byte[sendChunckSize];
            //encoder.GetBytes("{\"op\":\"unconfirmed_sub\"}");
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine("Status: " + "Send");
            while (webSocket.State == WebSocketState.Open)
            {
                await Task.Delay(delay);
            }
        }
        catch (Exception ee) {
            Debug.Log("Send: " + ee.ToString());
                }
        }
    /*
        private static async Task Send(String sendString)
        {
        try
        {
            ArraySegment<byte> recievesegment = new ArraySegment<byte>();
            byte[] buffer = encoder.GetBytes(sendString);
            await wsc.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None).ConfigureAwait(false);
                WebSocketReceiveResult wsrr = await wsc.ReceiveAsync(recievesegment, CancellationToken.None) ;
            sendString = null;
            buffer = null;
        }
        catch (Exception esr) {
            Debug.Log("SendString:"+esr.Message+"..."+esr.ToString()+"Send JSON: "+sendString); }
        }
        */
        private static async Task Receive(ClientWebSocket webSocket)
        {
            byte[] buffer = new byte[recieveChunckSize];
            var mstream = new MemoryStream();

            try
            {

                while (webSocket.State == WebSocketState.Open)
                {

                    Console.WriteLine("Recieve:");
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.EndOfMessage)
                    {
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, String.Empty, CancellationToken.None);
                        }
                        else if (result.MessageType == WebSocketMessageType.Text)
                        {

                            String resString = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        setSimulatorText(resString);

                            if (result.EndOfMessage)
                            {
                                JObject rj = JObject.Parse(resString);
                                Console.WriteLine("Recieved:" + resString);



                                if (rj["error"] != null) { Console.WriteLine("Error in request");
                                setSimulatorText("recieve: Error in request");
                            }
                                else
                                {
                                    String queryVal = "";
                                    if (rj["result"] != null)
                                    {
                                    setSimulatorText("Recieve: Result:..");
                                    //dynamic resArr = rj["result"];
                                    /*Dictionary<string, string> jreso = resArr.ToObject<Dictionary<string, string>>();
                                    */
                                    
                                            {
                                                JObject res = (JObject)rj["result"];
                                                var itm = res["_auth"].ToObject<String>();

                                                lastMethod = "";
                                                auth = itm;
                                                Console.WriteLine(auth);
                                            }
                                          /*  if (rj.ContainsKey("id"))
                                            {
                                                //  userid = rj["id"].ToString();
                                            }
                                            */

                                            if (lastMethod == "queryHeadsets")
                                            {
                                                JArray res = (JArray)rj["result"];
                                                var itm = from c in res["id"].Values<string>()
                                                          group c by c into g
                                                          select new { idents = g.Key };
                                                foreach (var id in itm)
                                                {
                                                    Console.WriteLine(id.idents);
                                                    currentHeadset = id.idents;
                                                }
                                                lastMethod = "";
                                            }
                                        
                                    }
                                }
                            }
                            else
                            {
                            Debug.LogError("Recieved: " + result.ToString());
                                Console.WriteLine("Status: Recieved..:" + result.ToString());

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Recieve exception:" + e.ToString());
            }
        }




   async Task getUserLogin()
        {
        try
        {
            getLogin g = new getLogin();
            g.id = "1";
            g.jsonrpc = "2.0";
            g.method = "getUserLogin";
            
            String js = JsonConvert.SerializeObject(g);
            Console.WriteLine(js);
          await SendRecieve(js);

        }
        catch (Exception gulError) {
            Debug.Log("GetUserLogin: " + gulError.ToString() + "..." + gulError.Message); }
        }

  private   async Task queryHeadset()
        {
            queryHeadsets q = new queryHeadsets();
            q.jsonrpc = "2.0";
            q.id = "1";
            q.method = "queryHeadsets";
            String js = JsonConvert.SerializeObject(q);
           // lastMethod = q.method;
        await Task.Run(async () => {await SendRecieve(js); });
        }


private async Task Authorize()
        {
        try { 
            Authorize a = new Authorize();
            a.jsonrpc = "2.0";
            a.method = "authorize";
           // lastMethod = "authorize";
            a.@params = new String[1];
            String js = JsonConvert.SerializeObject(a);
            await Task.Run(async () => { await SendRecieve(js); });
           //await SendRecieve(js);
    }
        catch (Exception galError) {
            Debug.Log("Authorze: " + galError.ToString() + "..." + galError.Message); }
        }

       async void Subscribe()
        {
  
            JObject se = new JObject();
            JObject param = new JObject(new JProperty("_auth", auth));
            JArray strea = new JArray("fac");
            param.Add("streams", strea);
            se.Add(new JProperty("id", "1"));
            se.Add("jsonrpc", "2.0");
            //  se.Add(new JProperty("_auth", auth));
            // se.Add(new JProperty("stream", "fac"));
            se.Add(new JProperty("method", "subscribe"));
            se.Add("params", param);

            String rJson = JsonConvert.SerializeObject(se);
            Console.WriteLine("subscribe:" + rJson);
           await SendRecieve(rJson);
        }
       async  Task createSession()
        {
            JObject jsess = new JObject();
            JObject param = new JObject(new JProperty("_auth", auth), new JProperty("status", "open"));
            jsess.Add("jsonrpc", "2.0");
            jsess.Add("method", "createSession");
            jsess.Add("params", param);
            jsess.Add("id", new JValue(1));
            String s = JsonConvert.SerializeObject(jsess);
       // await Task.Run(async () => { await SendRecieve(s); });
        await SendRecieve(s);
        }


        async Task getSessions()
        {
            JObject jsess = new JObject();
            JObject param = new JObject(new JProperty("_auth", auth));
            jsess.Add("jsonrpc", "2.0");
            jsess.Add("method", "querySessions");
            jsess.Add(new JProperty("params", param));
            jsess.Add("id", "1");
            String s = JsonConvert.SerializeObject(jsess);
        await Task.Run(async () => { await SendRecieve(s); });
        //await SendRecieve(s);
        // return s;
    }

    public bool CheckValidationResult(ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem)
    {
        setSimulatorText("validate certificate " + certificate.ToString());
        return true;
    }
    public bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        bool isOk = true;
        setSimulatorText("MyRemote validate certificate " + certificate.ToString());
        return isOk;
    }




}

struct getLogin
    {
        public String jsonrpc;
        public String method;
        public String id;
    }
    struct getCurrentUserData
    {
        public String userid;
        public String id;
    }
    struct userCredidentals
    {
        public String userid;
        public String password;
    }
    struct userLogOut
    {
        public String userId;
        public String id;
        public String jsonrpc;
        public String method;
        public String[] @params;
    }
    struct Authorize
    {
        public String jsonrpc;
        public String method;
        public String[] @params;
    }
    struct queryHeadsets
    {
        public String jsonrpc;
        public String method;
        public String[] @params;
        public String id;
    }
    struct AnonymousAuthorize
    {
        public String jsonrpc;
        public String method;
        public String[] @params;
        public String id;
    }
    struct SubscribeToStream
    {

        public String jsonrpc;
        public String method;
        public String[] @params;//= new String[]{  _auth, streams };
        public String id;

    }
    struct getSession
    {
        public String jsonrpc;
        public String method;
        public String id;

    }




class unsafeCertificatePolicy : System.Net.ICertificatePolicy
{
    public bool CheckValidationResult(ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem)
    {
        return true;
    }
}



