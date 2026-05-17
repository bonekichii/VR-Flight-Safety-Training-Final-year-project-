# Video Playback Setup for Safety Demo

## Problem
StereoKit does not have built-in video playback support. We need an external solution.

## Solution Options

### Option 1: Use Video as Image Sequence (SIMPLEST)
1. Download the Emirates video: https://www.youtube.com/watch?v=gFP5VLve3t4
2. Use a tool like FFmpeg to extract frames:
   ```
   ffmpeg -i emirates_video.mp4 -r 10 Assets/video_frame_%04d.png
   ```
3. Load and cycle through the images in code

### Option 2: Use LibVLCSharp (FULL VIDEO)
1. Install LibVLCSharp NuGet package
2. Reference video file
3. Render video texture to the screen

### Option 3: External Video Player Window (QUICK DEMO)
1. Download video to Assets folder
2. Launch system video player when "video starts"
3. User watches in separate window
4. Press C when done

## Current Implementation
Currently using a 15-second timer simulation. Replace with one of the above options.

## Recommended for Your Project
For a final year project demo, **Option 3** is fastest to implement and still demonstrates the concept.
