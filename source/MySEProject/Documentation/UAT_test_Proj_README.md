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



Temporal Memory (TM) is an essential component of Hierarchical Temporal Memory (HTM), a theoretical framework inspired by the structure and functionality of the human neocortex. Parallel processing in TM aims to enhance its computational efficiency and scalability, particularly for large-scale implementations.


## Technical Details

### Key Changes
1. **Replacing Sequential Loops**: Traditional `for` loops were replaced with `Parallel.For` to enable multithreaded execution.
2. **Async Methods**: Time-intensive operations were refactored to use asynchronous methods for better task concurrency.
3. **Concurrency Management**: Added locking mechanisms where necessary to ensure thread safety and avoid race conditions.



## Performance Comparison

We are comparing the performance of the newly modified method with the old method based on execution time.

### Approach:
- **Old Method**: The execution time of the `Init` method was measured to evaluate its performance.
- **New Method**: The execution time of the `InitAsync` method in the `TemporalMemoryParallelProcessing` class was measured to compare with the old method.

### Observations:
- The comparison focuses on the time taken by both methods to complete the same operation.
- The results provide insights into the performance improvements achieved with the new asynchronous implementation.



### Difference Between async/await and Synchronous Execution:

Synchronous Execution: Tasks are performed one after another, with each task blocking the execution of subsequent tasks until it is completed. This approach can lead to inefficiency in cases where tasks involve significant wait times.

Asynchronous Execution: The async/await pattern allows tasks to run concurrently without blocking the current thread. While waiting for a task to complete, the system can continue executing other tasks or handle other operations, leading to improved responsiveness and potentially faster execution in parallel scenarios.

### Conclusion:
This comparison helps determine whether the newly introduced asynchronous method (`InitAsync`) offers better performance and efficiency compared to the old synchronous method (`Init`).



## References

https://www.researchgate.net/publication/384490210_Parallel_Processing_of_Temporal_Anti-Joins_in_Memory

Introduction to Parallel Computing
https://hpc.llnl.gov/documentation/tutorials/introduction-parallel-computing-tutorial



