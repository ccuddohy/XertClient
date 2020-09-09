# XertClient
A .net lib, witten in .Net Core ver 3.1, for using the Xert api

Version of the API: 1.3.0
Documentation for the API is provied at:     
[Xert Online API](https://www.xertonline.com/API.html?fbclid=IwAR1sOg8XLDL44WyaeNVzbRA0V9JxfK879dBai3Y5KBCupw88HS1lXWC2xT0)

### Xert
Xert is a tool for athletes to analyze performance metrics. In particular Xert provides an Aapative Training Advisor to advise specific information
about optimal training that should be done, based on data from previous activities, the ammount of time before the next competition and the current
fatigue status.

### Solution Content
The solution includes:
- A simple command line integration project
- A unit test project, using NUnit 
- The main project  
  - The main project complies to a dll. 
  - local_nuget_push.bat, in the main project is provied as an example to push the package to a local package source.

### Library Objective
The primary reason for the library is to provide an alternative to the website presentations of some Xert features. For example Xert has a large collection of workouts designed
to address specific objectives characterized by a **Focus** parameter and a **Dificulity** parameter as well as **Duration** and **XSS** parameters (**X**ert **S**train **S**core). 


