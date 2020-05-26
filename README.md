# AvatarFacialControl
Reading peoples expressions in virtual reality is hard. One of the reasons for this is the lack of real facial expression readings from the user. This project is experimenting with facial animation reflecting emotions in avatars for Virtual Reality. using Unity 3D. This project using Emotive Insights for reading EEG/EMG signals, utilizing the Emotive Cortex API. 

Git includes the source required to build facial expressions using Emotive Cortex API, in Unity.


Requirements:
-Unity
- 3D facemodel. Model need's to have keyshapes defined in advance.


Installation:
Just drag and drop to you HeadModel in Unity 3D. 


Additonal dependencies:
NEWTONSOFT JSon. 

FaceExpressions.cs file is not required. the structures in here are a duplicate. This file will be removed later

Limitations:
Project do not support lipsync, sine there is a limitation in the headset's not reading the required frequency band for detecting lip motion.
