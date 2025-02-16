# JunkWaxDetection.LiveDemo

**JunkWaxDetection.LiveDemo** is an ML-powered, real-time Blazor web application built on **.NET 9** that detects **Junk Wax Baseball Cards** — including **retro baseball cards from the 80s and 90s** — using advanced **machine learning** models. This project leverages **ML.NET**, **ONNX**, and **Paddle OCR** to deliver precise image classification and optical character recognition (OCR) for authenticating and analyzing vintage baseball cards. My overall goal for this tech demo was to show that C# developers can have just as much fun with Machine Learning and Computer Vision as any other language!

## Table of Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [Technical Stack](#technical-stack)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Usage Instructions](#usage-instructions)
- [Machine Learning Integration](#machine-learning-integration)
- [Trading Card Datasets](#trading-card-datasets)
- [Contributing](#contributing)
- [License](#license)

## Overview

JunkWaxDetection.LiveDemo provides a robust solution for detecting junk wax baseball cards, with a special focus on retro baseball cards from the 80s and 90s. By integrating cutting-edge machine learning algorithms and computer vision techniques, this project helps collectors, dealers, and enthusiasts verify the authenticity and quality of their vintage baseball card collections.

## Key Features

- **Real-Time Junk Wax Detection**: Instant identification of junk wax on baseball cards.
- **Optimized for Retro Baseball Cards**: Special considerations for vintage baseball cards from the **80s and 90s**.
- **Interactive UI**: Modern user interface built with **Blazor**.
- **Machine Learning Integration**: Utilizes **ML.NET** and **ONNX** models for robust image classification.
- **Optical Character Recognition (OCR)**: Uses **Paddle OCR** to extract text from baseball card images for deeper analysis.
- **Data-Driven Insights**: Integrates with open sports card datasets to enhance card authentication and verification processes.

## Technical Stack

- **.NET 9 SDK**: Core framework powering the application.
- **Blazor**: Front-end framework for building interactive web UIs with C#.
- **ML.NET**: Machine learning framework for loading and executing the ONNX model.
- **ONNX**: Open Neural Network Exchange format for deploying pre-trained ML models.
- **Paddle OCR**: Advanced OCR tool for extracting text from images.
- **Visual Studio 2022**: Recommended IDE for development and debugging.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or later

## Getting Started

1. **Clone the Repository**  
   ```bash
   git clone https://github.com/JunkWaxData/JunkWaxDetection.LiveDemo.git
   ```
2. **Navigate to the Project Directory**  
   ```bash
   cd JunkWaxDetection.LiveDemo
   ```
3. **Open the Solution**  
   Open `JunkWaxDetection.LiveDemo.sln` in Visual Studio 2022.
4. **Restore Dependencies**  
   Run the following command in your terminal:
   ```bash
   dotnet restore
   ```
5. **Run the Application**  
   Start debugging or run:
   ```bash
   dotnet run
   ```

## Usage Instructions

- Open your browser and navigate to **https://localhost:5001**.
- Utilizing your webcam show your favorite **retro baseball card from the 80s or 90s**.
- The system processes the image using **machine learning** models to detect the card being presented.
- View detailed results, including OCR analysis and validation data.

## Machine Learning Integration

### Junk Wax Detection ONNX Model

I use the [JunkWaxDetection ONNX Model](https://github.com/JunkWaxData/JunkWaxDetection) for precise image classification. This model is integrated with **ML.NET** using the `MLContext` and `PredictionEngine` classes, ensuring efficient predictions and optimal performance for detecting junk wax baseball cards.

### ML.NET and ONNX

- **ML.NET**: Simplifies the integration and execution of machine learning models within .NET applications.
- **ONNX**: Ensures scalability and cross-platform compatibility for deploying pre-trained models across various environments.

### Paddle OCR for Optical Character Recognition

In addition to visual classification, **Paddle OCR** extracts textual data from baseball card images. This text extraction is essential for matching card details against vintage baseball card databases and further identifying retro cards from the **80s and 90s**.

## Trading Card Datasets

### Junk Wax Data Card Lists

The project leverages the open-source [Junk Wax Data Card Lists](https://github.com/JunkWaxData/CardLists) repository, which provides a comprehensive JSON dataset of sports trading cards. This dataset supports:
- Matching player names and card details extracted via OCR.
- Cross-referencing card information to enhance the validation process for vintage collections.

## Contributing

Contributions are welcome! To contribute:

- **Fork** the repository.
- Create your feature branch:
  ```bash
  git checkout -b feature/your-feature-name
  ```
- **Commit** your changes:
  ```bash
  git commit -m 'Add some feature'
  ```
- **Push** to the branch:
  ```bash
  git push origin feature/your-feature-name
  ```
- Open a **Pull Request** for review.

Please refer to the repository guidelines for detailed contribution instructions.

## License

This project is licensed under the [MIT License](LICENSE). You are free to use, modify, and distribute this project under the terms of the license.
