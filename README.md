# CAB401-Fourier-Transform-Parallelisation
This is a project that explores the analysis and parallelisation of an existing sequential program.

## Project Overview
For this project I decided to parallelise and optimise a sequential Digital Music Analysis C# application. The application is designed to give a music student without a teacher feedback on a recording of a song that they have played. It can tell whether each note played was sharp or flat and if the notes were played for the correct duration. The application applies a Fourier Transform to an audio file which unpacks the sampled signal back into its strongest individual frequencies which it then compares with the original musics notes contained in an XML file to determin whether the right note is being played or not.

I analysed the program to determin where most of the copmute power was being used. After which I analysed the expensive algorithms to determin dependancies and refactor the algorithm into a more efficient parallel form. A particular focus was the fourier transform algorithm implementation which is where most of the performance improvements could be found.

## Folders & Files
* Project Report.pdf
  * This file contains a full report detailing the overall program, dependancies analysed, restructured/replaced algorithms and time profiling of the parallel version.
* Program C# Files
  * This folder contains all the modified c# files detailed in the report.
* DigitalMusicAnalysis (Parallel).zip
  * This zip folder contains the full project which can be run in Visual Studio (further run instructions provided in report).
* test-music
  * This folder contains a test sample of music and scores to be used with the program.
