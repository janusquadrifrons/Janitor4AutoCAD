using Microsoft.ML;

using System.IO;
using System.Collections.Generic;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Exception = Autodesk.AutoCAD.Runtime.Exception; // --- Clash with System.Exception
using Microsoft.ML.Transforms;

namespace Janitor
{
    public class Janitor : BaseCommand
    {
        // Global variable - Path of the curent drawing
        public static string csvPath;

        // Command that extracts training data from the drawing
        [CommandMethod("ExtractTrainingData")]
        public void ExtractTrainingData()
        {
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;

                // Create a list to store the data
                List<string> dataList = new List<string>();
                dataList.Add("EntityType,Layer,Label"); // --- Main Header

                // Iterate through all the objects in current (model) space
                foreach (ObjectId objId in btr)
                {
                    try
                    {
                        Entity ent = tr.GetObject(objId, OpenMode.ForRead) as Entity;
                        string entityType = ent.GetType().Name;
                        string layer = ent.Layer;
                        string label = GetLabel(ent);

                        dataList.Add($"{entityType},{layer},{label}");
                    }
                    catch (Exception ex)
                    {
                        ed.WriteMessage($"\nError processing entity: {ex.Message}");
                    }
                }

                // TODO : Filename specification upgrade
                // Save the data to a CSV file to the path where the drawing is located
                string drawingPath = db.Filename;
                string drawingFolder = Path.GetDirectoryName(drawingPath);
                csvPath = Path.Combine(drawingFolder, "training_data.csv");
                File.WriteAllLines(csvPath, dataList);

                /* OR
                // Save the data to a CSV file to the path where the executable is located
                File.WriteAllLines("training_data.csv", dataList);
                */

                ed.WriteMessage($"\nTraining data saved to: {csvPath}");

                tr.Commit();
            }
        }

        private string GetLabel(Entity entity)
        {
            // Labeling logic for text entities :
            // If the layer name of the text entity includes the word "text", it is considered correct, otherwise incorrect
            if (entity is DBText text)
            {
                if (text.Layer.Contains("text"))
                {
                    return "correct";
                }
                else
                {
                    return "incorrect";
                }
            }

            // Labeling logic for dimension entities :
            // If the dimension style name of the dimension entity includes the word "dim", it is considered correct, otherwise incorrect
            else if (entity is Dimension dim)
            {
                if (dim.DimensionStyleName.Contains("dim"))
                {
                    return "correct";
                }
                else
                {
                    return "incorrect";
                }
            }
            
            // Labeling logic for hatch entities :
            // If the layer name of the hatch entity includes the word "hatch", it is considered correct, otherwise incorrect
            else if (entity is Hatch hatch)
            {
                if (hatch.Layer.Contains("hatch"))
                {
                    return "correct";
                }
                else
                {
                    return "incorrect";
                }
            }

            else
            {
                return "unknown";
            }
        }

        /////////////////////////////////////////////////////
        ///
        public class OutlierDetection : BaseCommand
        {
            private static PredictionEngine<DataPoint, Prediction> predictionEngine;

            [CommandMethod("DetectOutliers")]
            public void DetectOutliers()
            {
                LoadModel();

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;

                    // Iterate through all the objects in current (model) space
                    foreach (ObjectId objId in btr)
                    {
                        Entity ent = tr.GetObject(objId, OpenMode.ForRead) as Entity;
                        string entityType = ent.GetType().Name;
                        string layer = ent.Layer;

                        var dataPoint = new DataPoint
                        {
                            EntityType = entityType,
                            Layer = layer
                        };

                        CheckAndReport(dataPoint, ent);

                        // TODO : Add similar code blocks for other entity types
                    }
                }

            }

            // Helper method to load the model
            private void LoadModel()
            {
                if (predictionEngine == null)
                {
                    var mlContext = new MLContext();

                    // Load the model from the path where the drawing file is located
                    string drawingPath = db.Filename;
                    string drawingFolder = Path.GetDirectoryName(drawingPath);
                    string zipPath = Path.Combine(drawingFolder, "training_data.zip");

                    ITransformer model = mlContext.Model.Load(zipPath, out var modelInputSchema);

                    // For Debugging - Logging the schema data
                    ed.WriteMessage("Loading the model...\nModel Input Schema: ");
                    foreach (var column in modelInputSchema)
                    {
                        ed.WriteMessage($"Name: {column.Name}, Type: {column.Type}");
                    }

                    predictionEngine = mlContext.Model.CreatePredictionEngine<DataPoint, Prediction>(model);
                }
            }

            // Helper method to check and report outliers
            private void CheckAndReport(DataPoint dataPoint, Entity entity)
            {
                try
                {
                    var prediction = predictionEngine.Predict(dataPoint);

                    if (prediction.PredictedLabel == "incorrect")
                    {
                        ed.WriteMessage($"Outlier detected: {entity.GetType().Name} on layer {entity.Layer}\n");
                    }
                }
                catch (Exception ex)
                {
                    ed.WriteMessage($"Error predicting for {entity.GetType().Name}: {ex.Message}\n");
                }
            }
        }
    }
}



