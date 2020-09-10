# XertClient
A .net lib, witten in .Net Core ver 3.1, for using the Xert api

Version of the API: 1.3.0    
Documentation for the API is provied at:  
[Xert Online API](https://www.xertonline.com/API.html?fbclid=IwAR1sOg8XLDL44WyaeNVzbRA0V9JxfK879dBai3Y5KBCupw88HS1lXWC2xT0)  
[Xert web site](https://www.xertonline.com/)

### Solution Content
The solution includes:
- A simple command line integration project
- A unit test project, using NUnit 
- The main project  
  - The main project compiles to a dll. 
  - local_nuget_push.bat, in the main project is provied as an example to push the package to a local package source.

### About Xert
Xert is a tool for endurance athletes, cyclist and runners. The tool offers insight into training such as fitness signature, by analyzing previous performance data. In adition, Xert can predict an athletes performance potential, in real-time, as *_Maximum Power Available MPA_*, which can be extraordinary valuable for maximizing training effect or in some competitive scenarios. Xert also provides insight into the dynamic physiological stresses on an individuals energy systems. Xert divides energy systems into three categories, low, high and peak. Another compelling feature of Xert is an **_Adapative Training Advisor_** that recommends specific details about optimal workouts for the present time. The recomendations are based on various inputs including previous activities, the ammount of time before the next competition and the current fatigue status of the three energy systems.

### Library Objective
The primary reason for the library is to provide an alternative to the website presentations of some Xert features. For example Xert has a large collection of workouts designed
to address specific objectives characterized by certain parameters. The paramers include: **Focus, Dificulity, Duration and XSS** (**X**ert **S**train **S**core). Some people find the the current website presentation and filtering of the workouts less than ideal and a bit clumbsy. This library attempts to improve the experience for slecting workouts. 

### See XertExplorer 
I a have created a prject that uses this dll to present the workouts and filter them in a way that is hopefully easier to use.   
[XertExplorer](https://github.com/ccuddohy/XertExplorer)


