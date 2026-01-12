Free Seats Gauge: a real-time seat availability visualization system

Group ：bangbangbang

Group Members:  

Xintong Shao: Digital Twin dashboard (AR implemenetation) 

Tianrui Min: Physical device (circuit & API connection)

Yewei Bian: Digital Twin dashboard (AR implemenetation) 

Yifei Huang: Fusion (3D Modelling and Printing)

 Git Hub Repository:  https://github.com/Tongtong828/OPS-MSG-Free-Seats.git 



## Introduction

This report details the Free Seats Gauge, a real-time seat availability visualization system designed specifically for University College London's East Campus, covering the One Pool Street and Marshgate teaching buildings. The system provides users with information on remaining free seats by a custom-built gauge, enabling rapid and clear information retrieval. The project integrates physical gauges with a digital AR interface, translating real-time seat occupancy data into clear representations to assist users in decision-making.

 

This project explores a hybrid mode of data presentation: physical devices provide clear information through servo-driven pointers and colour-coded interfaces, while an AR-based digital twin system offers richer interactive experiences, including accurate data and temporal trends. By prioritising simplicity and readability, Free Seats Gauge aims to reduce the “search costs” when users locating study spaces during peak periods.

 

## Motivation

At University College London (UCL), seat occupancy data for buildings is available through an official API called UCL API, but this information is not easily accessible or readable for students to use. As a result, students often waste time moving between different buildings when searching for available study spaces, particularly during busy periods.

 

The motivation of develop Free Seats Gauge is to support UCL students by providing a clear and direct way to check real-time seat availability within UCL East buildings, including Marshgate and One Pool Street. By reducing uncertainty and search time, the project aims to help students make quicker decisions about where to study and improve their overall campus experience.

## Dataset

This project uses the official UCL’s API as dataset. The API provides real-time information on seat availability across multiple campus buildings in a structured JSON format and update each minute. The dataset includes the number of free and occupied seats at both building and floor levels.

 

The raw data retrieved from the UCL API contains information beyond the scope of this project and therefore requires preprocessing before use. First, the dataset is filtered to extract only the records related to One Pool Street and Marshgate. This reduces data complexity and ensures that only relevant information is processed. Second, the seat counts are converted into a seats availability rate by comparing the number of free seats with the total seat capacity. 

## Methodology

The design method which been used is an iterative practice led method, involving digital model, rapid prototyping and hand making part. This is in line with how established practice is in interaction design and physical computing, it involves lots of iterations and prototyping to gradually evolve form and function through repeated rounds of testing and playing with the materials, before transfer to digital model (Moggridge, 2007).

 

Use Autodesk Fusion 360 to design the Free-Seats’s enclosure and interior because of it being parametrically modifiable and quick to iterate. Modeling workflow contains 2D sketching, Extrude, Fillet, Shell, Tolerance. These processes were achieved in order to have control over the forms but changes can still be made efficiently once errors and misalignment were spotted (Autodesk, n.d.).

 

The 3d print design was based on workshop 4, which relevant to design and make data devices. Firstly, measuring every important dimension of the components such as the servo motor, LCD screen, wiring clearance and mount points. This preparatory step was to ensure precise components, spatial efficiency and structural integrity of the components inside the enclosure. This is the most important step before conducting modeling in Fusion. Because the model will be a common model applicable to both physical devices and data devices.

 

Besides, for the convenience of use the gauge and readability, a color distinction method for the dial surface was introduced. There are four different color areas of red, orange, green and blue on free seats gauge, which indicate different seat occupancies of low to high. Such qualitative visual encoding enables quick reading and is in accordance with data visualisation rules that stress clarity and directness (Jansen, Dragicevic and Fekete, 2013).

 

Obtaining data from the UCL API helps to achieve real-time data for the Free seats Gauge, which is a dynamic response to real environmental changes. It also enhances the authenticity and relevance of the data, keeping the physical instruments and digital twins always closely connected to the real world. Moreover, the data acquisition method based on the API has good scalability. Although the project currently focuses only on the two buildings of UCL East, in future design iterations, the same data pipeline can be extended to more buildings of UCL without changing the core logic of the code.

 

Unity was selected as the platform for implementing the digital twin due to its ability to integrate real-time data processing, interactive visualization, and spatial computing within a single environment. Augmented reality (AR) enables digital information to be situated within physical space, allowing data to be experienced contextually rather than as an abstract screen-based visualisation.

## 3D Modelling and Fabrication

Create specific voids on physical device’s enclosure for the lcd screen and a dial button which will toggle the screen between marshgate and one pool street. Pointer mechanism was particular attention as this mechanism of error directly relationship to data read substantively. Due to some small dimensional error at the central axis or clearance space which resulted to misaligned and limited rotational movement. Therefore, it took several changes to get the pointer component to have satisfactory dimension. It is found through trial and error that tolerance awareness is an important aspect of 3D printing, especially in the design of the moving parts of device.

 

For fabricating the gauge face, use digital design to make gauge graphics then print onto paper, cut by hand, and placed inside a glass layer. Glass cutting save time, lasted long and was bright. The use of lines for vector drawing allowed for the control of angles and scale markings, different from the volume and parameter-driven approach of Fusion 360. At last assembling, for the gauge opening and closing, achieved by cover magnets and an insert into that cover the bottom one. 

## Data Device

The Free-Seats Gauge is designed as a data device that is identical to physical equipment, converting real-time UCl API data into both physical and digital forms simultaneously. The program directly calls the UCL API to obtain real-time seat occupancy data, and the API returns structured JSON data. Then, the data is processed, only extracting the relevant data for the two buildings, Marshgate and One Pool Street, and converting the seat count into a percentage of empty seats.

 

Digital Gauge extends data devices to the user's physical environment through augmented reality technology. Firstly, the 3D model rendered by Blender is imported into Unity as a separate game object, and action listeners are bound to the pointers in the model to enable the pointers to rotate in accordance with the data obtained from the API. Moreover, before editing the code, it is important not to forget to check which axis (xyz) the pointer is rotating around in Unity.

 

Digital Gauge is behaviorally identical to the physical device. The empty seat rate data is converted into rotation angles through linear mapping, driving the pointer to produce continuous and proportional movements. And since the digital twin shares the same data source with the physical device, its display status remains synchronously updated in real time.

 

To present more detailed information, an additional dashboard has been added to the digital twin of the Free Seats Gauge. The dashboard was implemented in Unity as a world-space interface that forms part of the AR-based data device. A world-space canvas was created to allow the dashboard to exist as a three-dimensional object that can be positioned within the physical environment. The interface was composed of multiple UI elements, including numerical text, progress bars, a building selection dropdown, and a floor-level list, enabling both detailed inspection and rapid interpretation of occupancy data. Moreover, based on the real-time seat availability data obtained from the UCL API, the number of empty seats and the availability rate will be calculated. These data will then be used to drive all the visual elements within the dashboard. Additionally, a line graph has been implemented by using XCharts to visually represent the changes in seat availability over time.

 

## Hardware Integration

The hardware system of the Free-Seats includes a microcontroller, a servo motor, an LCD display and a dial button. The microcontroller is responsible for the overall control of the system, receiving the real-time status information of the seats and sending it into controlling status. Servo motor serves as the primary output device, which converts digital occupancy values into rotational displacement to move the pointer across the gauge face.

 

The LCD display presents additional textual prompts to help users get more precise data support for the quick judgment based on the qualitative display of the gauge. The physical button enables direct interaction by the user to achieve the switching of the two buildings.

 

Careful consideration was given to the placement of hardware and the wiring routes in the enclosure design stage to ensure easy access, maintenance and safety. Figure 1 is the design drawing of the circuit. The completed physic device successfully shows real-time response ability, establishes a good connection of strong legibility between live institutional data and physical movement.

<img width="606" height="659" alt="image" src="https://github.com/user-attachments/assets/51600939-a3ba-4cb0-9897-b1695796f012" />


​												Figure1: Wiring diagram

## Results 

## 8.1 Physical Devices

### 8.1.1 Overall Outcome

Through the team's efforts, the physical device was successfully fabricated and assembled. All components are integrated within a 3D-printed enclosure. Functionally, the physical prototype successfully achieves the integration of data acquisition and computation, data visualisation, and human-machine interaction. The system operates with stable and reliable performance, maintaining a stable Wi-Fi connection and continuous data updates without requiring manual intervention.
<img width="380" height="531" alt="image" src="https://github.com/user-attachments/assets/9071e445-07f3-48e5-8dfa-78d9cf454ece" />


​										Figure 2: Prototype appearance

### 8.1.2 Real-Time Data Acquisition 

The system successfully retrieves real-time occupancy data from the UCL API via a secure HTTPS connection. After establishing Wi-Fi connection, the ESP8266 microcontroller periodically sends requests to the API endpoint to obtain the latest seating information.

Upon receiving API data, the microprocessor employs programmed logic to identify and extract information for OPS and MSG zones. This includes occupied seats, vacant seats, and total seating capacity. Testing confirmed the system consistently retrieves valid responses from the API, with data matching real-time values displayed on the UCL server website.

 

### 8.1.3 Physical Gauge Behaviour and Servo Mapping

 
<img width="865" height="448" alt="image" src="https://github.com/user-attachments/assets/6c7d7875-6371-4666-99b0-4bd7ff275394" />


​										Figure 3: Gauge design draft

The gauge of this physical device employs a sector design, with the scale incrementally distributed from 0% to 100%, featuring numerical markings at the 25%, 50% and 75% positions. Additionally, the gauge employs warm and cool colour blocks to visually indicate the occupancy level. 

The MG90S servo motor drives the pointer to rotate across the 120-degree gauge. To ensure precise pointer alignment, the team mechanically calibrated the servo motor's zero-degree position to align perfectly with the gauge's zero mark. Following calibration, the pointer consistently maintains accurate positioning.

 

### 8.1.4 LCD Information Display

In addition to analogue gauges, the physical device also features an LCD screen. This display presents the building identifier (OPS or MSG), seat occupancy rate, and the corresponding numbers of occupied and vacant seats. 

The display updates only after the user confirms the building selection via the rotary encoder. This interactive design ensures the LCD screen remains synchronised with the physical gauges, preventing conflicts or premature updates during preview operations. Testing confirms the LCD screen refreshes reliably, maintaining readability under standard indoor lighting conditions.

# **8.2 Digital Twin**

### 8.2.1 Digital Twin (AR Dashboard) Implementation

The digital twin system serves as a digital extension of physical installations. It operates through augmented reality: users scan a surface with their mobile device to instantly generate a virtual information panel. This AR dashboard remains spatially anchored, seamlessly integrating with the surrounding real-world environment.

Functionally, the AR interface clearly displays the overall seating status of the selected building, including the number of vacant and occupied seats and the vacancy rate percentage. Users may switch between One Pool Street and Marshgate via the top dropdown menu. The system updates data content instantly during switching, with no noticeable delays or interface disruptions.

Compared to physical devices, the digital twin further provides detailed information broken down by floor. Occupied and vacant seat counts for each floor are presented in list format, enabling users to intuitively compare usage patterns. Concurrently, the line chart on the right displays changes in vacant seat numbers over the past 30 minutes, offering a temporal reference for users to assess spatial usage trends.

The AR interface supports drag-and-drop interaction, allowing users to freely adjust panel positions according to their actual viewpoint and spatial constraints. This feature enhances the system's adaptability across different scenarios while preventing real-world obstructions affecting the virtual interface.

Overall, the digital twin successfully transforms extensive seating data into a clearly structured AR dashboard, complementing physical devices. While the physical unit offers rapid perception, the digital twin facilitates deeper, explorable information viewing. 

# **Reflection** 

Upon reflection of the design and implementation process for this project, several issues requiring improvement have been identified. Physically, the magnetic alignment mechanism between the case body and lid exhibited design deviations at corresponding points, failing to achieve precise alignment. This resulted in misalignment after assembly, compromising overall finish and user experience. Secondly, the watch face design lacks cohesion with the case form. The circular dial and hexagonal case exhibit suboptimal dimensional control, resulting in an undersized dial area. This leaves substantial unused space on the display surface, diminishing information presentation efficiency. Concurrently, the physical device lacks clear usage instructions or guidance signage, making it difficult for first-time users to comprehend its functions and operation. Finally, the physical unit employs adhesive tape or glue for fixation rather than an embedded or integrated structure, compromising both structural stability and overall aesthetics. The digital twin component currently displays only partial historical data with a limited temporal scope, diminishing its potential value for long-term trend analysis. At the data level, the system's development architecture limits its capacity to process multi-building datasets, with insufficient scope for future customisation and scalability. These issues provide clear direction for subsequent iterations.

## Conclusion

This project demonstrates how combining a physical gauge with an AR-based digital twin can improve the accessibility of real-time seat availability data. By layering immediate visual feedback with detailed interactive information, the system supports efficient decision-making and highlights the potential of hybrid physical–digital interfaces in campus environments.

## References

·    Autodesk (n.d.) Fusion 360 Learning Panel. Available at: https://help.autodesk.com/view/fusion360/ENU/?guid=LEARNINGPANEL (Accessed: 6 January 2026). 

·    Jansen, Y., Dragicevic, P. and Fekete, J.-D. (2013) ‘Evaluating the efficiency of physical visualizations’, *Proceedings of the SIGCHI Conference on Human Factors in Computing Systems (CHI ’13)*, Paris, France, pp. 2593–2602. Available at: https://doi.org/10.1145/2470654.2481359 (Accessed: 7 January 2026)


·    Moggridge, B. (2007) Designing Interactions. *Cambridge, MA: MIT Press*. Available at: https://mitpress.mit.edu/9780262134743/designing-interactions/ (Accessed: 6 January 2026).
