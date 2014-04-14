using Connect4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Connect4Test
{
    /// <summary>
    ///This is a test class for TranspositionTableTest and is intended
    ///to contain all TranspositionTableTest Unit Tests
    ///</summary>
    [TestClass()]
    public class TranspositionTableTest
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
        ///A test for CreateEntry
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Connect4.exe")]
        public void CreateEntryTest()
        {
            const int minDepth = 0;
            const int maxDepth = 42;

            const int minBestMove = 0;
            const int maxBestMove = 6;

            const int minScore = -128;
            const int maxScore = 127;

            const int minNodeType = 1;
            const int maxNodeType = 3;

            const ulong hash = 1234567890;

            ulong entry;
            TranspositionTable table = new TranspositionTable();

            for (int bestMove = minBestMove; bestMove <= maxBestMove; bestMove++)
            {
                entry = TranspositionTable.CreateEntry(maxDepth, bestMove, hash,
                    maxScore, maxNodeType);
                Assert.AreEqual(bestMove, TranspositionTable.GetBestMove(entry));
            }

            for (int depth = minDepth; depth <= maxDepth; depth++)
            {
                entry = TranspositionTable.CreateEntry(depth, maxBestMove, hash,
                    maxScore, maxNodeType);
                Assert.AreEqual(depth, TranspositionTable.GetDepth(entry));
            }

            for (int nodeType = minNodeType; nodeType <= maxNodeType; nodeType++)
            {
                entry = TranspositionTable.CreateEntry(maxDepth, maxBestMove, hash,
                    maxScore, nodeType);
                Assert.AreEqual(nodeType, TranspositionTable.GetNodeType(entry));
            }

            for (int score = minScore; score <= maxScore; score++)
            {
                entry = TranspositionTable.CreateEntry(maxDepth, maxBestMove, hash,
                    score, maxNodeType);
                Assert.AreEqual(score, TranspositionTable.GetScore(entry));
            }
        }
    }
}
