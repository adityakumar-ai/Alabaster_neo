# **ML 24/25-06 Implement Temporal Memory Parallel Version**

**The Temporal Memory algorithm is currently implemented as a single-threaded process. This project focuses on refactoring key parts of the algorithm to leverage **multithreading** and improve performance. By identifying **parallelizable execution paths** and using efficient concurrency patterns, we aim to enable **parallel execution** while ensuring correctness and maintaining test coverage.**

# **Overview**
- **<u>[Introduction](#introduction)</u>**  
- **<u>[Overview](#overview)</u>**  
- **<u>[Methodology](#methodology)</u>**  
- **<u>[Implementation](#implementation)</u>**  
  - **<u>[Single-threaded Init() Method](#1-single-threaded-init-method)</u>**  
  - **<u>[Implementing Multithreading (Key Changes)](#2-implementing-multithreading-key-changes)</u>**  
  - **<u>[Why Parallel.For is Better Than Async for Matrix Initialization](#why-parallelfor-is-better-than-async-for-matrix-initialization)</u>**  
  - **<u>[Implementing Four Optimization Methods](#3-implementing-four-optimization-methods)</u>**  
    - **<u>[Single-threaded Init()](#1-single-threaded-init-method)</u>**  
    - **<u>[InitParallelRegularDictionary()](#32-initparallelregulardictionary)</u>**  
    - **<u>[InitParallelWithConcurrentDictionary()](#33-initparallelwithconcurrentdictionary)</u>**  
    - **<u>[InitParallelPartitioned()](#34-initparallelpartitioned)</u>**  
- **<u>[Performance Analysis](#4-performance-analysis-and-best-method-selection)</u>**  
- **<u>[Conclusion](#conclusion)</u>**  
- **<u>[Result and Visualization](#5-result-and-visualization)</u>**  


# **Problem Statement**
The Temporal Memory (TM) algorithm is currently implemented as a single-threaded process, which limits its performance and scalability. This project aims to optimize the algorithm by refactoring it to utilize multithreading, thereby improving execution speed and resource efficiency. The task involves identifying areas of the code that can be parallelized, such as iterative loops and independent operations, and applying appropriate multithreading techniques to enhance performance. The success of the refactoring will be evaluated through performance benchmarking, comparing execution times before and after parallelization. Additionally, all existing unit tests will be validated, and new tests will be introduced to ensure the correctness and stability of the parallelized algorithm under various conditions.




# **Introduction**

The Temporal Memory (TM) algorithm is a key component in hierarchical temporal memory (HTM), which is inspired by the human neocortex. This algorithm is responsible for learning and predicting patterns in sequential data, such as time-series or sensor data. While the algorithm performs well in a single-threaded implementation, there is a significant opportunity to improve its performance by leveraging multithreading.

Currently, the Temporal Memory algorithm is implemented as a single-threaded process, which means it processes tasks sequentially. However, for applications involving large datasets or real-time data processing, this can become a bottleneck. The task is to reimplement specific parts of the algorithm to make use of multithreading techniques, enabling concurrent execution of independent tasks. By parallelizing the code, we aim to reduce execution time, improve CPU utilization, and enhance overall system efficiency.

This project focuses on identifying the parts of the Temporal Memory algorithm that can be parallelized, refactoring them to work asynchronously, and measuring the performance improvements. Additionally, we will compare the performance of the original single-threaded implementation with the optimized multithreaded version, using a variety of metrics including execution time and memory usage.




# **Optimization Strategy**


To implement the parallelization improvement, the following steps were taken:

### 1. **Understanding Temporal Memory Algorithm** :  
   - First, we thoroughly understood the basic concept of Temporal Memory and how it works in the context of the current implementation.

### 2. **Replacing Original Synchronous Code**  **Parallelizing the Code** :  
   - The original synchronous code was analyzed, and critical parts that could be parallelized, such as  **for loops**, were identified and replaced with **Parallel.For loops** to enable concurrent execution. 
   - Replaced traditional **for loops** with **Parallel.For loops** to leverage multithreading capabilities, ensuring tasks could run concurrently, thus reducing execution time.

### 3. **Creating Four Methods with Different Logic for Optimization** :  
   - We created four different methods, each implementing a different logic or concept to optimize the execution time:
     - **Single_Threaded_Optimized_Init()**
     - **InitParallelRegularDictionary()**
     - **InitParallelWithConcurrentDictionary()**
     - **InitParallelPartitioned()**
   - Each method was designed to test various parallelization strategies, optimizations, and execution patterns.
   - This allowed us to compare and contrast the performance of the different methods under similar conditions and determine which one provided the best optimization for the Temporal Memory algorithm.

### 4. **Performance Analysis** :  
   - After implementing the parallelized methods, we analyzed the results by comparing the performance of the original and parallel implementations.
   - We measured metrics like **execution time**, **CPU utilization**, **memory usage**, and more.
   - For each method, we calculated performance statistics such as **mean**, **maximum**, **minimum**, **variance**, **standard deviation**, etc., to make an informed decision on which method performed better.

### 5. **Result** - Visualizing Performance Data for InitParallelWithConcurrentDictionary() :  
   - We visualized the performance improvements using **graphs** and **charts** to clearly highlight key metrics like **execution time** and **initialization time**.
   - The graphs helped in visually comparing the two implementations and provided insights into the performance difference between the original and parallelized Temporal Memory algorithm.


# **Implementation**

## 1. **Single-threaded `Init()` Method**

- The original `Init()` method is implemented in a **single-threaded** fashion.
- It uses a **single `for` loop** to iterate through all columns and initialize them one at a time.
- The loop processes each column **sequentially**, meaning each iteration depends on the previous one.
- Inside this loop, there is another `for` loop to initialize the cells within each column, also processed sequentially.
  


```csharp
public void Init(Connections conn)
{
    ---
    ---
    Column colZero = matrix.GetObject(0);
    for (int i = 0; i < numColumns; i++)
    {
        Column column = colZero == null ? 
            new Column(cellsPerColumn, i, this.connections.HtmConfig.SynPermConnected, this.connections.HtmConfig.NumInputs) : matrix.GetObject(i);

        for (int j = 0; j < cellsPerColumn; j++)
        {
            cells[i * cellsPerColumn + j] = column.Cells[j];
        }
        //If columns have not been previously configured
        if (colZero == null)
            matrix.set(i, column);

    }
    ---
    ---
}
```
## **2. Implementing Multithreading (Key Changes)**  
 The key changes in the multithreaded implementation include:  

- **Replacing traditional `for` loops with `Parallel.For`** to distribute work across multiple threads.  
- **Ensuring thread safety** by using appropriate data structures such as `ConcurrentDictionary`.  
- **Minimizing synchronization overhead** while maintaining correctness.  

#### **Key Code Modification (Parallel Loop)**  
Instead of sequentially processing columns and cells, we implemented parallel execution:  

```csharp
Parallel.For(0, numColumns, i =>
{
    // Create columns concurrently and store them in the dictionary
    var column = new Column(cellsPerColumn, i, this.connections.HtmConfig.SynPermConnected, this.connections.HtmConfig.NumInputs);
    columnDict[i] = column; // Thread-safe insertion into dictionary

    // Copy cells for each column
    for (int j = 0; j < cellsPerColumn; j++)
    {
        cells[i * cellsPerColumn + j] = column.Cells[j];
    }
});

// Set columns to matrix after parallel operations
foreach (var kvp in columnDict)
{
    matrix.set(kvp.Key, kvp.Value);
}
```
## **Why Parallel.For is Better Than Async for Matrix Initialization**

## **1. Understanding the Task**

The task involves initializing a **large matrix** and creating objects in **parallel**. This is a **CPU-bound** operation, meaning it requires maximum CPU efficiency rather than waiting for I/O.

When initializing a large matrix, each element in the matrix can be initialized independently of the others. The task at hand doesn't involve waiting for external resources but instead requires CPU power to process each element in parallel.

**Parallel.For** can split the work across multiple threads, fully utilizing the available CPU cores. By doing so, it can reduce the time it takes to initialize the entire matrix, as multiple threads can work concurrently, each processing different portions of the matrix.

The Parallel.For loop runs synchronously, dividing the matrix initialization work into chunks that are handled by different threads, making full use of the processor's multi-core architecture.

## **2. Why Parallel.For is the Better Choice**
###  **Parallel.For is optimized for CPU-bound tasks**
- **Automatic Load Balancing:**
Parallel.For automatically divides tasks into chunks and balances them across available threads, optimizing execution.

- **Scalability:**
As the dataset grows, Parallel.For scales efficiently, leveraging multiple CPU cores for faster processing.

- **Optimized for Short-Lived Tasks:**
It is ideal for tasks like matrix initialization, where each operation is computationally expensive but short-lived.

- **Reduced Synchronization Overhead:**
Since iterations are independent, there's no need for complex synchronization mechanisms, reducing potential bottlenecks.

- **No Explicit Thread Management:**
Parallel.For abstracts thread management, letting the .NET ThreadPool handle it, simplifying implementation.

- **Dynamic Thread Adjustment:**
The ThreadPool adjusts the number of threads based on system resources, ensuring efficient execution on both multi-core and single-core systems.

- **Non-Blocking Execution:**
Parallel.For runs multiple threads concurrently, maximizing throughput without waiting for each thread to finish before starting the next.


###  **Async is meant for I/O-bound tasks, not CPU-heavy operations**
- `async/await` works best when waiting for **network, database, or file operations**.
- In a CPU-intensive task, `Task.Run()` **adds unnecessary overhead**.
- **No performance improvement**, only extra task scheduling.
```csharp
public async Task InitParallelWithConcurrentDictionaryAsync(Connections conn)
{
    ---
    ---

    if (createNewColumns)
    {
        var columnDict = new ConcurrentDictionary<int, Column>();

        await Task.Run(() =>
        {
            Parallel.For(0, numColumns, i =>
            {
                var column = new Column(cellsPerColumn, i, 
                    this.connections.HtmConfig.SynPermConnected, 
                    this.connections.HtmConfig.NumInputs);
                
                columnDict[i] = column;
                for (int j = 0; j < cellsPerColumn; j++)
                    cells[i * cellsPerColumn + j] = column.Cells[j];
            });
        });

        foreach (var kvp in columnDict)
            matrix.set(kvp.Key, kvp.Value);
    }

    this.connections.Cells = cells;
}
```


## **3. Performance Comparison**

| Aspect                | Parallel.For (Recommended)           | Async (Unnecessary Overhead)     |
|-----------------------|--------------------------------------|----------------------------------|
| **Best For**          | CPU-bound tasks                     | I/O-bound tasks                 |
| **Execution Speed**   | Faster                               | Slower (Extra task scheduling)  |
| **CPU Utilization**   | Fully utilized                       | Context switching overhead      |
| **Use of ThreadPool** | Yes (efficient)                      | Yes, but unnecessary overhead   |
| **Performance**       | Optimized for multi-threading     |  Adds delay without benefits   |


## **Key Optimizations for Compute ActivateDendrites():**  

**Implemented Parallel Processing for Faster Execution**  
- **Used `Parallel.ForEach`** – Enables multi-threaded processing of synapses.  
- **Reduces Execution Time** – Processes active and matching segments concurrently.  

**Replaced List with `ConcurrentBag` for Thread Safety**  
-  **Ensures Safe Multi-Threaded Access** – Eliminates race conditions when adding segments.  
-  **Improves Performance** – Allows segments to be processed independently in parallel loops.  

**Sorted Active and Matching Segments Efficiently**  
- **Converted `ConcurrentBag` to List Before Sorting** – Ensures optimal sorting without affecting parallel execution.  
-  **Uses `GetComparer(conn.NextSegmentOrdinal)`** – Maintains correct order with minimal overhead.  

 **Optimized Learning Process with Parallel Execution**  
- **Processes Segment Activity in Parallel** – Uses `Parallel.ForEach` for recording segment activity.  
-  **Reduces Bottleneck in Learning Phase** – Ensures faster updates to segment records.  

**Removed Redundant Code and Simplified Logic**  
- **Removed Unnecessary Data Copies** – Avoids redundant operations on `cycle.ActiveSegments` and `cycle.MatchingSegments`.  
- **Optimized Predictive Cell Handling** – Calls `conn.ClearPredictiveCells()` efficiently before processing next iteration.  


```csharp
protected void ActivateDendrites2_(Connections conn, ComputeCycle cycle, bool learn, int[] externalPredictiveInputsActive = null, int[] externalPredictiveInputsWinners = null)
{
    ---
    ---

    // Step 3: Use AsParallel() to improve parallel performance dynamically
    activity.ActiveSynapses.AsParallel().ForAll(item =>
    {
        if (item.Value >= conn.HtmConfig.ActivationThreshold)
        {
            var seg = conn.GetSegmentForFlatIdx(item.Key);
            if (seg != null)
            {
                activeSegments.Enqueue(seg);
            }
        }
    });

    activity.PotentialSynapses.AsParallel().ForAll(item =>
    {
        var seg = conn.GetSegmentForFlatIdx(item.Key);
        if (seg != null && item.Value >= conn.HtmConfig.MinThreshold)
        {
            matchingSegments.Enqueue(seg);
        }
    });

    ---
    ---
}
```

## **3. Implementing Four Optimization Methods**  

To optimize the initialization process further, we developed four different methods, each implementing a unique approach to parallelization. These methods aim to improve execution time and efficiency while maintaining correctness.  

We will describe each method in detail:  


### **3.1 Single_Threaded_Optimized_Init()**  

This method refines the single-threaded initialization process by improving efficiency and reducing unnecessary operations.  

### **Key Optimizations:**  

### **1.Used Null-Coalescing Operator (`??`) Instead of Ternary Operator (`? :`)**  
-  **Faster Execution** – Eliminates unnecessary type casting, improving performance.  
-  **More Readable** – `??` provides a cleaner approach for checking and assigning a default value. 
```csharp
// Using the null-coalescing operator for cleaner assignment
SparseObjectMatrix<Column> matrix = this.connections.Memory as SparseObjectMatrix<Column>
    ?? new SparseObjectMatrix<Column>(this.connections.HtmConfig.ColumnDimensions);
```
### **2.Reduced Redundant Checks with `createNewColumns` Flag**  
- **Avoids Rechecking in Every Loop Iteration** – Previously, `matrix.GetObject(0) == null` was checked multiple times inside the loop.  
- **Stores the Result in a Variable** – The check is performed once before the loop, significantly reducing overhead.  
```csharp
// Storing the result of matrix.GetObject(0) == null in a flag
bool createNewColumns = matrix.GetObject(0) == null;

for (int i = 0; i < numColumns; i++)
{
    Column column = createNewColumns
        ? new Column(cellsPerColumn, i, this.connections.HtmConfig.SynPermConnected, this.connections.HtmConfig.NumInputs)
        : matrix.GetObject(i);
}

```
### **3.Removed Unnecessary Variable (`colZero`)**  
- Eliminates the `colZero` Variable – It was redundant and added unnecessary complexity.  
- Uses a Boolean Flag (`createNewColumns`) – More efficient than checking the same condition repeatedly.  
```csharp
// Removed the colZero variable and used the createNewColumns flag
bool createNewColumns = matrix.GetObject(0) == null;
for (int i = 0; i < numColumns; i++)
{
    Column column = createNewColumns
        ? new Column(cellsPerColumn, i, this.connections.HtmConfig.SynPermConnected, this.connections.HtmConfig.NumInputs)
        : matrix.GetObject(i);
}

```

---

### **3.2 InitParallelRegularDictionary()**  

This method enhances the initialization process by leveraging multithreading for improved performance and efficiency.  

### **Key Improvements & Optimizations:**  

### **1. Used `Parallel.For` for Parallel Column Initialization**  
- **Faster Execution** – Distributes column creation and cell assignment across multiple threads.  
- **Optimized for Large Data Sets** – Improves performance significantly when handling large numbers of columns. 
 
```csharp
// Parallel initialization of columns if needed
Parallel.For(0, numColumns, i =>
{
    var column = new Column(cellsPerColumn, i, this.connections.HtmConfig.SynPermConnected, this.connections.HtmConfig.NumInputs);

    // Lock around matrix modification to ensure thread safety
    lock (matrix)
    {
        matrix.set(i, column);
    }

    // Lock around the cells array to ensure thread safety while filling it
    lock (cells)
    {
        for (int j = 0; j < cellsPerColumn; j++)
        {
            cells[i * cellsPerColumn + j] = column.Cells[j];
        }
    }
});

```
### **2. Reduced Redundant Checks with `createNewColumns` Flag**  
- **Avoids Rechecking in Every Loop Iteration** – The original code checked `matrix.GetObject(0) == null` multiple times inside the loop.  
- **Stored the Result in a Variable** – The condition is checked once before the loop, reducing unnecessary computations.

```csharp
// Storing the result of matrix.GetObject(0) == null in a flag
bool createNewColumns = matrix.GetObject(0) == null;


```
### **3. Moved Column Creation and Cell Copying Inside Parallel Loop**  
-  **Consolidates Operations** – Column creation and the copying of cells are done together in parallel, ensuring better use of resources.  
-  **Eliminated Sequential Loops** – By merging column creation, matrix assignment, and cell copying, the code reduces overhead.  

```csharp
// Column creation and cell copying done inside the parallel loop
Parallel.For(0, numColumns, i =>
{
    var column = new Column(cellsPerColumn, i, this.connections.HtmConfig.SynPermConnected, this.connections.HtmConfig.NumInputs);

    // Lock around matrix modification to ensure thread safety
    lock (matrix)
    {
        matrix.set(i, column);
    }

    // Lock around the cells array to ensure thread safety while filling it
    lock (cells)
    {
        for (int j = 0; j < cellsPerColumn; j++)
        {
            cells[i * cellsPerColumn + j] = column.Cells[j];
        }
    }
});

```

### **4. Improved Thread Safety with Direct Matrix Set**  
-  **Minimized Synchronization Issues** – `Parallel.For` ensures columns are set in parallel, with thread-safe operations using `lock` mechanisms.  
- **Avoids Extra Loops** – The original method had a separate loop for setting the matrix; now it's handled within the parallel execution block.  
```csharp
// Lock around matrix modification to ensure thread safety
lock (matrix)
{
    matrix.set(i, column);
}
```

---

### **3.3 InitParallelWithConcurrentDictionary()**

This method improves the initialization of columns using **`Parallel.For`** and **`ConcurrentDictionary`**, ensuring better synchronization and faster execution.

### Key Optimizations & Enhancements:

###  **1. Used `Parallel.For` for Parallel Column Initialization**
-  **Faster Execution** – Distributes column creation and cell assignment across multiple threads.
- **Optimized for Large Data Sets** – Enhances performance significantly when handling thousands of columns.

```csharp
// Parallel column initialization if needed
Parallel.For(0, numColumns, i =>
{
    var column = new Column(cellsPerColumn, i, this.connections.HtmConfig.SynPermConnected, this.connections.HtmConfig.NumInputs);
    columnDict[i] = column; // Thread-safe insertion into dictionary

    // Copy cells for each column
    for (int j = 0; j < cellsPerColumn; j++)
    {
        cells[i * cellsPerColumn + j] = column.Cells[j];
    }
});
```

### **2. Reduced Redundant Checks with `createNewColumns` Flag**
- **Avoids Rechecking in Every Loop Iteration** – Prevents unnecessary evaluations inside the loop.
- **Stored the Result in a Variable** – The `matrix.GetObject(0) == null` condition is checked only once before the loop.

```csharp
// Storing the result of matrix.GetObject(0) == null in a flag
bool createNewColumns = matrix.GetObject(0) == null;

```
### **3. Utilized `ConcurrentDictionary` for Thread Safety**
- **Improved Synchronization** – Ensures thread-safe column storage during parallel execution.
- **Avoids Data Race** – Protects against race conditions by handling dictionary operations atomically.


```csharp
// Use ConcurrentDictionary to store columns during parallel initialization
var columnDict = new ConcurrentDictionary<int, Column>();

```

### **4. Parallelized Column Creation and Cell Copying**
- **Optimized for Performance** – Merges column creation, matrix assignment, and cell copying in parallel, maximizing thread utilization.
- **Eliminated Sequential Loops** – Reduces overhead by removing unnecessary separate loops.


```csharp
// Set columns to matrix after parallel operations
foreach (var kvp in columnDict)
{
    matrix.set(kvp.Key, kvp.Value);
}

```
---

### **Why ConcurrentDictionary?**

**ConcurrentDictionary** is a thread-safe collection designed for scenarios where multiple threads need to access and modify data concurrently. Unlike a regular `Dictionary`, it allows safe parallel insertions without causing data corruption or requiring manual synchronization.

- **Thread Safety:**  
  It ensures that multiple threads can safely add or update elements without risk of data inconsistency or race conditions.

- **Lock-Free Operations:**  
  Internal fine-grained locking minimizes blocking, allowing multiple threads to perform operations on different parts of the data without waiting for others.

- **Atomic Operations:**  
  Operations like `Add Or Update` are atomic, ensuring that each operation completes without interference from other threads.

- **Improved Performance:**  
  By reducing the need for blocking and synchronization, **ConcurrentDictionary** optimizes performance in high-concurrency scenarios, making it ideal for parallel matrix initialization.

In summary, **ConcurrentDictionary** enhances execution time by enabling safe, efficient, and concurrent updates to shared data in a multithreaded environment.


---


### **3.4 InitParallelPartitioned()**

### **Key Optimizations & Enhancements:**

###  **1. Partitioned Parallel Initialization**
-  **Faster Execution** – Processes columns in smaller partitions, reducing overhead and optimizing performance for large datasets.
- **Optimized for Large Data Sets** – Enhances performance when dealing with extensive data by dividing tasks into smaller, manageable chunks.

```csharp
// Partitioning the columns into chunks to avoid excessive overhead
var partitioner = Partitioner.Create(0, numColumns);

Parallel.ForEach(partitioner, (range, loopState) =>
{
    for (int i = range.Item1; i < range.Item2; i++)
    {
        Column column = new Column(cellsPerColumn, i, this.connections.HtmConfig.SynPermConnected, this.connections.HtmConfig.NumInputs);
        
        lock (matrix)  // Ensure thread-safe update
        {
            matrix.set(i, column);  // Set column at index i
        }

        // Copy cells for each column
        for (int j = 0; j < cellsPerColumn; j++)
        {
            int cellIndex = i * cellsPerColumn + j;

            if (cellIndex >= cells.Length)
                throw new IndexOutOfRangeException($"Invalid cell index {cellIndex}, max allowed: {cells.Length - 1}");

            cells[cellIndex] = column.Cells[j];
        }
    }
});
```

### **2. Thread-Safe Matrix Updates**
- **Ensures Safe Updates** – Uses `lock(matrix)` to prevent concurrent access issues while modifying the matrix in parallel.
- **Minimizes Synchronization Overhead** – The locking mechanism ensures safety while reducing performance bottlenecks.

```csharp
lock (matrix)  // Ensure thread-safe update
{
    matrix.set(i, column);  // Set column at index i
}
```

###  **3. Column and Cell Validation**
- **Validates Parameters** – Ensures a valid column count and correct `cellsPerColumn` to maintain data integrity.
-  **Prevents Initialization Errors** – Ensures that only valid and appropriate data is processed.

```csharp
// Validate numColumns
if (numColumns <= 0)
    throw new InvalidOperationException("Invalid number of columns calculated.");

// Validate cellsPerColumn
if (cellsPerColumn <= 0)
    throw new InvalidOperationException("Invalid number of cells per column.");
```

###  **4. Index Safety Check**
-  **Prevents Out-of-Range Errors** – Ensures `cellIndex` remains within bounds during parallel execution.
-  **Enhances Reliability** – Protects against runtime exceptions by validating array access.

```csharp
if (cellIndex >= cells.Length)
    throw new IndexOutOfRangeException($"Invalid cell index {cellIndex}, max allowed: {cells.Length - 1}");
```

### **5. Memory Optimization**
- **Reuses Memory** – Efficiently utilizes allocated memory for better performance.
- **Optimized Resource Usage** – Reduces memory overhead by avoiding unnecessary object creation.

```csharp
// Initialize matrix either from Memory or create a new one.
SparseObjectMatrix<Column> matrix = this.connections.Memory == null
    ? new SparseObjectMatrix<Column>(this.connections.HtmConfig.ColumnDimensions)
    : (SparseObjectMatrix<Column>)this.connections.Memory;
```

---
## **📊 Method Comparison**

| **Metric**              | **InitParallel** | **InitCompute** | **InitCount** | **InitParallelPartitioned** |
|-------------------------|:---------------:|:--------------:|:------------:|:--------------------------:|
| **Execution Speed**      | **✔** | **✔** | **✔** | **✔** |
| **Parallel Processing**  | **✔** | **❌** | **✔** | **✔** |
| **Thread Safety**        | **❌** | **❌** | **✔** | **✔** |
| **Column Creation**      | **✔** | **❌** | **✔** | **✔** |
| **Cell Validation**      | **❌** | **❌** | **✔** | **✔** |



## **4. Performance Analysis and Best Method Selection**

In this section, we analyze the performance of each method by calculating key metrics such as Mean, Max, Min, Standard Deviation, Variance, and worst-case performance comparisons for both **Initialization Time** and **Compute Time**. These calculations will help us determine the best performing method by comparing the efficiency and consistency across different methods.

## **4.1 Key Metrics Calculation for Initialization and Compute Time**

Calculated the following key metrics for both **Initialization Time** and **Compute Time** of each method to evaluate their performance:

- **Mean**: The average time across all runs, representing the overall performance.
- **Max**: The highest observed time, indicating the peak performance.
- **Min**: The lowest observed time, indicating the least efficient performance.

![Alt text](https://github.com/adityakumar-ai/Alabaster_neo/blob/UAT_Test/source/MySEProject/All_Images/Initialising-Min-max_01.jpg)

![Alt text](https://github.com/adityakumar-ai/Alabaster_neo/blob/UAT_Test/source/MySEProject/All_Images/ExecutionTime-Min-max_02.jpg)

## **4.2 Variance and Standard Deviation for Initialization and Compute Time**

To better understand the spread and consistency of both initialization and compute times, we calculated the **Variance** and **Standard Deviation** for each method:

- **Variance**: Measures the degree of variation in times across different runs.
- **Standard Deviation (Std Dev)**: Provides a more intuitive measure of spread, showing the average distance of times from the mean.

![Alt text](https://github.com/adityakumar-ai/Alabaster_neo/blob/UAT_Test/source/MySEProject/All_Images/Variance_SD_Initialising_03.jpg)

![Alt text](https://github.com/adityakumar-ai/Alabaster_neo/blob/UAT_Test/source/MySEProject/All_Images/Variance_SD_Execution_04.jpg)


## **4.3 Worst-Case Performance Comparison (Max vs Min Time for Initialization and Compute)**

In this step, we calculated the ratio of the **maximum execution time** to the **minimum execution time** for both initialization and compute phases to understand the worst-case performance of each method:

- **Max Time**: The longest time taken in each phase (initialization and compute).
- **Min Time**: The shortest time taken in each phase.
- **Ratio (Max/Min)**: This ratio indicates how much the execution time can vary in each phase. A lower ratio suggests better performance consistency.

![Alt text](https://github.com/adityakumar-ai/Alabaster_neo/blob/UAT_Test/source/MySEProject/All_Images/Min-Max_Initialising_05.jpg)

![Alt text](https://github.com/adityakumar-ai/Alabaster_neo/blob/UAT_Test/source/MySEProject/All_Images/Min-Max_Execution_06.jpg)


## **Optimal Method Selection:** 

**Why `InitParallelWithConcurrentDictionary()` is the Best Choice ??**

After thoroughly analyzing the execution times across all four methods, `InitParallelWithConcurrentDictionary()` has emerged as the best choice for the following compelling reasons:

### **1. Lowest Mean Execution Time**   
   `InitParallelWithConcurrentDictionary()` demonstrates the fastest average execution time, outperforming all other methods. This is the most significant factor in determining the overall efficiency and speed of the method.

### **2. Low Standard Deviation and Variance**  
   With a low standard deviation and low variance, `InitParallelWithConcurrentDictionary()` offers consistent performance across different runs. A smaller standard deviation means the execution time doesn't vary widely, ensuring reliable results every time.

### **3. Low Max/Min Ratio**  
   `InitParallelWithConcurrentDictionary()` also boasts the lowest max/min ratio, signifying that even in the worst-case scenario, its performance is comparable to its best performance, ensuring stability and predictability.

In summary, `InitParallelWithConcurrentDictionary()` strikes the perfect balance between speed, consistency, and stability, making it the optimal method for our use case. Given its superior performance in terms of:
- **Mean Execution Time**
- **Consistent Performance** (Low variance)
- **Predictable Stability** (Low Max/Min ratio)

`InitParallelWithConcurrentDictionary()` is the clear best choice for this analysis, offering both efficiency and reliability.

---

# **5. Result and Visualization**
 

**Visualizing Performance Data for InitParallelWithConcurrentDictionary()**

To further understand the performance of **InitParallelWithConcurrentDictionary()**, we are visualizing key metrics through various graphs. These graphs will provide insights into the method's execution times, variance, and consistency. By analyzing these visuals, we can gain a deeper understanding of its efficiency and stability in comparison to other methods.


### **5.1 Bar Chart for Initialization and Computation Time Comparison**

This code generates **grouped bar charts** comparing both **Initialization Times** and **Computation Times** across different test cases for the `InitParallelWithConcurrentDictionary()` method.

- **X-axis:** Represents the test cases (with the first 3 letters of each test case name displayed).
- **Y-axis:** Represents the time in seconds.
- **Color Coding:**
  - Blue for **Initialization Time**
  - Green for **Computation Time**

The first chart compares the initialization performance between standard and parallel methods for each test case, while the second chart focuses on computation times. These visuals help us assess both the **initialization** and **computation** performance of the **InitParallelWithConcurrentDictionary()** method in relation to traditional methods.

![Alt text](https://github.com/adityakumar-ai/Alabaster_neo/blob/UAT_Test/source/MySEProject/All_Images/Bar_Graph_07.jpg)

![Alt text](https://github.com/adityakumar-ai/Alabaster_neo/blob/UAT_Test/source/MySEProject/All_Images/Bar_Graph_08.jpg)


---

### **5.2 Scatter Plot for Initialization and Computation Time Comparison**

This **scatter plot** visualizes the relationship between the initialization time (`InitParallel_Time`) and computation time for the `InitParallelWithConcurrentDictionary()` method.

- **X-axis:** Represents the test cases.
- **Y-axis:** Represents the execution times in seconds (both initialization and computation times).
- **Color Coding:**
  - **Blue Circles**: Represent **`Init_Time`** (Standard Initialization Time).
  - **Red Crosses**: Represent **`InitParallel_Time`** (Parallel Initialization Time).
  
The first scatter plot compares the initialization performance between standard and parallel methods for each test case. The second plot compares the computation times.

These scatter plots allow for a visual comparison of the two methods (standard vs parallel) in terms of both initialization and computation times, helping us assess how closely the performance of these methods relates across different test cases.

![Alt text](https://github.com/adityakumar-ai/Alabaster_neo/blob/UAT_Test/source/MySEProject/All_Images/Scatter_plot_09.jpg)

![Alt text](https://github.com/adityakumar-ai/Alabaster_neo/blob/UAT_Test/source/MySEProject/All_Images/Scatter_plot_10.jpg)



### **5.3 Comparison of New Method (InitParallel - Multi-thread) vs. Old Method (Init - Single-thread) Across Various CPU Cores**

In this section, we compare the performance of the new multi-threaded method (`InitParallel`) with the old single-threaded method (`Init`) across different CPU core configurations (4, 6, 8, 10, and All cores) on differnt PC'S. The results are visualized through box-and-whisker plots, highlighting the differences in initialization and computation times.

- **New Method (InitParallel)**: Utilizes a multi-threading approach for faster initialization.
- **Old Method (Init)**: Relies on a single-thread approach for initialization.


### Graph between InitParallel_Time vs iComputeParallel_Time for All CPU Cores on Different PCs
This graph visualizes the performance comparison between initialization time (`InitParallel_Time`) and computation time (`iComputeParallel_Time`) for different CPU core configurations across multiple PCs. The graph provides insight into how both tasks scale with the number of CPU cores.

![Graph 1](https://github.com/adityakumar-ai/Alabaster_neo/blob/UAT_Test/source/MySEProject/All_Images/Graph_01.jpg)
![Graph 1](https://github.com/adityakumar-ai/Alabaster_neo/blob/UAT_Test/source/MySEProject/All_Images/Graph_02.jpg)
![Graph 1](https://github.com/adityakumar-ai/Alabaster_neo/blob/UAT_Test/source/MySEProject/All_Images/Graph_03.jpg)

### Graph for Evaluation Across Different PCs Using Various CPU Cores
This graph compares the performance of both the new multi-threaded method (`InitParallel`) and the old single-threaded method (`Init`) across various CPU configurations (4, 6, 8, 10, and All cores) on different PCs. The goal is to evaluate how the number of cores affects the execution time of both methods.

![Graph 1](https://github.com/adityakumar-ai/Alabaster_neo/blob/UAT_Test/source/MySEProject/All_Images/Graph_04.jpg)
![Graph 1](https://github.com/adityakumar-ai/Alabaster_neo/blob/UAT_Test/source/MySEProject/All_Images/Graph_05.jpg)
![Graph 1](https://github.com/adityakumar-ai/Alabaster_neo/blob/UAT_Test/source/MySEProject/All_Images/Graph_06.jpg)




### View Detailed Analysis and Visualizations

For a comprehensive look at the analysis and visualizations, including interactive graphs and data analysis, please click on this [notebook](#https://github.com/adityakumar-ai/Alabaster_neo/blob/UAT_Test/source/MySEProject/Documentation/NoteBook.ipynb) to explore the full dataset and visual comparisons. The notebook provides an in-depth exploration of the performance metrics and the effects of various configurations across different CPUs.


## **Conclusion :**

In conclusion, this project successfully optimized the Temporal Memory algorithm by refactoring it to leverage multi-threading techniques, significantly improving both execution speed and resource efficiency. Through various methods, the InitParallelWithConcurrentDictionary() emerged as the most effective approach, outperforming other strategies in terms of execution time, consistency, and stability. The use of Parallel.For for parallel execution and ConcurrentDictionary for thread-safe concurrent updates proved to be highly beneficial, ensuring minimal contention and better scalability with increasing CPU cores. This multi-threaded approach not only enhanced the algorithm's performance for large datasets but also made it more suitable for real-time applications. As the number of CPU cores increased, the performance improvements were evident, highlighting the scalability of the solution. The project demonstrated that by optimizing critical sections of the algorithm, significant performance gains can be achieved, making the Temporal Memory algorithm more efficient and robust.  Overall, the optimized algorithm offers a reliable and high-performing solution for large-scale and real-time data processing tasks.

[Go to Top](#)



