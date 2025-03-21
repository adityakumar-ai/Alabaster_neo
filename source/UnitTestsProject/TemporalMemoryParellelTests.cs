using NeoCortexApi;
using NeoCortexApi.Entities;
using NeoCortexEntities.NeuroVisualizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestsProject
{
    public class TemporalMemoryParellelTests
    {


        // -------------------------------------------------------------------------------------------------------------------
        // Test Overview for InitParallelWithConcurrentDictionary Method
        // -------------------------------------------------------------------------------------------------------------------
        //
        // The following test cases cover various scenarios for the `InitParallelWithConcurrentDictionary` method 
        // in the TemporalMemoryParallelProcessing class. These tests ensure the method handles multiple edge cases 
        // and works as expected under different conditions.
        //
        // 0. Test Initialization of Temporal Memory with Invalid Configuration Values
        //    - Test Initialization of Temporal Memory with Invalid Configuration Values (SynPermConnected, NumInputs, ColumnDimensions, CellsPerColumn).
        //
        // 1. Test when Memory is null and columns need to be created: 
        //    - Verifies that the method correctly initializes a new SparseObjectMatrix and creates columns when Memory is null.
        //
        // 2. Test with a large number of columns to check for scalability: 
        //    - Tests the method's scalability by simulating a large number of columns (e.g., thousands) and verifying correct operation.
        //
        // 3. Test with a small number of columns (edge case): 
        //    - Verifies the behavior when the number of columns is minimal (e.g., 1 or 2), which is a possible edge case.
        //
        // 4. Test with an unusually high synPermConnected value: 
        //    - Verifies the behavior when the `synPermConnected` value is unusually high (e.g., 1.0), ensuring that the system handles extreme values correctly.
        //
        // 5. Test with the maximum possible number of inputs: 
        //    - Tests the method with the maximum possible number of inputs (e.g., an extremely large value) to verify that the system can handle a large input space.
        //
        // ------------------------------------------------------------------------------------------------------------------

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




    }
}
