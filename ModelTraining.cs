using Autodesk.AutoCAD.Runtime;

using Microsoft.ML;
//using Microsoft.ML.Data;
//using Microsoft.ML.Transforms;
//using Microsoft.ML.Transforms.Text;

using System.IO;

namespace Janitor
{
    public class ModelTraining : BaseCommand
    {
        // Command that trains the model
        // TODO : properly handle the case where there are not enough classes in the training data.

        [CommandMethod("TrainModel")]
        public void TrainModel()
        {
            try
            {
                // Create a new ML context
                MLContext mlContext = new MLContext(seed: 0);

                // Path of the training data file
                string dataPath = Janitor.csvPath;

                // Check if file exits
                if (!File.Exists(dataPath))
                {
                    ed.WriteMessage("\nTraining data file not found at {dataPath}.");
                    return;
                }

                // Load the data from csvPath variable of the Janitor class of ObjectDetection.cs file
                IDataView data;

                try
                {
                    data = mlContext.Data.LoadFromTextFile<DataPoint>(dataPath, separatorChar: ',', hasHeader: true); // --- Change the separatorChar if needed
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage($"\nError loading data: {ex.Message}");
                    return;
                }

                // Debugging - Preview data in AutoCad
                var preview = data.Preview(maxRows: 10);
                ed.WriteMessage($"\nPreview of the data:");

                foreach (var row in preview.RowView) // --- Iterate through the rows of the preview
                {
                    foreach (var col in row.Values) // --- Iterate through the columns of the row
                    {
                        ed.WriteMessage($"{col.Key}: {col.Value} \n");
                    }
                    ed.WriteMessage("\n");
                }

                // Debugging - Check for empty dataset
                if (preview.RowView.Length == 0)
                {
                    ed.WriteMessage("\nThe dataset is empty.");
                    return;
                }

                // Split data into training and testing datasets
                var splitData = mlContext.Data.TrainTestSplit(data, testFraction: 0.2);

                // Define data preparation and training pipeline
                #region useless pipelines
                /*
                var pipeline = mlContext.Transforms.Conversion.MapValueToKey("Label")
                .Append(mlContext.Transforms.Text.FeaturizeText("EntityTypeFeaturized", "EntityType"))
                .Append(mlContext.Transforms.Text.FeaturizeText("LayerFeaturized", "Layer"))
                .Append(mlContext.Transforms.Text.FeaturizeText("TextStyleFeaturized", "TextStyle"))
                .Append(mlContext.Transforms.Text.FeaturizeText("DimStyleFeaturized", "DimStyle"))
                .Append(mlContext.Transforms.Text.FeaturizeText("PatternNameFeaturized", "PatternName"))
                .Append(mlContext.Transforms.Concatenate("Features", "EntityTypeFeaturized", "LayerFeaturized",
                    "TextStyleFeaturized", "DimStyleFeaturized", "PatternNameFeaturized", "TextHeight", "Scale"))
                .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
                */ // → Autocad error : schema mismatch

                /*
                var pipeline = mlContext.Transforms.Conversion.MapValueToKey("Label")
                .Append(mlContext.Transforms.Text.FeaturizeText("EntityTypeFeaturized", "EntityType"))
                .Append(mlContext.Transforms.Text.FeaturizeText("LayerFeaturized", "Layer"))
                .Append(mlContext.Transforms.Text.FeaturizeText("TextStyleFeaturized", "TextStyle"))
                .Append(mlContext.Transforms.Text.FeaturizeText("DimStyleFeaturized", "DimStyle"))
                .Append(mlContext.Transforms.Text.FeaturizeText("PatternNameFeaturized", "PatternName"))
                .Append(mlContext.Transforms.Concatenate("NumericFeatures", "TextHeight", "Scale"))
                .Append(mlContext.Transforms.Concatenate("Features", "EntityTypeFeaturized", "LayerFeaturized",
                    "TextStyleFeaturized", "DimStyleFeaturized", "PatternNameFeaturized", "NumericFeatures"))
                .Append(mlContext.Transforms.NormalizeMinMax("Features"))
                .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
                */
                /*
                var pipeline = mlContext.Transforms.Conversion.MapValueToKey("Label")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("EntityTypeEncoded", "EntityType"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("LayerEncoded", "Layer"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("TextStyleEncoded", "TextStyle"))

                .Append(mlContext.Transforms.Categorical.OneHotEncoding("DimStyleEncoded", "DimStyle"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("PatternNameEncoded", "PatternName"))
                .Append(mlContext.Transforms.ReplaceMissingValues("TextHeight", "TextHeight"))
                .Append(mlContext.Transforms.ReplaceMissingValues("Scale", "Scale"))
                .Append(mlContext.Transforms.NormalizeMinMax("TextHeightNormalized", "TextHeight"))
                .Append(mlContext.Transforms.NormalizeMinMax("ScaleNormalized", "Scale"))
                .Append(mlContext.Transforms.Concatenate("Features",
                    "EntityTypeEncoded", "LayerEncoded", "TextStyleEncoded", "DimStyleEncoded",
                    "PatternNameEncoded", "TextHeightNormalized", "ScaleNormalized"))
                .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
                */
                /* EA
                var pipeline = mlContext.Transforms.Conversion.MapValueToKey("Label")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("EntityTypeEncoded", "EntityType"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("LayerEncoded", "Layer"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("TextStyleEncoded", "TextStyle"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("TextHeightEncoded", "TextHeight"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("DimStyleEncoded", "DimStyle"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("ScaleEncoded", "Scale"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("PatternNameEncoded", "PatternName"))
                .Append(mlContext.Transforms.Concatenate("Features",
                    "EntityTypeEncoded", "LayerEncoded", "TextStyleEncoded", "TextHeightEncoded", "DimStyleEncoded",
                    "ScaleEncoded", "PatternNameEncoded"))
                .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
                */
                /* Nullable
                var pipeline = mlContext.Transforms.Conversion.MapValueToKey("Label")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("EntityTypeEncoded", "EntityType"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("LayerEncoded", "Layer"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("TextStyleEncoded", "TextStyle"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("DimStyleEncoded", "DimStyle"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("PatternNameEncoded", "PatternName"))
                .Append(mlContext.Transforms.ReplaceMissingValues("TextHeight", replacementMode: MissingValueReplacingEstimator.ReplacementMode.Mean))
                .Append(mlContext.Transforms.ReplaceMissingValues("Scale", replacementMode: MissingValueReplacingEstimator.ReplacementMode.Mean))
                .Append(mlContext.Transforms.NormalizeMinMax("TextHeightNormalized", "TextHeight"))
                .Append(mlContext.Transforms.NormalizeMinMax("ScaleNormalized", "Scale"))
                .Append(mlContext.Transforms.Concatenate("Features",
                    "EntityTypeEncoded", "LayerEncoded", "TextStyleEncoded", "DimStyleEncoded",
                    "PatternNameEncoded", "TextHeightNormalized", "ScaleNormalized"))
                .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
                */
                /*
                var pipeline = mlContext.Transforms.Conversion.MapValueToKey("Label")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("EntityTypeEncoded", "EntityType"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("LayerEncoded", "Layer"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("TextStyleEncoded", "TextStyle"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("DimStyleEncoded", "DimStyle"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("PatternNameEncoded", "PatternName"))
                .Append(mlContext.Transforms.ReplaceMissingValues("TextHeight",
                    replacementMode: MissingValueReplacingEstimator.ReplacementMode.Mean,
                    missingValueIndicator: -1))
                .Append(mlContext.Transforms.ReplaceMissingValues("Scale",
                    replacementMode: MissingValueReplacingEstimator.ReplacementMode.Mean,
                    missingValueIndicator: -1))
                .Append(mlContext.Transforms.NormalizeMinMax("TextHeightNormalized", "TextHeight"))
                .Append(mlContext.Transforms.NormalizeMinMax("ScaleNormalized", "Scale"))
                .Append(mlContext.Transforms.Concatenate("Features",
                    "EntityTypeEncoded", "LayerEncoded", "TextStyleEncoded", "DimStyleEncoded",
                    "PatternNameEncoded", "TextHeightNormalized", "ScaleNormalized"))
                .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
                */
                #endregion

                var pipeline = mlContext.Transforms.Conversion.MapValueToKey("Label")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("EntityTypeFeaturized", "EntityType"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("LayerFeaturized", "Layer"))
                .Append(mlContext.Transforms.Concatenate("Features", "EntityTypeFeaturized", "LayerFeaturized"))
                .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

                // Train the model
                ed.WriteMessage("\nTraining the model..."); // --- Debugging
                var model = pipeline.Fit(splitData.TrainSet);

                // Evaluate the model
                ed.WriteMessage("\nEvaluating the model..."); // --- Debugging
                var predictions = model.Transform(splitData.TestSet);
                var metrics = mlContext.MulticlassClassification.Evaluate(predictions);

                // Print evaluation metrics
                
                ed.WriteMessage($"Log-loss: {metrics.LogLoss}");
                ed.WriteMessage($"Macro accuracy: {metrics.MacroAccuracy}");
                ed.WriteMessage($"Micro accuracy: {metrics.MicroAccuracy}");

                // Save the model to path of drawing file
                ed.WriteMessage("\nSaving the model..."); // --- Debugging
                mlContext.Model.Save(model, splitData.TrainSet.Schema, Path.ChangeExtension(dataPath, ".zip"));

                ed.WriteMessage("\nModel training completed successfully.");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\nSystem Error: \n{ex.Message}");
                ed.WriteMessage($"\nStack Trace: \n{ex.StackTrace}");
                ed.WriteMessage($"\nInner Exceptions : \n{ex.InnerException}"); 
            }
        }
    }
}
