# FaceTrackerDemo
Demo integrating openCVSharp4 and wpf to make an app that performs facial/eye recognition on a live camera stream.

## Requirements
- Windows PC
- Video camera connected to PC (I used the internal webcam on my PC for development, but can see no reason an external camera shouldn't work)
- Visual Studio 2022 (may compile/build on earlier or later versions, but this is what I used for development)

## Instructions
1. Download git repository.
2. Open the FaceTrackerDemo.sln file in VS and build either the Debug|Any CPU or Release|Any CPU configuration.
3. Run the program.
4. There will be a Start button to start the camera stream, and a Stop button to stop it.  When one is enabled, the other is disabled.
5. There will be a checkbox to turn facial recognition on and off.  It can be toggled at any time and when streaming is started, and facial recognition is toggled on, will cause blue highlighting around the face and eyes of a person in the camera stream.
6. Close the app window at any time

### Algorithmic notes
The CamModel is the heart of the app.  Basically it:
- use OpenCV to open the camera stream
- loops and reads frames from the camera
- optionally, performs a haar cascade based scan of the image for facial then eye recognition, and draws blue highlights on the frame when found
- converts the read in frame to a bitmap to show on the app view

#### Additional Note
the demo app is hard-coded to use the primary camera on the PC