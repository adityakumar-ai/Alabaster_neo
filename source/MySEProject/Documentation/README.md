# **ML 24/25-06 Implement Temporal Memory Parallel Version**


## Table of Contents
- [Introduction](#introduction)
- [Overview ](#overview)
- [Methodology](#methodology)
- [Implementation](#implementation)
     -  [Single-threaded Init() Method](#1-single-threaded-init-method)

     -  [Implementing Multithreading (Key Changes)](#2-implementing-multithreading-key-changes)

     - [Why Parallel.For is Better Than Async for Matrix Initialization](#why-parallelfor-is-better-than-async-for-matrix-initialization)

     - [Implementing Four Optimization Methods](#3-implementing-four-optimization-methods)
       - [Single-threaded Init()](#1-single-threaded-init-method)
       - [InitParallelRegularDictionary()](#32-initparallelregulardictionary)
       - [InitParallelWithConcurrentDictionary()](#33-initparallelwithconcurrentdictionary)
       - [InitParallelPartitioned()](#34-initparallelpartitioned)


- [Performance Analysis](#4-performance-analysis-and-best-method-selection)


- [Conclusion](#conclusion)

- [Result and Visualization](#5-result-and-visualization)






# <u>Introduction</u>

The Temporal Memory (TM) algorithm is a key component in hierarchical temporal memory (HTM), which is inspired by the human neocortex. This algorithm is responsible for learning and predicting patterns in sequential data, such as time-series or sensor data. While the algorithm performs well in a single-threaded implementation, there is a significant opportunity to improve its performance by leveraging multithreading.

Currently, the Temporal Memory algorithm is implemented as a single-threaded process, which means it processes tasks sequentially. However, for applications involving large datasets or real-time data processing, this can become a bottleneck. The task is to reimplement specific parts of the algorithm to make use of multithreading techniques, enabling concurrent execution of independent tasks. By parallelizing the code, we aim to reduce execution time, improve CPU utilization, and enhance overall system efficiency.

This project focuses on identifying the parts of the Temporal Memory algorithm that can be parallelized, refactoring them to work asynchronously, and measuring the performance improvements. Additionally, we will compare the performance of the original single-threaded implementation with the optimized multithreaded version, using a variety of metrics including execution time and memory usage.


# <u>Overview</u> 

The **Temporal Memory (TM) Algorithm** is currently implemented as a single-threaded process, meaning that it processes tasks one by one, which can be slow for larger datasets or real-time applications. The task is to re-implement specific algorithmic components to leverage the power of **multithreading**, thereby enhancing performance and efficiency. By enabling parallel execution, we aim to reduce processing time and allow the algorithm to scale effectively with larger datasets, making it more suitable for real-time or big data applications. The goal is to optimize the underlying processes that can be executed concurrently, ensuring faster and more efficient performance without compromising the integrity of the algorithm.



# <u>Methodology</u>


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


# <u>**Implementation**</u>

# 1. **Single-threaded `Init()` Method**

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
# **2. Implementing Multithreading (Key Changes)**  
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
# **Why Parallel.For is Better Than Async for Matrix Initialization**

## **1. Understanding the Task**

The task involves initializing a **large matrix** and creating objects in **parallel**. This is a **CPU-bound** operation, meaning it requires maximum CPU efficiency rather than waiting for I/O.

## **2. Why Parallel.For is the Better Choice**
###  **Parallel.For is optimized for CPU-bound tasks**
- It **efficiently utilizes all CPU cores**.
- Uses the **.NET ThreadPool**, avoiding unnecessary thread overhead.
- Provides **fast execution** since it runs in multiple threads without context switching.


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

# **3. Implementing Four Optimization Methods**  

To optimize the initialization process further, we developed four different methods, each implementing a unique approach to parallelization. These methods aim to improve execution time and efficiency while maintaining correctness.  

We will describe each method in detail:  


## **3.1 Single_Threaded_Optimized_Init()**  

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

## **3.2 InitParallelRegularDictionary()**  

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

## **3.3 InitParallelWithConcurrentDictionary()**

This method improves the initialization of columns using **`Parallel.For`** and **`ConcurrentDictionary`**, ensuring better synchronization and faster execution.

## Key Optimizations & Enhancements:

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

### Why `ConcurrentDictionary`?
 **ConcurrentDictionary** allows safe parallel insertions without blocking other threads. This ensures that multiple threads can update the column dictionary concurrently before committing changes to the matrix and reducing contention and improving execution time.


---


## **3.4 InitParallelPartitioned()**

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



# <u>**4. Performance Analysis and Best Method Selection**</u>

In this section, we analyze the performance of each method by calculating key metrics such as Mean, Max, Min, Standard Deviation, Variance, and worst-case performance comparisons for both **Initialization Time** and **Compute Time**. These calculations will help us determine the best performing method by comparing the efficiency and consistency across different methods.

## **4.1 Key Metrics Calculation for Initialization and Compute Time**

Calculated the following key metrics for both **Initialization Time** and **Compute Time** of each method to evaluate their performance:

- **Mean**: The average time across all runs, representing the overall performance.
- **Max**: The highest observed time, indicating the peak performance.
- **Min**: The lowest observed time, indicating the least efficient performance.



## **4.2 Variance and Standard Deviation for Initialization and Compute Time**

To better understand the spread and consistency of both initialization and compute times, we calculated the **Variance** and **Standard Deviation** for each method:

- **Variance**: Measures the degree of variation in times across different runs.
- **Standard Deviation (Std Dev)**: Provides a more intuitive measure of spread, showing the average distance of times from the mean.



## **4.3 Worst-Case Performance Comparison (Max vs Min Time for Initialization and Compute)**

In this step, we calculated the ratio of the **maximum execution time** to the **minimum execution time** for both initialization and compute phases to understand the worst-case performance of each method:

- **Max Time**: The longest time taken in each phase (initialization and compute).
- **Min Time**: The shortest time taken in each phase.
- **Ratio (Max/Min)**: This ratio indicates how much the execution time can vary in each phase. A lower ratio suggests better performance consistency.



## <u>**Conclusion**</u>: 

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

# <u>**5. Result and Visualization**</u>
 

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


---




