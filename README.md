# Simple-Self-Driving-Character

> Bismillahi Rahmani Rahim. All praise be to Allah, Who teaches human to His Kalam and gives him a skills. *This expression is **not** propaganda. It expresses only a tribute of respect and gratitude of the author to the true Creator.*

Simple self driving character project on Unity, with machine learning was inspired by [Udacity self driving car simulator](https://github.com/udacity/self-driving-car-sim). 

The aim of this project - is to help people to upgrade their skills, increase knowledge and share some ideas in machine learning. 

## Content
This project consist of two main parts: 
1. Unity part
2. Machine learning part

Also project has one additional part: 

3. Dataset. 

### Unity part
Unity project responsible for environment, agent & environment interaction and data collecting. In order to describe this part in more detail we gonna divide it to some logical blocks:

+ Map
+ Character control
+ Data collecting
+ TCP Socket communication

#### Map
On environment map we have 5 zones, 2 of them are passable in clockwise and counterclockwise directions. They are shown at the picture below with names.

![alt text](https://github.com/meiirbergali24/Simple-Self-Driving-Character/blob/master/Images/Map.png "Map")

Red dots represents starting points at each zone. The zones Octus and Quadro are passable in both directions.

![alt text](https://github.com/meiirbergali24/Simple-Self-Driving-Character/blob/master/Images/Octus.png "Octus") ![alt text](https://github.com/meiirbergali24/Simple-Self-Driving-Character/blob/master/Images/Quadro.png "Quadro")

You can choose the zone on "Choose zone" menu.

![alt text](https://github.com/meiirbergali24/Simple-Self-Driving-Character/blob/master/Images/Choise.png "Zones")

#### Character control
The character is controlling using the keyboard "arrow" buttons.

![alt text](https://github.com/meiirbergali24/Simple-Self-Driving-Character/blob/master/Images/Keyboard.png "Arrow buttons")

At any time of character controlling process you can press "Esc" button and choose another zone.

#### Data collecting
You can start collecting your data by chosing "Collecting data for training" mode at the "Choose mode" menu.

![alt text](https://github.com/meiirbergali24/Simple-Self-Driving-Character/blob/master/Images/Modes.png "Modes")

Then you need to choose zone on "Choose zone" menu. And finally you get playing screen on which you have 2 buttons and field that displays name of current zone. Press "Start collecting!" button when you will be ready to collect data. You can finish collecting data by pressing "Stop collecting!" button. "Restart" button will return you to initial coordinates and direction.

![alt text](https://github.com/meiirbergali24/Simple-Self-Driving-Character/blob/master/Images/DataCollectingPlaying.png "Data collecting playing screen")

The data collected by simulating ["lidar"](https://en.wikipedia.org/wiki/Lidar). Simulation provides by ray casting as on [Udacity self driving car simulator](https://github.com/udacity/self-driving-car-sim). It has vertical slice consist of 64 rays. Rays start at +12.7 degrees from the horizon, to -37.3 degrees (picture A). It has 180 degree view range by horizontal, from left to right (picture B). Also we have by one slice for each of 180 degree. So we get pictures with a resolution of 180х64 pixels, and green channel only (picture C). Each pixel displays the distance from the character to the object. The maximal distance is 120. 

##### *Picture A*
![alt text](https://github.com/meiirbergali24/Simple-Self-Driving-Character/blob/master/Images/Slice.png "Slice")

##### *Picture B*
![alt text](https://github.com/meiirbergali24/Simple-Self-Driving-Character/blob/master/Images/View.png "View")

##### *Picture C*
![alt text](https://github.com/meiirbergali24/Simple-Self-Driving-Character/blob/master/Images/Lidar.png "Lidar")

For each zone created it's own folder called "Zone_name Dataset" with subfolder called "images" which contains an images generated by lidar. All images from specific zone saves to its specified folder. There is also a file called "labels.csv". In first column of this file are recording names of generated images with ".jpg" extension. The second column contain a direction of character at the moment of generating an image from the first column.

*Example*

|    Image Name    | Direction |
|------------------|:---------:|
|   image_1.jpg    |     1     |
|   image_2.jpg    |     4     |
|   image_3.jpg    |     2     |

There are four directions:

| Label  |    Name     |   Button    |
|:------:|:-----------:|:-----------:|
|    1   |   Forward   |  Up Arrow   |
|    2   |  Rightward  | Right Arrow |
|    3   |  Stopward   | Down Arrow  |
|    4   |  Leftward   | Left Arrow  |

#### TCP Socket communication
There is a class in the project called "TCPConnection" which responsible for environment & agent interaction via TCP sockets. To connect to the agent via TCP socket you need to choose "Testing algorithm" mode in "Choose mode" menu. 
> **Note: Unity part work as client and ML part work as server. So that is why you need to start Machine learning part(server side) first and then press "Connect" button.**

![alt text](https://github.com/meiirbergali24/Simple-Self-Driving-Character/blob/master/Images/Modes.png "Modes")



Then you need to choose zone on "Choose zone" menu. And finally you get playing screen on which you have 1 "Connect" button and 2 input field for local IP address and port number. Also there is a field that displays name of current zone. After entering ip address and port number of your ML part, press "Connect" button to start environment and agent communication.

![alt text](https://github.com/meiirbergali24/Simple-Self-Driving-Character/blob/master/Images/TestingPlayingScreen.png "Testing playing screen")

After pressing "Connect" button ML part(server) send first command as string, for example "forward". The Unity part(client) receive this command and change character direction. After that Unity send new image generated by lidar, to ML part as byte array. When MP part receive this byte array, it converts to image for a model to predict next direction.


![alt text](https://github.com/meiirbergali24/Simple-Self-Driving-Character/blob/master/Images/Env&Agent.png "Environment & Agent")

### Dataset
Dataset was created by combinig data, collected from Quadro clockwise and conterclockwise directions. the passage in both directions was carried out along two paths shown below: 

![alt text](https://github.com/meiirbergali24/Simple-Self-Driving-Character/blob/master/Images/Quadro_way1.png "Way №1") ![alt text](https://github.com/meiirbergali24/Simple-Self-Driving-Character/blob/master/Images/Quadro_way2.png "Way №2")

The Quadro zone was chosen, because it good way to train model to turn till the 90 degree. Also this zone is simple so it easy to regulate the number of classes like: turn right, turn left and go forward. Whereas, complex zones, including different types and ranges of turning could be hard to keep ballance between classes. It should also be noted that when choosing this way of data collection, I did not rely only on logic but also on intuition. So feel free to try your own way to collect and process data. Good luck!
