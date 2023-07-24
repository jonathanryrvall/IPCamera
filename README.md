# Summary
Experimental application for connecting and recording from an IP camera via rtsp stream.

# Features
- Timelapse saved as image files
- Simple motion detection
- Pre recording before motion detection trigger
- Reference image for motion detection gradually remodels becoming an average of the last N frames, increasing detection signal strength for slow moving objects without increasing sensitivity for noise.
- Events logged to log file
- Video files saved in .mp4 format
