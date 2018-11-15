using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
 
    public struct FaceExpression {
        String eyeExpression;
        String upperFaceExpression;
        String lowerFaceExpression;
        float upperFaceExpressionPower;
        float lowerFaceExpressionPower;
    }


    #region JSon Structures for Emotive Cortex API.

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
    #endregion

}
