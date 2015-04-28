using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestApplicationv2_0.Models;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;

namespace TestApplicationv2_0.Controllers
{
    public class DefaultController : Controller
    {
        //
        // GET: /Default/

        public ActionResult Index()
        {
            Session["value"] = "Hi";
            return View();
        }

        public ActionResult PrintSessionVal()
        {
            string val = (Session["value"] == null) ? "" : Session["value"].ToString();
            ViewBag.sessionVal = val;
            return View();
        }

        public ActionResult PrintSessionValDouble()
        {
            var val = (double)Session["value"];
            ViewBag.sessionVal = val.ToString("G");
            return View("~/Views/Default/PrintSessionVal.aspx");
        }

        public ActionResult SetSessionValInt(int newSesVal = 0)
        {
            Session["value"] = newSesVal;
            return View("~/Views/Default/SetSessionVal.aspx");
        }

        public ActionResult SetSessionValBool(bool newSesVal = false)
        {
            Session["value"] = newSesVal;
            return View("~/Views/Default/SetSessionVal.aspx");
        }

        public ActionResult SetSessionValDouble()
        {
            double newSesVal = 3.1416F;
            Session["value"] = newSesVal;
            return View("~/Views/Default/SetSessionVal.aspx");
        }

        public ActionResult SetSessionVal(string newSesVal = "")
        {
            Session["value"] = newSesVal;
            return View();
        }

        public ActionResult SessionAbandon()
        {
            Session.Abandon();
            return View();
        }

        public ActionResult SerializePerson(
            string name = "Marc",
            string surname = "Cortada",
            string city = "Barcelona")
        {
            Person p = new Person()
            {
                Name = name,
                Surname = surname,
                City = city
            };

            Session["person"] = p;

            return View();
        }

        public ActionResult GetSerializedPerson()
        {
            Person p = new Person();
            if (Session["person"] != null)
            {
                if (Session["person"] is BsonDocument)
                {
                    var obj = Session["person"] as BsonDocument;
                    if (obj != null)
                        p = BsonSerializer.Deserialize<Person>(obj);
                }
                else if (Session["person"] is JObject)
                {
                    var obj = Session["person"] as JObject;
                    if (obj != null) {
                        p = obj.ToObject<Person>();
                    }
                }
            }
            return View(p);
        }

        public ActionResult SerializePersonWithLists(
            string name = "Marc",
            string surname = "Cortada",
            string city = "Barcelona")
        {
            PersonPetsList p = new PersonPetsList()
            {
                Name = name,
                Surname = surname,
                City = city,
                PetsList = new List<string>() { "Dog", "Cat", "Shark" }
            };

            Session["personWithPetsList"] = p;

            return View();
        }

        public ActionResult GetSerializedPersonWithPets()
        {
            PersonPetsList p = new PersonPetsList();
            if (Session["personWithPetsList"] != null)
            {
                if (Session["personWithPetsList"] is BsonDocument)
                {
                    var obj = Session["personWithPetsList"] as BsonDocument;
                    if (obj != null)
                        p = BsonSerializer.Deserialize<PersonPetsList>(obj);
                }
                else if (Session["personWithPetsList"] is JObject)
                {
                    var obj = Session["personWithPetsList"] as JObject;
                    if (obj != null)
                    {
                        p = obj.ToObject<PersonPetsList>();
                    }
                }
            }
            return View(p);
        }

    }
}
