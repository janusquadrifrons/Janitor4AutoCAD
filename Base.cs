// AutoCad to graph conversion tool

using Autodesk.AutoCAD.Runtime; 
using Autodesk.AutoCAD.ApplicationServices; 
using Autodesk.AutoCAD.DatabaseServices; 
using Autodesk.AutoCAD.EditorInput; 
using Autodesk.AutoCAD.Geometry;

using Newtonsoft.Json;

using System.Collections.Generic;
using System.IO;
using Microsoft.ML.Data;

namespace Janitor
{
    // Base class for common declarations
    public abstract class BaseCommand
    {
        protected Document doc;
        protected Database db;
        protected Editor ed;

        protected BaseCommand()
        {
            doc = Application.DocumentManager.MdiActiveDocument;
            db = doc.Database;
            ed = doc.Editor;
        }
    }

    // Class definiton → ModelTraining.cs
  
    public class DataPoint
    {
        [LoadColumn(0)] public string EntityType;
        [LoadColumn(1)] public string Layer;
        //[LoadColumn(2)] public string TextStyle;
        //[LoadColumn(3)] public float TextHeight;
        //[LoadColumn(4)] public string DimStyle;
        //[LoadColumn(5)] public float Scale;
        //[LoadColumn(6)] public string PatternName;
        [LoadColumn(2)] public string Label;
    }
    

    // Class definiton → ObjectDetection.cs
    public class Prediction
    {
        public string PredictedLabel;
        public float[] Score;
    }


}
