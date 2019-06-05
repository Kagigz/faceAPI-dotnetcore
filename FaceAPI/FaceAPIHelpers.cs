using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http;

namespace FaceAPI
{

    #region HelperClasses
    public class JsonContent : StringContent
    {
        public JsonContent(object obj) :
            base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
        {
        }
    }

    public class Rectangle
    {
        public int top;
        public int left;
        public int width;
        public int height;
    }

    public class FacialHair
    {
        public double moustache;
        public double beard;
        public double sideburns;
    }

    public class HeadPose
    {
        public double roll;
        public double yaw;
        public double pitch;
    }

    public class Emotion
    {
        public double anger;
        public double contempt;
        public double disgust;
        public double fear;
        public double happiness;
        public double neutral;
        public double sadness;
        public double surprise;
    }

    public class HairColor
    {
        public string color;
        public float confidence;
    }

    public class Hair
    {
        public double bald;
        public bool invisible;
        public List<HairColor> hairColor;
    }

    public class Makeup
    {
        public bool eyeMakeup;
        public bool lipMakeup;
    }

    public class Occlusion
    {
        public bool foreheadOccluded;
        public bool eyeOccluded;
        public bool mouthOccluded;
    }

    public class Accessory
    {
        public string type;
        public double confidence;
    }

    public class Blur
    {
        public string blurLevel { get; set; }
        public double value { get; set; }
    }

    public class Exposure
    {
        public string exposureLevel { get; set; }
        public double value { get; set; }
    }

    public class Noise
    {
        public string noiseLevel { get; set; }
        public double value { get; set; }
    }

    public class FaceAttributes
    {
        public double smile = 0f;
        public string gender = "";
        public double age = 0;
        public FacialHair facialHair = null;
        public string glasses = "";
        public Emotion emotion = null;
        public Makeup makeup = null;
        public List<Accessory> accessories = null;
        public Occlusion occlusion = null;
        public Hair hair = null;
        public HeadPose headPose = null;
        public Blur blur = null;
        public Exposure exposure = null;
        public Noise noise = null;
    }

    public class Face
    {
        public string faceId;
        public Rectangle faceRectangle;
        public FaceAttributes faceAttributes = null;
        public List<string> tags = new List<string>();
    }

    public class PersistedFace
    {
        public string persistedFaceId;
        public float confidence = 100f;
        public string userData = "";
    }

    public class LargeFaceList
    {
        public string largeFaceListId;
        public string name;
        public string userData;
        public string recognitionModel;
    }

    public class Status
    {
        public string status;
        public string createdDateTime;
        public string lastActionDateTime;
        public string lastSuccessfulTrainingDateTime;
        public string message;
    }

    #endregion

    public class FaceAPIHelpers
    {

        /// <summary>
        /// Face - Detect API: Detects faces in the given image
        /// With face attributes detected or not
        /// </summary>
        public static async Task<List<Face>> DetectFaces(string img, bool detectAttributes = false)
        {

            Console.WriteLine("\n Detecting Faces...");

            string uri = "https://westeurope.api.cognitive.microsoft.com/face/v1.0/detect?recognitionModel=recognition_02";

            string response = null;
            List<Face> results = null;

            if (detectAttributes)
            {
                //uri += "&returnFaceAttributes=age,gender,smile,facialHair,glasses,emotion,hair,makeup,occlusion,accessories,headPose,blur,noise,exposure";
                uri += "&returnFaceAttributes=age,gender,smile,facialHair,glasses,emotion,hair,makeup,occlusion,accessories";
            }

            try
            {
                response = await RequestHelpers.PostRequest(uri, new JsonContent(new
                {
                    url = img
                }));
            }
            catch(Exception e)
            {
                Console.WriteLine($"\n{e.GetType().Name}: Error detecting faces.");
                throw;
            }

            if (response != null)
            {

                try
                {
                    results = JsonConvert.DeserializeObject<List<Face>>(response);

                    Console.WriteLine($"\n{results.Count} faces detected.");

                    if (detectAttributes)
                    {
                        foreach (Face f in results)
                        {
                            CreateTags(f);
                            Console.Write("\nTags created:\n");
                            foreach(string tag in f.tags)
                            {
                                Console.Write($"{tag} ");
                            }
                            
                        }
               
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine($"\n{e.GetType().Name}: Error parsing response after detecting faces.");
                    throw;
                }

            }
            
            return results;
        }

        /// <summary>
        /// Face - Find Similar API: finds similar faces to the one given as argument in the Large Face List provided
        /// maxN is the maximum number of results
        /// </summary>
        public static async Task<List<PersistedFace>> SimilarFaces(Face face, string faceListName, int maxN = 1000)
        {
            Console.WriteLine("\n Finding similar faces...");

            string uri = "https://westeurope.api.cognitive.microsoft.com/face/v1.0/findsimilars";

            string response = null;
            List<PersistedFace> results = null;

            try
            {
                response = await RequestHelpers.PostRequest(uri, new JsonContent(new
                {
                    faceId = face.faceId,
                    largeFaceListId = faceListName,
                    maxNumOfCandidatesReturned = maxN,
                    mode = "matchFace"
                }));
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n{e.GetType().Name}: Error finding similar faces.");
                throw;
            }

            if (response != null)
            {

                try
                {
                    results = JsonConvert.DeserializeObject<List<PersistedFace>>(response);
                    Console.WriteLine($"\n{results.Count} similar faces found.");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"\n{e.GetType().Name}: Error parsing response after finding similar faces.");
                    throw;
                }

            }

            return results;
        }

        /// <summary>
        /// LargeFaceList - List API: Lists all Large Face Lists in a subscription
        /// </summary>
        public static async Task<List<LargeFaceList>> GetFaceLists()
        {
            Console.WriteLine("\n Listing Large Face Lists...");

            string uri = "https://westeurope.api.cognitive.microsoft.com/face/v1.0/largefacelists";

            string response = null;
            List<LargeFaceList> results = null;
    
            try
            {
                response = await RequestHelpers.GetRequest(uri);
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n{e.GetType().Name}: Error listing Large Face Lists.");
                throw;
            }

            if (response != null)
            {
                try
                {
                    results = JsonConvert.DeserializeObject<List<LargeFaceList>>(response);
                    Console.WriteLine($"\n{results.Count} Large Face Lists found.");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"\n{e.GetType().Name}: Error parsing response after listing Large Face Lists.");
                    throw;
                }
            }

            return results;
        }

        /// <summary>
        /// LargeFaceList - Create API: Creates a Large Face List
        /// </summary>
        public static async Task CreateFaceList(string newFaceListID,string newFaceListName, string newFaceListUserData = "")
        {
            Console.WriteLine("\n Creating Large Face List...");

            string uri = "https://westeurope.api.cognitive.microsoft.com/face/v1.0/largefacelists/"+newFaceListID;

            string response = null;

            try
            {
                response = await RequestHelpers.PutRequest(uri, new JsonContent(new
                {
                    name = newFaceListName,
                    userData = newFaceListUserData,
                    recognitionModel = "recognition_02"
                }));

                Console.WriteLine($"\n New Large Face List '{newFaceListName}' created.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n{e.GetType().Name}: Error creating Large Face List.");
                throw;
            }

        }

        /// <summary>
        /// LargeFaceList - Add Face API: adds a face to a Large Face List
        /// If several faces are in the given image, the target rectangle must be provided
        /// </summary>
        public static async Task<PersistedFace> AddFace(string img, string faceListID, Rectangle target = null, string userData = "")
        {
            Console.WriteLine("\n Adding face to Large Face List...");

            string uri = "https://westeurope.api.cognitive.microsoft.com/face/v1.0/largefacelists/"+faceListID+"/persistedfaces";

            if (userData != "" || target != null)
            {
                uri += "?";
            }
            if(userData != "")
            {
                uri += "userData=";
                uri += userData;
            }
            if(target != null)
            {
                if (userData != "")
                {
                    uri += "&";
                }
                uri += "targetFace=";
                uri += target.left;
                uri += ",";
                uri += target.top;
                uri += ",";
                uri += target.width;
                uri += ",";
                uri += target.height;
            }

            string response = null;
            PersistedFace result = null;

            try
            {
                response = await RequestHelpers.PostRequest(uri, new JsonContent(new
                {
                    url = img
                }));

                Console.WriteLine($"\nFace added to Large Face List.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n{e.GetType().Name}: Error adding face to Large Face List.");
                throw;
            }

            if (response != null)
            {
                try
                {
                    result = JsonConvert.DeserializeObject<PersistedFace>(response);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"\n{e.GetType().Name}: Error parsing response after adding face to Large Face List.");
                    throw;
                }
            }

            return result;
        }

        /// <summary>
        /// LargeFaceList - Train API: Trains a Large Face List, after new faces have been added
        /// </summary>
        public static async Task TrainFaceList(string faceListID)
        {
            Console.WriteLine("\n Training Large Face List...");

            string uri = "https://westeurope.api.cognitive.microsoft.com/face/v1.0/largefacelists/"+faceListID+"/train";

            string response = null;

            try
            {
                response = await RequestHelpers.PostRequest(uri, new JsonContent(new { }));
     
                Console.WriteLine($"\n Training successfully started.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n{e.GetType().Name}: Error training Large Face List.");
                throw;
            }

        }

        /// <summary>
        /// LargeFaceList - Get Training Status API: Checks the training status of a Large Face List
        /// </summary>
        public static async Task<Status> GetFaceListStatus(string faceListID)
        {
            Console.WriteLine("\n Getting Large Face List training status...");

            string uri = "https://westeurope.api.cognitive.microsoft.com/face/v1.0/largefacelists/"+faceListID+"/training";

            string response = null;
            Status result = null;

            try
            {
                response = await RequestHelpers.GetRequest(uri);
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n{e.GetType().Name}: Error getting Large Face List training status.");
                throw;
            }

            if (response != null)
            {
                try
                {
                    result = JsonConvert.DeserializeObject<Status>(response);
                    Console.WriteLine($"\nTraining status found: {result.status}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"\n{e.GetType().Name}: Error parsing response after getting Large Face List training status.");
                    throw;
                }
            }

            return result;
        }

        /// <summary>
        /// LargeFaceList - List Face API: Lists all faces in a Large Face List
        /// n is the maximum number of faces returned (up to 1000)
        /// </summary>
        public static async Task<List<PersistedFace>> GetAllFaces(string faceListID, int n = 1000)
        {
            Console.WriteLine($"\n Getting faces in Large Face List {faceListID}...");

            string uri = "https://westeurope.api.cognitive.microsoft.com/face/v1.0/largefacelists/" + faceListID + "/persistedfaces";

            if(n < 1000)
            {
                uri += "?top=";
                uri += n;
            }

            string response = null;
            List<PersistedFace> results = null;

            try
            {
                response = await RequestHelpers.GetRequest(uri);
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n{e.GetType().Name}: Error getting faces in Large Face List.");
                throw;
            }

            if (response != null)
            {
                try
                {
                    results = JsonConvert.DeserializeObject<List<PersistedFace>>(response);
                    Console.WriteLine($"\n{results.Count} faces in Large Face List {faceListID} found.");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"\n{e.GetType().Name}: Error parsing response after listing faces in Large Face List.");
                    throw;
                }
            }

            return results;

        }

        private static void CreateTags(Face face)
        {
            // Age tag
            double age = face.faceAttributes.age;
            string ageTag = "adult";
            if (age < 2)
            {
                ageTag = "baby";
            }
            if (age < 5)
            {
                ageTag = "toddler";
            }
            else if (age < 13)
            {
                ageTag = "child";
            }
            else if (age < 19)
            {
                ageTag = "teenager";
            }
            else if (age < 26)
            {
                ageTag = "young adult";
            }
            else if (age > 65)
            {
                ageTag = "elderly";
            }
            face.tags.Add(ageTag);

            // Smile tag
            float thresholdSmile = 0.5f;
            if(face.faceAttributes.smile > thresholdSmile)
            {
                face.tags.Add("smiling");
            }

            // Gender tag
            if(face.faceAttributes.gender == "female")
            {
                if (age < 18)
                {
                    face.tags.Add("girl");
                }
                else
                {
                    face.tags.Add("woman");
                }
            }
            else if (face.faceAttributes.gender == "male")
            {
                if (age < 18)
                {
                    face.tags.Add("boy");
                }
                else
                {
                    face.tags.Add("man");
                }
            }

            // Facial hair tag
            float thresholdFacialHair = 0.5f;
            if (face.faceAttributes.facialHair.moustache > thresholdFacialHair)
            {
                face.tags.Add("moustache");
            }
            if (face.faceAttributes.facialHair.beard > thresholdFacialHair)
            {
                face.tags.Add("beard");
            }
            if (face.faceAttributes.facialHair.sideburns > thresholdFacialHair)
            {
                face.tags.Add("sideburns");
            }

            // Glasses tag
            if (face.faceAttributes.glasses != "NoGlasses")
            {
                face.tags.Add(face.faceAttributes.glasses);
            }

            // Makeup tag
            if (face.faceAttributes.makeup.eyeMakeup)
            {
                face.tags.Add("eye makeup");
            }
            if (face.faceAttributes.makeup.lipMakeup)
            {
                face.tags.Add("lip makeup");
            }

            // Emotion tag
            float thresholdEmotion = 0.6f;
            if (face.faceAttributes.emotion.anger > thresholdEmotion)
            {
                face.tags.Add("angry");
            }
            if (face.faceAttributes.emotion.contempt > thresholdEmotion)
            {
                face.tags.Add("arrogant");
            }
            if (face.faceAttributes.emotion.disgust > thresholdEmotion)
            {
                face.tags.Add("disgusted");
            }
            if (face.faceAttributes.emotion.fear > thresholdEmotion)
            {
                face.tags.Add("scared");
            }
            if (face.faceAttributes.emotion.happiness > thresholdEmotion)
            {
                face.tags.Add("happy");
            }
            if (face.faceAttributes.emotion.neutral > thresholdEmotion)
            {
                face.tags.Add("neutral");
            }
            if (face.faceAttributes.emotion.sadness > thresholdEmotion)
            {
                face.tags.Add("sad");
            }
            if (face.faceAttributes.emotion.surprise > thresholdEmotion)
            {
                face.tags.Add("surprised");
            }

            // Accessories tag
            float thresholdAccessory = 0.5f;
            foreach(Accessory a in face.faceAttributes.accessories)
            {
                if (a.confidence > thresholdAccessory)
                {
                    face.tags.Add(a.type);
                }
            }

            // Hair tag
            float thresholdHair = 0.6f;
            if (face.faceAttributes.hair.bald > thresholdHair)
            {
                face.tags.Add("bald");
            }
            foreach(HairColor h in face.faceAttributes.hair.hairColor)
            {
                if (h.confidence > thresholdHair)
                {
                    face.tags.Add($"{h.color} hair");
                }
            }

        }




    }
}
