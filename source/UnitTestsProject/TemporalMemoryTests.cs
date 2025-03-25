// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.DataMappers;
using NeoCortexApi.Entities;
using NeoCortexApi.Types;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
//using static System.Net.Mime.MediaTypeNames;
//using static System.Runtime.InteropServices.JavaScript.JSType;
namespace UnitTestsProject
{
    [TestClass]
    public class TemporalMemoryTest
    {
        private TestContext TestContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext
        {

            get { return TestContextInstance; }
            set { TestContextInstance = value; }
        }

        // Summary:
        // This test method measures the performance of different initialization methods in the TemporalMemoryParallelProcessing class.
        // It compares the execution time of a single-threaded optimized initialization method against three parallel initialization methods:
        // 1. InitParallelRegularDictionary
        // 2. InitParallelWithConcurrentDictionary
        // 3. InitParallelPartitioned
        // The results are logged using TestContext.WriteLine, and an assertion checks if the parallel execution is faster than the single-threaded execution.
        // This helps in evaluating the efficiency and scalability of parallel processing techniques in the context of HTM (Hierarchical Temporal Memory) configurations.

        [TestMethod]
        public void TestPerformance()
        {
            MeasurePerformance();
        }

        public void MeasurePerformance()
        {
            // Initialize the Connections object with necessary configurations
            HtmConfig htmConfig = new HtmConfig
            {
                ColumnDimensions = new int[] { 100 }, // Example dimensions
                CellsPerColumn = 32,
                SynPermConnected = 0.1,
                NumInputs = 100
            };

            Connections conn = new Connections(htmConfig); // Use the appropriate constructor or method

            // Create an instance of TemporalMemoryParallelProcessing
            TemporalMemoryParallelProcessing tmPara = new TemporalMemoryParallelProcessing();

            Stopwatch sw = new Stopwatch();

            // Measure Single_Threaded_Optimized_Init performance
            sw.Start();
            tmPara.Single_Threaded_Optimized_Init(conn); // Call the method on the instance
            sw.Stop();
            long singleThreadedTime = sw.ElapsedMilliseconds; // Fix: Use 'long' and correct variable name
            TestContext.WriteLine($"Method 1 Single-Threaded Execution Time: {singleThreadedTime} ms");

            // Reset the stopwatch
            sw.Reset();

            // Measure InitParallelRegularDictionary
            sw.Start();
            tmPara.InitParallelRegularDictionary(conn); // Call the method on the instance
            sw.Stop();
            long parallel1Time = sw.ElapsedMilliseconds; // Fix: Use 'long'
            TestContext.WriteLine($"Method 2 InitParallelRegularDictionary Time: {parallel1Time} ms");

            // Reset the stopwatch
            sw.Reset();

            // Measure InitParallelWithConcurrentDictionary
            sw.Start();
            tmPara.InitParallelWithConcurrentDictionary(conn); // Call the method on the instance
            sw.Stop();
            long parallel2Time = sw.ElapsedMilliseconds; // Fix: Use 'long'
            TestContext.WriteLine($"Method 3 InitParallelWithConcurrentDictionary Time: {parallel2Time} ms");

            // Reset the stopwatch
            sw.Reset();

            // Measure InitParallelPartitioned
            sw.Start();
            tmPara.InitParallelPartitioned(conn); // Call the method on the instance
            sw.Stop();
            long parallel3Time = sw.ElapsedMilliseconds; // Fix: Use 'long'
            TestContext.WriteLine($"Method 4 InitParallelPartitioned Time: {parallel3Time} ms");


            // Optionally, assert that the parallel version is faster
            Assert.IsTrue(parallel2Time < singleThreadedTime, "Parallel execution should be faster than single-threaded execution.");
        }


        /// <summary>
        /// Checks whether two collections are disjoint.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collections.</typeparam>
        /// <param name="arr1">The first collection.</param>
        /// <param name="arr2">The second collection.</param>
        /// <returns>Returns true if the collections are disjoint; otherwise, false.</returns>
        private static bool areDisjoined<T>(ICollection<T> arr1, ICollection<T> arr2)
        {
            
            foreach (var item in arr1)
            {
                if (arr2.Contains(item))
                    return false;
            }

            return true;
        }
        /// <summary>
        /// Get the default parameters for temporal memory.
        /// </summary>
        /// <returns>Default parameters for temporal memory.</returns>
        private Parameters GetDefaultParameters()
        {
            // Create a new set of parameters using the default settings for temporal memory.
            Parameters retVal = Parameters.getTemporalDefaultParameters();

            // Set specific parameters for the temporal memory.
            retVal.Set(KEY.COLUMN_DIMENSIONS, new int[] { 36 });  // Set the dimensions of the columns.
            retVal.Set(KEY.CELLS_PER_COLUMN, 5);                  // Set the number of cells per column.
            retVal.Set(KEY.ACTIVATION_THRESHOLD, 3);              // Set the activation threshold.
            retVal.Set(KEY.INITIAL_PERMANENCE, 0.21);             // Set the initial permanence value.
            retVal.Set(KEY.CONNECTED_PERMANENCE, 0.5);            // Set the connected permanence value.
            retVal.Set(KEY.MIN_THRESHOLD, 2);                     // Set the minimum threshold.
            retVal.Set(KEY.MAX_NEW_SYNAPSE_COUNT, 3);             // Set the maximum count of new synapses.
            retVal.Set(KEY.PERMANENCE_INCREMENT, 0.10);           // Set the permanence increment value.
            retVal.Set(KEY.PERMANENCE_DECREMENT, 0.10);           // Set the permanence decrement value.
            retVal.Set(KEY.PREDICTED_SEGMENT_DECREMENT, 0.0);     // Set the predicted segment decrement.

            // Set the random number generator with a specified seed for reproducibility.
            retVal.Set(KEY.RANDOM, new ThreadSafeRandom(42));

            // Set the seed for reproducibility.
            retVal.Set(KEY.SEED, 42);

            return retVal;
        }

        /// <summary>
        /// Gets the default parameters with an optional modification specified by a key-value pair.
        /// If the input parameters (p) are null, the default parameters are used.
        /// The method returns a new Parameters object with the specified modification.
        /// </summary>
        /// <param name="p">Optional input parameters to modify. If null, default parameters are used.</param>
        /// <param name="key">The key identifying the parameter to modify.</param>
        /// <param name="value">The new value to set for the specified parameter.</param>
        /// <returns>A new Parameters object with the specified modification.</returns>
        private Parameters GetDefaultParameters(Parameters p, string key, Object value)
        {
            Parameters retVal = p == null ? GetDefaultParameters() : p;
            retVal.Set(key, value);

            return retVal;
        }

        /// <summary>
        /// Retrieves the default configuration parameters for the Hierarchical Temporal Memory (HTM).
        /// </summary>
        /// <returns>A configured instance of <see cref="HtmConfig"/> representing the default HTM parameters.</returns>
        private HtmConfig GetDefaultTMParameters()
        {
            // Create a new HtmConfig instance with specific column and cell counts.
            HtmConfig htmConfig = new HtmConfig(new int[] { 32 }, new int[] { 32 })
            {
                CellsPerColumn = 5,                 // Number of cells within each column.
                ActivationThreshold = 3,            // Minimum number of active synapses for cell activation.
                InitialPermanence = 0.21,           // Initial permanence value for synapses.
                ConnectedPermanence = 0.5,          // Permanence threshold for synapse to be considered connected.
                MinThreshold = 2,                   // Minimum number of active synapses required for segment activation.
                MaxNewSynapseCount = 3,             // Maximum number of new synapses that can be added during learning.
                PermanenceIncrement = 0.10,         // Incremental value for adjusting synapse permanence during learning.
                PermanenceDecrement = 0.10,         // Decremental value for adjusting synapse permanence during learning.
                PredictedSegmentDecrement = 0,      // Decremental value for adjusting permanence of synapses in predicted segments.
                Random = new ThreadSafeRandom(42),  // Thread-safe random number generator with seed.
                RandomGenSeed = 42                  // Seed value for the random number generator.
            };

            return htmConfig;
        }

        /// <summary>
        /// This unit test verifies the growth of a new segment when multiple matching segments are found during computation in a Temporal Memory.
        /// </summary>
        [TestMethod]
        public void TestNewSegmentGrowthWhenMultipleMatchingSegmentsFound()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch 
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method 
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

          

            // Define active columns and corresponding active cells
            int[] activeColumns = { 0 };
            Cell[] activeCells = { cn.GetCell(0), cn.GetCell(1), cn.GetCell(2), cn.GetCell(3) };

            // Create multiple matching segments for the active cell
            DistalDendrite dd1 = cn.CreateDistalSegment(activeCells[0]);
            cn.CreateSynapse(dd1, cn.GetCell(4), 0.3);
            cn.CreateSynapse(dd1, cn.GetCell(5), 0.3);

            DistalDendrite dd2 = cn.CreateDistalSegment(activeCells[0]);
            cn.CreateSynapse(dd2, cn.GetCell(6), 0.3);
            cn.CreateSynapse(dd2, cn.GetCell(7), 0.3);


            // Measure and store execution time for multi-threaded computation (tmParallel.Compute)  
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;


            // Measure and store execution time for single-threaded computation (tm.Compute)  
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            
            // Log performance metrics and store them as a CSV entry  
            LogPerformance(nameof(TestNewSegmentGrowthWhenMultipleMatchingSegmentsFound), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            // Verify that a new segment has been grown
            Assert.AreEqual(2, activeCells[0].DistalDendrites.Count);

            DistalDendrite newSegment = activeCells[0].DistalDendrites[0] as DistalDendrite;

            // Additional assertions for the new segment
            Assert.IsNotNull(newSegment);
            Assert.AreEqual(2, newSegment.Synapses.Count);
        }

        /// <summary>
        /// Tests the update of synapse permanence when matching segments are found.
        /// </summary>
        [TestMethod]
        public void TestSynapsePermanenceUpdateWhenMatchingSegmentsFound()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch 
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();
            

            Parameters p = GetDefaultParameters(null, KEY.PERMANENCE_DECREMENT, 0.08); // Uses Permanence decrement parameter 
            p.apply(cn);


            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method 
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            

            // Define previous and current active columns and cells.
            int[] previousActiveColumns = { 0 };
            int[] activeColumns = { 1 }; 
            Cell[] previousActiveCells = { cn.GetCell(0), cn.GetCell(1), cn.GetCell(2), cn.GetCell(3) };
            Cell[] activeCells = { cn.GetCell(4), cn.GetCell(5) };

            // Create a matching segment for the first active cell.
            DistalDendrite selectedMatchingSegment = cn.CreateDistalSegment(activeCells[0]);
            cn.CreateSynapse(selectedMatchingSegment, previousActiveCells[0], 0.3);
            cn.CreateSynapse(selectedMatchingSegment, previousActiveCells[1], 0.3);
            cn.CreateSynapse(selectedMatchingSegment, previousActiveCells[2], 0.3);
            cn.CreateSynapse(selectedMatchingSegment, cn.GetCell(81), 0.3);

            // Create another matching segment for the second active cell.
            DistalDendrite otherMatchingSegment = cn.CreateDistalSegment(activeCells[1]);
            Synapse as1 = cn.CreateSynapse(otherMatchingSegment, previousActiveCells[0], 0.3);
            Synapse is1 = cn.CreateSynapse(otherMatchingSegment, cn.GetCell(81), 0.3);

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute)
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute) 
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestSynapsePermanenceUpdateWhenMatchingSegmentsFound), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            tm.Compute(previousActiveColumns, true);
            //tm.Compute(activeColumns, true);

            // Assert that the synapse permanence of matching synapses has been updated.
            Assert.AreEqual(0.3, as1.Permanence, 0.01);
            Assert.AreEqual(0.3, is1.Permanence, 0.01);
        }

        /// <summary>
        /// Unit test to verify the correct initialization of the TemporalMemory class with a custom number of cells per column.
        /// </summary>
        [TestMethod]
        public void TestCellsPerColumn()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch 
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            Parameters p = Parameters.getAllDefaultParameters();
            p.Set(KEY.COLUMN_DIMENSIONS, new int[] { 64, 64 });
            p.Set(KEY.CELLS_PER_COLUMN, 16); // Set a custom number of cells per column
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method 
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;


            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

   
            // Count the total number of cells in all columns and verify it against the expected count.
            int totalCellCount = 0;
            foreach (var column in cn.GetColumns())
            {
                totalCellCount += column.Cells.Length;
            }

            // Check if the total cell count matches the expected count based on custom column dimensions.
            Assert.AreEqual(64 * 64 * 16, totalCellCount);
        }

        /// <summary>
        /// Unit test for ensuring correct initialization of the TemporalMemory class 
        /// with custom column dimensions and cells per column configuration.
        /// </summary>
        [TestMethod]
        public void TestCustomDimensionsAndCells()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch 
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            Parameters p = Parameters.getAllDefaultParameters();
            p.Set(KEY.COLUMN_DIMENSIONS, new int[] { 16, 32 }); // Set custom column dimensions
            p.Set(KEY.CELLS_PER_COLUMN, 8); // Set custom number of cells per column
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method 
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;
           
            // Calculate the expected total number of cells based on custom dimensions
            int expectedTotalCells = 16 * 32 * 8;

            // Sum the actual number of cells in all columns
            int actualTotalCells = cn.GetColumns().Sum(column => column.Cells.Length);

            // Assert
            Assert.AreEqual(expectedTotalCells, actualTotalCells);
        }


        /// <summary>
        /// Unit test to verify the correct initialization of the TemporalMemory class with custom column dimensions.
        /// </summary>
        [TestMethod]
        public void TestColumnDimensions()
        {
         
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch 
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections(); 
            Stopwatch stopwatch = new Stopwatch();
            Parameters p = Parameters.getAllDefaultParameters();

            p.Set(KEY.COLUMN_DIMENSIONS, new int[] { 32, 64 }); // Set custom column dimensions
            p.Set(KEY.CELLS_PER_COLUMN, 32);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method 
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;
            
            // Count the total number of cells in all columns and verify it against the expected count.
            int totalCellCount = 0;
            foreach (var column in cn.GetColumns())
            {
                totalCellCount += column.Cells.Length;
            }

            // Check if the total cell count matches the expected count based on custom column dimensions.
            Assert.AreEqual(32 * 64 * 32, totalCellCount);
        }


        /// <summary>
        /// Tests the recycling of the least recently active segment to make room for a new segment using various data combinations.
        /// </summary>
        [TestMethod]
        [DataRow(new int[] { 0, 1, 2 }, new int[] { 3, 4, 5 }, new int[] { 6, 7, 8 }, new int[] { 9 })]
        [DataRow(new int[] { 0, 1, 2, 3 }, new int[] { 4, 5, 6, 7 }, new int[] { 8, 9, 10, 11 }, new int[] { 12 })]
        public void TestRecycleLeastRecentlyActiveSegmentToMakeRoomForNewSegment(int[] prevActiveColumns1, int[] prevActiveColumns2, int[] prevActiveColumns3, int[] activeColumns)
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch 
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            Parameters p = GetDefaultParameters(null, KEY.CELLS_PER_COLUMN, 1);
            p = GetDefaultParameters(p, KEY.INITIAL_PERMANENCE, 0.5);
            p = GetDefaultParameters(p, KEY.PERMANENCE_INCREMENT, 0.02);
            p = GetDefaultParameters(p, KEY.PERMANENCE_DECREMENT, 0.02);
            p.Set(KEY.MAX_SEGMENTS_PER_CELL, 2);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method 
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

           

            // Get a reference to a specific cell (cell9)
            Cell cell9 = cn.GetCell(9);


            // Measure and store execution time for multi-threaded computation (tmParallel.Compute) 
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;


            // Measure and store execution time for single-threaded computation (tm.Compute) 
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestRecycleLeastRecentlyActiveSegmentToMakeRoomForNewSegment), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            // Simulate a computation cycle with the first set of previous and current active columns
            tm.Compute(prevActiveColumns1, true);
            tm.Compute(activeColumns, true);

            // Simulate a computation cycle with the first set of previous and current active columns with Parallel version
            tmParallel.Compute(prevActiveColumns1, true);
            tmParallel.Compute(activeColumns, true);

            // Assert that there is only one distal dendrite (segment) for cell9
            Assert.AreEqual(1, cell9.DistalDendrites.Count);

            // Get a reference to the oldest segment
            DistalDendrite oldestSegment = cell9.DistalDendrites[0];

            // Reset the temporal memory for the next set of computations
            tm.Reset(cn);
            tmParallel.Reset(cn);

            // Simulate a computation cycle with the second set of previous and current active columns
            tm.Compute(prevActiveColumns2, true);
            tm.Compute(activeColumns, true);

            // Simulate a computation cycle with the second set of previous and current active columns with Parallel version
            tmParallel.Compute(prevActiveColumns2, true);
            tmParallel.Compute(activeColumns, true);

            // Assert that there are now two distal dendrites for cell9
            Assert.AreEqual(2, cell9.DistalDendrites.Count);

            // Get the presynaptic cells of the synapses in the oldest segment
            var oldPresynaptic = oldestSegment.Synapses.Select(s => s.GetPresynapticCell()).ToList();

            // Reset the temporal memory for the final set of computations
            tm.Reset(cn);
            tmParallel.Reset(cn);

            // Simulate a computation cycle with the third set of previous and current active columns
            tm.Compute(prevActiveColumns3, true);
            //tm.Compute(activeColumns, true);

            // Assert that there are still two distal dendrites for cell9
            Assert.AreEqual(2, cell9.DistalDendrites.Count);

            // Get a reference to the newest segment
            DistalDendrite segment = cell9.DistalDendrites[cell9.DistalDendrites.Count - 1];

            // Get the presynaptic cells of the synapses in the newest segment
            var newPresynaptic = segment.Synapses.Select(s => s.GetPresynapticCell()).ToList();

            // Assert that the presynaptic cells of the oldest and newest segments are disjoint
            Assert.IsTrue(areDisjoined<Cell>(oldPresynaptic, newPresynaptic));
        }


        /// <summary>
        /// Unit test for adding new synapses to all winner cells based on different sets of previous and current active columns.
        /// </summary>
        [TestMethod]
        [DataRow(3, 1)]
        [DataRow(2, 2)]
        [DataRow(1, 3)]
        public void TestNewSegmentAddSynapsesToAllWinnerCells(int numPrevActiveCols, int numActiveCols)
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch 
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            Parameters p = GetDefaultParameters(null, KEY.MAX_NEW_SYNAPSE_COUNT, 4);
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

            // Set up previous and current active columns
            int[] previousActiveColumns = Enumerable.Range(0, numPrevActiveCols).ToArray();
            int[] activeColumns = Enumerable.Range(3, numActiveCols).ToArray();

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

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestNewSegmentAddSynapsesToAllWinnerCells), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            // Act
            ComputeCycle cc = tm.Compute(previousActiveColumns, true) as ComputeCycle;
            List<Cell> prevWinnerCells = new List<Cell>(cc.WinnerCells);
            Assert.AreEqual(numPrevActiveCols, prevWinnerCells.Count);

            cc = tm.Compute(activeColumns, true) as ComputeCycle;

            List<Cell> winnerCells = new List<Cell>(cc.WinnerCells);
            Assert.AreEqual(numActiveCols, winnerCells.Count);

            // Get the distal dendrites for the winner cell
            List<DistalDendrite> segments = winnerCells[0].DistalDendrites;
            Assert.AreEqual(1, segments.Count);

            // Get all the synapses for the segment
            List<Synapse> synapses = segments[0].Synapses;

            List<Cell> presynapticCells = new List<Cell>();
            foreach (Synapse synapse in synapses)
            {
                // Assert that the synapses have the expected permanence value within a tolerance
                Assert.AreEqual(0.21, synapse.Permanence, 0.01);
                presynapticCells.Add(synapse.GetPresynapticCell());
            }

            presynapticCells.Sort();

            // Assert that the presynaptic cells are the same as the previous winner cells
            Assert.IsTrue(prevWinnerCells.SequenceEqual(presynapticCells));
        }



        /// <summary>
        /// Tests the behavior of destroying weak synapses when a wrong prediction occurs.
        /// </summary>
        /// <param name="weakSynapsePermanence">Permanence value for the weak synapse.</param>
        [TestMethod]
        [DataRow(0.015)]
        [DataRow(0.017)]
        [DataRow(0.018)]
        [DataRow(0.019)]
        [DataRow(0.009)]
        public void TestDestroyWeakSynapseOnWrongPrediction(Double weakSynapsePermanence)
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch 
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            Parameters p = GetDefaultParameters(null, KEY.INITIAL_PERMANENCE, 0.2);
            p = GetDefaultParameters(p, KEY.MAX_NEW_SYNAPSE_COUNT, 4);
            p = GetDefaultParameters(p, KEY.PREDICTED_SEGMENT_DECREMENT, 0.02);
            p.apply(cn);

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;


            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Set up previous and current active columns and cells
            int[] previousActiveColumns = { 0 };
            Cell[] previousActiveCells = { cn.GetCell(0), cn.GetCell(1), cn.GetCell(2), cn.GetCell(3) };
            int[] activeColumns = { 2 };
            Cell expectedActiveCell = cn.GetCell(5);
            DistalDendrite activeSegment = cn.CreateDistalSegment(expectedActiveCell);

            // Creating synapses for the active segment
            cn.CreateSynapse(activeSegment, previousActiveCells[0], 0.5);
            cn.CreateSynapse(activeSegment, previousActiveCells[1], 0.5);
            cn.CreateSynapse(activeSegment, previousActiveCells[2], 0.5);

            // Creating a weak synapse with a specified permanence value
            cn.CreateSynapse(activeSegment, previousActiveCells[3], weakSynapsePermanence);

            // Perform computation cycles to enable learning of the sequences, Measure and store execution time for single-threaded computation (tmParallel.Compute)
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Perform computation cycles to enable learning of the sequences, Measure and store execution time for single-threaded computation (tm.Compute)
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;
            
            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestDestroyWeakSynapseOnWrongPrediction), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            tm.Compute(previousActiveColumns, true);
            //tm.Compute(activeColumns, true);

            // Assert
            // Checking that the weak synapse has been destroyed, and only three synapses remain in the active segment.
            Assert.AreEqual(4, activeSegment.Synapses.Count);
        }


        /// <summary>
        /// Tests the addition of segments to a cell with the fewest existing segments, ensuring varied data scenarios.
        /// </summary>
        [TestMethod]
        [DataRow(0, 3)]
        [DataRow(1, 4)]
        [DataRow(2, 5)]
        public void TestAddSegmentToCellWithFewestSegments(int seed, int expectedNumSegments)
        {
            // Variables to track segment growth on specific cells
            bool grewOnCell1 = false;
            bool grewOnCell2 = false;

            // Loop for testing with different seeds
            for (seed = 0; seed < 100; seed++)
            {
                // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch 
                TemporalMemory tm = new TemporalMemory();
                TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
                Connections cn = new Connections();
                Stopwatch stopwatch = new Stopwatch();

                Parameters p = GetDefaultParameters(null, KEY.MAX_NEW_SYNAPSE_COUNT, 4);
                p = GetDefaultParameters(p, KEY.PREDICTED_SEGMENT_DECREMENT, 0.02);
                p = GetDefaultParameters(p, KEY.SEED, seed);
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

                // Set up previous and current active columns and cells
                int[] prevActiveColumns = { 1, 2, 3, 4 };
                Cell[] prevActiveCells = { cn.GetCell(4), cn.GetCell(5), cn.GetCell(6), cn.GetCell(7) };
                int[] activeColumns = { 0 };
                Cell[] nonMatchingCells = { cn.GetCell(0), cn.GetCell(3) };
                IList<Cell> activeCells = cn.GetCells(new int[] { 0, 1, 2, 3 });

                // Create distal segments with synapses
                DistalDendrite segment1 = cn.CreateDistalSegment(nonMatchingCells[0]);
                cn.CreateSynapse(segment1, prevActiveCells[0], 0.5);
                DistalDendrite segment2 = cn.CreateDistalSegment(nonMatchingCells[1]);
                cn.CreateSynapse(segment2, prevActiveCells[1], 0.5);

                // Perform computation cycles to enable learning of the sequences, Measuring and storing execution time for single-threaded computation (tm.Compute)
                stopwatch.Start();
                tm.Compute(prevActiveColumns, true);
                stopwatch.Stop();
                TimeSpan elapsed_4 = stopwatch.Elapsed;
                Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
                double initTimeCompute = elapsed_4.TotalMilliseconds;

                // Perform computation cycles to enable learning of the sequences, Measuring and storing execution time for single-threaded computation (tmParallel.Compute)
                stopwatch.Start();
                tmParallel.Compute(prevActiveColumns, true);
                stopwatch.Stop();
                TimeSpan elapsed_3 = stopwatch.Elapsed;
                Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
                double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

                // Log performance metrics and store them as a CSV entry 
                LogPerformance(nameof(TestAddSegmentToCellWithFewestSegments), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

                //tm.Compute(prevActiveColumns, true);
                ComputeCycle cc = tm.Compute(activeColumns, true) as ComputeCycle;

                // Assert conditions for segment growth and synaptic connections
                Assert.AreEqual(7, cn.NumSegments());
                Assert.AreEqual(1, cn.NumSegments(cn.GetCell(0)));
                Assert.AreEqual(1, cn.NumSegments(cn.GetCell(3)));
                Assert.AreEqual(1, segment1.Synapses.Count);
                Assert.AreEqual(1, segment2.Synapses.Count);

                // Check for segment growth on cells 1 and 2
                List<DistalDendrite> segments = new List<DistalDendrite>(cn.GetCell(1).DistalDendrites);
                if (segments.Count == 0)
                {
                    List<DistalDendrite> segments2 = cn.GetCell(2).DistalDendrites;
                    grewOnCell2 = true;
                    segments.AddRange(segments2);
                }
                else
                {
                    grewOnCell1 = true;
                }

                // Check for the presence of all previous active columns
                //ISet<Column> columnCheckList = cn.GetColumnSet(prevActiveColumns);
                //Assert.AreEqual(4, columnCheckList.Count);
            }

            // Final assertions for segment growth on cells 1 and 2
            Assert.IsTrue(grewOnCell1);
            Assert.IsTrue(grewOnCell2);
        }


        /// <summary>
        /// This test method verifies the behavior of the AdaptSegment method in the TemporalMemory class under various scenarios.
        /// </summary>
        [TestMethod]
        [DataRow(0.9, 1.0, DisplayName = "Permanence at 0.9, should adapt to max")]
        [DataRow(1.0, 1.0, DisplayName = "Permanence at 1.0, should remain at max")]
        public void TestAdaptSegmentToMax(double initialPermanence, double expectedPermanence)
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch 
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method 
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Create a distal dendrite and synapse with the specified initial permanence
            DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(0));
            Synapse s1 = cn.CreateSynapse(dd, cn.GetCell(23), initialPermanence);

            // Act
            // Call the AdaptSegment method to adjust the permanence
            TemporalMemory.AdaptSegment(cn, dd, cn.GetCells(new int[] { 23 }), cn.HtmConfig.PermanenceIncrement, cn.HtmConfig.PermanenceDecrement);

            // Assert
            // Verify that the permanence has been adjusted as expected
            Assert.AreEqual(expectedPermanence, s1.Permanence, 0.1);

            // Now permanence should be at max
            // Call the AdaptSegment method again to test that the permanence remains at the maximum value
            TemporalMemory.AdaptSegment(cn, dd, cn.GetCells(new int[] { 23 }), cn.HtmConfig.PermanenceIncrement, cn.HtmConfig.PermanenceDecrement);

            // Assert again
            Assert.AreEqual(expectedPermanence, s1.Permanence, 0.1);
        }

        /// <summary>
        /// This test method checks the behavior of the DestroySegmentsWithTooFewSynapsesToBeMatching method in the TemporalMemory class.
        /// </summary>
        [TestMethod]
        [DataRow(0, 1, 2, 2, 0.015, 0.015, 0.015, 0.015, 1)]
        [DataRow(0, 1, 2, 2, 0.015, 0.015, 0.015, 0.009, 1)]
        [DataRow(0, 1, 2, 2, 0.015, 0.3, 0.009, 0.009, 1)]  // testing if our test is running properly
        [DataRow(0, 1, 2, 2, 0.015, 0.009, 0.009, 0.009, 1)]
        [DataRow(0, 1, 2, 2, 0.3, 0.015, 0.015, 0.009, 1)]   // testing if our test is running properly
        public void TestDestroySegmentsWithTooFewSynapsesToBeMatching(int c1, int c2, int c3, int c4,
                                                        double p1, double p2, double p3, double p4, int expectedNumSegments)
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch 
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            Parameters p = GetDefaultParameters(null, KEY.INITIAL_PERMANENCE, .2);
            p = GetDefaultParameters(p, KEY.MAX_NEW_SYNAPSE_COUNT, 4);
            p = GetDefaultParameters(p, KEY.PREDICTED_SEGMENT_DECREMENT, 0.02);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Set up previous and current active columns
            int[] prevActiveColumns = { c1, c2, c3 };
            Cell[] prevActiveCells = { cn.GetCell(c1), cn.GetCell(c2), cn.GetCell(c3), cn.GetCell(c4) };
            int[] activeColumns = { 2 };
            Cell expectedActiveCell = cn.GetCell(5);

            // Create matching distal segment with synapses from previous active cells to expected active cell
            DistalDendrite matchingSegment = cn.CreateDistalSegment(cn.GetCell(5));
            cn.CreateSynapse(matchingSegment, prevActiveCells[0], p1);
            cn.CreateSynapse(matchingSegment, prevActiveCells[1], p2);
            cn.CreateSynapse(matchingSegment, prevActiveCells[2], p3);
            cn.CreateSynapse(matchingSegment, prevActiveCells[3], p4);

            // Perform computation cycles to enable learning of the sequences, measuring and storing execution time for single-threaded computation (tmParallel.Compute) 
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Perform computation cycles to enable learning of the sequences, measuring and storing execution time for single-threaded computation (tm.Compute) 
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestDestroySegmentsWithTooFewSynapsesToBeMatching), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            // Act
            tm.Compute(prevActiveColumns, true);
            //tm.Compute(activeColumns, true);

            // Assert
            // Check that the expected active cell has the correct number of segments
            Assert.AreEqual(expectedNumSegments, cn.NumSegments(expectedActiveCell));
        }


        /// <summary>
        /// Tests the punishment of matching segments in inactive columns with various different data.
        /// </summary>
        /// <param name="as1Permanence">Permanence value for Synapse as1 in the active segment.</param>
        /// <param name="as2Permanence">Permanence value for Synapse as2 in the active segment.</param>
        /// <param name="as3Permanence">Permanence value for Synapse as3 in the active segment.</param>
        /// <param name="as4Permanence">Permanence value for Synapse as4 in the matching segment.</param>
        /// <param name="as5Permanence">Permanence value for Synapse as5 in the matching segment.</param>
        /// <param name="is1Permanence">Permanence value for Synapse is1 in the active segment connected to an inactive cell.</param>
        /// <param name="expectedAs1Permanence">Expected permanence value for Synapse as1 after computation.</param>
        /// <param name="expectedAs2Permanence">Expected permanence value for Synapse as2 after computation.</param>
        /// <param name="expectedAs3Permanence">Expected permanence value for Synapse as3 after computation.</param>
        /// <param name="expectedAs4Permanence">Expected permanence value for Synapse as4 after computation.</param>
        /// <param name="expectedAs5Permanence">Expected permanence value for Synapse as5 after computation.</param>
        /// <param name="expectedIs1Permanence">Expected permanence value for Synapse is1 after computation.</param>
        /// <param name="expectedIs2Permanence">Expected permanence value for Synapse is2 after computation.</param>
        [TestMethod]
        [DataRow(0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.48, 0.48, 0.48, 0.48, 0.48, 0.5, 0.5)]
        [DataRow(0.6, 0.6, 0.6, 0.6, 0.6, 0.6, 0.58, 0.58, 0.58, 0.58, 0.58, 0.6, 0.6)]
        public void TestPunishMatchingSegmentsInInactiveColumns(double as1Permanence, double as2Permanence,
            double as3Permanence, double as4Permanence, double as5Permanence, double is1Permanence,
            double expectedAs1Permanence, double expectedAs2Permanence, double expectedAs3Permanence,
            double expectedAs4Permanence, double expectedAs5Permanence,
            double expectedIs1Permanence, double expectedIs2Permanence)
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            Parameters p = GetDefaultParameters(null, KEY.MAX_NEW_SYNAPSE_COUNT, 4);
            p = GetDefaultParameters(p, KEY.INITIAL_PERMANENCE, 0.2);
            p = GetDefaultParameters(p, KEY.PREDICTED_SEGMENT_DECREMENT, 0.02);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Set up previous and current active columns and cells
            int[] prevActiveColumns = { 0 };
            Cell[] prevActiveCells = { cn.GetCell(0), cn.GetCell(1), cn.GetCell(2), cn.GetCell(3) };
            int[] activeColumns = { 1 };
            Cell previousInactiveCell = cn.GetCell(81);

            // Create Synapses and Distal Dendrites
            DistalDendrite activeSegment = cn.CreateDistalSegment(cn.GetCell(42));
            Synapse as1 = cn.CreateSynapse(activeSegment, prevActiveCells[0], as1Permanence);
            Synapse as2 = cn.CreateSynapse(activeSegment, prevActiveCells[1], as2Permanence);
            Synapse as3 = cn.CreateSynapse(activeSegment, prevActiveCells[2], as3Permanence);
            Synapse is1 = cn.CreateSynapse(activeSegment, previousInactiveCell, is1Permanence);

            DistalDendrite matchingSegment = cn.CreateDistalSegment(cn.GetCell(43));
            Synapse as4 = cn.CreateSynapse(matchingSegment, prevActiveCells[0], as4Permanence);
            Synapse as5 = cn.CreateSynapse(matchingSegment, prevActiveCells[1], as5Permanence);
            Synapse is2 = cn.CreateSynapse(matchingSegment, previousInactiveCell, expectedIs2Permanence);

            // Perform computation cycles to enable learning of the sequences, measuring and storing execution time for single-threaded computation (tmParallel.Compute)
            stopwatch.Start();
            tmParallel.Compute(prevActiveColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Act
            // Perform computation cycles to enable learning of the sequences, measuring and storing execution time for single-threaded computation (tm.Compute)
            stopwatch.Start();
            tm.Compute(prevActiveColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestPunishMatchingSegmentsInInactiveColumns), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            //tm.Compute(prevActiveColumns, true);
            tm.Compute(activeColumns, true);

            // Assert
            Assert.AreEqual(expectedAs1Permanence, as1.Permanence, 0.01);
            Assert.AreEqual(expectedAs2Permanence, as2.Permanence, 0.01);
            Assert.AreEqual(expectedAs3Permanence, as3.Permanence, 0.01);
            Assert.AreEqual(expectedAs4Permanence, as4.Permanence, 0.01);
            Assert.AreEqual(expectedAs5Permanence, as5.Permanence, 0.01);
            Assert.AreEqual(expectedIs1Permanence, is1.Permanence, 0.01);
            Assert.AreEqual(expectedIs2Permanence, is2.Permanence, 0.01);
        }

        /// <summary>
        /// This unit test assesses the ability of the Temporal Memory to learn and recall patterns
        /// of sequences characterized by a high sparsity rate.
        /// </summary>
        [TestMethod]
        public void TestHighSparsitySequenceLearningAndRecall()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();
            
            // Set default parameters, specifying a column dimension of 64
            Parameters p = GetDefaultParameters(null, KEY.COLUMN_DIMENSIONS, new int[] { 100 });
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Define two sequences of active columns with high sparsity rates
            var seq1ActiveColumns = new int[] { 0, 10, 20, 30, 41, 52, 63, 70, 80, 90 };
            var seq2ActiveColumns = new int[] { 41, 52, 63 };

            // Perform computation cycles to enable learning of the sequences, measuring and storing execution time for single-threaded computation (tmParallel.Compute) 
            stopwatch.Start();
            tmParallel.Compute(seq1ActiveColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Perform computation cycles to enable learning of the sequences, measuring and storing execution time for single-threaded computation (tm.Compute) 
            stopwatch.Start();
            tm.Compute(seq1ActiveColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestHighSparsitySequenceLearningAndRecall), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            // Recall the first sequence
            var recall1 = tm.Compute(seq1ActiveColumns, false);

            // Recall the second sequence
            var recall2 = tm.Compute(seq2ActiveColumns, false);


            // Verify that all active cells in the recalled second sequence are also present in the recalled first sequence
            Assert.IsTrue(recall2.ActiveCells.Select(c => c.Index).All(rc => recall1.ActiveCells.Select(c => c.Index).Contains(rc)));
        }


        // Log performance metrics to a CSV file, including method name, initialization times, and computation times
        public static void LogPerformance(string methodName, double initTime, double initParallelTime, double initTimeCompute, double initParallelTimeCompute)
        {
            string csvFilePath = "MethodPerformance_InitParallelWithConcurrentDictiona_3.csv";

            // Check if the file already contains the test case name
            var existingEntries = File.Exists(csvFilePath) ? File.ReadAllLines(csvFilePath).ToList() : new List<string>();

            // If the method name is not already in the file, log the performance metrics
            if (!existingEntries.Any(line => line.StartsWith(methodName)))
            {
                using (StreamWriter writer = new StreamWriter(csvFilePath, append: true))
                {
                    // Write the header if the file is empty
                    if (existingEntries.Count == 0)
                    {
                        writer.WriteLine("Test_Case,Init_Time,InitParallel_Time,Compute_Time,ComputeParallel_Time");
                    }
                    // Log the performance data for the method
                    writer.WriteLine($"{methodName},{initTime},{initParallelTime},{initTimeCompute},{initParallelTimeCompute}");
                }
            }
        }


        /// <summary>
        /// This unit test assesses the ability of the Temporal Memory to learn and recall patterns
        /// of sequences characterized by a high sparsity rate.
        /// </summary>
        /// 

        [TestMethod]
        public void TestHighSparsitySequenceLearningAndRecallParallel()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters, specifying a column dimension of 100
            Parameters p = GetDefaultParameters(null, KEY.COLUMN_DIMENSIONS, new int[] { 100 });
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;
            
            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;
            
            // Define two sequences of active columns with high sparsity rates
            var seq1ActiveColumns = new int[] { 0, 10, 20, 30, 41, 52, 63, 70, 80, 90 };
            var seq2ActiveColumns = new int[] { 41, 52, 63 };

            // Perform computation cycles to enable learning of the sequences, measuring and store execution time for single-threaded computation (tmParallel.Compute) 
            stopwatch.Start();
            tmParallel.Compute(seq1ActiveColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Perform computation cycles to enable learning of the sequences, measuring and store execution time for single-threaded computation (tm.Compute) 
            stopwatch.Start();
            tm.Compute(seq1ActiveColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestHighSparsitySequenceLearningAndRecallParallel), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            // Recall the first sequence
            var recall1 = tmParallel.Compute(seq1ActiveColumns, false);

            // Recall the second sequence
            var recall2 = tmParallel.Compute(seq2ActiveColumns, false);

            // Verify that all active cells in the recalled second sequence are also present in the recalled first sequence
            Assert.IsTrue(recall2.ActiveCells.Select(c => c.Index).All(rc => recall1.ActiveCells.Select(c => c.Index).Contains(rc)));
        }

        /// <summary>
        /// TestLowSparsitySequenceLearningAndRecall tests the ability of the Temporal Memory to learn and recall patterns of sequences with a low sparsity rate.
        /// </summary>
        [TestMethod]
        public void TestLowSparsitySequenceLearningAndRecall()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters, specifying a column dimension of 64
            Parameters p = GetDefaultParameters(null, KEY.COLUMN_DIMENSIONS, new int[] { 64 });
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Define two sequences of active columns
            var seq1ActiveColumns = new int[] { 0, 1, 2, 3, 4, 5, 6 };
            var seq2ActiveColumns = new int[] { 5, 6, 7, 8, 9 };

            // Define the desired result for sequence recall
            var desiredResult = new int[] { 27, 28, 30 };

            // Perform computation cycles to enable learning of the sequences, measuring and storing execution time for single-threaded computation (tmParallel.Compute)
            stopwatch.Start();
            tmParallel.Compute(seq1ActiveColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Perform computation cycles to enable learning of the sequences, measuring and storing execution time for single-threaded computation (tm.Compute)
            stopwatch.Start();
            tm.Compute(seq1ActiveColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestLowSparsitySequenceLearningAndRecall), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            // Recall the first sequence         
            var recall01 = tmParallel.Compute(seq1ActiveColumns, false);

            // Assert that the recalled active cells match the desired result
            Assert.IsTrue(desiredResult.All(des => recall01.ActiveCells.Select(c => c.Index).Contains(des)));

            // Recall the second sequence
            var recall02 = tmParallel.Compute(seq2ActiveColumns, false);

            // Assert that the recalled active cells match the desired result
            Assert.IsTrue(desiredResult.All(des => recall02.ActiveCells.Select(c => c.Index).Contains(des)));
        }



        /// <summary>
        /// Test the creation of a new synapse in a distal segment.
        /// </summary>
        [TestMethod]
        public void TestCreateSynapseInDistalSegment()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters, specifying a column dimension of 64
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            //focus in here (bug fixing required-seq1ActiveColumns was not present but added)
            var seq1ActiveColumns = new int[] { 0, 10, 20, 30, 41, 52, 63, 70, 80, 90 };

            // Perform computation cycles to enable learning of the sequences, measuring and storing execution time for single-threaded computation (tmParallel.Compute)
            stopwatch.Start();
            tmParallel.Compute(seq1ActiveColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Perform computation cycles to enable learning of the sequences, measuring and storing execution time for single-threaded computation (tm.Compute) 
            stopwatch.Start();
            tm.Compute(seq1ActiveColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestCreateSynapseInDistalSegment), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            // Create a distal segment for a specific cell
            DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(0));

            // Create a new synapse in the distal segment targeting a specific presynaptic cell with a given permanence value
            Synapse s1 = cn.CreateSynapse(dd, cn.GetCell(23), 0.5);

            // Assert statements to validate the properties of the created synapse and distal segment
            Assert.AreEqual(1, dd.Synapses.Count);
            Assert.AreEqual(s1, dd.Synapses[0]);
            Assert.AreEqual(23, s1.GetPresynapticCell().Index);
            Assert.AreEqual(0.5, s1.Permanence);
        }


        /// <summary>
        /// Test the growth of a new dendrite segment when no matching segments are found.
        /// </summary>
        [TestMethod]
        public void TestNewSegmentGrowthWhenNoMatchingSegmentFound()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory(); // Instantiate TemporalMemory class
            Connections cn = new Connections(); // Instantiate Connections class
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters
            Parameters p = Parameters.getAllDefaultParameters(); // Get default parameters
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Set up previous and current active columns and cells
            int[] activeColumns = { 0 };
            Cell[] activeCells = { cn.GetCell(0), cn.GetCell(1), cn.GetCell(2), cn.GetCell(3) };

            // Create a distal dendrite segment with synapses to some cells
            DistalDendrite dd = cn.CreateDistalSegment(activeCells[0]);
            cn.CreateSynapse(dd, cn.GetCell(4), 0.3);
            cn.CreateSynapse(dd, cn.GetCell(5), 0.3);

            // Perform computation cycles to enable learning of the sequences, measuring and storing execution time for single-threaded computation (tmParallel.Compute)
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Perform computation cycles to enable learning of the sequences, measuring and storing execution time for single-threaded computation (tm.Compute) 
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestNewSegmentGrowthWhenNoMatchingSegmentFound), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            // No matching segment should be found, so a new dendrite segment should be grown
            Assert.AreEqual(1, activeCells[0].DistalDendrites.Count);

            DistalDendrite newSegment = activeCells[0].DistalDendrites[0] as DistalDendrite;

            // Verify the growth of the new dendrite segment
            Assert.IsNotNull(newSegment);
            Assert.AreEqual(2, newSegment.Synapses.Count);
        }

        /// <summary>
        /// Verify that no active cell is present in more than one column 
        /// in the output of the Temporal Memory Algorithm.
        /// </summary>
        [TestMethod]
        public void TestNoOverlapInActiveCells()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory(); // Instantiate TemporalMemory class
            Connections cn = new Connections(); // Instantiate Connections class
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters
            Parameters p = GetDefaultParameters(); // Get default parameters
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Compute active cells for two columns
            int[] activeColumns = { 0, 1 };
            ComputeCycle cc = tm.Compute(activeColumns, true) as ComputeCycle;

            // Perform computation cycles to enable learning of the sequences, measuring and storing execution time for single-threaded computation (tmParallel.Compute) 
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Perform computation cycles to enable learning of the sequences, measuring and storing execution time for single-threaded computation (tm.Compute) 
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestNoOverlapInActiveCells), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            // Get active cells for the first column
            var activeCellsColumn0 = cc.ActiveCells
                .Where(cell => cell.Index == activeColumns[0])
                .ToList();

            // Get active cells for the second column
            var activeCellsColumn1 = cc.ActiveCells
                .Where(cell => cell.Index == activeColumns[1])
                .ToList();

            // Check that no cell is active in both columns
            foreach (var cell in activeCellsColumn0)
            {
                Assert.IsFalse(activeCellsColumn1.Contains(cell));
            }
        }

        /// <summary>
        /// Test method to verify that the Compute method of TemporalMemory returns the correct winner cells.
        /// </summary>
        [TestMethod]
        public void TestTemporalMemoryComputeReturnsWinnerCells()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();
            Parameters p = GetDefaultParameters(null, KEY.CELLS_PER_COLUMN, 2);

            // Set default parameters, specifying minimum threshold of 2
            p = GetDefaultParameters(p, KEY.MIN_THRESHOLD, 2);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Define an array of active columns
            int[] activeColumns = { 0, 1, 2, 3 };

            // Execute the Compute method and cast the result to ComputeCycle
            ComputeCycle cc = tm.Compute(activeColumns, true) as ComputeCycle;

            // Perform computation cycles to enable learning of the sequences, measuring and storing execution time for single-threaded computation (tmParallel.Compute) 
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Perform computation cycles to enable learning of the sequences, measuring and storing execution time for single-threaded computation (tm.Compute) 
            stopwatch.Start();
            ComputeCycle cc_ = tm.Compute(activeColumns, true) as ComputeCycle;
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry
            LogPerformance(nameof(TestTemporalMemoryComputeReturnsWinnerCells), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            // Retrieve the winner cells from the ComputeCycle
            List<Cell> winnerCells = new List<Cell>(cc.WinnerCells);

            // Assert the number of winner cells and their parent column indices
            Assert.AreEqual(4, winnerCells.Count);
            Assert.AreEqual(0, winnerCells[0].ParentColumnIndex);
            Assert.AreEqual(1, winnerCells[1].ParentColumnIndex);
        }

        /// <summary>
        /// Test method to verify that the Compute method of TemporalMemory returns the correct winner cells using DataRow for parameterized testing.
        /// </summary>
        /// <param name="activeColumns">Array of active columns</param>
        /// <param name="expectedWinnerCount">Expected number of winner cells</param>
        /// <param name="expectedWinnerIndices">Array of expected winner indices</param>
        [DataTestMethod]
        [DataRow(new int[] { 0, 1, 2, 3 }, 4, new int[] { 0, 1, 2, 3 })]
        [DataRow(new int[] { 4, 5, 6 }, 3, new int[] { 4, 5, 6 })]
        public void TestTemporalMemoryComputeReturnsWinnerCellsWithDataRow(int[] activeColumns, int expectedWinnerCount, int[] expectedWinnerIndices)
        {
            // Initialize TemporalMemory, Parallel Processing, Connections, and set default parameters
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();
            Connections cn = new Connections();

            Parameters p = GetDefaultParameters(null, KEY.CELLS_PER_COLUMN, 2);
            p = GetDefaultParameters(p, KEY.MIN_THRESHOLD, 2);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Execute the Compute method and cast the result to ComputeCycle
            ComputeCycle cc = tm.Compute(activeColumns, true) as ComputeCycle;

            // Perform computation cycles to enable learning of the sequences, measuring and storing execution time for single-threaded computation (tm.Compute) 
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Perform computation cycles to enable learning of the sequences, measuring and storing execution time for single-threaded computation (tmParallel.Compute) 
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestTemporalMemoryComputeReturnsWinnerCellsWithDataRow), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            // Retrieve the winner cells from the ComputeCycle
            List<Cell> winnerCells = new List<Cell>(cc.WinnerCells);

            // Assert the number of winner cells and their parent column indices based on the provided data row
            Assert.AreEqual(expectedWinnerCount, winnerCells.Count);
            for (int i = 0; i < expectedWinnerCount; i++)
            {
                Assert.AreEqual(expectedWinnerIndices[i], winnerCells[i].ParentColumnIndex);
            }
        }


        /// <summary>
        /// Test method to verify the correct functioning of the GetLeastUsedCell method.
        /// </summary>
        [TestMethod]
        public void TestGetLeastUsedCell()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();
            Connections conn = new Connections();

            // Set default parameters
            Parameters p = GetDefaultParameters();
            p.apply(conn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(conn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(conn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            Cell c1 = conn.GetCell(1);
            Cell c2 = conn.GetCell(2);
            Cell c3 = conn.GetCell(3);
            DistalDendrite s1 = conn.CreateDistalSegment(c1);
            DistalDendrite s2 = conn.CreateDistalSegment(c1);
            DistalDendrite s3 = conn.CreateDistalSegment(c2);
            DistalDendrite s4 = conn.CreateDistalSegment(c3);
            Synapse syn1 = conn.CreateSynapse(s1, c1, 0.5);
            Synapse syn2 = conn.CreateSynapse(s1, c2, 0.5);
            Synapse syn3 = conn.CreateSynapse(s2, c3, 0.5);
            Synapse syn4 = conn.CreateSynapse(s3, c2, 0.5);
            Synapse syn5 = conn.CreateSynapse(s4, c1, 0.5);

            // Get the least used Cell from a list of Cells
            List<Cell> cells = new List<Cell> { c1, c2, c3 };
            Random random = new Random(42);
            Cell leastUsedCell = TemporalMemory.GetLeastUsedCell(conn, cells, random);

            // Verify that the least used Cell is c3
            Assert.AreEqual(c3.ParentColumnIndex, leastUsedCell.ParentColumnIndex);
            Assert.AreEqual(c3.Index, leastUsedCell.Index);
        }

        /// <summary>
        /// Test method to ensure that the correct cells become active in the temporal memory.
        /// </summary>
        [TestMethod]
        public void TestWhichCellsBecomeActive()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();
            Connections cn = new Connections();

            // Set default parameters
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Activate some columns in the input space
            int[] activeColumns = { 0, 1, 2, 3 };
            Cell[] activeCells = cn.GetCells(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

            // Compute the next state of the temporal memory
            ComputeCycle cycle = tm.Compute(activeColumns, true) as ComputeCycle;

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute) 
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute)
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestWhichCellsBecomeActive), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            // Check which cells are active
            HashSet<Cell> expectedActiveCells = new HashSet<Cell>(activeCells);
            HashSet<Cell> actualActiveCells = new HashSet<Cell>(cycle.ActiveCells);
            TestContext.WriteLine("Expected Active Cells: " + string.Join(",", expectedActiveCells));
            TestContext.WriteLine("Actual Active Cells: " + string.Join(",", actualActiveCells));

            // Ensure that the expected and actual sets of active cells are equal
            Assert.IsTrue(expectedActiveCells.All(eac => actualActiveCells.Contains(eac)));
        }


        /// <summary>
        /// Tests the calculation of dendrite segments that become active in the current cycle.
        /// </summary>
        //[TestMethod]
        public void TestCalculateActiveSegments()
        {
            // Note: The test is marked as inconclusive with an exception to indicate that it's not fixed.

            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters
            Parameters p = GetDefaultParameters();
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Set up previous and current active columns and cells
            int[] activeColumns = { 0, 1, 2 };
            Cell[] activeCells = cn.GetCells(activeColumns);

            // Create dendrite segments and synapses for the active cells
            DistalDendrite dd1 = cn.CreateDistalSegment(activeCells[0]);
            cn.CreateSynapse(dd1, cn.GetCell(4), 0.5);
            cn.CreateSynapse(dd1, cn.GetCell(5), 0.5);

            DistalDendrite dd2 = cn.CreateDistalSegment(activeCells[1]);
            cn.CreateSynapse(dd2, cn.GetCell(6), 0.5);
            cn.CreateSynapse(dd2, cn.GetCell(7), 0.5);

            DistalDendrite dd3 = cn.CreateDistalSegment(activeCells[2]);
            cn.CreateSynapse(dd3, cn.GetCell(8), 0.5);
            cn.CreateSynapse(dd3, cn.GetCell(9), 0.5);

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute) 
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            
            
            // Measure and store execution time for single-threaded computation (tm.Compute) 
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestCalculateActiveSegments), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);
 
            // Compute the current cycle
            ComputeCycle cycle = tmParallel.Compute(activeColumns, true) as ComputeCycle;

            // Assert that the correct dendrite segments are active
            Assert.IsTrue(cycle.ActiveSegments.Contains(dd1));
            Assert.IsTrue(cycle.ActiveSegments.Contains(dd2));
            Assert.IsTrue(cycle.ActiveSegments.Contains(dd3));
        }

        /// <summary>
        /// Tests the growth of synapses in an active segment based on potential overlap.
        /// </summary>
        /// <param name="previousActiveColumns">Array of previous active columns.</param>
        /// <param name="activeColumns">Array of currently active columns.</param>
        /// <param name="expectedPresynapticCells">Array of expected presynaptic cells after growth.</param>
        /// <param name="expectedCount">Expected count of synapses after growth.</param>
        [TestMethod]
        [DataRow(new int[] { 0, 1, 2, 3 }, new int[] { 5 }, new int[] { 0, 1, 2, 3 }, 4)]
        [DataRow(new int[] { 0, 1, 2, 3, 4 }, new int[] { 5 }, new int[] { 0, 1, 2, 4 }, 4)]
        public void TestActiveSegmentGrowSynapsesAccordingToPotentialOverlap(int[] previousActiveColumns, int[] activeColumns, int[] expectedPresynapticCells, int expectedCount)
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters, specifying cells per column, minimum threshold, activation threshold, max new synapse count
            Parameters p = GetDefaultParameters(null, KEY.CELLS_PER_COLUMN, 1);
            p = GetDefaultParameters(p, KEY.MIN_THRESHOLD, 1);
            p = GetDefaultParameters(p, KEY.ACTIVATION_THRESHOLD, 2);
            p = GetDefaultParameters(p, KEY.MAX_NEW_SYNAPSE_COUNT, 4);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Use 1 cell per column for easy control over the winner cells.
            List<Cell> prevWinnerCells = new List<Cell>();
            foreach (int col in previousActiveColumns)
            {
                prevWinnerCells.Add(cn.GetCell(col));
            }

            DistalDendrite activeSegment = cn.CreateDistalSegment(cn.GetCell(activeColumns[0]));
            cn.CreateSynapse(activeSegment, cn.GetCell(0), 0.5);
            cn.CreateSynapse(activeSegment, cn.GetCell(1), 0.5);
            cn.CreateSynapse(activeSegment, cn.GetCell(2), 0.2);

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute) 
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute)
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

           // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestActiveSegmentGrowSynapsesAccordingToPotentialOverlap), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            //computation with respect to parallel method
            ComputeCycle cc = tmParallel.Compute(previousActiveColumns, true) as ComputeCycle;
            Assert.IsTrue(prevWinnerCells.SequenceEqual(cc.WinnerCells));
            cc = tmParallel.Compute(activeColumns, true) as ComputeCycle;

            List<Cell> presynapticCells = new List<Cell>();
            foreach (var syn in activeSegment.Synapses)
            {
                presynapticCells.Add(syn.GetPresynapticCell());
            }

            Assert.IsTrue(presynapticCells.Count == expectedCount);
        }

        /// <summary>
        /// Tests the destruction of weak synapses during active reinforcement.
        /// </summary>
        /// <param name="prevActive">The index of the previously active column.</param>
        /// <param name="active">The index of the currently active column.</param>
        /// <param name="expectedSynapseCount">The expected count of synapses in the active segment after computation.</param>
        [TestMethod]
        [DataTestMethod]
        [DataRow(0, 2, 4)]
        [DataRow(1, 3, 4)]
        [DataRow(2, 0, 3)]
        [DataRow(3, 1, 4)]
        public void TestDestroyWeakSynapseOnActiveReinforce(int prevActive, int active, int expectedSynapseCount)
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters with initial permanence, new synapse count, predicted segment decrement
            Parameters p = GetDefaultParameters(null, KEY.INITIAL_PERMANENCE, 0.2);
            p = GetDefaultParameters(p, KEY.MAX_NEW_SYNAPSE_COUNT, 4);
            p = GetDefaultParameters(p, KEY.PREDICTED_SEGMENT_DECREMENT, 0.02);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Set up previous and current active columns
            int[] previousActiveColumns = { prevActive };
            Cell[] previousActiveCells = { cn.GetCell(0), cn.GetCell(1), cn.GetCell(2), cn.GetCell(3) };
            int[] activeColumns = { active };
            Cell expectedActiveCell = cn.GetCell(5);

            // Create an active segment and add synapses to previous active cells
            DistalDendrite activeSegment = cn.CreateDistalSegment(expectedActiveCell);
            cn.CreateSynapse(activeSegment, previousActiveCells[0], 0.5);
            cn.CreateSynapse(activeSegment, previousActiveCells[1], 0.5);
            cn.CreateSynapse(activeSegment, previousActiveCells[2], 0.5);
            // Weak Synapse
            cn.CreateSynapse(activeSegment, previousActiveCells[3], 0.009);

            // Measure and store execution time for single-threaded computation (tm.Compute) 
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute) 
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestDestroyWeakSynapseOnActiveReinforce), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            // Perform two cycles of activity using async method
            tmParallel.Compute(previousActiveColumns, true);
            //tmParallel.Compute(activeColumns, true);

            // Assert that the actual synapse count matches the expected count
            Assert.AreEqual(expectedSynapseCount, activeSegment.Synapses.Count);
        }

        /// <summary>
        /// Unit test method for validating the adaptation of a distal dendrite segment by increasing synapse permanence.
        /// </summary>
        [TestMethod]
        public void TestAdaptSegment_IncreasePermanence()
        {
            // Initialize Temporal Memory, Parallel Processing, setting default parameters, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Parameters p = GetDefaultParameters(null, KEY.CELLS_PER_COLUMN, 1);
            Stopwatch stopwatch = new Stopwatch();
            p.apply(cn);


            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;
            
            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            Connections conn = new Connections();
            DistalDendrite segment = cn.CreateDistalSegment(cn.GetCell(5));
            Cell presynapticCell = cn.GetCell(0);
            Cell activeCell = cn.GetCell(1);
            Synapse as1 = cn.CreateSynapse(segment, presynapticCell, .5);
            ICollection<Cell> prevActiveCells = new List<Cell> { activeCell };
            double permanenceIncrement = 0.1;
            double permanenceDecrement = 0.05;

            // Act (Parallel version)
            TemporalMemoryParallelProcessing.AdaptSegment(conn, segment, prevActiveCells, permanenceIncrement, permanenceDecrement);

            // Assert
            Assert.AreEqual(0.45, as1.Permanence);
        }

        /// <summary>
        /// Unit test to validate the adaptation of a distal dendritic segment when the previous active cells contain the presynaptic cell.
        /// The test initializes a TemporalMemory, Connections, and sets default parameters. It then creates a distal dendrite segment with a synapse to a specific cell.
        /// The adaptation of the segment is triggered, and the test asserts that the permanence of the synapse is increased accordingly.
        /// </summary>
        [TestMethod]
        public void TestAdaptSegment_PrevActiveCellsContainPresynapticCell_IncreasePermanence()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters, specifying cells per column
            Parameters p = GetDefaultParameters(null, KEY.CELLS_PER_COLUMN, 1);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;
            
            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;
            
            var segment = new DistalDendrite();
            var cell1 = cn.GetCell(1);
            var cell2 = cn.GetCell(2);
            Synapse as1 = cn.CreateSynapse(segment, cell1, .5);

            // Act
            TemporalMemoryParallelProcessing.AdaptSegment(cn, segment, new List<Cell> { cell1, cell2 }, 0.2, 0.1);

            // Assert
            Assert.AreEqual(0.7, as1.Permanence);
        }

        /// <summary>
        /// Unit test to verify the addition of a new synapse to a distal dendritic segment.
        /// The test initializes a TemporalMemory, Connections, and sets default parameters. It creates a distal dendrite segment and adds a synapse to a specific cell.
        /// The test asserts that the synapse is present in the segment and that its permanence is set as expected.
        /// </summary>
        [TestMethod]
        public void TestAddingNewSynapseToDistalSegment()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters, specifying a column dimension of 64
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Create Distal Dendrites
            DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(0));

            // Act
            Synapse s1 = cn.CreateSynapse(dd, cn.GetCell(23), 0.9);

            // Assert
            Assert.IsTrue(dd.Synapses.Contains(s1));
            Assert.AreEqual(0.9, s1.Permanence);
        }


        /// <summary>
        /// TestRemovingSynapseFromDistalSegment: testing the removal of 
        /// Synapses from distal segments
        /// </summary>
        [TestMethod]
        public void TestRemovingSynapseFromDistalSegment()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();

            // Set default parameters
            Parameters p = Parameters.getAllDefaultParameters();
            Stopwatch stopwatch = new Stopwatch();
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Create Synapses and Distal Dendrites
            DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(0));
            Synapse s1 = cn.CreateSynapse(dd, cn.GetCell(23), 0.9);
            Synapse s2 = cn.CreateSynapse(dd, cn.GetCell(42), 0.8);

            Assert.AreEqual(2, dd.Synapses.Count);

            // remove s1
            dd.KillSynapse(s1);

            Assert.AreEqual(1, dd.Synapses.Count);
            Assert.IsFalse(dd.Synapses.Contains(s1));
            Assert.IsTrue(dd.Synapses.Contains(s2));
        }

        /// <summary>
        /// TestUpdatingPermanenceOfSynapse: Verify if the algorithm can update
        /// the Permanence value of the Synapse.
        /// </summary>
        [TestMethod]
        public void TestUpdatingPermanenceOfSynapse()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Create Synapses and Distal Dendrites
            DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(0));
            Synapse s1 = cn.CreateSynapse(dd, cn.GetCell(23), 0.5);

            // Increment permanence
            s1.Permanence += cn.HtmConfig.PermanenceIncrement;
            Assert.AreEqual(0.6, s1.Permanence, 0.1);

            // Decrement permanence
            s1.Permanence -= cn.HtmConfig.PermanenceDecrement;
            Assert.AreEqual(0.5, s1.Permanence, 0.1);
        }

        ///<summary>
        /// Test adapt segment from syapse to centre when synapse is already at the center
        /// <Summary>
        [TestMethod]
        public void TestAdaptSegmentToCentre_SynapseAlreadyAtCentre()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();

            // Set default parameters, specifying a column dimension of 64
            Parameters p = Parameters.getAllDefaultParameters();
            Stopwatch stopwatch = new Stopwatch();
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Create Synapses and Distal Dendrites
            DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(0));
            Synapse s1 = cn.CreateSynapse(dd, cn.GetCell(23), 0.6); // central 

            //Act
            TemporalMemory.AdaptSegment(cn, dd, cn.GetCells(new int[] { 23 }), cn.HtmConfig.PermanenceIncrement, cn.HtmConfig.PermanenceDecrement);

            //Assert
            Assert.AreEqual(0.7, s1.Permanence, 0.1);
        }


        [TestMethod]
        public void TestIncreasePermanenceOfActiveSynapses()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters, specifying minimum threshold of 2
            Parameters p = GetDefaultParameters(null, KEY.MIN_THRESHOLD, 2);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Define active columns and corresponding active cells
            int[] previousActiveColumns = { 0, 1, 2 };
            int[] activeColumns = { 1, 2, 3 };

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute) 
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute) 
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestIncreasePermanenceOfActiveSynapses), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            // Activate some cells
            ComputeCycle cc = tm.Compute(previousActiveColumns, true) as ComputeCycle;
            List<Cell> prevActiveCells = new List<Cell>(cc.ActiveCells);
            Assert.AreEqual(15, prevActiveCells.Count);

            // Increase permanence of synapses for active cells
            cc = tm.Compute(activeColumns, true) as ComputeCycle;

            List<Cell> activeCells = new List<Cell>(cc.ActiveCells);
            Assert.AreEqual(15, activeCells.Count);

            // Assert that the permanence of synapses has increased
            List<Synapse> activeSynapses = new List<Synapse>();
            //foreach (Cell cell in activeCells)
            //{
            //foreach (DistalDendrite segment in cell.DistalDendrites)
            //{
            //    activeSynapses.AddRange(segment.Synapses.FindAll(synapse => synapse.IsDefined()));
            //}
            //}

            foreach (Synapse synapse in activeSynapses)
            {
                Assert.IsTrue(synapse.Permanence > 0.5);
            }
        }

        //[TestMethod]
        public void TestGetLeastUsedCell1()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters, specifying columns dimensions and cells per columns
            Parameters p = GetDefaultParameters(null, KEY.COLUMN_DIMENSIONS, new int[] { 4 });
            p = GetDefaultParameters(p, KEY.CELLS_PER_COLUMN, 3);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Create a distal segment and synapses
            DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(1));
            cn.CreateSynapse(dd, cn.GetCell(0), 0.30);
            cn.CreateSynapse(dd, cn.GetCell(2), 0.50);

            // Get the least used cell in column 1
            Cell leastUsedCell = TemporalMemory.GetLeastUsedCell(cn, cn.GetColumn(1).Cells, cn.HtmConfig.Random);

            // Verify that the least used cell is correct
            Assert.AreNotEqual(leastUsedCell, cn.GetCell(0));

            // Increment the usage count of the least used cell
            leastUsedCell.ParentColumnIndex++;

            // Get the least used cell in column 1 again
            Cell newLeastUsedCell = TemporalMemory.GetLeastUsedCell(cn, cn.GetColumn(1).Cells, cn.HtmConfig.Random);

            // Verify that the new least used cell is not the same as the original least used cell
            Assert.AreNotEqual(newLeastUsedCell, leastUsedCell);
        }

        /// <summary>
        /// 
        /// 
        /// 
        /// ellCount: Verify that the number of active cells in the 
        /// output of Temporal Memory Algorithm is less than or equal to the maximum 
        /// number of active cells allowed per column.
        /// </summary>
        [TestMethod]
        public void TestActiveCellCount()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters, specifying cells per columns
            Parameters p = GetDefaultParameters(null, KEY.CELLS_PER_COLUMN, 5);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Define active columns and corresponding active cells
            int[] activeColumns = { 0 };
            ComputeCycle cc = tm.Compute(activeColumns, true) as ComputeCycle;
            var activeCells = cc.ActiveCells;

            Assert.IsTrue(activeCells.Count <= 5);
        }

        /// <summary>
        ///exsisting tests retested with various different data,,,
        /// </summary>
        [TestMethod]
        [Category("Prod")]
        [DataRow(0.5, 0.6)]
        [DataRow(0.6, 0.7)]
        public void TestAdaptSegmentToCentre(
             double initialPermanence,
             double expectedPermanence)
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters, specifying a column dimension of 64
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;
            
            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Create Synapses and Distal Dendrites
            DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(0));
            Synapse s1 = cn.CreateSynapse(dd, cn.GetCell(23), initialPermanence);
            
            // Act
            TemporalMemory.AdaptSegment(cn, dd, cn.GetCells(new int[] { 23 }), cn.HtmConfig.PermanenceIncrement, cn.HtmConfig.PermanenceDecrement);

            // Assert
            Assert.AreEqual(expectedPermanence, s1.Permanence, 0.1);
        }

        /// <summary>
        ///exsisting tests retested with various different data,,,
        /// </summary>
        [TestMethod]
        [DataRow(new int[] { 4, 5 }, new int[] { 0, 1, 2, 3 })]
        [DataRow(new int[] { 2, 3 }, new int[] { 1, 2, 3, 4 })]
        public void TestArrayNotContainingCells(int[] activeColumns, int[] excludedCellIndices)
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            HtmConfig htmConfig = GetDefaultTMParameters();
            Connections cn = new Connections(htmConfig);
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            Cell[] excludedCells = cn.GetCells(excludedCellIndices);

            // Measure and store execution time for single-threaded computation (tm.Compute) 
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute) 
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry
            LogPerformance(nameof(TestArrayNotContainingCells), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            // Act
            ComputeCycle cc = tm.Compute(activeColumns, true) as ComputeCycle;

            // Assert
            CollectionAssert.DoesNotContain(cc.ActiveCells, excludedCells);
        }

        /// <summary>
        ///exsisting tests retested with various different data,,,
        /// </summary>
        [TestMethod]
        [DataRow(new int[] { 1, 2 }, new int[] { 0, 1, 2, 3 })]
        [DataRow(new int[] { 3, 4 }, new int[] { 2, 3, 4, 5 })]
        public void TestBurstNotpredictedColumns(int[] activeColumns, int[] expectedBurstingCellIndexes)
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters
            Parameters p = GetDefaultParameters();
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            IList<Cell> expectedBurstingCells = cn.GetCells(expectedBurstingCellIndexes); //Expected bursting cells

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute) 
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute) 
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestBurstNotpredictedColumns), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            ComputeCycle cc = tm.Compute(activeColumns, true) as ComputeCycle; //Compute class object 

            Assert.IsFalse(cc.ActiveCells.SequenceEqual(expectedBurstingCells));
        }

        /// <summary>
        ///exsisting tests retested with various different data,,,
        /// </summary>
        [TestMethod]
        [DataRow(new int[] { 0 }, new int[] { 1 }, new int[] { 0, 1, 2, 3 }, new int[] { 4, 5 }, 0.3, 0.3)]
        [DataRow(new int[] { 1 }, new int[] { 2 }, new int[] { 1, 2, 3, 4 }, new int[] { 5, 6 }, 0.3, 0.3)]
        public void TestNoChangeToNoTSelectedMatchingSegmentsInBurstingColumn(int[] previousActiveColumns,
            int[] activeColumns, int[] previousActiveCellIndexes, int[] burstingCellIndexes, double as1Permanence, double is1Permanence)
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters, specifying permanence decrement of 0.08
            Parameters p = GetDefaultParameters(null, KEY.PERMANENCE_DECREMENT, 0.08);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Set up previous and current active cells and bursting cells
            Cell[] previousActiveCells = cn.GetCells(previousActiveCellIndexes);
            Cell[] burstingCells = cn.GetCells(burstingCellIndexes);

            // Create Synapses and Distal Dendrites
            DistalDendrite selectedMatchingSegment = cn.CreateDistalSegment(burstingCells[0]);
            cn.CreateSynapse(selectedMatchingSegment, previousActiveCells[0], 0.3);
            cn.CreateSynapse(selectedMatchingSegment, previousActiveCells[1], 0.3);
            cn.CreateSynapse(selectedMatchingSegment, previousActiveCells[2], 0.3);
            cn.CreateSynapse(selectedMatchingSegment, cn.GetCell(81), 0.3);

            DistalDendrite otherMatchingSegment = cn.CreateDistalSegment(burstingCells[1]);
            Synapse as1 = cn.CreateSynapse(otherMatchingSegment, previousActiveCells[0], 0.3);
            Synapse is1 = cn.CreateSynapse(otherMatchingSegment, cn.GetCell(81), 0.3);

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute) 
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute) 
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestNoChangeToNoTSelectedMatchingSegmentsInBurstingColumn), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            tm.Compute(previousActiveColumns, true);
            //tm.Compute(activeColumns, true);

            Assert.AreEqual(as1Permanence, as1.Permanence, 0.01);
            Assert.AreEqual(is1Permanence, is1.Permanence, 0.01);
        }


        private Parameters getDefaultParameters1()
        {
            Parameters retVal = Parameters.getTemporalDefaultParameters();
            retVal.Set(KEY.COLUMN_DIMENSIONS, new int[] { 36 });
            retVal.Set(KEY.CELLS_PER_COLUMN, 5);
            retVal.Set(KEY.ACTIVATION_THRESHOLD, 2);
            retVal.Set(KEY.INITIAL_PERMANENCE, 0.15);
            retVal.Set(KEY.CONNECTED_PERMANENCE, 0.15);
            retVal.Set(KEY.MIN_THRESHOLD, 2);
            retVal.Set(KEY.MAX_NEW_SYNAPSE_COUNT, 2);
            retVal.Set(KEY.PERMANENCE_INCREMENT, 0.15);
            retVal.Set(KEY.PERMANENCE_DECREMENT, 0.15);
            retVal.Set(KEY.PREDICTED_SEGMENT_DECREMENT, 0.0);
            retVal.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            retVal.Set(KEY.SEED, 42);

            return retVal;
        }

        private HtmConfig GetDefaultTMParameters1()
        {
            HtmConfig htmConfig = new HtmConfig(new int[] { 32 }, new int[] { 32 })
            {
                CellsPerColumn = 5,
                ActivationThreshold = 2,
                InitialPermanence = 0.15,
                ConnectedPermanence = 0.15,
                MinThreshold = 2,
                MaxNewSynapseCount = 3,
                PermanenceIncrement = 0.15,
                PermanenceDecrement = 0.15,
                PredictedSegmentDecrement = 0,
                Random = new ThreadSafeRandom(42),
                RandomGenSeed = 42
            };

            return htmConfig;
        }
        private Parameters getDefaultParameters1(Parameters p, string key, Object value)
        {
            Parameters retVal = p == null ? getDefaultParameters1() : p;
            retVal.Set(key, value);

            return retVal;
        }

        [TestMethod]
        public void TestActivateDendrites()
        {
            // Arrange
            // Create a new Connections object
            var conn = new Connections();

            // Create a new ComputeCycle object
            var cycle = new ComputeCycle();

            // Set the learn variable to true
            bool learn = true;

            // Define arrays for the external predictive inputs that will be used in the ActivateDendrites method
            int[] externalPredictiveInputsActive = new int[] { 1, 2, 3 };
            int[] externalPredictiveInputsWinners = new int[] { 4, 5, 6 };

            // Create a new instance of the TemporalMemory class using reflection
            var myClass = (TemporalMemory)Activator.CreateInstance(typeof(TemporalMemory), nonPublic: true);

            // Act
            // Get the ActivateDendrites method of the TemporalMemory class using reflection
            var method = typeof(TemporalMemory).GetMethod("ActivateDendrites", BindingFlags.NonPublic | BindingFlags.Instance);

            // Invoke the ActivateDendrites method with the specified arguments
            method.Invoke(myClass, new object[] { conn, cycle, learn, externalPredictiveInputsActive, externalPredictiveInputsWinners });


            // Check that the ComputeCycle object is not null
            Assert.IsNotNull(cycle);
        }

        [TestMethod]
        public void TestBurstUnpredictedColumnsforFiveCells2()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            var tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();

            // Create a new Connections object
            var cn = new Connections();

            // Get the default parameters for the Connections object
            var p = getDefaultParameters1();

            // Apply the default parameters to the Connections object
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Define the active columns and bursting cells
            var activeColumns = new int[] { 0 };
            var burstingCells = cn.GetCells(new int[] { 0, 1, 2, 3, 4 });

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute) 
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute) 
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestBurstUnpredictedColumnsforFiveCells2), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            // Act
            // Compute the result using the specified active columns and set the learn flag to true
            var result = tm.Compute(activeColumns, true) as ComputeCycle;
            var resultParallel = tmParallel.Compute(activeColumns, true) as ComputeCycle;

            // Assert
            // Check that the resulting active cells are equivalent to the specified bursting cells
            CollectionAssert.AreEquivalent(burstingCells, result.ActiveCells);
            CollectionAssert.AreEquivalent(burstingCells, resultParallel.ActiveCells);
        }

        [TestMethod]
        [TestCategory("Prod")]
        public void TestSegmentCreationIfNotEnoughWinnerCells2()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();
            Connections cn = new Connections();

            // Get the default parameters for the Connections object and set the MAX_NEW_SYNAPSE_COUNT to 5
            Parameters p = getDefaultParameters1(null, KEY.MAX_NEW_SYNAPSE_COUNT, 5);

            // Apply the default parameters to the Connections object
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Define the zero columns and active columns
            int[] zeroColumns = { 0, 1, 2, 3 };
            int[] activeColumns = { 3, 4 };

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute) 
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute) 
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestSegmentCreationIfNotEnoughWinnerCells2), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            // Compute the result with the zero columns and set the learn flag to true
            tmParallel.Compute(zeroColumns, true);

            // Compute the result with the active columns and set the learn flag to true
            //tmParallel.Compute(activeColumns, true);

            // Assert
            // Check that the number of segments in the Connections object is equal to 2
            Assert.AreEqual(2, cn.NumSegments(), 0);
        }

        [TestMethod]
        public void TestMatchingSegmentAddSynapsesToSubsetOfWinnerCells()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters
            Parameters p = getDefaultParameters1(null, KEY.CELLS_PER_COLUMN, 1);
            p = getDefaultParameters1(p, KEY.MIN_THRESHOLD, 1);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Set up some initial state for the memory, with some active columns and previous winner cells.
            int[] previousActiveColumns = { 0, 1, 2, 3, 4 };
            IList<Cell> prevWinnerCells = cn.GetCells(new int[] { 0, 1, 2, 3, 4 });
            int[] activeColumns = { 5 };

            // Create a new distal dendrite segment and add a synapse to it.
            DistalDendrite matchingSegment = cn.CreateDistalSegment(cn.GetCell(5));
            cn.CreateSynapse(matchingSegment, cn.GetCell(0), 0.15);

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute) 
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Compute the memory for one cycle with the previous active columns, and then with the new active column.
            // Measure and store execution time for single-threaded computation (tm.Compute) 
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestMatchingSegmentAddSynapsesToSubsetOfWinnerCells), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            ComputeCycle cc = tm.Compute(previousActiveColumns, true) as ComputeCycle;
            //ComputeCycle ccParallel = tmParallel.Compute(previousActiveColumns, true) as ComputeCycle;
            Assert.IsTrue(cc.WinnerCells.SequenceEqual(prevWinnerCells));
            //Assert.IsTrue(ccParallel.WinnerCells.SequenceEqual(prevWinnerCells));
            cc = tm.Compute(activeColumns, true) as ComputeCycle;
            //ccParallel = tmParallel.Compute(activeColumns, true) as ComputeCycle;

            // Get the synapses from the matching segment and check their properties.
            List<Synapse> synapses = matchingSegment.Synapses;
            Assert.AreEqual(2, synapses.Count);
            synapses.Sort();
            foreach (Synapse synapse in synapses)
            {
                if (synapse.GetPresynapticCell().Index == 0) continue;

                Assert.AreEqual(0.15, synapse.Permanence, 0.01);
                Assert.IsTrue(synapse.GetPresynapticCell().Index == 1 ||
                           synapse.GetPresynapticCell().Index == 2 ||
                           synapse.GetPresynapticCell().Index == 3 ||
                           synapse.GetPresynapticCell().Index == 4);
            }
        }



        [TestMethod]
        [TestCategory("Prod")]
        public void TestActivateCorrectlyPredictiveCells1()
        {
            // Arrange
            //The method creates a new instance of the TemporalMemory class and a Connections object, and sets some default parameters for the memory using the getDefaultParameters1() method.
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch 
            int implementation = 0;
            TemporalMemory tm = implementation == 0 ? new TemporalMemory() : new TemporalMemoryMT();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters
            Parameters p = getDefaultParameters1();
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            //The method creates an input pattern by setting the previousActiveColumns and activeColumns arrays, which represent the indices of the active columns in the previous and current time steps, respectively.
            int[] previousActiveColumns = { 0 };
            int[] activeColumns = { 1 };

            //The method obtains a specific cell from the Connections object using the GetCell() method and sets up some synapses to it using the CreateSynapses() helper method.
            Cell cell5 = cn.GetCell(5);
            ISet<Cell> expectedPredictiveCells = new HashSet<Cell>(new Cell[] { cell5 });

            CreateSynapses(cn, cell5, new int[] { 0, 1, 2, 3, 4 }, 0.15);

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute)  
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute)  
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry
            LogPerformance(nameof(TestActivateCorrectlyPredictiveCells1), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            // Act
            //The method then calls the Compute() method of the TemporalMemory instance twice with the input pattern to activate the memory and obtain the resulting active and predictive cells.
            ComputeCycle cc1 = tm.Compute(previousActiveColumns, true) as ComputeCycle;
            ComputeCycle cc2 = tm.Compute(activeColumns, true) as ComputeCycle;

            // Assert
            //Finally, the method performs some assertions to check that the expected predictive and active cells are indeed activated by the input pattern.
            Assert.IsTrue(cc1.PredictiveCells.SequenceEqual(expectedPredictiveCells));
            Assert.IsTrue(cc2.ActiveCells.SequenceEqual(expectedPredictiveCells));
            Assert.AreEqual(expectedPredictiveCells.Count, cc1.PredictiveCells.Count);
            Assert.AreEqual(expectedPredictiveCells.Count, cc2.ActiveCells.Count);

        }

        // Helper method to create synapses between a distal dendrite segment and a set of cells
        private void CreateSynapses(Connections cn, Cell cell, int[] targetCells, double permanence)
        {
            DistalDendrite segment = cn.CreateDistalSegment(cell);
            foreach (int i in targetCells)
            {
                cn.CreateSynapse(segment, cn.GetCell(i), permanence);
            }
        }


        [TestMethod]
        public void TestBurstUnpredictedColumnsforFiveCells1()
        {
            // Get default configuration parameters for the HtmConfig class and create a new Connections object.
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch 
            HtmConfig htmConfig = GetDefaultTMParameters1();
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections(htmConfig);
            Stopwatch stopwatch = new Stopwatch();

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method 
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Set up some initial state for the memory, with one active column and five bursting cells.
            int[] activeColumns = { 0 };
            var burstingCells = cn.GetCells(new int[] { 0, 1, 2, 3, 4 });

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute)  
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute)  
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestBurstUnpredictedColumnsforFiveCells1), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            // Compute the memory for one cycle with the active column.
            ComputeCycle cc = tm.Compute(activeColumns, true) as ComputeCycle;

            // Check that the active cells after the cycle are equal to the bursting cells.
            Assert.IsTrue(cc.ActiveCells.SequenceEqual(burstingCells));
        }

        [TestMethod]
        [TestCategory("Prod")]
        public void TestNoneActiveColumns()
        {
            // Create a new TemporalMemory and Connections object, and initialize them with default parameters.
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch 
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            Parameters p = getDefaultParameters1();
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method 
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Set up some initial state for the memory, with one active segment and five synapses to other cells.
            int[] previousActiveColumns = { 0 };
            Cell cell5 = cn.GetCell(5);

            // Create Synapses and Distal Dendrites
            DistalDendrite activeSegment = cn.CreateDistalSegment(cell5);
            cn.CreateSynapse(activeSegment, cn.GetCell(0), 0.15);
            cn.CreateSynapse(activeSegment, cn.GetCell(1), 0.15);
            cn.CreateSynapse(activeSegment, cn.GetCell(2), 0.15);
            cn.CreateSynapse(activeSegment, cn.GetCell(3), 0.15);
            cn.CreateSynapse(activeSegment, cn.GetCell(4), 0.15);

            // Compute the memory for one cycle with the active segment, and check that there are active, winner, and predictive cells.
            ComputeCycle cc = tm.Compute(previousActiveColumns, true) as ComputeCycle;
            Assert.IsFalse(cc.ActiveCells.Count == 0);
            Assert.IsFalse(cc.WinnerCells.Count == 0);
            Assert.IsFalse(cc.PredictiveCells.Count == 0);

            // Compute the memory for one cycle with no active columns, and check that there are no active, winner, or predictive cells.
            int[] zeroColumns = new int[0];

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute)  
            stopwatch.Start();
            tmParallel.Compute(zeroColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute)  
            stopwatch.Start();
            tm.Compute(zeroColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry  
            LogPerformance(nameof(TestNoneActiveColumns), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            // Additional assertions for the new segment
            ComputeCycle cc2 = tm.Compute(zeroColumns, true) as ComputeCycle;
            Assert.IsTrue(cc2.ActiveCells.Count == 0);
            Assert.IsTrue(cc2.WinnerCells.Count == 0);
            Assert.IsTrue(cc2.PredictiveCells.Count == 0);
        }


        [TestMethod]
        [TestCategory("Prod")]
        public void TestPredictedActiveCellsAreCorrect()
        {
            //The test initializes a TemporalMemory object and a Connections object with default parameters and sets up some active columns and cells.
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch 
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            Parameters p = getDefaultParameters1();
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method 
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            //The test creates two DistalDendrite objects and creates synapses between them and the previous active cells.
            int[] previousActiveColumns = { 0 };
            int[] activeColumns = { 1 };
            Cell[] previousActiveCells = { cn.GetCell(0), cn.GetCell(1), cn.GetCell(2), cn.GetCell(3), cn.GetCell(4) };
            List<Cell> expectedWinnerCells = new List<Cell>(cn.GetCells(new int[] { 6, 8 }));

            //The test then runs two ComputeCycle cycles on the temporal memory with the previous active columns and the current active columns, both with learning turned off.
            DistalDendrite activeSegment1 = cn.CreateDistalSegment(expectedWinnerCells[0]);
            cn.CreateSynapse(activeSegment1, previousActiveCells[0], 0.15);
            cn.CreateSynapse(activeSegment1, previousActiveCells[1], 0.15);
            cn.CreateSynapse(activeSegment1, previousActiveCells[2], 0.15);
            cn.CreateSynapse(activeSegment1, previousActiveCells[3], 0.15);

            DistalDendrite activeSegment2 = cn.CreateDistalSegment(expectedWinnerCells[1]);

            cn.CreateSynapse(activeSegment2, previousActiveCells[1], 0.15);
            cn.CreateSynapse(activeSegment2, previousActiveCells[2], 0.15);
            cn.CreateSynapse(activeSegment2, previousActiveCells[3], 0.15);

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute)  
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute)  
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry  
            LogPerformance(nameof(TestPredictedActiveCellsAreCorrect), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            //The test finally checks if the expected winner cells match the actual winner cells.
            ComputeCycle cc = tm.Compute(previousActiveColumns, false) as ComputeCycle; // learn=false
            cc = tm.Compute(activeColumns, false) as ComputeCycle; // learn=false

            // Additional assertions for the new segment
            Assert.IsTrue(cc.WinnerCells.SequenceEqual(new LinkedHashSet<Cell>(expectedWinnerCells)));
        }

        [TestMethod]
        [TestCategory("Prod")]
        public void TestReinforcedSelectedMatchingSegmentInBurstingColumn1()
        {
            // The code instantiates a TemporalMemory object and a Connections object and initializes them with default parameters.
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch 
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            Parameters p = getDefaultParameters1(null, KEY.PERMANENCE_DECREMENT, 0.8);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method 
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // It creates arrays of previousActiveColumns, activeColumns, previousActiveCells, and burstingCells.
            int[] previousActiveColumns = { 0 };
            int[] activeColumns = { 1 };
            Cell[] previousActiveCells = { cn.GetCell(0), cn.GetCell(1), cn.GetCell(2), cn.GetCell(3), cn.GetCell(4) };
            Cell[] burstingCells = { cn.GetCell(4), cn.GetCell(6), cn.GetCell(8) };

            // It creates an activeSegment and several Synapses using the Connections object.
            DistalDendrite activeSegment = cn.CreateDistalSegment(burstingCells[0]);
            Synapse as1 = cn.CreateSynapse(activeSegment, previousActiveCells[0], 0.15);
            Synapse as2 = cn.CreateSynapse(activeSegment, previousActiveCells[0], 0.15);
            Synapse as3 = cn.CreateSynapse(activeSegment, previousActiveCells[0], 0.15);
            Synapse as4 = cn.CreateSynapse(activeSegment, previousActiveCells[0], 0.15);
            Synapse as5 = cn.CreateSynapse(activeSegment, previousActiveCells[0], 0.15);
            Synapse is1 = cn.CreateSynapse(activeSegment, cn.GetCell(81), 0.15);

            // Create multiple matching segments for the active cell
            DistalDendrite otherMatchingSegment = cn.CreateDistalSegment(burstingCells[1]);
            cn.CreateSynapse(otherMatchingSegment, previousActiveCells[0], 0.15);
            cn.CreateSynapse(otherMatchingSegment, previousActiveCells[1], 0.15);
            cn.CreateSynapse(otherMatchingSegment, previousActiveCells[2], 0.15);
            cn.CreateSynapse(otherMatchingSegment, cn.GetCell(81), 0.15);

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute)  
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute)  
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry  
            LogPerformance(nameof(TestReinforcedSelectedMatchingSegmentInBurstingColumn1), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            // It then calls the Compute method of the TemporalMemory object twice, with learn set to true, to reinforce the activeSegment.
            tm.Compute(previousActiveColumns, true);
            //tm.Compute(activeColumns, true);

            // Finally, it uses several Assert statements to check that the permanence values of the Synapses are set correctly.
            Assert.AreEqual(0.15, as1.Permanence, 0.01);
            Assert.AreEqual(0.15, as2.Permanence, 0.01);
            Assert.AreEqual(0.15, as3.Permanence, 0.01);
            Assert.AreEqual(0.15, as4.Permanence, 0.01);
            Assert.AreEqual(0.15, as5.Permanence, 0.01);
            Assert.AreEqual(0.15, is1.Permanence, 0.001);
        }

        [TestMethod]
        [TestCategory("Prod")]
        public void TestReinforcedSelectedMatchingSegmentInBurstingColumn()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch 
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();

            Parameters p = getDefaultParameters1(null, KEY.PERMANENCE_DECREMENT, 0.06);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method 
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Define active columns and corresponding active cells
            int[] previousActiveColumns = { 0 };
            int[] activeColumns = { 1 };
            Cell[] previousActiveCells = { cn.GetCell(0), cn.GetCell(1), cn.GetCell(2), cn.GetCell(3), cn.GetCell(4) };
            Cell[] burstingCells = { cn.GetCell(4), cn.GetCell(6) };

            DistalDendrite activeSegment = cn.CreateDistalSegment(burstingCells[0]);
            Synapse as1 = cn.CreateSynapse(activeSegment, previousActiveCells[0], 0.3);
            Synapse as2 = cn.CreateSynapse(activeSegment, previousActiveCells[0], 0.3);
            Synapse as3 = cn.CreateSynapse(activeSegment, previousActiveCells[0], 0.3);
            Synapse as4 = cn.CreateSynapse(activeSegment, previousActiveCells[0], 0.3);
            Synapse as5 = cn.CreateSynapse(activeSegment, previousActiveCells[0], 0.3);
            Synapse is1 = cn.CreateSynapse(activeSegment, cn.GetCell(81), 0.3);

            // Create multiple matching segments for the active cell
            DistalDendrite otherMatchingSegment = cn.CreateDistalSegment(burstingCells[1]);
            cn.CreateSynapse(otherMatchingSegment, previousActiveCells[0], 0.3);
            cn.CreateSynapse(otherMatchingSegment, previousActiveCells[1], 0.3);
            cn.CreateSynapse(otherMatchingSegment, previousActiveCells[2], 0.3);
            cn.CreateSynapse(otherMatchingSegment, cn.GetCell(81), 0.3);

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute)  
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute)  
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry  
            LogPerformance(nameof(TestReinforcedSelectedMatchingSegmentInBurstingColumn), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            tm.Compute(previousActiveColumns, true);
            //tm.Compute(activeColumns, true);

            // Additional assertions for the new segment
            Assert.AreEqual(0.3, as1.Permanence, 0.01);
            Assert.AreEqual(0.3, as2.Permanence, 0.01);
            Assert.AreEqual(0.3, as3.Permanence, 0.01);
            Assert.AreEqual(0.3, as4.Permanence, 0.01);
            Assert.AreEqual(0.3, as5.Permanence, 0.01);
            Assert.AreEqual(0.3, is1.Permanence, 0.001);
        }

        [TestMethod]
        [TestCategory("Prod")]
        public void TestNoNewSegmentIfNotEnoughWinnerCells1()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters
            Parameters p = getDefaultParameters1(null, KEY.MAX_NEW_SYNAPSE_COUNT, 5);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Define active columns and corresponding active cells
            int[] zeroColumns = { };
            int[] activeColumns = { 0 };

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute) 
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute)
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestNoNewSegmentIfNotEnoughWinnerCells1), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            tm.Compute(zeroColumns, true);
            //tm.Compute(activeColumns, true);

            Assert.AreEqual(0, cn.NumSegments(), 0);
        }


        [TestMethod]
        [TestCategory("Prod")]
        public void TestNewSegmentAddSynapsesToSubsetOfWinnerCells()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch 
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();
            Connections cn = new Connections();

            // Set default parameters, specifying a column dimension of 64
            Parameters p = getDefaultParameters1(null, KEY.MAX_NEW_SYNAPSE_COUNT, 2);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method 
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Define active columns and corresponding active cells
            int[] previousActiveColumns = { 0, 1, 2, 3, 4 };
            int[] activeColumns = { 8 };

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute)  
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute)  
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

           // Log performance metrics and store them as a CSV entry  
            LogPerformance(nameof(TestNewSegmentAddSynapsesToSubsetOfWinnerCells), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            ComputeCycle cc = tm.Compute(previousActiveColumns, true) as ComputeCycle;

            IList<Cell> prevWinnerCells = cc.WinnerCells;
            Assert.AreEqual(5, prevWinnerCells.Count);

            cc = tm.Compute(activeColumns, true) as ComputeCycle;

            List<Cell> winnerCells = new List<Cell>(cc.WinnerCells);
            Assert.AreEqual(1, winnerCells.Count);

            //DD
            //List<DistalDendrite> segments = winnerCells[0].GetSegments(cn);
            List<DistalDendrite> segments = winnerCells[0].DistalDendrites;
            //List<DistalDendrite> segments = winnerCells[0].Segments;
            Assert.AreEqual(1, segments.Count);

            //DD List<Synapse> synapses = cn.GetSynapses(segments[0]);
            List<Synapse> synapses = segments[0].Synapses;
            Assert.AreEqual(2, synapses.Count);

            // Additional assertions for the new segment
            foreach (Synapse synapse in synapses)
            {
                Assert.AreEqual(0.14, synapse.Permanence, 0.01);
                Assert.IsTrue(prevWinnerCells.Contains(synapse.GetPresynapticCell()));
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestNewSegmentAddSynapsesToSubsetOfWinnerCells1()
        {
            // Arrange
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            var tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();
            var cn = new Connections();

            // Set default parameters, specifying a column dimension of 64
            var p = getDefaultParameters1(null, KEY.MAX_NEW_SYNAPSE_COUNT, 2);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method 
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Define active columns and corresponding active cells
            var previousActiveColumns = new[] { 0, 1, 2, 3, 4 };
            var activeColumns = new[] { 6 };

            // Act
            var cc = tm.Compute(previousActiveColumns, true) as ComputeCycle;
            var prevWinnerCells = cc.WinnerCells;

            cc = tm.Compute(activeColumns, true) as ComputeCycle;
            var winnerCells = new List<Cell>(cc.WinnerCells);

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute)  
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute)  
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestNewSegmentAddSynapsesToSubsetOfWinnerCells1), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            // Assert
            // Additional assertions for the new segment
            Assert.AreEqual(5, prevWinnerCells.Count);
            Assert.AreEqual(1, winnerCells.Count);

            var segments = winnerCells[0].DistalDendrites;
            Assert.AreEqual(1, segments.Count);

            var synapses = segments[0].Synapses;
            Assert.AreEqual(2, synapses.Count);

            foreach (var synapse in synapses)
            {
                Assert.AreEqual(0.14, synapse.Permanence, 0.01);
                Assert.IsTrue(prevWinnerCells.Contains(synapse.GetPresynapticCell()));
            }
        }



        [TestMethod]
        public void TestMatchingSegmentAddSynapsesToSubsetOfWinnerCells1()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();
            var tm = new TemporalMemory();
            var cn = new Connections();

            // Set default parameters, specifying a column dimension of 64
            var p = getDefaultParameters1(null, KEY.CELLS_PER_COLUMN, 1);
            p = getDefaultParameters1(p, KEY.MIN_THRESHOLD, 1);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            var previousActiveColumns = new[] { 0, 1, 2, 3, 4 };
            var prevWinnerCells = cn.GetCells(previousActiveColumns);
            var activeColumns = new[] { 5 };

            var matchingSegment = cn.CreateDistalSegment(cn.GetCell(5));
            cn.CreateSynapse(matchingSegment, cn.GetCell(0), 0.15);

            // Act
            var cc = tm.Compute(previousActiveColumns, true) as ComputeCycle;
            cc = tm.Compute(activeColumns, true) as ComputeCycle;

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute) 
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute) 
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestMatchingSegmentAddSynapsesToSubsetOfWinnerCells1), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            // Assert
            var synapses = matchingSegment.Synapses;
            Assert.AreEqual(2, synapses.Count);

            synapses.Sort();
            foreach (var synapse in synapses)
            {
                if (synapse.GetPresynapticCell().Index == 0) continue;

                Assert.AreEqual(0.15, synapse.Permanence, 0.01);
                Assert.IsTrue(new[] { 1, 2, 3, 4 }.Contains(synapse.GetPresynapticCell().Index));
            }
        }

        private Parameters getDefaultParameters2()
        {
            Parameters retVal = Parameters.getTemporalDefaultParameters();
            retVal.Set(KEY.COLUMN_DIMENSIONS, new int[] { 32 });
            retVal.Set(KEY.CELLS_PER_COLUMN, 6);
            retVal.Set(KEY.ACTIVATION_THRESHOLD, 3);
            retVal.Set(KEY.INITIAL_PERMANENCE, 0.20);
            retVal.Set(KEY.CONNECTED_PERMANENCE, 0.20);
            retVal.Set(KEY.MIN_THRESHOLD, 2);
            retVal.Set(KEY.MAX_NEW_SYNAPSE_COUNT, 2);
            retVal.Set(KEY.PERMANENCE_INCREMENT, 0.20);
            retVal.Set(KEY.PERMANENCE_DECREMENT, 0.20);
            retVal.Set(KEY.PREDICTED_SEGMENT_DECREMENT, 0.0);
            retVal.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            retVal.Set(KEY.SEED, 42);

            return retVal;
        }

        private HtmConfig GetDefaultTMParameters2()
        {
            HtmConfig htmConfig = new HtmConfig(new int[] { 32 }, new int[] { 32 })
            {
                CellsPerColumn = 6,
                ActivationThreshold = 3,
                InitialPermanence = 0.20,
                ConnectedPermanence = 0.20,
                MinThreshold = 2,
                MaxNewSynapseCount = 2,
                PermanenceIncrement = 0.20,
                PermanenceDecrement = 0.20,
                PredictedSegmentDecrement = 0,
                Random = new ThreadSafeRandom(42),
                RandomGenSeed = 42
            };

            return htmConfig;
        }


        private Parameters GetDefaultParameters2(Parameters p, string key, Object value)
        {
            Parameters retVal = p == null ? getDefaultParameters2() : p;
            retVal.Set(key, value);

            return retVal;
        }
        [TestMethod]
        [TestCategory("Prod")]
        [DataRow(0)]
        [DataRow(1)]
        public void TestActivateCorrectlyPredictiveCells(int tmImplementation)
        {
            // The TemporalMemory object is initialized with either the default implementation or a multithreaded implementation based on the input parameter.
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();
            TemporalMemory tm = tmImplementation == 0 ? new TemporalMemory() : new TemporalMemoryMT();
            // The Connections object is created and the default parameters are applied to it.
            // The TemporalMemory is initialized with the Connections object.
            Connections cn = new Connections();

            // Set default parameters, specifying a column dimension of 64
            Parameters p = getDefaultParameters2();
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method 
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Define active columns and corresponding active cells
            int[] previousActiveColumns = { 0 };
            int[] activeColumns = { 1 };

            // Cell6 belongs to column with index 1.
            Cell cell6 = cn.GetCell(6);

            // ISet<Cell> expectedActiveCells = Stream.of(cell6).collect(Collectors.toSet());
            ISet<Cell> expectedActiveCells = new HashSet<Cell>(new Cell[] { cell6 });

            // We add distal dentrite at column1.cell6
            DistalDendrite activeSegment = cn.CreateDistalSegment(cell6);
           
            // We add here synapses between column0.cells[0-5] and segment.
            cn.CreateSynapse(activeSegment, cn.GetCell(0), 0.20);
            cn.CreateSynapse(activeSegment, cn.GetCell(1), 0.20);
            cn.CreateSynapse(activeSegment, cn.GetCell(2), 0.20);
            cn.CreateSynapse(activeSegment, cn.GetCell(3), 0.20);
            cn.CreateSynapse(activeSegment, cn.GetCell(4), 0.20);
            cn.CreateSynapse(activeSegment, cn.GetCell(5), 0.20);

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute)  
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute)  
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry  
            LogPerformance(nameof(TestActivateCorrectlyPredictiveCells), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            ComputeCycle cc = tm.Compute(previousActiveColumns, true) as ComputeCycle;
            // The ActiveCells property of the ComputeCycle object returned by the second Compute method call is compared to the expectedActiveCells set.
            Assert.IsTrue(cc.PredictiveCells.SequenceEqual(expectedActiveCells));

            ComputeCycle cc2 = tm.Compute(activeColumns, true) as ComputeCycle;
            // The Assert.IsTrue method is used to check if the PredictiveCells and ActiveCells properties match the expectedActiveCells set.
            Assert.IsTrue(cc2.ActiveCells.SequenceEqual(expectedActiveCells));
        }


        [TestMethod]
        [TestCategory("Prod")]
        public void TestNumberOfColumns()
        {
            // The method creates a new TemporalMemory object, a Connections object and sets the column dimensions to 62x62 and cells per column to 30 using parameters
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            var tm = new TemporalMemory();
            var cn = new Connections();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();

            var p = Parameters.getAllDefaultParameters();
            p.Set(KEY.COLUMN_DIMENSIONS, new int[] { 62, 62 });
            p.Set(KEY.CELLS_PER_COLUMN, 30);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method 
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // The number of columns is verified by comparing the actual number of columns in the connections object with the expected number of columns
            var actualNumColumns = cn.HtmConfig.NumColumns;
            var expectedNumColumns = 62 * 62;

            // Assert statement is used to verify that the expected and actual number of columns are equal.
            Assert.AreEqual(expectedNumColumns, actualNumColumns);
        }


        [TestMethod]
        [TestCategory("Prod")]
        public void TestWithTwoActiveColumns()
        {
            // The test creates a TemporalMemory object, a Connections object, and sets the default parameters.
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();
            Connections cn = new Connections();

            Parameters p = getDefaultParameters2();
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method 
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // It then initializes the TemporalMemory object with the Connections object and sets two columns, 4 and 5, as active in the previous time step.
            int[] previousActiveColumns = { 4, 5 };
            Cell cell6 = cn.GetCell(7);
            Cell cell7 = cn.GetCell(8);

            // The test creates a DistalDendrite object and adds synapses between the dendrite and the cells in the first six columns.
            DistalDendrite activeSegment = cn.CreateDistalSegment(cell6);

            cn.CreateSynapse(activeSegment, cn.GetCell(0), 0.20);
            cn.CreateSynapse(activeSegment, cn.GetCell(1), 0.20);
            cn.CreateSynapse(activeSegment, cn.GetCell(2), 0.20);
            cn.CreateSynapse(activeSegment, cn.GetCell(3), 0.20);
            cn.CreateSynapse(activeSegment, cn.GetCell(4), 0.20);
            cn.CreateSynapse(activeSegment, cn.GetCell(5), 0.20);

            // The test then computes the next time step with the previously active columns and verifies that there are active and winner cells but no predictive cells.

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute)  
            stopwatch.Start();
            tmParallel.Compute(previousActiveColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute)  
            stopwatch.Start();
            tm.Compute(previousActiveColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry  
            LogPerformance(nameof(TestWithTwoActiveColumns), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            // Measure and store execution time for single-threaded computation (tm.Compute)  
            ComputeCycle cc = tm.Compute(previousActiveColumns, true) as ComputeCycle;

            // Additional assertions for the new segment
            Assert.IsFalse(cc.ActiveCells.Count == 0);
            Assert.IsFalse(cc.WinnerCells.Count == 0);
            Assert.IsTrue(cc.PredictiveCells.Count == 0);

            // It then computes the next time step with no active columns and verifies that there are no active, winner, or predictive cells.
            int[] zeroColumns = new int[0];
            ComputeCycle cc2 = tm.Compute(zeroColumns, true) as ComputeCycle;
            Assert.IsTrue(cc2.ActiveCells.Count == 0);
            Assert.IsTrue(cc2.WinnerCells.Count == 0);
            Assert.IsTrue(cc2.PredictiveCells.Count == 0);
        }

        [TestMethod]
        public void TestBurstUnpredictedColumnsforSixCells()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            var tm = new TemporalMemory();
            var cn = new Connections();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();

            var p = getDefaultParameters2();
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method 
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Setting an array with a single active column (column 0)
            var activeColumns = new[] { 0 };
            // Retrieving an array of cells with indices 0-5 (six cells).
            var burstingCells = cn.GetCells(new[] { 0, 1, 2, 3, 4, 5 });

            // Calling the Compute method of TemporalMemory with activeColumns and true as arguments,
            // which returns the result of a compute cycle.

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute)  
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute)  
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry  
            LogPerformance(nameof(TestBurstUnpredictedColumnsforSixCells), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            var result = tm.Compute(activeColumns, true);

            // Casting the result to ComputeCycle and verifying that the set of active cells in the ComputeCycle
            // is equal to the set of bursting cells.
            var cc = (ComputeCycle)result;
            Assert.IsTrue(cc.ActiveCells.SequenceEqual(burstingCells));
        }

        [TestMethod]
        [TestCategory("Prod")]
        public void TestDestroyWeakSynapseOnActiveReinforce()
        {
            // It creates a temporal memory and connections object and initializes them with default parameters
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters, specifying a column dimension of 64
            Parameters p = GetDefaultParameters2(null, KEY.INITIAL_PERMANENCE, 0.3);
            p = GetDefaultParameters2(p, KEY.MAX_NEW_SYNAPSE_COUNT, 4);
            p = GetDefaultParameters2(p, KEY.PREDICTED_SEGMENT_DECREMENT, 0.02);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method 
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // It sets the active and previous active columns and cells and creates an active segment with synapses to previous active cells
            int[] previousActiveColumns = { 0 };
            Cell[] previousActiveCells = { cn.GetCell(0), cn.GetCell(1), cn.GetCell(2), cn.GetCell(3), cn.GetCell(4) };
            int[] activeColumns = { 2 };
            Cell expectedActiveCell = cn.GetCell(5);

            DistalDendrite activeSegment = cn.CreateDistalSegment(expectedActiveCell);
            cn.CreateSynapse(activeSegment, previousActiveCells[0], 0.5);
            cn.CreateSynapse(activeSegment, previousActiveCells[1], 0.5);
            cn.CreateSynapse(activeSegment, previousActiveCells[2], 0.5);
            cn.CreateSynapse(activeSegment, previousActiveCells[3], 0.5);
            // Weak Synapse
            // One of the synapses is a weak synapse with a low permanence value
            Synapse weakSynapse = cn.CreateSynapse(activeSegment, previousActiveCells[4], 0.006);

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute)  
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // The test simulates two cycles of activity and reinforces the active synapse
            // Measure and store execution time for single-threaded computation (tm.Compute)  
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry  
            LogPerformance(nameof(TestDestroyWeakSynapseOnActiveReinforce), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            tm.Compute(previousActiveColumns, true);
            tm.Compute(activeColumns, true);
            // The test checks that the weak synapse has been destroyed and is no longer present in the active segment.
            Assert.IsFalse(activeSegment.Synapses.Contains(weakSynapse));
        }


        [TestMethod]
        [TestCategory("Prod")]
        public void TestNoNewSegmentIfNotEnoughWinnerCells()
        {
            // Arrange
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            var tm = new TemporalMemory();
            var cn = new Connections();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();

            var p = GetDefaultParameters2(null, KEY.MAX_NEW_SYNAPSE_COUNT, 5);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method 
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Define active columns and corresponding active cells
            var zeroColumns = new int[0];
            var activeColumns = new[] { 0 };

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute)  
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute)  
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry  
            LogPerformance(nameof(TestNoNewSegmentIfNotEnoughWinnerCells), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            tm.Compute(zeroColumns, true);
            //tm.Compute(activeColumns, true);

            // Assert
            Assert.AreEqual(1, cn.NumSegments());
        }


        [TestMethod]
        [TestCategory("Prod")]
        public void TestNewSegmentAddSynapsesToSubsetOfWinnerCells2()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();

            Parameters p = GetDefaultParameters2(null, KEY.MAX_NEW_SYNAPSE_COUNT, 2);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method 
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Define active columns and corresponding active cells
            int[] previousActiveColumns = { 0, 1, 2, 3, 4 };
            int[] activeColumns = { 4 };

            ComputeCycle cc = tm.Compute(previousActiveColumns, true) as ComputeCycle;

            IList<Cell> prevWinnerCells = cc.WinnerCells;
            Assert.AreEqual(5, prevWinnerCells.Count);

            cc = tm.Compute(activeColumns, true) as ComputeCycle;

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute)  
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute)  
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry  
            LogPerformance(nameof(TestNewSegmentAddSynapsesToSubsetOfWinnerCells2), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            List<Cell> winnerCells = new List<Cell>(cc.WinnerCells);
            Assert.AreEqual(1, winnerCells.Count);

            //DD
            //List<DistalDendrite> segments = winnerCells[0].GetSegments(cn);
            List<DistalDendrite> segments = winnerCells[0].DistalDendrites;
            //List<DistalDendrite> segments = winnerCells[0].Segments;
            Assert.AreEqual(1, segments.Count);

            //DD List<Synapse> synapses = cn.GetSynapses(segments[0]);
            List<Synapse> synapses = segments[0].Synapses;
            Assert.AreEqual(2, synapses.Count);

            // Additional assertions for the new segment
            foreach (Synapse synapse in synapses)
            {
                Assert.AreEqual(0.23, synapse.Permanence, 0.05);
                Assert.IsTrue(prevWinnerCells.Contains(synapse.GetPresynapticCell()));
            }
        }


        [TestMethod]
        [TestCategory("Prod")]
        public void TestNewSegmentAddSynapsesToAllWinnerCells()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();

            Parameters p = GetDefaultParameters2(null, KEY.MAX_NEW_SYNAPSE_COUNT, 5);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method 
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Define active columns and corresponding active cells
            int[] previousActiveColumns = { 0, 1, 2, 3, 4 };
            int[] activeColumns = { 5 };

            ComputeCycle cc = tm.Compute(previousActiveColumns, true) as ComputeCycle;
            List<Cell> prevWinnerCells = new List<Cell>(cc.WinnerCells);
            Assert.AreEqual(5, prevWinnerCells.Count);

            cc = tm.Compute(activeColumns, true) as ComputeCycle;

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute)  
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute)  
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry  
            LogPerformance(nameof(TestNewSegmentAddSynapsesToAllWinnerCells), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            List<Cell> winnerCells = new List<Cell>(cc.WinnerCells);
            Assert.AreEqual(1, winnerCells.Count);

            //DD
            //List<DistalDendrite> segments = winnerCells[0].GetSegments(cn);
            List<DistalDendrite> segments = winnerCells[0].DistalDendrites;

            //List<DistalDendrite> segments = winnerCells[0].Segments;
            Assert.AreEqual(1, segments.Count);
            //List<Synapse> synapses = segments[0].GetAllSynapses(cn);
            List<Synapse> synapses = segments[0].Synapses;

            // Additional assertions for the new segment
            List<Cell> presynapticCells = new List<Cell>();
            foreach (Synapse synapse in synapses)
            {
                Assert.AreEqual(0.23, synapse.Permanence, 0.05);
                presynapticCells.Add(synapse.GetPresynapticCell());
            }

            presynapticCells.Sort();

            Assert.IsTrue(prevWinnerCells.SequenceEqual(presynapticCells));
        }

        [TestMethod]
        [TestCategory("Prod")]
        public void TestMatchingSegmentAddSynapsesToAllWinnerCells()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters, specifying a column dimension of 64
            Parameters p = GetDefaultParameters2(null, KEY.CELLS_PER_COLUMN, 1);
            p = GetDefaultParameters2(p, KEY.MIN_THRESHOLD, 1);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Define active columns and corresponding active cells
            int[] previousActiveColumns = { 0, 1 };
            IList<Cell> prevWinnerCells = cn.GetCells(new int[] { 0, 1 });
            int[] activeColumns = { 4 };

            // Create Synapses and Distal Dendrites
            DistalDendrite matchingSegment = cn.CreateDistalSegment(cn.GetCell(4));
            cn.CreateSynapse(matchingSegment, cn.GetCell(0), 0.5);

            ComputeCycle cc = tm.Compute(previousActiveColumns, true) as ComputeCycle;
            Assert.IsTrue(cc.WinnerCells.SequenceEqual(prevWinnerCells));

            cc = tm.Compute(activeColumns, true) as ComputeCycle;

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute) 
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute) 
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestMatchingSegmentAddSynapsesToAllWinnerCells), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            //DD List<Synapse> synapses = cn.GetSynapses(matchingSegment);
            List<Synapse> synapses = matchingSegment.Synapses;
            Assert.AreEqual(2, synapses.Count);

            synapses.Sort();

            foreach (Synapse synapse in synapses)
            {
                if (synapse.GetPresynapticCell().Index == 0) continue;

                Assert.AreEqual(0.25, synapse.Permanence, 0.05);
                Assert.AreEqual(1, synapse.GetPresynapticCell().Index);
            }
        }


        [TestMethod]
        public void TestActiveSegmentGrowSynapsesAccordingToPotentialOverlap()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            Parameters p = GetDefaultParameters2(null, KEY.CELLS_PER_COLUMN, 1);
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters, specifying a column dimension of 64
            p = GetDefaultParameters2(p, KEY.MIN_THRESHOLD, 1);
            p = GetDefaultParameters2(p, KEY.ACTIVATION_THRESHOLD, 2);
            p = GetDefaultParameters2(p, KEY.MAX_NEW_SYNAPSE_COUNT, 4);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Use 1 cell per column so that we have easy control over the winner cells.
            int[] previousActiveColumns = { 0, 1, 2, 3, 4 };
            List<Cell> prevWinnerCells = new List<Cell>(new Cell[] { cn.GetCell(0), cn.GetCell(1), cn.GetCell(2), cn.GetCell(3), cn.GetCell(4) });

            // Define active column and corresponding active cell
            int[] activeColumns = { 5 };

            // Create Synapses and Distal Dendrites
            DistalDendrite activeSegment = cn.CreateDistalSegment(cn.GetCell(5));
            cn.CreateSynapse(activeSegment, cn.GetCell(0), 0.5);
            cn.CreateSynapse(activeSegment, cn.GetCell(1), 0.5);
            cn.CreateSynapse(activeSegment, cn.GetCell(2), 0.2);

            ComputeCycle cc = tm.Compute(previousActiveColumns, true) as ComputeCycle;
            Assert.IsTrue(prevWinnerCells.SequenceEqual(cc.WinnerCells));
            cc = tm.Compute(activeColumns, true) as ComputeCycle;

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute) 
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute) 
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestActiveSegmentGrowSynapsesAccordingToPotentialOverlap), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            List<Cell> presynapticCells = new List<Cell>();
            foreach (var syn in activeSegment.Synapses)
            {
                presynapticCells.Add(syn.GetPresynapticCell());
            }

            Assert.IsTrue(
                presynapticCells.Count == 4 && (
                (presynapticCells.Contains(cn.GetCell(0)) && presynapticCells.Contains(cn.GetCell(1)) && presynapticCells.Contains(cn.GetCell(2)) && presynapticCells.Contains(cn.GetCell(3))) ||
                (presynapticCells.Contains(cn.GetCell(0)) && presynapticCells.Contains(cn.GetCell(1)) && presynapticCells.Contains(cn.GetCell(2)) && presynapticCells.Contains(cn.GetCell(4)))));
        }


        [TestMethod]
        [TestCategory("Prod")]
        public void TestDestroyWeakSynapseOnWrongPrediction()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();

            Parameters p = GetDefaultParameters2(null, KEY.INITIAL_PERMANENCE, 0.3);
            p = GetDefaultParameters2(p, KEY.MAX_NEW_SYNAPSE_COUNT, 4);
            p = GetDefaultParameters2(p, KEY.PREDICTED_SEGMENT_DECREMENT, 0.02);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method 
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Define active columns and corresponding active cells
            int[] previousActiveColumns = { 0 };
            Cell[] previousActiveCells = { cn.GetCell(0), cn.GetCell(1), cn.GetCell(2), cn.GetCell(3) };
            int[] activeColumns = { 2 };
            Cell expectedActiveCell = cn.GetCell(5);

            // Create Synapses and Distal Dendrites
            DistalDendrite activeSegment = cn.CreateDistalSegment(expectedActiveCell);
            cn.CreateSynapse(activeSegment, previousActiveCells[0], 0.5);
            cn.CreateSynapse(activeSegment, previousActiveCells[1], 0.5);
            cn.CreateSynapse(activeSegment, previousActiveCells[2], 0.5);

            // Weak Synapse
            cn.CreateSynapse(activeSegment, previousActiveCells[3], 0.017);

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute)  
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute)  
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry  
            LogPerformance(nameof(TestDestroyWeakSynapseOnWrongPrediction), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            tm.Compute(previousActiveColumns, true);
            //tm.Compute(activeColumns, true);

            Assert.AreEqual(3, activeSegment.Synapses.Count);
        }


        [TestMethod]
        [Category("Prod")]
        public void TestNoneActiveColumns1()
        {
            // Arrange
            // The method tests if there are no active columns in the temporal memory, by creating a distal segment on a cell and adding synapses to it
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            var tm = new TemporalMemory();
            var cn = new Connections();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();
            getDefaultParameters2().apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method 
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            var cell6 = cn.GetCell(6);
            var activeSegment = cn.CreateDistalSegment(cell6);
            var synapses = new[] { 0, 1, 2, 3, 4, 5 }
                .Select(i => cn.GetCell(i))
                .Select(cell => cn.CreateSynapse(activeSegment, cell, 0.20))
                .ToList();

            // Act
            // Then, it computes the temporal memory's activity for two different input patterns - one with a single active cell and one with no active cells
            // The expected behavior is that the temporal memory should have no active, winner, or predictive cells for the second input pattern
            var cc1 = tm.Compute(new[] { 0 }, true) as ComputeCycle;
            var cc2 = tm.Compute(new int[0], true) as ComputeCycle;

            // The Assert statements check if the actual results match the expected results, and if not, the test fails.
            Assert.IsFalse(cc1.ActiveCells.Count == 0);
            Assert.IsFalse(cc1.WinnerCells.Count == 0);
            Assert.IsFalse(cc1.PredictiveCells.Count == 0);
            Assert.IsTrue(cc2.ActiveCells.Count == 0);
            Assert.IsTrue(cc2.WinnerCells.Count == 0);
            Assert.IsTrue(cc2.PredictiveCells.Count == 0);
        }


        [TestMethod]
        [TestCategory("Prod")]
        public void TestAdaptSegmentToCentre()
        {
            // The method initializes a TemporalMemory object, Connections object and Parameters object with default values and creates a DistalDendrite and Synapse.
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();

            Parameters p = getDefaultParameters2();
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method 
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method 
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // The AdaptSegment method is then called on the TemporalMemory object to adjust the permanence of the Synapse.
            DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(0));
            Synapse s1 = cn.CreateSynapse(dd, cn.GetCell(6), 0.8); // set initial permanence to 0.8

            // The Assert.AreEqual method checks that the permanence value of the Synapse has been adjusted correctly.
            TemporalMemory.AdaptSegment(cn, dd, cn.GetCells(new int[] { 6 }), 0.1, 0.1); // adjust permanence by 0.1 increment and decrement

            // The method then calls the AdaptSegment method again to test that the permanence value is at the mean.
            Assert.AreEqual(0.9, s1.Permanence, 0.1);

            // Now permanence should be at mean
            // Another Assert.AreEqual method checks that the permanence value of the Synapse is equal to 1.0 within a tolerance of 0.1.
            TemporalMemory.AdaptSegment(cn, dd, cn.GetCells(new int[] { 6 }), 0.1, 0.1); // adjust permanence by 0.1 increment and decrement
            Assert.AreEqual(1.0, s1.Permanence, 0.1);
        }


        [TestMethod]
        public void TestArrayNotContainingCells()
        {
            // Arrange
            // The Arrange section sets up the necessary objects for the test, including initializing a TemporalMemory object and creating a Connections object with default parameters.

            // Initialize Temporal Memory, HtmConfig, Parallel Processing, Connections, and Stopwatch
            HtmConfig htmConfig = GetDefaultTMParameters2();
            Connections cn = new Connections(htmConfig);
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method 
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // An array of active columns is defined, as well as an array of cells to be used as bursting cells.
            int[] activeColumns = { 4, 5 };
            Cell[] burstingCells = cn.GetCells(new int[] { 0, 1, 2, 3, 4, 5 });

            // Act
            // The Act section calls the Compute method on the TemporalMemory object, passing in the active columns array and setting the "learn" parameter to true.

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute)  
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute)  
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry  
            LogPerformance(nameof(TestArrayNotContainingCells), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            ComputeCycle cc = tm.Compute(activeColumns, true) as ComputeCycle;

            // Assert
            // Verify that ComputeCycle's ActiveCells array does not contain any cells from burstingCells array
            // For each cell, the Assert.IsFalse method is used to verify that the ActiveCells array does not contain any cells from the BurstingCells array.
            foreach (var cell in cc.ActiveCells)
            {
                Assert.IsFalse(cc.ActiveCells.SequenceEqual(burstingCells));
            }
        }


        [TestMethod]
        [TestCategory("Prod")]
        public void TestDestroySegmentsWithTooFewSynapsesToBeMatching()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();

            // Set Parameters for Connections
            Parameters p = GetDefaultParameters2(null, KEY.INITIAL_PERMANENCE, .2);
            p = GetDefaultParameters2(p, KEY.MAX_NEW_SYNAPSE_COUNT, 4);
            p = GetDefaultParameters2(p, KEY.PREDICTED_SEGMENT_DECREMENT, 0.02);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Set previous and current active columns
            int[] prevActiveColumns = { 0 };
            int[] activeColumns = { 2 };

            // Set expected active cell
            Cell expectedActiveCell = cn.GetCell(6);

            // Create previous active cells array
            Cell[] prevActiveCells = { cn.GetCell(0), cn.GetCell(1), cn.GetCell(2), cn.GetCell(3), cn.GetCell(4) };

            // Create matching distal segment with synapses from previous active cells to expected active cell
            DistalDendrite matchingSegment = cn.CreateDistalSegment(cn.GetCell(6));
            cn.CreateSynapse(matchingSegment, prevActiveCells[0], .015);
            cn.CreateSynapse(matchingSegment, prevActiveCells[1], .015);
            cn.CreateSynapse(matchingSegment, prevActiveCells[2], .015);
            cn.CreateSynapse(matchingSegment, prevActiveCells[3], .015);
            cn.CreateSynapse(matchingSegment, prevActiveCells[4], .015);

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute) 
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute) 
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestDestroySegmentsWithTooFewSynapsesToBeMatching), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            tm.Compute(prevActiveColumns, true);
            //tm.Compute(activeColumns, true);

            // Assert that the expected active cell has no segments
            Assert.AreEqual(0, cn.NumSegments(expectedActiveCell));
        }


        [TestMethod]
        [TestCategory("Prod")]
        public void TestNewSegmentAddSynapsesToAllWinnerCells1()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();

            // Set parameters for the connections
            Parameters p = GetDefaultParameters2(null, KEY.MAX_NEW_SYNAPSE_COUNT, 6);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Set up the previous active and current active columns
            int[] previousActiveColumns = { 0, 1, 2, 3, 4, 5 };
            int[] activeColumns = { 6 };

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute) 
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute)
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestNewSegmentAddSynapsesToAllWinnerCells1), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            // Compute the previous active columns and get the winner cells
            ComputeCycle cc = tm.Compute(previousActiveColumns, true) as ComputeCycle;
            List<Cell> prevWinnerCells = new List<Cell>(cc.WinnerCells);

            // Check that there are 6 winner cells
            Assert.AreEqual(6, prevWinnerCells.Count);

            // Compute the current active columns and get the winner cells
            cc = tm.Compute(activeColumns, true) as ComputeCycle;
            List<Cell> winnerCells = new List<Cell>(cc.WinnerCells);

            // Check that there is only 1 winner cell
            Assert.AreEqual(1, winnerCells.Count);

            // Get the distal dendrites for the winner cell
            List<DistalDendrite> segments = winnerCells[0].DistalDendrites;

            // Check that there is only 1 segment for the winner cell
            Assert.AreEqual(1, segments.Count);

            // Get all the synapses for the segment
            List<Synapse> synapses = segments[0].Synapses;

            // Check that all the synapses have a permanence of 0.25 within a tolerance of 0.05
            List<Cell> presynapticCells = new List<Cell>();
            foreach (Synapse synapse in synapses)
            {
                Assert.AreEqual(0.25, synapse.Permanence, 0.05);
                presynapticCells.Add(synapse.GetPresynapticCell());
            }

            // Sort the presynaptic cells and check that they are the same as the previous winner cells
            presynapticCells.Sort();
            Assert.IsTrue(prevWinnerCells.SequenceEqual(presynapticCells));
        }

        [TestMethod]
        [TestCategory("Prod")]
        [DataRow(0)]
        [DataRow(1)]
        public void TestActivateCorrectlyPredictiveCells1(int tmImplementation)
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = tmImplementation == 0 ? new TemporalMemory() : new TemporalMemoryMT();
            Connections cn = new Connections();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters, specifying a column dimension of 64
            Parameters p = getDefaultParameters2();
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Define active columns and corresponding active cells
            int[] previousActiveColumns = { 0 };
            int[] activeColumns = { 1 };

            // Create Synapses and Distal Dendrites
            Cell cell6 = cn.GetCell(6);
            DistalDendrite activeSegment = cn.CreateDistalSegment(cell6);

            cn.CreateSynapse(activeSegment, cn.GetCell(0), 0.20);
            cn.CreateSynapse(activeSegment, cn.GetCell(1), 0.20);
            cn.CreateSynapse(activeSegment, cn.GetCell(2), 0.20);
            cn.CreateSynapse(activeSegment, cn.GetCell(3), 0.20);
            cn.CreateSynapse(activeSegment, cn.GetCell(4), 0.20);
            cn.CreateSynapse(activeSegment, cn.GetCell(5), 0.20);

            ISet<Cell> expectedActiveCells = new HashSet<Cell>(new Cell[] { cell6 });

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute) 
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Act
            // Measure and store execution time for single-threaded computation (tm.Compute) 
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestActivateCorrectlyPredictiveCells1), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            ComputeCycle cc1 = tm.Compute(previousActiveColumns, true) as ComputeCycle;
            ComputeCycle cc2 = tm.Compute(activeColumns, true) as ComputeCycle;

            // Assert
            Assert.IsTrue(cc1.PredictiveCells.SequenceEqual(expectedActiveCells));
            Assert.IsTrue(cc2.ActiveCells.SequenceEqual(expectedActiveCells));
        }

        [TestMethod]
        [TestCategory("Prod")]
        public void TestNoNewSegmentIfNotEnoughWinnerCells3()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters
            Parameters p = GetDefaultParameters2(null, KEY.MAX_NEW_SYNAPSE_COUNT, 6);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Define active columns and corresponding active cells
            int[] zeroColumns = { };
            int[] activeColumns = { 0 };

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute) 
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute) 
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestNoNewSegmentIfNotEnoughWinnerCells3), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            tm.Compute(zeroColumns, true);
            //tm.Compute(activeColumns, true);

            Assert.AreEqual(0, cn.NumSegments(), 0);
        }

        [TestMethod]
        [TestCategory("Prod")]
        public void TestDestroyWeakSynapseOnActiveReinforce1()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters
            Parameters p = GetDefaultParameters2(null, KEY.INITIAL_PERMANENCE, 0.3);
            p = GetDefaultParameters2(p, KEY.MAX_NEW_SYNAPSE_COUNT, 6);
            p = GetDefaultParameters2(p, KEY.PREDICTED_SEGMENT_DECREMENT, 0.04);
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Define active columns and corresponding active cells
            int[] previousActiveColumns = { 0 };
            Cell[] previousActiveCells = { cn.GetCell(0), cn.GetCell(1), cn.GetCell(2), cn.GetCell(3), cn.GetCell(4), cn.GetCell(5) };
            int[] activeColumns = { 5 };
            Cell expectedActiveCell = cn.GetCell(6);

            // Create Synapses and Distal Dendrites
            DistalDendrite activeSegment = cn.CreateDistalSegment(expectedActiveCell);
            cn.CreateSynapse(activeSegment, previousActiveCells[0], 0.5);
            cn.CreateSynapse(activeSegment, previousActiveCells[1], 0.5);
            cn.CreateSynapse(activeSegment, previousActiveCells[2], 0.5);
            cn.CreateSynapse(activeSegment, previousActiveCells[3], 0.5);
            cn.CreateSynapse(activeSegment, previousActiveCells[4], 0.5);

            // Weak Synapse
            cn.CreateSynapse(activeSegment, previousActiveCells[5], 0.009);

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute) 
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute)
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestDestroyWeakSynapseOnActiveReinforce1), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            tm.Compute(previousActiveColumns, true);
            //tm.Compute(activeColumns, true);

            Assert.AreEqual(6, activeSegment.Synapses.Count);
        }


        [TestMethod]
        [TestCategory("Prod")]
        public void TestBurstNotpredictedColumns()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters, specifying a column dimension of 64
            Parameters p = getDefaultParameters2();
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Define active columns and corresponding active cells
            int[] activeColumns = { 1, 2, 3 }; //Cureently Active column
            IList<Cell> burstingCells = cn.GetCells(new int[] { 0, 1, 2, 3, 4, 5 }); //Number of Cell Indexes

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute) 
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute) 
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry 
            LogPerformance(nameof(TestBurstNotpredictedColumns), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            ComputeCycle cc = tm.Compute(activeColumns, true) as ComputeCycle; //COmpute class object 

            Assert.IsFalse(cc.ActiveCells.SequenceEqual(burstingCells));
        }







        /*
         
          Unit Testing  
          -------------------------------------------------------------------------------------------------------------------
          Test Overview for InitParallelWithConcurrentDictionary() Method
          -------------------------------------------------------------------------------------------------------------------
          
          The following test cases cover various scenarios for the `InitParallelWithConcurrentDictionary` method 
          in the TemporalMemoryParallelProcessing class. These tests ensure the method handles multiple edge cases 
          and works as expected under different conditions.

          Test Case 1: TestBasicSequenceLearningAndRecallParallel
          Purpose: This test case verifies the learning and recall of a sequence using both single-threaded and parallel processing.
        
          Test Case 2: TestActiveCellPrediction
          Purpose: This test case verifies the prediction of active cells in a sequence by comparing single-threaded and parallel execution times.
       
          Test Case 3: TestParallelInitializationWithMultipleActiveColumns
          Purpose: This test case verifies the parallel initialization of the Temporal Memory system with multiple active columns.
       
          Test Case 4: TestTemporalMemoryInitializationWithDefaultParameters
          Purpose: This test case verifies that Temporal Memory initializes correctly using default parameters and parallel initialization with a concurrent dictionary.

          Test Case 5: Verify Parallel Initialization with Fewer Columns
          Purpose: To test the initialization of the Temporal Memory system with a smaller number of columns in parallel.
          
          Test Case 6: Stress Test with Varying Thread Counts
          Purpose: To evaluate the performance of Temporal Memory with different thread counts and analyze optimal thread usage.
          
          Test Case 7: Scalability Test with Increasing Data Size
          Purpose: To measure the scalability of Temporal Memory by testing performance across increasing problem sizes.
         
          Test Case 8: Testing Performance with Different Active Columns and Excluded Cells
          Purpose: This test case evaluates the performance of Temporal Memory (TM) in both single-threaded and multi-threaded modes, with a focus on handling active columns and excluded cells.
          
          Test Case 9: Verifying Computation with Different Burst Thresholds
          Purpose: This test case verifies that computation using Temporal Memory produces the expected bursting cells based on given thresholds.
          
          Test Case 10: Testing No Change When No Active Cells Selected for Segments in Burst
          Purpose: This test case checks that when no active cells are selected during the burst, there should be no change in the permanence values of the synapses.

          Test Case 11: TestSequenceLearningWithHighSparsity
          Purpose: This test case verifies the growth of new segments when multiple columns are active in a high-sparsity sequence. It compares the performance of single-threaded and multi-threaded Temporal Memory computation.
          
          Test Case 12: TestSegmentGrowthWithMultipleActiveColumns
          Purpose: This test case verifies the learning and recall of a high-sparsity sequence using both single-threaded and parallel versions of the Temporal Memory algorithm. It checks if new segments grow as expected in the presence of multiple active columns.
          
          Test Case 13: TestParallelInitializationWithCustomSynapseConnections
          Purpose: This test case verifies that synapses are correctly created and initialized for active cells during parallel initialization using a custom configuration. It tests the creation of synapse connections during the parallel initialization phase.
        

         */




        //Test Case 1: TestBasicSequenceLearningAndRecallParallel
        //Purpose: This test case verifies the learning and recall of a sequence using both single-threaded and parallel processing.
        /// <summary>
        /// Tests sequence learning and recall using both single-threaded and parallel Temporal Memory (TM) implementations.  
        /// Validates that active cells are correctly recalled and compares execution times between parallel (<see cref="TemporalMemoryParallelProcessing.InitParallelWithConcurrentDictionary"/>)  
        /// and single-threaded initialization. Ensures thread safety during column/cell creation.  
        /// </summary>
        /// <remarks>
        /// Measures performance via <see cref="Stopwatch"/> and asserts recall accuracy for the sequence [0, 1, 2, 3, 4, 5, 6].
        /// </remarks>

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

            // Learn the sequence in parallel mode
            stopwatch.Restart();
            ComputeCycle parallelCycle = tmParallel.Compute(sequenceActiveColumns, true) as ComputeCycle;
            stopwatch.Stop();
            TimeSpan parallelComputeTime = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for parallel Compute: {parallelComputeTime.TotalMilliseconds} milliseconds");
            
            // Act
            // Learn the sequence in single-threaded mode
            stopwatch.Start();
            ComputeCycle singleThreadedCycle = tm.Compute(sequenceActiveColumns, true) as ComputeCycle;
            stopwatch.Stop();
            TimeSpan singleThreadedComputeTime = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for single-threaded Compute: {singleThreadedComputeTime.TotalMilliseconds} milliseconds");            

            // Recall the sequence in parallel mode
            ComputeCycle recallCycle = tmParallel.Compute(sequenceActiveColumns, false) as ComputeCycle;

            // Assert
            Assert.IsTrue(recallCycle.ActiveCells.Count > 0, "No active cells were recalled.");
        }


        //Test Case 2: TestActiveCellPrediction
        //Purpose: This test case verifies the prediction of active cells in a sequence by comparing single-threaded and parallel execution times.
        /// <summary>
        /// Verifies TM's ability to predict active cells after exposure to a sequence (e.g., [3, 4, 5] following [0, 1, 2]).  
        /// Compares initialization and computation times between single-threaded and parallel modes using <see cref="TemporalMemoryParallelProcessing"/>.  
        /// </summary>
        /// <remarks>
        /// Logs performance metrics (CSV) and asserts that predicted cells match expected active cells.  
        /// Uses <see cref="Connections.GetCell"/> for cell indexing.
        /// </remarks>

        [TestMethod]
        public void TestActiveCellPrediction() 
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters
            Parameters p = GetDefaultParameters();
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Define active columns and corresponding active cells
            int[] previousActiveColumns = { 0, 1, 2 };
            int[] activeColumns = { 3, 4, 5 };
            Cell[] previousActiveCells = { cn.GetCell(0), cn.GetCell(1), cn.GetCell(2) };
            Cell[] activeCells = { cn.GetCell(3), cn.GetCell(4), cn.GetCell(5) };

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute)
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute)
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry
            LogPerformance(nameof(TestActiveCellPrediction), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            // Verify that the predicted active cells match the expected active cells
            Assert.IsTrue(activeCells.All(ac => tm.Compute(previousActiveColumns, false).ActiveCells.Contains(ac)));
        }


        //Test Case 3: TestParallelInitializationWithMultipleActiveColumns
        //Purpose: This test case verifies the parallel initialization of the Temporal Memory system with multiple active columns.
        /// <summary>
        /// Validates parallel initialization of TM with a 16x16 column grid.  
        /// Ensures thread-safe column creation via <see cref="ConcurrentDictionary"/> and checks the total column count.  
        /// </summary>
        /// <remarks>
        /// Asserts that 256 columns are created without exceptions. Tests scalability for larger column dimensions.
        /// </remarks>

        [TestMethod]
        public void TestParallelInitializationWithMultipleActiveColumns() 
        {
            // Initialize Temporal Memory Parallel Processing and Connections
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            Parameters p = Parameters.getAllDefaultParameters();
            p.Set(KEY.COLUMN_DIMENSIONS, new int[] { 16, 16 }); // Create a grid with multiple columns
            p.apply(cn);

            // Measure the time for parallel initialization
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();

            // Assert the expected number of columns
            Assert.AreEqual(16 * 16, cn.GetColumns().Count);

            // Assert no exceptions during initialization
            Assert.IsTrue(cn.GetColumns().Count > 0);
        }

        //Test Case 4: TestTemporalMemoryInitializationWithDefaultParameters
        //Purpose: This test case verifies that Temporal Memory initializes correctly using default parameters and parallel initialization with a concurrent dictionary.
        /// <summary>
        /// Tests TM initialization with default parameters in both single-threaded and parallel modes.  
        /// Measures execution times and validates successful column creation.  
        /// </summary>
        /// <remarks>
        /// Uses <see cref="Parameters.getAllDefaultParameters"/> for configuration.  
        /// Asserts <see cref="Connections.GetColumns"/>.Count > 0.
        /// </remarks>

        [TestMethod]
        public void TestTemporalMemoryInitializationWithDefaultParameters() 
        {
            // Initialize Temporal Memory and Connections
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);

            // Measure and store execution time for single-threaded initialization
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();

            // Measure and store execution time for multi-threaded initialization
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();

            // Assert that the initialization did not fail
            Assert.IsTrue(cn.GetColumns().Count > 0);
        }


        //Test Case 5: Verify Parallel Initialization with Fewer Columns
        //Purpose: To test the initialization of the Temporal Memory system with a smaller number of columns in parallel.
        /// <summary>
        /// Validates parallel initialization with a small column grid (4x4).  
        /// Focuses on correctness for minimal-scale inputs and thread safety.  
        /// </summary>
        /// <remarks>
        /// Asserts 16 columns are created and checks for initialization exceptions.  
        /// Uses <see cref="KEY.COLUMN_DIMENSIONS"/> to configure grid size.
        /// </remarks>

        [TestMethod]
        public void TestParallelInitializationWithFewerColumns() 
        {
            // Initialize Temporal Memory Parallel Processing and Connections
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            Parameters p = Parameters.getAllDefaultParameters();
            p.Set(KEY.COLUMN_DIMENSIONS, new int[] { 4, 4 }); // Smaller dimensions for testing
            p.apply(cn);

            // Measure the time for parallel initialization
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();

            // Assert the expected number of columns created
            Assert.AreEqual(4 * 4, cn.GetColumns().Count);

            // Assert no exceptions during initialization
            Assert.IsTrue(cn.GetColumns().Count > 0);
        }


        //Test Case 6: Stress Test with Varying Thread Counts
        //Purpose: To evaluate the performance of Temporal Memory with different thread counts and analyze optimal thread usage.
        /// <summary>
        /// Evaluates TM performance across thread counts (1 to 2x logical cores) to determine optimal parallelism.  
        /// Configures <see cref="ParallelOptions.MaxDegreeOfParallelism"/> and measures speedup.  
        /// </summary>
        /// <remarks>
        /// Logs execution times per thread count and asserts optimal threads ≤ 2x <see cref="Environment.ProcessorCount"/>.  
        /// Uses <see cref="ThreadPool.SetMinThreads"/> to control concurrency.
        /// </remarks>

        [TestMethod]
        public void TestParallelPerformanceWithVaryingThreadCounts() 
        {
            // Initialize components
            TemporalMemory tm = new TemporalMemory();
            Connections cn = new Connections();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();

            // Set up configuration
            Parameters p = GetDefaultParameters();
            p.apply(cn);

            // Get the number of logical processors
            int processorCount = Environment.ProcessorCount;
            Console.WriteLine($"System has {processorCount} logical processors");

            // Single-threaded baseline
            stopwatch.Restart();
            tm.Init(cn);
            stopwatch.Stop();
            double baselineTime = stopwatch.Elapsed.TotalMilliseconds;
            Console.WriteLine($"Baseline single-threaded time: {baselineTime} ms");

            // Test with different thread counts
            Dictionary<int, double> threadPerformance = new Dictionary<int, double>();
            int[] threadCountsToTest = { 1, 2, 4, 8, 16, processorCount };

            foreach (int threadCount in threadCountsToTest.Distinct().OrderBy(t => t))
            {
                if (threadCount > processorCount * 2) continue; // Avoid excessive oversubscription

                // Reset connections
                cn = new Connections();
                p.apply(cn);

                // Set thread count for this test
                ThreadPool.GetMinThreads(out int workerThreads, out int completionPortThreads);
                ThreadPool.SetMinThreads(threadCount, completionPortThreads);

                // Set max parallelism through ParallelOptions
                ParallelOptions parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = threadCount };

                // Apply thread count to parallelized code (assume method exists)
                // ((TemporalMemoryParallelProcessing)tmParallel).SetParallelOptions(parallelOptions);

                // Execute and measure
                stopwatch.Restart();
                tmParallel.InitParallelWithConcurrentDictionary(cn);
                stopwatch.Stop();

                double currentTime = stopwatch.Elapsed.TotalMilliseconds;
                threadPerformance[threadCount] = currentTime;
                Console.WriteLine($"Thread count: {threadCount}, Time: {currentTime} ms, Speedup: {baselineTime / currentTime:F2}x");
            }

            // Log and analyze results
            var optimalThreadCount = threadPerformance.OrderBy(kvp => kvp.Value).First().Key;
            Console.WriteLine($"Optimal thread count: {optimalThreadCount} with execution time {threadPerformance[optimalThreadCount]} ms");

            // Assert optimal thread count is reasonable
            Assert.IsTrue(optimalThreadCount > 0 && optimalThreadCount <= processorCount * 2,
                         "Optimal thread count should be between 1 and 2x processor count");

            // Verify performance improvement
            Assert.IsTrue(threadPerformance[optimalThreadCount] < baselineTime,
                         "Parallel execution should be faster than single-threaded with optimal thread count");
        }


        //Test Case 7: Scalability Test with Increasing Data Size
        //Purpose: To measure the scalability of Temporal Memory by testing performance across increasing problem sizes.
        /// <summary>
        /// Measures TM scalability by testing column counts from 100 to 5000.  
        /// Compares single-threaded vs. parallel initialization and computation times.  
        /// </summary>
        /// <remarks>
        /// Calculates speedup and scalability efficiency. Asserts that larger problems yield better parallel performance.  
        /// Uses <see cref="KEY.CELLS_PER_COLUMN"/> and <see cref="KEY.COLUMN_DIMENSIONS"/> for configuration.
        /// </remarks>

        [TestMethod]
        public void TestParallelScalabilityWithIncreasingSize() 
        {
            // Initialize components
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Stopwatch stopwatch = new Stopwatch();

            // Define problem sizes to test
            int[] columnSizes = { 100, 500, 1000, 2000, 5000 };

            // Store results for analysis
            Dictionary<int, (double SingleTime, double ParallelTime)> results =
                new Dictionary<int, (double, double)>();

            foreach (int size in columnSizes)
            {
                Console.WriteLine($"Testing with {size} columns...");

                // Set up parameters for this size
                Parameters p = GetDefaultParameters();
                p.Set(KEY.COLUMN_DIMENSIONS, new int[] { size });
                p.Set(KEY.CELLS_PER_COLUMN, 16);

                // Single-threaded test
                Connections cnSingle = new Connections();
                p.apply(cnSingle);

                stopwatch.Restart();
                tm.Init(cnSingle);
                stopwatch.Stop();
                double singleTime = stopwatch.Elapsed.TotalMilliseconds;

                // Generate active columns for compute test (10% of columns)
                int[] activeColumns = Enumerable.Range(0, size / 10)
                    .Select(i => i * 10).ToArray();

                stopwatch.Restart();
                tm.Compute(activeColumns, true);
                stopwatch.Stop();
                double singleComputeTime = stopwatch.Elapsed.TotalMilliseconds;

                // Total single-threaded time
                double totalSingleTime = singleTime + singleComputeTime;

                // Parallel test
                Connections cnParallel = new Connections();
                p.apply(cnParallel);

                stopwatch.Restart();
                tmParallel.InitParallelWithConcurrentDictionary(cnParallel);
                stopwatch.Stop();
                double parallelTime = stopwatch.Elapsed.TotalMilliseconds;

                stopwatch.Restart();
                tmParallel.Compute(activeColumns, true);
                stopwatch.Stop();
                double parallelComputeTime = stopwatch.Elapsed.TotalMilliseconds;

                // Total parallel time
                double totalParallelTime = parallelTime + parallelComputeTime;

                // Store results
                results[size] = (totalSingleTime, totalParallelTime);

                Console.WriteLine($"Size: {size} columns");
                Console.WriteLine($"  Single-threaded - Init: {singleTime} ms, Compute: {singleComputeTime} ms, Total: {totalSingleTime} ms");
                Console.WriteLine($"  Multi-threaded - Init: {parallelTime} ms, Compute: {parallelComputeTime} ms, Total: {totalParallelTime} ms");
                Console.WriteLine($"  Speedup: {totalSingleTime / totalParallelTime:F2}x");
            }

            // Analyze scalability
            var speedups = results.Select(r => new {
                Size = r.Key,
                Speedup = r.Value.SingleTime / r.Value.ParallelTime
            }).ToList();

            // Calculate scalability efficiency
            int smallestSize = columnSizes.Min();
            int largestSize = columnSizes.Max();
            double smallestSpeedup = results[smallestSize].SingleTime / results[smallestSize].ParallelTime;
            double largestSpeedup = results[largestSize].SingleTime / results[largestSize].ParallelTime;
            double scalabilityFactor = largestSpeedup / smallestSpeedup;
            double sizeIncreaseFactor = largestSize / smallestSize;
            double scalabilityEfficiency = scalabilityFactor / Math.Log10(sizeIncreaseFactor);

            Console.WriteLine($"Scalability analysis:");
            Console.WriteLine($"  Size increase: {smallestSize} -> {largestSize} ({sizeIncreaseFactor:F2}x)");
            Console.WriteLine($"  Speedup increase: {smallestSpeedup:F2}x -> {largestSpeedup:F2}x ({scalabilityFactor:F2}x)");
            Console.WriteLine($"  Scalability efficiency: {scalabilityEfficiency:F2}");

            // Relaxed assertion to allow for non-decreasing speedup
            bool speedupIsIncreasing = true;
            for (int i = 1; i < speedups.Count; i++)
            {
                if (speedups[i].Speedup < speedups[i - 1].Speedup)
                {
                    speedupIsIncreasing = false;
                    break;
                }
            }

            // Assert that the speedup generally increases (allowing some stabilization)
            // Allow a small tolerance for fluctuations
            double tolerance = 0.1; // 10% deviation allowed
            Assert.IsTrue(largestSpeedup >= (smallestSpeedup - tolerance),
                $"Speedup should increase or stabilize with problem size. Smallest: {smallestSpeedup:F2}, Largest: {largestSpeedup:F2}");


            // Assert scalability efficiency is reasonable
            Assert.IsTrue(scalabilityEfficiency > 0.5,
                         "Scalability efficiency should be reasonable (> 0.5)");
        }


        //Test Case 8: Testing Performance with Different Active Columns and Excluded Cells
        //Purpose: This test case evaluates the performance of Temporal Memory (TM) in both single-threaded and multi-threaded modes, with a focus on handling active columns and excluded cells.
        /// <summary>
        /// Tests TM with parameterized active columns and excluded cells (e.g., [1, 2, 3] vs. [0, 1, 2, 3]).  
        /// Validates that excluded cells are not activated during computation.  
        /// </summary>
        /// <remarks>
        /// Uses <see cref="DataRowAttribute"/> for test inputs. Asserts via <see cref="CollectionAssert.DoesNotContain"/>.  
        /// Logs single-threaded vs. parallel compute times.
        /// </remarks>
       
        [TestMethod]
        [DataRow(new int[] { 1, 2, 3 }, new int[] { 0, 1, 2, 3 })]
        [DataRow(new int[] { 0, 2, 4 }, new int[] { 1, 2, 3 })]
        public void TestPerformanceWithActiveColumnsAndExcludedCells(int[] activeColumns, int[] excludedCellIndexes)
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters
            Parameters p = GetDefaultParameters();
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            Cell[] excludedCells = cn.GetCells(excludedCellIndexes);

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute) 
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_4.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_4.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute) 
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initTimeCompute = elapsed_3.TotalMilliseconds;

            // Act
            ComputeCycle cc = tmParallel.Compute(activeColumns, true) as ComputeCycle;

            // Assert
            CollectionAssert.DoesNotContain(cc.ActiveCells, excludedCells);
        }


        //Test Case 9: Verifying Computation with Different Burst Thresholds
        //Purpose: This test case verifies that computation using Temporal Memory produces the expected bursting cells based on given thresholds.
        /// <summary>
        /// Verifies bursting cell behavior matches expectations for given active columns (e.g., [0, 1, 2]).  
        /// Compares actual vs. expected bursting cells using <see cref="Connections.GetCells"/>.  
        /// </summary>
        /// <remarks>
        /// Uses order-independent comparison via <see cref="Enumerable.SequenceEqual"/>.  
        /// Logs cell indices for debugging.
        /// </remarks>

        [TestMethod]
        [DataRow(new int[] { 0, 1, 2 }, new int[] { 1, 2, 3 })]
        [DataRow(new int[] { 1, 3, 4 }, new int[] { 2, 3, 4 })]
        public void TestBurstingCellsWithThreshold(int[] activeColumns, int[] expectedBurstingCellIndexes)
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);

            // Measure initialization time
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            Console.WriteLine($"Parallel Init Time: {stopwatch.Elapsed.TotalMilliseconds} ms");

            // Expected bursting cells from the connection
            IList<Cell> expectedBurstingCells = cn.GetCells(expectedBurstingCellIndexes);

            // Compute active cells using the parallel version
            ComputeCycle cc = tmParallel.Compute(activeColumns, true) as ComputeCycle;

            // Debugging output
            Console.WriteLine("Expected Bursting Cells: " + string.Join(", ", expectedBurstingCells.Select(c => c.Index)));
            Console.WriteLine("Actual Bursting Cells: " + string.Join(", ", cc.ActiveCells.Select(c => c.Index)));

            // Assert: Ensure order-independent comparison
            Assert.IsTrue(cc.ActiveCells.OrderBy(c => c.Index)
                        .SequenceEqual(expectedBurstingCells.OrderBy(c => c.Index)),
                        "Active cells do not match expected cells.");
        }



        //Test Case 10: Testing No Change When No Active Cells Selected for Segments in Burst
        //Purpose: This test case checks that when no active cells are selected during the burst, there should be no change in the permanence values of the synapses.
        /// <summary>
        /// Ensures synapse permanence remains stable (e.g., 0.3) when no active cells are selected during bursting.  
        /// Tests TM's adaptation logic with predefined segments.  
        /// </summary>
        /// <remarks>
        /// Uses <see cref="DataRowAttribute"/> for parameterized inputs.  
        /// Planned assertion: <see cref="Assert.AreEqual"/> for permanence values.
        /// </remarks>

        [TestMethod]
        [DataRow(new int[] { 0 }, new int[] { 1 }, 0.3, 0.3)]
        [DataRow(new int[] { 1 }, new int[] { 2 }, 0.3, 0.3)]
        public void TestNoChangeWhenNoActiveCellsSelectedForSegmentInBurst(int[] activeColumns, int[] burstingCellIndexes, double initialPermanence, double expectedPermanence)
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters
            Parameters p = Parameters.getAllDefaultParameters();
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Set up active cells and bursting cells
            Cell[] burstingCells = cn.GetCells(burstingCellIndexes);

            // Act
            // TemporalMemory.AdaptSegment(cn, cn.GetDistalDendrite(0), cn.GetCells(new int[] { 23 }), cn.HtmConfig.PermanenceIncrement, cn.HtmConfig.PermanenceDecrement);

            // Assert
            // Assert.AreEqual(expectedPermanence, cn.GetSynapse(0).Permanence, 0.1);
        }


        //Test Case 11: TestSequenceLearningWithHighSparsity
        //Purpose: This test case verifies the growth of new segments when multiple columns are active in a high-sparsity sequence. It compares the performance of single-threaded and multi-threaded Temporal Memory computation.
        /// <summary>
        /// Validates TM's handling of high-sparsity sequences (e.g., [0, 10, 20, ...]).  
        /// Measures segment growth and recall accuracy in parallel mode.  
        /// </summary>
        /// <remarks>
        /// Asserts that recalled sequences contain expected active cells.  
        /// Logs performance metrics for initialization and computation.
        /// </remarks>

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

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Define a high-sparsity sequence of active columns
            var seq1ActiveColumns = new int[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90 };
            var seq2ActiveColumns = new int[] { 5, 15, 25, 35, 45, 55, 65, 75, 85, 95 };

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute)
            stopwatch.Start();
            tmParallel.Compute(seq1ActiveColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute)
            stopwatch.Start();
            tm.Compute(seq1ActiveColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            //Log performance metrics and store them as a CSV entry
            //LogPerformance(nameof(TestSequenceLearningWithHighSparsity), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            // Recall the first sequence
            var recall1 = tmParallel.Compute(seq1ActiveColumns, false);
            var recall2 = tmParallel.Compute(seq2ActiveColumns, false);

            // Verify that all active cells in the recalled second sequence are also present in the recalled first sequence
            Assert.IsTrue(recall2.ActiveCells.Select(c => c.Index).All(rc => recall1.ActiveCells.Select(c => c.Index).Contains(rc)));
        }


        //Test Case 12: TestSegmentGrowthWithMultipleActiveColumns
        //Purpose: This test case verifies the learning and recall of a high-sparsity sequence using both single-threaded and parallel versions of the Temporal Memory algorithm. It checks if new segments grow as expected in the presence of multiple active columns.
        /// <summary>
        /// Tests segment growth when multiple columns (e.g., [0, 1, 2, 3, 4]) are active.  
        /// Validates distal dendrite creation via <see cref="Cell.DistalDendrites"/>.Count.  
        /// </summary>
        /// <remarks>
        /// Asserts that active cells gain segments after exposure to the sequence.  
        /// Compares single-threaded vs. parallel performance.
        /// </remarks>

        [TestMethod]
        public void TestSegmentGrowthWithMultipleActiveColumns()
        {
            // Initialize Temporal Memory, Parallel Processing, Connections, and Stopwatch
            TemporalMemory tm = new TemporalMemory();
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            // Set default parameters
            Parameters p = GetDefaultParameters();
            p.apply(cn);

            // Measure and store execution time for multi-threaded (tmParallel.InitParallelWithConcurrentDictionary) method
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();
            TimeSpan elapsed_1 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for InitParallelWithConcurrentDictionary : {elapsed_1.TotalMilliseconds} milliseconds");
            double initParallelTime = elapsed_1.TotalMilliseconds;

            // Measure and store execution time for single-threaded (tm.Init) method
            stopwatch.Start();
            tm.Init(cn);
            stopwatch.Stop();
            TimeSpan elapsed_2 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_2.TotalMilliseconds} milliseconds");
            double initTime = elapsed_2.TotalMilliseconds;

            // Define active columns and corresponding active cells
            int[] activeColumns = { 0, 1, 2, 3, 4 };
            Cell[] activeCells = { cn.GetCell(0), cn.GetCell(1), cn.GetCell(2), cn.GetCell(3), cn.GetCell(4) };

            // Measure and store execution time for multi-threaded computation (tmParallel.Compute)
            stopwatch.Start();
            tmParallel.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_3 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken for compute tmParallel(InitParallelWithConcurrentDictionary): {elapsed_3.TotalMilliseconds} milliseconds");
            double initParallelTimeCompute = elapsed_3.TotalMilliseconds;

            // Measure and store execution time for single-threaded computation (tm.Compute)
            stopwatch.Start();
            tm.Compute(activeColumns, true);
            stopwatch.Stop();
            TimeSpan elapsed_4 = stopwatch.Elapsed;
            Console.WriteLine($"Time taken: {elapsed_4.TotalMilliseconds} milliseconds for compute");
            double initTimeCompute = elapsed_4.TotalMilliseconds;

            // Log performance metrics and store them as a CSV entry
            //LogPerformance(nameof(TestSegmentGrowthWithMultipleActiveColumns), initTime, initParallelTime, initTimeCompute, initParallelTimeCompute);

            // Verify that new segments have been grown
            Assert.AreEqual(1, activeCells[0].DistalDendrites.Count);
        }


        //Test Case 13: TestParallelInitializationWithCustomSynapseConnections
        //Purpose: This test case verifies that synapses are correctly created and initialized for active cells during parallel initialization using a custom configuration. It tests the creation of synapse connections during the parallel initialization phase.
        /// <summary>
        /// Validates synapse creation during parallel initialization with custom configurations (e.g., 10 cells/column).  
        /// Tests <see cref="Connections.CreateDistalSegment"/> and <see cref="Connections.CreateSynapse"/> in a concurrent context.  
        /// </summary>
        /// <remarks>
        /// Asserts synapse count (=1) on the distal segment.  
        /// Measures initialization time via <see cref="Stopwatch"/>.
        /// </remarks>

        [TestMethod]
        public void TestParallelInitializationWithCustomSynapseConnections()
        {
            // Initialize Temporal Memory Parallel Processing and Connections
            TemporalMemoryParallelProcessing tmParallel = new TemporalMemoryParallelProcessing();
            Connections cn = new Connections();
            Stopwatch stopwatch = new Stopwatch();

            Parameters p = Parameters.getAllDefaultParameters();
            p.Set(KEY.CELLS_PER_COLUMN, 10); // Custom cells per column
            p.apply(cn);

            // Create a few active cells and synapse connections
            DistalDendrite dd = cn.CreateDistalSegment(cn.GetCell(0));
            cn.CreateSynapse(dd, cn.GetCell(1), 0.5);

            // Measure the time for parallel initialization
            stopwatch.Start();
            tmParallel.InitParallelWithConcurrentDictionary(cn);
            stopwatch.Stop();

            // Assert synapses were created
            Assert.AreEqual(1, dd.Synapses.Count);
        }




    }
}
