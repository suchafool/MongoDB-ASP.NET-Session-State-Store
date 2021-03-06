﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestApplicationv2_0.Models;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using Newtonsoft.Json;
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
            var val = Session.Mongo<double>("value");
            double dobVal = (double)BsonTypeMapper.MapToDotNetValue(val);
            ViewBag.sessionVal = dobVal.ToString("G");
            return View("~/Views/Default/PrintSessionVal.cshtml");
        }

        public ActionResult SetSessionValInt(int newSesVal = 0)
        {
            Session["value"] = newSesVal;
            return View("~/Views/Default/SetSessionVal.cshtml");
        }

        public ActionResult SetSessionValBool(bool newSesVal = false)
        {
            Session["value"] = newSesVal;
            return View("~/Views/Default/SetSessionVal.cshtml");
        }

        public ActionResult SetSessionValDouble()
        {
            double newSesVal = 3.1416F;
            Session["value"] = newSesVal;
            return View("~/Views/Default/SetSessionVal.cshtml");
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
                var value = Session["person"];
                if (value is BsonDocument)
                {
                    var obj = value as BsonDocument;

                    if (obj != null)
                        p = BsonSerializer.Deserialize<PersonPetsList>(obj);
                }
                else if (value is JObject)
                {
                    var obj = value as JObject;
                    p = obj.ToObject<PersonPetsList>();
                }
                else
                {
                    Response.Write(value);
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
                var value = Session["personWithPetsList"];
                if (value is BsonDocument)
                {
                    var obj = value as BsonDocument;

                    if (obj != null)
                        p = BsonSerializer.Deserialize<PersonPetsList>(obj);
                }
                else if (value is JObject)
                {
                    var obj = value as JObject;
                    p = obj.ToObject<PersonPetsList>();
                }
                else
                {
                    Response.Write(value);
                }
            }
            return View(p);
        }

    }
}
