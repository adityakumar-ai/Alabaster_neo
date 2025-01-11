# Implementation of Temporal Memory Parallel Version
## Group name- Alabaster_neo


This group project aims to implement the parallel version of Temporal Memory. As this algorithm is currenty implemented as single threaded process, the task is to re-implement specific algorithmic components in order to benefit from multithreading and Enhance performance and efficiency.

Enhancing Temporal Memory Algorithm with Parallelization
To Implement this, in short:

* We first understand the basic concept of Temporal Memory Algoritm and its working.
* Replacing the oringal SYNC code, FOR loops and making use of async methods and parallel loops
* Replaced traditional for loops with Parallel.For loops to take advantage of multi-threading capabilities
* Creating addional unit tests and making sure the existing unit test are succesful with the parallel TM Implementation
* Ensured all existing unit tests passed successfully to maintain algorithm integrity and correctness.
* Comparing Performances/Creating graphs
* Conducted performance comparisons between the original and parallel implementations.
* Measured metrics such as execution time, CPU utilization, and memory usage.


## Introduction

Temporal Memory is a foundational algorithm in Hierarchical Temporal Memory (HTM) systems, designed for sequence learning and prediction. While effective, the current implementation operates sequentially, which limits its scalability and performance when processing large datasets or handling real-time applications.

This project seeks to overcome these limitations by introducing parallelization into the Temporal Memory algorithm. Leveraging modern multi-threading techniques, our objective is to enhance efficiency, reduce execution time, and maintain the algorithm's predictive capabilities and correctness.

Additionally, the project involves comparing and analyzing the performance of both single-threaded and multi-threaded implementations, focusing on key metrics such as execution time, CPU utilization, and scalability. Our aim is to deliver a significant performance improvement while preserving the algorithm's integrity.




