# Artificial Intelligence for FAULT prediction

A project developed in collaboration with the *University of Camerino* and the *Loccioni* company.

**Team:**<br><br>
<a href="https://github.com/damiano-cacchiarelli/AIPF-CSDProject/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=damiano-cacchiarelli/AIPF-CSDProject" />
</a>

**Project manager:** Luca Mazzuferi

**Tutor:** Matteo Calisti

## Application Domain
The industrial manufacturing and the huge amount of data produced cannot be managed by hand and in order to do data analysis we have to use automatic or semi-automatic tools. Analyzing the data coming from sensors, alarms and any failures of industrial machines is very important nowadays, because it allows to improve working conditions, create new business models, increase plant productivity and improve the quality of the product. Plenty of semi-automatic data analysis and management tools can be used, for example the Machine Learning technique used in the Big Data Analysis process.
In particular, the system involved is based on the integration of Machine Learning algorithms in the data scheme for managing alarms and fault events of the Loccioni automatic machines.

## Objective
Design and develop a system, using ML.NET, which allows to integrate and make "Plug-in" Machine Learning algorithms of various kinds applied on the data belonging to Loccioni.

## Work plane
The following are the activities to be carried out; in order to improve the organization and distribution of the workload, a GANTT diagram has been defined, which contains the milestones and related deadlines.

1. **Analysis of the ML.NET package:** 
     - Overview of ML.NET;
     - Analysis of some use cases;
     - Understanding how it is possible to plug / unplug an ML algorithm in ML.NET;
     - Analysis of ONNX and TensorFlow models and integration with ML.NET;

2. **Creation of a prototype (console application) capable of:**
     - Executing low-complexity ML algorithms;
     - Extracting the data to be analyzed from a local file (JSON, ...);
     - Importing an ONNX / TensorFlow  model;
     - Define unit case tests for the implemented functionalities;

3. **Refine the prototype (REST API application) adding the functionalities:**
     - Linking of more ML algorithms;
     - Operations on data (filter, mapping ...);
     - Connect the Loccioni database and the existing systems with the developed system;
     - Define unit case tests for the implemented functionalities;


4. **Perform system scaling test:**
     - Usage-based tests of large amounts of data;
     - Tests based on the use of increasingly complex ML algorithms;

5. **Other possible extensions:**
     - Add system progress;
     - Improve velocity and efficiency of the system;
     - Asynchronicity of the data output;

## License

MIT License
