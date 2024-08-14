// TODO : Append extracted data to big data file
// TODO : Add similar code blocks for other entity types

using Microsoft.ML;

using System.IO;
using System.Collections.Generic;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Exception = Autodesk.AutoCAD.Runtime.Exception; // --- Clash with System.Exception

namespace Janitor
{
    public class Janitor : BaseCommand // --- Inheritance from BaseCommand
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
                if (text.Layer.ToLower().Contains("text") ||
                    text.Layer.ToLower().Contains("yazı"))
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
                if (dim.Layer.ToLower().Contains("dim")
                    || dim.Layer.ToLower().Contains("dım"))
                {
                    return "correct";
                }
                else
                {
                    return "incorrect";
                }
            }

            // Labeling logic for rotated dimension entities:
            // If the dimension style name of the dimension entity includes the word "dim", it is considered correct, otherwise incorrect
            else if (entity is RotatedDimension rDim)
            {
                if (rDim.Layer.ToLower().Contains("dim")
                    || rDim.Layer.ToLower().Contains("dım"))
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
                if (hatch.Layer.ToLower().Contains("hatch"))
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
        public class OutlierDetection : BaseCommand
        {
            private static PredictionEngine<DataPoint, Prediction> predictionEngine; 
            int outlierHatchCount = 0; int outlierTextCount = 0; int outlierDimCount = 0;

            [CommandMethod("DetectOutliers")]
            public void DetectOutliers()
            {
                outlierHatchCount = 0; outlierTextCount = 0; outlierDimCount = 0; // --- Reset counters

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
                    }

                    ed.WriteMessage("\nOutlier detection completed.");
                    ed.WriteMessage("\nText Outliers Count: " + outlierTextCount);
                    ed.WriteMessage("\nDimension Outliers Count: " + outlierDimCount);
                    ed.WriteMessage("\nHatch Outliers Count: " + outlierHatchCount);
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
                        // Counter for the number of outliers
                        if(entity is Hatch)
                        {
                            outlierHatchCount++;
                        }
                        else if (entity is DBText)
                        {
                            outlierTextCount++;
                        }
                        else if (entity is Dimension)
                        {
                            outlierDimCount++;
                        }

                        ed.WriteMessage($"Outlier detected: {entity.GetType().Name} on layer {entity.Layer}\n");
                    }
                    if (prediction.PredictedLabel == "unknown")
                    {
                        ed.WriteMessage($"Unknown entity detected: {entity.GetType().Name} on layer {entity.Layer}\n");
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



