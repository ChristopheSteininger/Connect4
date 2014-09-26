using Connect4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Connect4Test
{
    
    
    /// <summary>
    ///This is a test class for AIPlayerTest and is intended
    ///to contain all AIPlayerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class AIPlayerTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for MergeSort
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Connect4.exe")]
        public void MergeSortTest()
        {
            // TODO: Use AIPlayer_Accessor to test MergeSort.
            Random random = new Random(1);
            AIPlayer target = new AIPlayer(0, new Board(7, 6, null));

            for (int iteration = 0; iteration < 1000; iteration++)
            {
                // Generate a random moves and weights array.
                int[] moves = new int[random.Next(1, 31)];
                int[] weights = new int[moves.Length];
                for (int i = 0; i < moves.Length; i++)
                {
                    moves[i] = i;
                    weights[i] = random.Next();
                }

                // Copy both arrays, so the result can be checked against the original.
                int[] sortedMoves = new int[moves.Length];
                int[] sortedWeights = new int[weights.Length];
                Buffer.BlockCopy(moves, 0, sortedMoves, 0, moves.Length * sizeof(int));
                Buffer.BlockCopy(weights, 0, sortedWeights, 0, weights.Length * sizeof(int));

                // Sort moves using weights.
                //target.MergeSort(sortedMoves, sortedWeights, 0, moves.Length);

                for (int i = 0; i < sortedMoves.Length; i++)
                {
                    // Check that the weight array is sorted.
                    if (i > 0)
                    {
                        Assert.IsTrue(sortedWeights[i - 1] > sortedWeights[i]);
                    }

                    // Check that each move still has the same weight.
                    Assert.IsTrue(sortedWeights[i] == weights[sortedMoves[i]]);
                }
            }
        }

        /// <summary>
        ///A test for SelectionSort
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Connect4.exe")]
        public void SelectionSortTest()
        {
            //PrivateObject param0 = null; // TODO: Initialize to an appropriate value
            //AIPlayer_Accessor target = new AIPlayer_Accessor(param0); // TODO: Initialize to an appropriate value
            //int[] moves = null; // TODO: Initialize to an appropriate value
            //int[] weights = null; // TODO: Initialize to an appropriate value
            //int n = 0; // TODO: Initialize to an appropriate value
            //target.SelectionSort(moves, weights, n);
            //Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }
    }
}
