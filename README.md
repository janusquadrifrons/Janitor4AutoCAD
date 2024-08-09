# Janitor4AutoCad
Outlier detection plug-in implementing ML.NET in AutoCad.

## Overview

This AutoCAD plug-in leverages ML techniques to detect outliers in your drawings. It analyzes entities (hatches, dimensions, text) within the drawing to identify potential errors that may impact the quality and accuracy of your projects. The plug-in is designed to integrate ML.NET seamlessly into your AutoCAD workflow, offering a powerful tool for quality control and error detection.

## Features

- **Entity Type Analysis:** The plug-in evaluates various AutoCAD entities like DBText, Line, and Dimension to identify potential outliers based on their positions, layers, and other attributes.
- **Layer Naming Verification:** It checks for incorrect or inconsistent layer naming conventions, helping maintain uniformity across your CAD projects.
- **Customizable Training Data:** Users can generate and customize their own training data sets to tailor the model for specific project needs.
- **Real-Time Outlier Detection:** The plug-in runs within AutoCAD, providing real-time feedback on detected outliers during the design process.
- **User-Friendly Interface:** Easy-to-use commands and seamless integration with AutoCAD.

## Installation

**1- Clone or Download the Repository:**
```ruby
git clone https://github.com/yourusername/AutoCAD-Outlier-Detection.git
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


**- Assembly reference versioning :** Such as,
```
Unable to locate 'System.Threading.Tasks.Extensions.dll' and its dependency System.Runtime.CompilerServices.Unsafe.dll
```
```
Solution : Copy dll files to acad.exe folder.
```

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

## Requirements
- AutoCAD 2022
- .NET Framework 4.8
- ML.NET 3.0.1

Contributing

Contributions are welcome! Feel free to submit issues, fork the repository, and make pull requests. For major changes, please open an issue first to discuss what you would like to change.
License

This project is licensed under the MIT License. See the LICENSE file for details.
Acknowledgments

    Thanks to the ML.NET team for providing a robust framework for machine learning in .NET applications.
    Special thanks to the AutoCAD developer community for their continuous support and resources

```
{
  "firstName": "John",
  "lastName": "Smith",
  "age": 25
}
```
