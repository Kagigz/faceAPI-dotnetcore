using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FaceAPI
{

    class Program
    {

        static void Main(string[] args)
        {
            AskUser();
        }


        private static void AskUser()
        {
            Console.WriteLine("\n\nWhat do you want to do?" +
                "\n 1. Search Similar images" +
                "\n 2. Add a new image (Ingestion)" +
                "\n 3. Create a Large Face List" +
                "\n 4. List Face Lists" +
                "\n 5. List Faces in one Face List");
            string answer = Console.ReadLine();

            int number = 0;

            try
            {
                number = Convert.ToInt32(answer);
            }
            catch
            {
                Console.WriteLine("\nPlease enter a number.");
            }

            switch (number)
            {
                case 1:
                    try
                    {
                        SearchSimilarPipeline().Wait();
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine($"\n{e.GetType().Name}: Search Similar Pipeline failed.");
                    }
                    break;
                case 2:
                    try
                    {
                        IngestionPipeline().Wait();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"\n{e.GetType().Name}: Ingestion Pipeline failed.");
                    }
                    break;
                case 3:
                    try
                    {
                        CreateFaceList().Wait();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"\n{e.GetType().Name}: Creating Large Face List failed.");
                    }
                    break;
                case 4:
                    try
                    {
                        ListFaceLists().Wait();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"\n{e.GetType().Name}: Listing Large Face Lists failed.");
                    }
                    break;
                case 5:
                    try
                    {
                        ListFaces().Wait();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"\n{e.GetType().Name}: Listing faces in Large Face List failed.");
                    }
                    break;
                default:
                    Console.WriteLine("\n Please choose between the 5 options.");
                    AskUser();
                    break;
            }
            Console.ReadLine();
        }

        private static async Task SearchSimilarPipeline()
        {
            Console.WriteLine("\nSearch Similar Pipeline");
            float threshold = 80f;

            Console.WriteLine("\nEnter image url: ");
            string img = Console.ReadLine();

            try
            {
                List<Face> detectedFaces = await FaceAPIHelpers.DetectFaces(img);

                foreach (Face f in detectedFaces)
                {
                    try
                    {
                        List<PersistedFace> similarFaces = await FaceAPIHelpers.SimilarFaces(f, "testfacelistkatia", 5);
                        Console.WriteLine("\n Face ID:" + f.faceId);
                        foreach (PersistedFace simFace in similarFaces)
                        {
                            if (simFace.confidence > threshold)
                            {
                                Console.Write("\n Similar Face ID: " + simFace.persistedFaceId);
                                Console.Write("\n Confidence: " + simFace.confidence + "\n");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"\n{e.GetType().Name}: Could not find Similar Faces.");
                        throw(e);
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n{e.GetType().Name}: Could not detect faces.");
                throw (e);
            }

            AskUser();
        }

        private static async Task IngestionPipeline()
        {
            Console.WriteLine("\n Ingestion Pipeline");

            Console.WriteLine("\n Enter new image url:");
            string img = Console.ReadLine();
            Console.WriteLine("\n Enter Large Face List ID:");
            string faceListID = Console.ReadLine();

            try
            {
                List<Face> detectedFaces = await FaceAPIHelpers.DetectFaces(img,true);

                foreach (Face f in detectedFaces)
                {
                    try
                    {
                        PersistedFace persistedFace = await FaceAPIHelpers.AddFace(img, faceListID, f.faceRectangle);
                        Console.WriteLine("\n Face " + persistedFace.persistedFaceId + " added.");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"\n{e.GetType().Name}: Could not add face to Large Face List.");
                        throw (e);
                    }
                }

                try
                {
                    await FaceAPIHelpers.TrainFaceList(faceListID);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"\n{e.GetType().Name}: Could not train Large Face List.");
                    throw (e);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n{e.GetType().Name}: Could not detect faces.");
                throw (e);
            }

            AskUser();
        }

        private static async Task CreateFaceList()
        {
            Console.WriteLine("\n Create Large Face List Pipeline");

            string listName = "name-facelist";
            Guid listID = Guid.NewGuid();

            try
            {
                await FaceAPIHelpers.CreateFaceList(listID.ToString(), listName);

                Console.WriteLine("\n Large Face list created.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n{e.GetType().Name}: Could not create Large Face List.");
                throw (e);
            }

            try
            {
                await FaceAPIHelpers.GetFaceLists();
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n{e.GetType().Name}: Could not list Large Face Lists.");
                throw (e);
            }

            AskUser();
        }

        private static async Task ListFaceLists()
        {

            try
            {
                await FaceAPIHelpers.GetFaceLists();
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n{e.GetType().Name}: Could not list Large Face Lists.");
                throw (e);
            }

            AskUser();
        }

        private static async Task ListFaces()
        {
            Console.WriteLine("\n Enter ID of the Large Face List");
            string faceListID = Console.ReadLine();

            try
            {
                List<PersistedFace> persistedFaces = await FaceAPIHelpers.GetAllFaces(faceListID);

                foreach (PersistedFace f in persistedFaces)
                {
                    Console.WriteLine("\n Face " + f.persistedFaceId + "\n User data: " + f.userData);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n{e.GetType().Name}: Could not list faces in Large Face List.");
                throw (e);
            }

            AskUser();
        }

    }
}
