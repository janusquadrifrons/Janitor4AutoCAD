# Janitor4AutoCad
Outlier detection plug-in implementing ML.NET in AutoCad.

## Overview

This AutoCAD plug-in leverages ML techniques to detect outliers in your drawings. It analyzes entities (hatches, dimensions, text) within the drawing to identify potential errors that may impact the quality and accuracy of your projects. Designed to integrate ML.NET seamlessly into your AutoCAD workflow, offering a powerful tool for quality control and error detection. **Stochastic Dual Coordinate Ascent with Maximum Entropy**, a popular algorithm for multiclass classification tasks, is used for learning algorithm as it is suitable for high-dimensional data without fine-tuning.

## Features

- **Entity Type Analysis:** The plug-in evaluates various AutoCAD entities like DBText, Line, and Dimension to identify potential outliers based on their positions, layers, and other attributes.
- **Layer Naming Verification:** It checks for incorrect or inconsistent layer naming conventions, helping maintain uniformity across your CAD projects.
- **Customizable Training Data:** Users can generate and customize their own training data sets to tailor the model for specific project needs.
- **Real-Time Outlier Detection:** The plug-in runs within AutoCAD, providing real-time feedback on detected outliers during the design process.
- **User-Friendly Interface:** Easy-to-use commands and seamless integration with AutoCAD.

## Installation

**1- Clone or Download the Repository:**
```ruby
git clone https://github.com/janusquadrifrons/Janitor4AutoCAD
```
**2- Build the Project:** Open the solution file in your preferred IDE and build the project. Make sure you have all dependencies installed, including ML.NET and the necessary AutoCAD libraries.

**3- Load the Plug-in in AutoCAD:** Type the NETLOAD command in AutoCAD to load the plug-in within an active session.

## Common Build Errors

**- Assembly reference versioning :** Such as,
```
Could not load file or assembly 'System.Threading.Tasks.Extensions, Version=4.5.4.0, 
Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51' or one of its dependencies. 
The located assembly's manifest definition does not match the assembly reference. 
```
```
Solution : Assembly binding by redirections in app.config and acad.exe.config files...
```


**- Missing assembly references :** Such as,
```
Unable to locate 'System.Threading.Tasks.Extensions.dll' and its dependency System.Runtime.CompilerServices.Unsafe.dll
```
```
Solution : Copy dll files to acad.exe folder.
```

## Data Points
For a basic outlier detection model with a few features, you might start with *500-1000* data points. This assumes a balanced dataset and relatively simple features. If your model involves moderate complexity, with some non-linear relationships or a slightly larger feature set, aim for *2,000-5,000* data points. This range allows the model to learn a broader set of patterns and anomalies. For more complex models, especially if you plan to add more features or use a more sophisticated algorithm, consider *10,000+* data points. This ensures the model can generalize well to new, unseen drawings.

## Usage with Commands

#### EXTRACTTRAININGDATA      
-Function       : Extracts relevant data from the current drawing and saves it as a .csv file.\
-Purpose        : The file can be used to train the model.

#### TRAINMODEL      
-Function       : Trains the machine learning model on the extracted data.The model is saved as a .zip file in the same directory as your drawing.\
-Purpose        : As its name implies.

#### DETECTOUTLIERS      
-Function       : Runs the trained model against the current drawing.\
-Purpose        : identifying any entities that are potential outliers based on the trained data.

## Usage with UI
*Coming Soon*

## Requirements
- AutoCAD 2022
- .NET Framework 4.8
- ML.NET 3.0.1

## Contributing
Contributions are welcome! Feel free to submit issues, fork the repository, and make pull requests. For major changes, please open an issue first to discuss what you would like to change.
