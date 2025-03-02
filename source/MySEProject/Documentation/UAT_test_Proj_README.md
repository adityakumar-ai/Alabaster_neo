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




### Time Comparision 

eg:- Method Name :- TestNewSegmentGrowthWhenMultipleMatchingSegmentsFound()


| Method                                  | Difference (ms) | Percentage Time Increase (Compared to `Init`) |
|-----------------------------------------|-----------------|-----------------------------------------------|
| Init                                    | -               | -                                             |
| InitAsync (Using Parallel.for)          | 3.9923          | 19.24%                                        |
| async await method (Using Parallel.for) | 4.196           | 20.22%                                        |

### Conclusion:
This comparison helps determine whether the newly introduced asynchronous method (`InitAsync`) offers better performance and efficiency compared to the old synchronous method (`Init`).



## Single_Threaded_Optimized_Init()

### 1. Used Null-Coalescing Operator (`??`) Instead of Ternary Operator (`? :`)
- ✅ **Faster Execution** – Eliminates unnecessary type casting.
- ✅ **More Readable** – `??` is a cleaner approach for checking and assigning a default value.

### 2. Reduced Redundant Checks with `createNewColumns` Flag
- ✅ **Avoids Rechecking in Every Loop Iteration** – The original code checked `matrix.GetObject(0) == null` multiple times inside the loop.
- ✅ **Stores the Result in a Variable** – The check is done once before the loop, making it faster.

### 3. Removed Unnecessary Variable (`colZero`)
- ✅ **Eliminates the `colZero` Variable** – It was unnecessary and just duplicated the check.
- ✅ **Uses a Boolean Flag (`createNewColumns`)** – More efficient than checking the same condition repeatedly.



### `InitParallelRegularDictionary()

### 1. Used `Parallel.For` for Parallel Column Initialization**
   - ✅ **Faster Execution** – Distributes column creation and cell assignment across multiple threads.
   - ✅ **Optimized for Large Data Sets** – Improves performance significantly when handling large numbers of columns.

### 2. Reduced Redundant Checks with `createNewColumns` Flag**
   - ✅ **Avoids Rechecking in Every Loop Iteration** – The original code checked `matrix.GetObject(0) == null` multiple times inside the loop.
   - ✅ **Stored the Result in a Variable** – The condition is checked once before the loop, reducing unnecessary computations.

### 3. Moved Column Creation and Cell Copying Inside Parallel Loop**
   - ✅ **Consolidates Operations** – Column creation and the copying of cells are done together in parallel, ensuring better use of resources.
   - ✅ **Eliminated Sequential Loops** – By merging column creation, matrix assignment, and cell copying, the code reduces overhead.

### 4. Improved Thread Safety with Direct Matrix Set**
   - ✅ **Minimized Synchronization Issues** – The `Parallel.For` ensures columns are set in parallel, with thread-safe operations using `ConcurrentDictionary`.
   - ✅ **Avoids Extra Loops** – The original method had a separate loop for setting the matrix; now it's handled within the parallel block.



### `InitParallelWithConcurrentDictionary()

### 1. Parallelized Column Initialization**
   - **In the InitParallelRegularDictionary Approach:** Used `Parallel.For` for parallel column initialization.
   - **In the InitParallelWithConcurrentDictionary Approach:** Still using `Parallel.For`, but with the addition of `ConcurrentDictionary` to handle parallel writes to the `matrix`. This ensures thread-safe column assignment.

### 2. Thread-Safe Column Assignment**
   - **In the InitParallelRegularDictionary Approach:** The matrix was updated directly within the `Parallel.For` loop without a mechanism for ensuring thread safety.
   - **In the InitParallelWithConcurrentDictionary Approach:** Introduces `ConcurrentDictionary`, which is specifically designed for thread-safe operations. This prevents data races when updating columns concurrently, improving reliability in multithreaded environments.

### 3. Optimized Copying of Cells**
   - **In the InitParallelRegularDictionary Approach:** Cells were copied after the parallel column creation in a separate loop, causing redundant operations.
   - **In the InitParallelWithConcurrentDictionary Approach:** Cells are now copied within the parallel loop itself, eliminating the need for a separate loop and making the operation more efficient.

  
### InitParallelPartitionedForEach()
### 1. Used `Parallel.ForEach` with Partitioner for Efficient Work Distribution**
- ✅ **Optimized for Large Data Sets** – Partitions the work into smaller chunks, allowing efficient execution across multiple threads.
- ✅ **Improved Thread Management** – Divides the task into ranges and processes each range in parallel, optimizing CPU usage.

### 2. Column Initialization and Cell Assignment Inside Parallel Block**
- ✅ **Consolidates Operations** – Combines column retrieval, assignment, and cell copying within a single parallelized block.
- ✅ **Avoids Extra Loops** – No need for a separate loop for column assignment or cell copying, minimizing overhead.

### 3. Partitioning Ensures Better Performance in Highly Parallelized Workloads**
- ✅ **Improved Thread Safety** – Using partitioned ranges reduces potential race conditions and ensures better synchronization between threads.
- ✅ **More Efficient on Large Datasets** – Perfect for handling large numbers of columns (`numColumns ≥ 1000`) while distributing the workload evenly across threads.



### Comparison of `InitParallelPartitionedForEach()` vs `Parallel.For` vs `Single-Threaded Optimized

| **Aspect**                          | **`InitParallelPartitionedForEach()`**                                         | **`Parallel.For`**                                               | **`Single-Threaded Optimized`**                                  |
|-------------------------------------|----------------------------------------------------------------------------|------------------------------------------------------------------|------------------------------------------------------------------|
| **Work Distribution**               | ✅ Uses `Partitioner` to efficiently divide work into smaller chunks.      | ✅ Works on the entire dataset, may be less efficient for large sets. | ❌ Sequential execution, less efficient for large datasets.      |
| **Thread Management**               | ✅ Efficient thread management via partitioning.                           | ✅ Parallel execution but lacks fine thread control.             | ❌ Single-threaded execution.                                   |
| **Column Initialization and Cell Assignment** | ✅ Combines both in the parallel block, reducing overhead.               | ✅ Parallelizes column init, but requires separate loops for setting and copying. | ❌ Sequential, with separate loops for each task.                |
| **Performance on Large Datasets**   | ✅ Optimized for large datasets (`numColumns ≥ 1000`).                    | ✅ Works well for large datasets but lacks partitioning.         | ❌ Performance drops with larger datasets.                       |



### Conclusion:
- **`InitParallelPartitionedForEach()`** is the most optimized solution for large datasets (`numColumns ≥ 1000`), ensuring better performance and thread management through partitioning.
- **`Parallel.For`** is better than single-threaded but lacks partitioning and fine-grained control over threads.
- **`Single-Threaded Optimized`** is the simplest and most efficient for small datasets but doesn't scale well for large numbers of columns.



## Optimized ActivateDendrites2Async()

### 1. Used `Task.Run()` for Asynchronous Activity Computation
- ✅ **Non-Blocking Execution** – Offloads `ComputeActivity` computation to a separate thread.
- ✅ **Improves Responsiveness** – Prevents blocking the main thread while computing activity.

### 2. Replaced `Parallel.ForEach` with `Parallel.Invoke` for Independent Tasks
- ✅ **Better CPU Utilization** – Executes active and potential synapse processing in parallel.
- ✅ **Minimizes Task Scheduling Overhead** – Reduces thread context switching.

### 3. Used `Parallel.Invoke` for Sorting
- ✅ **Parallel Sorting** – Sorts `sortedActiveSegments` and `sortedMatchingSegments` concurrently.
- ✅ **Faster Execution** – Reduces sequential processing time.

### 4. Maintained Parallel Learning Processing
- ✅ **Preserved Parallel Execution** – `RecordSegmentActivity` runs in `Parallel.ForEach`.
- ✅ **Ensures Efficient Learning** – Maximizes CPU usage while maintaining logic.


## Optimized ActivateCells Function (ActivateCells_Omi)

- ✅ **Eliminated Unnecessary Ordering** – Removed `.OrderBy(i => i)`, since sorting is not needed for parallel execution.
- ✅ **Used Parallel.ForEach with Partitioner** – Instead of a simple `Parallel.For`, used `Partitioner` to optimize thread utilization.
- ✅ **Avoided Unnecessary Object Conversions** – Directly used `ConcurrentBag<Column>` and avoided `.ToArray()` where possible.
- ✅ **Used HashSet<Cell> for Faster Lookups** – Lookups in `prevActiveCells` and `prevWinnerCells` are now O(1) instead of O(n).
- ✅ **Removed Redundant List Conversions** – Avoided unnecessary `.Cast<object>().ToList()` operations.
- ✅ **Reduced Memory Allocations** – Directly iterated over existing structures instead of creating unnecessary intermediate lists.


### Parallel.Invoke() is used to run active synapse processing and potential synapse processing in parallel, ensuring both tasks execute simultaneously without blocking each other.

## References

https://www.researchgate.net/publication/384490210_Parallel_Processing_of_Temporal_Anti-Joins_in_Memory

Introduction to Parallel Computing
https://hpc.llnl.gov/documentation/tutorials/introduction-parallel-computing-tutorial



