using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.SessionState;

namespace MongoSessionStateStore
{
    internal static class MongoSessionStateStoreHelpers
    {
        internal static string GetDocumentSessionId(
            string sessionId,
            string applicationName)
        {
            return string.Format("{0}-{1}", sessionId, applicationName);
        }

        internal static BsonDocument GetNewBsonSessionDocument(
            string id,
            string applicationName,
            DateTime created,
            DateTime lockDate,
            int lockId,
            int timeout,
            bool locked,
            BsonArray jsonSessionItemsArray = null,
            int flags = 1)
        {
            return new BsonDocument
                {
                    {"_id", GetDocumentSessionId(id, applicationName)},
                    {"SessionID", id},
                    {"ApplicationName", applicationName},
                    {"Created", created},
                    {"Expires", DateTime.Now.AddMinutes(timeout).ToUniversalTime()},
                    {"LockDate", lockDate},
                    {"LockId", lockId},
                    {"Timeout", timeout},
                    {"Locked", locked},
                    {"SessionItemJSON", jsonSessionItemsArray},
                    {"Flags", flags}
                };
        }

        /// <summary>
        /// Creates TTL index if does not exist in collection.
        /// TTL index will remove the expired session documents.
        /// </summary>
        internal static bool CreateTTLIndex(
            IMongoCollection<BsonDocument> sessionCollection)
        {
            while (true)
            {
                try
                {
                    var idx = Builders<BsonDocument>.IndexKeys.Ascending("Expires");
                    var option = new CreateIndexOptions();
                    option.ExpireAfter = TimeSpan.Zero;
                    var task = sessionCollection.Indexes.CreateOneAsync(idx, option);
                    task.Wait();
                    return true;
                }
                catch (Exception)
                {
                    // if index is not created, not retries. App can continue without index but
                    // you should create it or clear the documents manually
                    return false;
                }
            }
        }

        internal static BsonDocument FindOneSessionItem(
            this MongoSessionStateStore obj,
            IMongoCollection<BsonDocument> sessionCollection,
            FilterDefinition<BsonDocument> q)
        {
            int nAtempts = 0;
            while (true)
            {
                try
                {
                    var task = sessionCollection.Find(q).FirstOrDefaultAsync();
                    task.Wait();
                    return task.Result;
                }
                catch (Exception e)
                {
                    PauseOrThrow(ref nAtempts, obj, sessionCollection, e);
                }
            }
        }

        internal static UpdateResult UpdateSessionCollection(
            this MongoSessionStateStore obj,
            IMongoCollection<BsonDocument> sessionCollection,
            FilterDefinition<BsonDocument> query,
            UpdateDefinition<BsonDocument> update)
        {
            int attempts = 0;
            while (true)
            {
                try
                {
                    var task = sessionCollection.UpdateManyAsync(query, update);
                    task.Wait();
                    return task.Result;
                }
                catch (Exception e)
                {
                    PauseOrThrow(ref attempts, obj, sessionCollection, e);
                }
            }
        }

        internal static DeleteResult DeleteSessionDocument(
           this MongoSessionStateStore obj,
           IMongoCollection<BsonDocument> sessionCollection,
           FilterDefinition<BsonDocument> query)
        {
            int attempts = 0;
            while (true)
            {
                try
                {
                    var task = sessionCollection.DeleteManyAsync(query);
                    task.Wait();

                    return task.Result;
                }
                catch (Exception e)
                {
                    PauseOrThrow(ref attempts, obj, sessionCollection, e);
                }
            }
        }

        internal static void UpsertEntireSessionDocument(
            this MongoSessionStateStore obj,
            IMongoCollection<BsonDocument> sessionCollection,
            BsonDocument insertDoc)
        {
            int attempts = 0;
            while (true)
            {
                try
                {
                    var task = sessionCollection.InsertOneAsync(insertDoc);
                    task.Wait();

                    if (task.IsFaulted)
                    {
                        throw new Exception("retry!");
                    }

                    return;
                }
                catch (Exception e)
                {
                    PauseOrThrow(ref attempts, obj, sessionCollection, e);
                }
            }
        }

        private static void PauseOrThrow(
            ref int attempts,
            MongoSessionStateStore obj,
            IMongoCollection<BsonDocument> sessionCollection,
            Exception e)
        {
            if (attempts < obj.MaxUpsertAttempts)
            {
                attempts++;
                System.Threading.Thread.CurrentThread.Join(obj.MsWaitingForAttempt);
            }
            else
            {
                throw new ProviderException(MongoSessionStateStore.EXCEPTION_MESSAGE);
            }
        }

        internal static BsonArray Serialize(SessionStateStoreData item)
        {
            BsonArray arraySession = new BsonArray();
            for (int i = 0; i < item.Items.Count; i++)
            {
                string key = item.Items.Keys[i];
                arraySession.Add(new BsonDocument(key, Newtonsoft.Json.JsonConvert.SerializeObject(item.Items[key])));
            }
            return arraySession;
        }

        internal static SessionStateStoreData Deserialize(
            HttpContext context,
            BsonArray serializedItems,
            int timeout)
        {
            var jSonSessionItems = new SessionStateItemCollection();
            foreach (var value in serializedItems.Values)
            {
                var document = value as BsonDocument;
                string name = document.Names.FirstOrDefault();
                string JSonValues = document.Values.FirstOrDefault().AsString;
                jSonSessionItems[name] = Newtonsoft.Json.JsonConvert.DeserializeObject(JSonValues);
            }

            return new SessionStateStoreData(jSonSessionItems,
              SessionStateUtility.GetSessionStaticObjects(context),
              timeout);
        }


        /// <summary>
        /// NOT used. It's preserved for future implementations.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        /// <param name="action"></param>
        /// <param name="eventType"></param>
        internal static void WriteToEventLog(
            this MongoSessionStateStore obj,
            Exception e,
            string action,
            EventLogEntryType eventType = EventLogEntryType.Error)
        {
            if (obj.WriteExceptionsToEventLog)
            {
                using (var log = new EventLog())
                {
                    log.Source = MongoSessionStateStore.EVENT_SOURCE;
                    log.Log = MongoSessionStateStore.EVENT_LOG;

                    string message =
                      String.Format("An exception occurred communicating with the data source.\n\nAction: {0}\n\nException: {1}",
                      action, e);

                    log.WriteEntry(message, eventType);
                }
            }
        }
    }
}
