using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System.Web;
using System.Web.SessionState;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace MongoSessionStateStore.Helpers
{
    public static class MongoSessionUserHelpers
    {
        public static T getObjValue<T>(object sessionObj)
        {
            if (sessionObj == null)
                return default(T);

            if (sessionObj is T)
                return (T)sessionObj;

            if (sessionObj is BsonDocument)
                return (T)BsonSerializer.Deserialize<T>(sessionObj as BsonDocument);

            if (sessionObj is BsonValue)
                return (T)BsonTypeMapper.MapToDotNetValue(sessionObj as BsonValue);

            if (sessionObj is Newtonsoft.Json.Linq.JObject)
                return ((JObject)sessionObj).ToObject<T>();

            if (sessionObj is JArray)
            {
                var jarr = sessionObj as JArray;
                if (typeof(T) == typeof(BsonDocument))
                {
                    var doc = new BsonDocument();

                    foreach (var item in jarr)
                    {
                        doc.Add((string)item["Name"], BsonValue.Create(((JValue)item["Value"]).Value));
                    }

                    return (T)(object)doc;
                }
                else
                {
                    return jarr.ToObject<T>();
                }
            }
            

            return default(T);
        }
    }
}

namespace System.Web.Mvc
{
    public static class MongoSessionUserHelpersMvc
    {
        /// <summary>
        /// Gets the session value stored in MongoDB.
        /// </summary>
        /// <typeparam name="T">Type of the value to get.</typeparam>
        /// <param name="session">HttpSessionStateBase object for MVC pages.</param>
        /// <param name="key">The session key name.</param>
        /// <returns>The object requested. Null if not exists.</returns>
        public static T Mongo<T>(
            this HttpSessionStateBase session,
            string key)
        {
            var sessionObj = session[key];
            return MongoSessionStateStore.Helpers.MongoSessionUserHelpers.getObjValue<T>(sessionObj);
        }

        /// <summary>
        /// Sets the session value to store in MongoDB.
        /// </summary>
        /// <typeparam name="T">Type of value to store.</typeparam>
        /// <param name="session">HttpSessionStateBase object for MVC pages.</param>
        /// <param name="key">The session key name.</param>
        /// <param name="newValue">The value to store.</param>
        public static void Mongo<T>(
            this HttpSessionStateBase session,
            string key,
            T newValue)
        {
            session[key] = newValue;
        }
    }
}

namespace System.Web
{
    public static class MongoSessionUserHelpersWeb
    {
        /// <summary>
        /// Gets the session value stored in MongoDB.
        /// </summary>
        /// <typeparam name="T">Type of the value to get.</typeparam>
        /// <param name="session">HttpSessionState object for WebForm pages.</param>
        /// <param name="key">The session key name.</param>
        /// <returns>The object requested. Null if not exists.</returns>
        public static T Mongo<T>(
            this HttpSessionState session,
            string key)
        {
            var sessionObj = session[key];
            return MongoSessionStateStore.Helpers.MongoSessionUserHelpers.getObjValue<T>(sessionObj);
        }

        /// <summary>
        /// Sets the session value to store in MongoDB.
        /// </summary>
        /// <typeparam name="T">Type of value to store.</typeparam>
        /// <param name="session">HttpSessionState object for WebForm pages.</param>
        /// <param name="key">The session key name.</param>
        /// <param name="newValue">The value to store.</param>
        public static void Mongo<T>(
            this HttpSessionState session,
            string key,
            T newValue)
        {
            session[key] = newValue;
        }
    }
}