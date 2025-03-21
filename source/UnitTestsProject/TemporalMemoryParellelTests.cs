using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexEntities.NeuroVisualizer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTestsProject
{
    public class TemporalMemoryParellelTests
    {

    /*
       -------------------------------------------------------------------------------------------------------------------
       Test Overview for InitParallelWithConcurrentDictionary Method
       -------------------------------------------------------------------------------------------------------------------
       
       The following test cases cover various scenarios for the `InitParallelWithConcurrentDictionary` method 
       in the TemporalMemoryParallelProcessing class. These tests ensure the method handles multiple edge cases 
       and works as expected under different conditions.
       
       0. Test Initialization of Temporal Memory with Invalid Configuration Values
          - Test Initialization of Temporal Memory with Invalid Configuration Values (SynPermConnected, NumInputs, ColumnDimensions, CellsPerColumn).
       
       1. Test when Memory is null and columns need to be created: 
          - Verifies that the method correctly initializes a new SparseObjectMatrix and creates columns when Memory is null.
       
       2. Test with a large number of columns to check for scalability: 
          - Tests the method's scalability by simulating a large number of columns (e.g., thousands) and verifying correct operation.
       
       3. Test with a small number of columns (edge case): 
          - Verifies the behavior when the number of columns is minimal (e.g., 1 or 2), which is a possible edge case.
       
       4. Test with an unusually high synPermConnected value: 
          - Verifies the behavior when the `synPermConnected` value is unusually high (e.g., 1.0), ensuring that the system handles extreme values correctly.
       
       5. Test with the maximum possible number of inputs: 
          - Tests the method with the maximum possible number of inputs (e.g., an extremely large value) to verify that the system can handle a large input space.
       
       6. Correctness of ConcurrentDictionary Thread-Safety with DataRow
          - Validates the thread-safety of ConcurrentDictionary during parallel column initialization, ensuring data integrity across threads.
       
       
       7. This test case compares the performance and functionality 
          - This test case compares the performance and functionality of single-threaded vs. multi-threaded Temporal Memory initialization and computation.
       
       8. The growth of new segments when multiple columns are active.
          - This test case verifies the growth of new segments when multiple columns are active
       
       
       9. Correctness of ConcurrentDictionary Thread-Safety with DataRow
          - This test case verifies the learning and recall of a high-sparsity sequence using both single-threaded and parallel versions of the Temporal Memory algorithm.
       
       10. Active Cells in a Sequence
          - This test case verifies the prediction of active cells in a sequence
       ------------------------------------------------------------------------------------------------------------------
    */

        public class HtmConfig_
        {
            public int ColumnDimensions { get; set; }
            public int CellsPerColumn { get; set; }
            public double SynPermConnected { get; set; }
            public int NumInputs { get; set; }
        }


        // Test Case 0: Test Initialization of Temporal Memory with Invalid Configuration Values

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))] // Expecting an exception for invalid configurations
        [DataRow(-1, 5, 0.1, 5)]  // Invalid ColumnDimensions (negative), valid SynPermConnected, valid NumInputs
        [DataRow(0, 0, 0.1, 5)]   // Invalid ColumnDimensions (zero), invalid CellsPerColumn (zero)
        [DataRow(5, -1, 0.1, 5)]  // Invalid CellsPerColumn (negative), valid SynPermConnected, valid NumInputs
        [DataRow(-5, -5, 0.1, 5)] // Invalid ColumnDimensions and CellsPerColumn (both negative)
        [DataRow(5, 0, 0.1, 5)]   // Invalid CellsPerColumn (zero), valid SynPermConnected, valid NumInputs
        [DataRow(0, 10, 0.1, 5)]  // Invalid ColumnDimensions (zero), valid SynPermConnected, valid NumInputs
        [DataRow(10, 100000, 0.1, 5)] // Unreasonably large CellsPerColumn, valid SynPermConnected, valid NumInputs
        public void Test_InvalidConfiguration(int columnDimensions, int cellsPerColumn, double synPermConnected, int numInputs)
        {
            // Arrange
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();

            var cn = new Connections { Memory = null };

            var config = new HtmConfig_
            {
                ColumnDimensions = columnDimensions,  
                CellsPerColumn = cellsPerColumn,      
                SynPermConnected = synPermConnected,  
                NumInputs = numInputs                 
            };

            // Set the config property using reflection (as it's private)
            var htmConfigProperty = typeof(Connections).GetProperty("HtmConfig", BindingFlags.NonPublic | BindingFlags.Instance);
            htmConfigProperty.SetValue(cn, config);

            // Assert: Check that the SynPermConnected and NumInputs are set correctly
            Assert.AreEqual(synPermConnected, config.SynPermConnected, "SynPermConnected value is not set correctly.");
            Assert.AreEqual(numInputs, config.NumInputs, "NumInputs value is not set correctly.");

            // Act: Try to initialize the matrix with invalid configuration
            tmParallel.InitParallelWithConcurrentDictionary(cn);
        }




        // Test Case 1:  when Memory is null and columns need to be created: 

        [TestClass]
        public class TemporalMemoryTests
        {
            [TestMethod]
            [DataRow(10, 5, 0.1, 5)]
            [DataRow(15, 4, 0.2, 10)]
            [DataRow(20, 6, 0.15, 8)]
            public void Test_InitParallelWithConcurrentDictionary_NewMatrix(int columnDimensions, int cellsPerColumn, double synPermConnected, int numInputs)
            {
                // Arrange
                TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
                var cn = new Connections();
                cn.Memory = null; // Simulate Memory being null

                // Create an HtmConfig instance
                var config = new HtmConfig_
                {
                    ColumnDimensions = columnDimensions,
                    CellsPerColumn = cellsPerColumn,
                    SynPermConnected = synPermConnected,
                    NumInputs = numInputs
                };

                // Use reflection to set the private property HtmConfig
                var htmConfigProperty = typeof(Connections).GetProperty("HtmConfig", BindingFlags.NonPublic | BindingFlags.Instance);
                if (htmConfigProperty != null)
                {
                    htmConfigProperty.SetValue(cn, config);  // Set the value using reflection
                }

                // Act
                tmParallel.InitParallelWithConcurrentDictionary(cn);

                // Assert
                Assert.IsNotNull(cn.Memory, "Matrix should be initialized.");
                Assert.IsTrue(cn.Memory is SparseObjectMatrix<Column>, "Matrix should be of correct type.");
                Assert.AreEqual(columnDimensions, cn.Memory.GetMaxIndex() + 1, "Matrix should contain the expected number of columns.");
                Assert.AreEqual(columnDimensions * cellsPerColumn, cn.Cells.Length, "Cells array should contain the correct number of cells.");
            }
        }





        // Test Case 2: Test with a large number of columns

        [TestMethod]
        [DataRow(1000, 10, 0.2, 50)]  // 1000 columns, 10 cells per column, synPermConnected = 0.2, numInputs = 50
        public void Test_InitParallelWithConcurrentDictionary_LargeColumnCount(int columnDimensions, int cellsPerColumn, double synPermConnected, int numInputs)
        {
            // Arrange
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            var cn = new Connections { Memory = null };  // Simulate Memory being null

            var config = new HtmConfig_
            {
                ColumnDimensions = columnDimensions,
                CellsPerColumn = cellsPerColumn,
                SynPermConnected = synPermConnected,
                NumInputs = numInputs
            };

            var htmConfigProperty = typeof(Connections).GetProperty("HtmConfig", BindingFlags.NonPublic | BindingFlags.Instance);
            htmConfigProperty.SetValue(cn, config);

            // Act
            tmParallel.InitParallelWithConcurrentDictionary(cn);

            // Assert
            Assert.IsNotNull(cn.Memory, "Matrix should be initialized.");
            Assert.IsTrue(cn.Memory is SparseObjectMatrix<Column>, "Matrix should be of correct type.");
            Assert.AreEqual(columnDimensions, cn.Memory.GetMaxIndex() + 1, "Matrix should contain the expected number of columns.");
            Assert.AreEqual(columnDimensions * cellsPerColumn, cn.Cells.Length, "Cells array should contain the correct number of cells.");
        }




        // Test Case 3: Test with a small number of columns (edge case)

        [TestMethod]
        [DataRow(1, 5, 0.1, 5)]  // 1 column, 5 cells per column, synPermConnected = 0.1, numInputs = 5
        public void Test_InitParallelWithConcurrentDictionary_SingleColumn(int columnDimensions, int cellsPerColumn, double synPermConnected, int numInputs)
        {
            // Arrange
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            var cn = new Connections { Memory = null };  // Simulate Memory being null

            var config = new HtmConfig_
            {
                ColumnDimensions = columnDimensions,
                CellsPerColumn = cellsPerColumn,
                SynPermConnected = synPermConnected,
                NumInputs = numInputs
            };

            var htmConfigProperty = typeof(Connections).GetProperty("HtmConfig", BindingFlags.NonPublic | BindingFlags.Instance);
            htmConfigProperty.SetValue(cn, config);

            // Act
            tmParallel.InitParallelWithConcurrentDictionary(cn);

            // Assert
            Assert.IsNotNull(cn.Memory, "Matrix should be initialized.");
            Assert.IsTrue(cn.Memory is SparseObjectMatrix<Column>, "Matrix should be of correct type.");
            Assert.AreEqual(columnDimensions, cn.Memory.GetMaxIndex() + 1, "Matrix should contain the expected number of columns.");
            Assert.AreEqual(columnDimensions * cellsPerColumn, cn.Cells.Length, "Cells array should contain the correct number of cells.");
        }




        // Test Case 4: Test with an unusually high synPermConnected value
        // synPermConnected:- Determines how likely the cells in a column are to be connected to each other and their neighboring columns

        [TestMethod]
        [DataRow(10, 5, 1.0, 5)]  // 10 columns, 5 cells per column, synPermConnected = 1.0 (maximum), numInputs = 5
        public void Test_InitParallelWithConcurrentDictionary_MaxSynPermConnected(int columnDimensions, int cellsPerColumn, double synPermConnected, int numInputs)
        {
            // Arrange
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            var cn = new Connections { Memory = null };  // Simulate Memory being null

            var config = new HtmConfig_
            {
                ColumnDimensions = columnDimensions,
                CellsPerColumn = cellsPerColumn,
                SynPermConnected = synPermConnected,
                NumInputs = numInputs
            };

            var htmConfigProperty = typeof(Connections).GetProperty("HtmConfig", BindingFlags.NonPublic | BindingFlags.Instance);
            htmConfigProperty.SetValue(cn, config);

            // Act
            tmParallel.InitParallelWithConcurrentDictionary(cn);

            // Assert
            Assert.IsNotNull(cn.Memory, "Matrix should be initialized.");
            Assert.IsTrue(cn.Memory is SparseObjectMatrix<Column>, "Matrix should be of correct type.");
            Assert.AreEqual(columnDimensions, cn.Memory.GetMaxIndex() + 1, "Matrix should contain the expected number of columns.");
            Assert.AreEqual(columnDimensions * cellsPerColumn, cn.Cells.Length, "Cells array should contain the correct number of cells.");
        }



        // Test Case 5: Test with the maximum possible number of inputs

        [TestMethod]
        [DataRow(10, 5, 0.1, 1000)]  // 10 columns, 5 cells per column, synPermConnected = 0.1, numInputs = 1000 (max inputs)
        public void Test_InitParallelWithConcurrentDictionary_MaxInputs(int columnDimensions, int cellsPerColumn, double synPermConnected, int numInputs)
        {
            // Arrange
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            var cn = new Connections { Memory = null };  // Simulate Memory being null

            var config = new HtmConfig_
            {
                ColumnDimensions = columnDimensions,
                CellsPerColumn = cellsPerColumn,
                SynPermConnected = synPermConnected,
                NumInputs = numInputs
            };

            var htmConfigProperty = typeof(Connections).GetProperty("HtmConfig", BindingFlags.NonPublic | BindingFlags.Instance);
            htmConfigProperty.SetValue(cn, config);

            // Act
            tmParallel.InitParallelWithConcurrentDictionary(cn);

            // Assert
            Assert.IsNotNull(cn.Memory, "Matrix should be initialized.");
            Assert.IsTrue(cn.Memory is SparseObjectMatrix<Column>, "Matrix should be of correct type.");
            Assert.AreEqual(columnDimensions, cn.Memory.GetMaxIndex() + 1, "Matrix should contain the expected number of columns.");
            Assert.AreEqual(columnDimensions * cellsPerColumn, cn.Cells.Length, "Cells array should contain the correct number of cells.");
        }






        // Test Case 6: Correctness of ConcurrentDictionary Thread-Safety with DataRow
        [TestMethod]
        [DataRow(10, 5, 0.1, 5)]  // Valid input configuration
        [DataRow(20, 10, 0.5, 10)] // Different configuration
        [DataRow(50, 25, 0.3, 15)] // Another configuration
        [DataRow(100, 50, 0.7, 20)] // Large input configuration
        public void Test_ConcurrentDictionary_ThreadSafety(int columnDimensions, int cellsPerColumn, double synPermConnected, int numInputs)
        {
            // Arrange
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            var cn = new Connections { Memory = null };

            // Define a configuration for the test using the DataRow values
            var config = new HtmConfig_
            {
                ColumnDimensions = columnDimensions,   // Value from DataRow
                CellsPerColumn = cellsPerColumn,        // Value from DataRow
                SynPermConnected = synPermConnected,    // Value from DataRow
                NumInputs = numInputs                   // Value from DataRow
            };

            // Set the config property using reflection (as it's private)
            var htmConfigProperty = typeof(Connections).GetProperty("HtmConfig", BindingFlags.NonPublic | BindingFlags.Instance);
            htmConfigProperty.SetValue(cn, config);

            // Act: Initialize the matrix concurrently with multiple threads
            tmParallel.InitParallelWithConcurrentDictionary(cn);

            // Assert: Validate the correctness of ConcurrentDictionary thread-safety
            var matrix = (SparseObjectMatrix<Column>)cn.Memory;
            var numColumns = matrix.GetMaxIndex() + 1;

            // Check if columns are correctly initialized
            for (int i = 0; i < numColumns; i++)
            {
                var column = matrix.GetObject(i);
                Assert.IsNotNull(column, $"Column {i} should not be null.");
                Assert.AreEqual(config.CellsPerColumn, column.Cells.Length, $"Column {i} should have {config.CellsPerColumn} cells.");
            }

            // Validate that there are no race conditions by ensuring no column is missing or duplicated
            var columnDict = new ConcurrentDictionary<int, Column>();

            // Add columns to dictionary and verify each one is unique
            Parallel.For(0, numColumns, i =>
            {
                var column = matrix.GetObject(i);
                columnDict.TryAdd(i, column);
            });

            Assert.AreEqual(numColumns, columnDict.Count, "There should be exactly as many columns as specified in the configuration.");

            // Ensure no duplication of keys (columns should not overwrite one another)
            for (int i = 0; i < numColumns; i++)
            {
                Assert.IsTrue(columnDict.ContainsKey(i), $"Column with key {i} is missing in the dictionary.");
            }

        }



        //Test Case 7 : This test case compares the performance and functionality of single-threaded vs. multi-threaded Temporal Memory initialization and computation.
        [TestMethod]
        public void TestBasicSequenceLearningAndRecallParallel()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();


            // Set default parameters, specifying a column dimension of 64
            Parameters p = GetDefaultParameters(null, KEY.COLUMN_DIMENSIONS, new int[] { 64 });
            p.apply(cn);

            // Initialize single-threaded and parallel versions
            tm.Init(cn);
            tmParallel.InitParallelWithConcurrentDictionary(cn);

            // Define a basic sequence of active columns
            int[] sequenceActiveColumns = { 0, 1, 2, 3, 4, 5, 6 };

            // Act
            // Learn the sequence in single-threaded mode
            stopwatch.Start();
            ComputeCycle singleThreadedCycle = tm.Compute(sequenceActiveColumns, true) as ComputeCycle;
            stopwatch.Stop();
            TimeSpan singleThreadedComputeTime = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for single-threaded Compute: {singleThreadedComputeTime.TotalMilliseconds} milliseconds");

            // Learn the sequence in parallel mode
            stopwatch.Restart();
            ComputeCycle parallelCycle = tmParallel.Compute(sequenceActiveColumns, true) as ComputeCycle;
            stopwatch.Stop();
            TimeSpan parallelComputeTime = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for parallel Compute: {parallelComputeTime.TotalMilliseconds} milliseconds");

            // Recall the sequence in parallel mode
            ComputeCycle recallCycle = tmParallel.Compute(sequenceActiveColumns, false) as ComputeCycle;

            // Assert
            Assert.IsTrue(recallCycle.ActiveCells.Count > 0, "No active cells were recalled.");
        }


        // Test Case 8: This test case verifies the growth of new segments when multiple columns are active
        [TestMethod]
        public void TestSequenceLearningWithHighSparsity()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters, specifying a column dimension of 100
            Parameters p = GetDefaultParameters(null, KEY.COLUMN_DIMENSIONS, new int[] { 100 });
            p.apply(cn);

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Define a high-sparsity sequence of active columns
            var seq1ActiveColumns = new int[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90 };
            var seq2ActiveColumns = new int[] { 5, 15, 25, 35, 45, 55, 65, 75, 85, 95 };

            // Measure and store execution time for single-threaded computation (tm.Compute)
            stopwatch.Start();
            tm.Compute(seq1ActiveColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute)
            stopwatch.Start();
            tmParallel.Compute(seq1ActiveColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Recall the first sequence
            var recall1 = tm.Compute(seq1ActiveColumns, false);
            var recall2 = tm.Compute(seq2ActiveColumns, false);

            // Verify that all active cells in the recalled second sequence are also present in the recalled first sequence
            Assert.IsTrue(recall2.ActiveCells.Select(c => c.Index).All(rc => recall1.ActiveCells.Select(c => c.Index).Contains(rc)));
        }


        // Test Case 9: This test case verifies the learning and recall of a high-sparsity sequence using both single-threaded and parallel versions of the Temporal Memory algorithm.
        [TestMethod]
        public void TestSegmentGrowthWithMultipleActiveColumns()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters
            Parameters p = GetDefaultParameters(null, KEY.COLUMN_DIMENSIONS, new int[] { 100 });
            p.apply(cn);

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Define active columns and corresponding active cells
            int[] activeColumns = { 0, 1, 2, 3, 4 };
            Cell[] activeCells = { cn.GetCell(0), cn.GetCell(1), cn.GetCell(2), cn.GetCell(3), cn.GetCell(4) };

            // Measure and store execution time for single-threaded computation (tm.Compute)
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute)
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Verify that new segments have been grown
            Assert.AreEqual(5, activeCells[0].DistalDendrites.Count);
        }



        // Test Case 10: This test case verifies the prediction of active cells in a sequence
        [TestMethod]
        public void TestActiveCellPrediction()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters
            Parameters p = GetDefaultParameters(null, KEY.COLUMN_DIMENSIONS, new int[] { 100 });
            p.apply(cn);

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Define active columns and corresponding active cells
            int[] previousActiveColumns = { 0, 1, 2 };
            int[] activeColumns = { 3, 4, 5 };
            Cell[] previousActiveCells = { cn.GetCell(0), cn.GetCell(1), cn.GetCell(2) };
            Cell[] activeCells = { cn.GetCell(3), cn.GetCell(4), cn.GetCell(5) };

            // Measure and store execution time for single-threaded computation (tm.Compute)
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute)
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Verify that the predicted active cells match the expected active cells
            Assert.IsTrue(activeCells.All(ac => tm.Compute(previousActiveColumns, false).ActiveCells.Contains(ac)));
        }


        private Parameters GetDefaultParameters(object parameter1, string key, int[] values)
        {
            // Create a new set of parameters using the default settings for temporal memory.
            Parameters retVal = Parameters.getTemporalDefaultParameters();

            // Set specific parameters for the temporal memory.
            retVal.Set(key, values);  // Set the value based on the key and values provided.
            retVal.Set(KEY.CELLS_PER_COLUMN, 5);                  // Default cells per column
            retVal.Set(KEY.ACTIVATION_THRESHOLD, 3);              // Default activation threshold
            retVal.Set(KEY.INITIAL_PERMANENCE, 0.21);             // Default initial permanence value
            retVal.Set(KEY.CONNECTED_PERMANENCE, 0.5);            // Default connected permanence value
            retVal.Set(KEY.MIN_THRESHOLD, 2);                     // Default minimum threshold
            retVal.Set(KEY.MAX_NEW_SYNAPSE_COUNT, 3);             // Default max new synapse count
            retVal.Set(KEY.PERMANENCE_INCREMENT, 0.10);           // Default permanence increment
            retVal.Set(KEY.PERMANENCE_DECREMENT, 0.10);           // Default permanence decrement
            retVal.Set(KEY.PREDICTED_SEGMENT_DECREMENT, 0.0);     // Default predicted segment decrement

            // Set the random number generator with a specified seed for reproducibility
            retVal.Set(KEY.RANDOM, new ThreadSafeRandom(42));

            // Set the seed for reproducibility
            retVal.Set(KEY.SEED, 42);

            return retVal;
        }


    }
}
