namespace Accord.Tests.MachineLearning
{
    using Accord.MachineLearning;
    using Accord.Math;
    using Accord.Math.Random;
    using NUnit.Framework;
    using System;
    using System.Linq;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters.Binary;
    using Accord.IO;
    using Accord.Math.Distances;
    using Accord.Statistics.Filters;
    using Accord.Statistics;

    [TestFixture]
    public class MiniBatchKMeansTest
    {


        [Test]
        public void MiniBatchKMeansConstructorTest()
        {
            Accord.Math.Random.Generator.Seed = 0;

            // Declare some observations
            double[][] observations =
            {
                new double[] { -5, -2, -1 },
                new double[] { -5, -5, -6 },
                new double[] {  2,  1,  1 },
                new double[] {  1,  1,  2 },
                new double[] {  1,  2,  2 },
                new double[] {  3,  1,  2 },
                new double[] { 11,  5,  4 },
                new double[] { 15,  5,  6 },
                new double[] { 10,  5,  6 },
            };

            double[][] orig = observations.MemberwiseClone();

            // Create a new Mini-Batch K-Means algorithm with 3 clusters 
            MiniBatchKMeans mbkmeans = new MiniBatchKMeans(3, 2);

            // Compute the algorithm, retrieving an integer array
            //  containing the labels for each of the observations
            int[] labels = mbkmeans.Learn(observations).Decide(observations);

            // As a result, the first two observations should belong to the
            //  same cluster (thus having the same label). The same should
            //  happen to the next four observations and to the last three.

            Assert.AreEqual(labels[0], labels[1]);

            Assert.AreEqual(labels[2], labels[3]);
            Assert.AreEqual(labels[2], labels[4]);
            Assert.AreEqual(labels[2], labels[5]);

            Assert.AreEqual(labels[6], labels[7]);
            Assert.AreEqual(labels[6], labels[8]);

            Assert.AreNotEqual(labels[0], labels[2]);
            Assert.AreNotEqual(labels[2], labels[6]);
            Assert.AreNotEqual(labels[0], labels[6]);


            int[] labels2 = mbkmeans.Clusters.Decide(observations);
            Assert.IsTrue(labels.IsEqual(labels2));

            // the data must not have changed!
            Assert.IsTrue(orig.IsEqual(observations));
        }

        [Test]
        public void learn_test()
        {
            #region doc_learn
            Accord.Math.Random.Generator.Seed = 0;

            // Declare some observations
            double[][] observations =
            {
                new double[] { -5, -2, -1 },
                new double[] { -5, -5, -6 },
                new double[] {  2,  1,  1 },
                new double[] {  1,  1,  2 },
                new double[] {  1,  2,  2 },
                new double[] {  3,  1,  2 },
                new double[] { 11,  5,  4 },
                new double[] { 15,  5,  6 },
                new double[] { 10,  5,  6 },
            };

            // Create a new Mini-Batch K-Means algorithm
            MiniBatchKMeans mbkmeans = new MiniBatchKMeans(k: 3, batchSize: 2);

            // Compute and retrieve the data centroids
            var clusters = mbkmeans.Learn(observations);

            // Use the centroids to parition all the data
            int[] labels = clusters.Decide(observations);
            #endregion

            Assert.AreEqual(labels[0], labels[1]);

            Assert.AreEqual(labels[2], labels[3]);
            Assert.AreEqual(labels[2], labels[4]);
            Assert.AreEqual(labels[2], labels[5]);

            Assert.AreEqual(labels[6], labels[7]);
            Assert.AreEqual(labels[6], labels[8]);

            Assert.AreNotEqual(labels[0], labels[2]);
            Assert.AreNotEqual(labels[2], labels[6]);
            Assert.AreNotEqual(labels[0], labels[6]);

            int[] labels2 = mbkmeans.Clusters.Decide(observations);
            Assert.IsTrue(labels.IsEqual(labels2));

            // the data must not have changed!
            double[][] orig =
            {
                new double[] { -5, -2, -1 },
                new double[] { -5, -5, -6 },
                new double[] {  2,  1,  1 },
                new double[] {  1,  1,  2 },
                new double[] {  1,  2,  2 },
                new double[] {  3,  1,  2 },
                new double[] { 11,  5,  4 },
                new double[] { 15,  5,  6 },
                new double[] { 10,  5,  6 },
            };

            Assert.IsTrue(orig.IsEqual(observations));

            var c = new KMeansClusterCollection.KMeansCluster[clusters.Count];
            int i = 0;
            foreach (var cluster in clusters)
                c[i++] = cluster;

            for (i = 0; i < c.Length; i++)
                Assert.AreSame(c[i], clusters[i]);
        }

        [Test]
        public void learn_test_mixed()
        {
            #region doc_learn_mixed
            Accord.Math.Random.Generator.Seed = 0;

            // Declare some mixed discrete and continuous observations
            double[][] observations =
            {
                //             (categorical) (discrete) (continuous)
                new double[] {       1,          -1,        -2.2      },
                new double[] {       1,          -6,        -5.5      },
                new double[] {       2,           1,         1.1      },
                new double[] {       2,           2,         1.2      },
                new double[] {       2,           2,         2.6      },
                new double[] {       3,           2,         1.4      },
                new double[] {       3,           4,         5.2      },
                new double[] {       1,           6,         5.1      },
                new double[] {       1,           6,         5.9      },
            };

            // Create a new codification algorithm to convert 
            // the mixed variables above into all continuous:
            var codification = new Codification<double>()
            {
                CodificationVariable.Categorical,
                CodificationVariable.Discrete,
                CodificationVariable.Continuous
            };

            // Learn the codification from observations
            var model = codification.Learn(observations);

            // Transform the mixed observations into only continuous:
            double[][] newObservations = model.ToDouble().Transform(observations);

            // (newObservations will be equivalent to)
            double[][] expected =
            {
                //               (one hot)    (discrete)    (continuous)
                new double[] {    1, 0, 0,        -1,          -2.2      },
                new double[] {    1, 0, 0,        -6,          -5.5      },
                new double[] {    0, 1, 0,         1,           1.1      },
                new double[] {    0, 1, 0,         2,           1.2      },
                new double[] {    0, 1, 0,         2,           2.6      },
                new double[] {    0, 0, 1,         2,           1.4      },
                new double[] {    0, 0, 1,         4,           5.2      },
                new double[] {    1, 0, 0,         6,           5.1      },
                new double[] {    1, 0, 0,         6,           5.9      },
            };

            // Create a new K-Means algorithm
            MiniBatchKMeans mbkmeans = new MiniBatchKMeans(k: 3, batchSize: 2);

            // Compute and retrieve the data centroids
            var clusters = mbkmeans.Learn(observations);

            // Use the centroids to parition all the data
            int[] labels = clusters.Decide(observations);
            #endregion

            
            Assert.IsTrue(expected.IsEqual(newObservations, 1e-8));

            Assert.AreEqual(3, codification.NumberOfInputs);
            Assert.AreEqual(5, codification.NumberOfOutputs);
            Assert.AreEqual(3, codification.Columns.Count);
            Assert.AreEqual("0", codification.Columns[0].ColumnName);
            Assert.AreEqual(3, codification.Columns[0].NumberOfSymbols);
            Assert.AreEqual(1, codification.Columns[0].NumberOfInputs);
            Assert.AreEqual(1, codification.Columns[0].NumberOfOutputs);
            Assert.AreEqual(3, codification.Columns[0].NumberOfClasses);
            Assert.AreEqual(CodificationVariable.Categorical, codification.Columns[0].VariableType);
            Assert.AreEqual("1", codification.Columns[1].ColumnName);
            Assert.AreEqual(1, codification.Columns[1].NumberOfSymbols);
            Assert.AreEqual(1, codification.Columns[1].NumberOfInputs);
            Assert.AreEqual(1, codification.Columns[1].NumberOfOutputs);
            Assert.AreEqual(1, codification.Columns[1].NumberOfClasses);
            Assert.AreEqual(CodificationVariable.Discrete, codification.Columns[1].VariableType);
            Assert.AreEqual("2", codification.Columns[2].ColumnName);
            Assert.AreEqual(1, codification.Columns[2].NumberOfSymbols);
            Assert.AreEqual(1, codification.Columns[2].NumberOfInputs);
            Assert.AreEqual(1, codification.Columns[2].NumberOfOutputs);
            Assert.AreEqual(1, codification.Columns[2].NumberOfClasses);
            Assert.AreEqual(CodificationVariable.Continuous, codification.Columns[2].VariableType);

            Assert.AreEqual(labels[0], labels[2]);
            Assert.AreEqual(labels[0], labels[3]);
            Assert.AreEqual(labels[0], labels[4]);
            Assert.AreEqual(labels[0], labels[5]);

            Assert.AreEqual(labels[6], labels[7]);
            Assert.AreEqual(labels[6], labels[8]);

            Assert.AreNotEqual(labels[0], labels[1]);
            Assert.AreNotEqual(labels[0], labels[6]);

            int[] labels2 = mbkmeans.Clusters.Decide(observations);
            Assert.IsTrue(labels.IsEqual(labels2));

            var c = new KMeansClusterCollection.KMeansCluster[clusters.Count];
            int i = 0;
            foreach (var cluster in clusters)
                c[i++] = cluster;

            for (i = 0; i < c.Length; i++)
                Assert.AreSame(c[i], clusters[i]);
        }

        [Test]
        public void uniform_sampling_test()
        {
            Accord.Math.Random.Generator.Seed = 0;

            // Declare some observations
            double[][] observations =
            {
                new double[] { -5, -2, -1 },
                new double[] { -5, -5, -6 },
                new double[] {  2,  1,  1 },
                new double[] {  1,  1,  2 },
                new double[] {  1,  2,  2 },
                new double[] {  3,  1,  2 },
                new double[] { 11,  5,  4 },
                new double[] { 15,  5,  6 },
                new double[] { 10,  5,  6 },
            };

            // Create a new K-Means algorithm
            MiniBatchKMeans mbkmeans = new MiniBatchKMeans(k: 3, batchSize: 2)
            {
                UseSeeding = Seeding.Uniform
            };

            // Compute and retrieve the data centroids
            var clusters = mbkmeans.Learn(observations);

            int[] labels = clusters.Decide(observations);

            int[] labels2 = mbkmeans.Clusters.Decide(observations);

            // the data must not have changed!
            double[][] orig =
            {
                new double[] { -5, -2, -1 },
                new double[] { -5, -5, -6 },
                new double[] {  2,  1,  1 },
                new double[] {  1,  1,  2 },
                new double[] {  1,  2,  2 },
                new double[] {  3,  1,  2 },
                new double[] { 11,  5,  4 },
                new double[] { 15,  5,  6 },
                new double[] { 10,  5,  6 },
            };

            Assert.IsTrue(orig.IsEqual(observations));
        }

        [Test]
        public void learn_test_weights()
        {
            #region doc_learn_weights
            Accord.Math.Random.Generator.Seed = 0;

            // A common desire when doing clustering is to attempt to find how to 
            // weight the different components / columns of a dataset, giving them 
            // different importances depending on the end goal of the clustering task.

            // Declare some observations
            double[][] observations =
            {
                new double[] { -5, -2, -1 },
                new double[] { -5, -5, -6 },
                new double[] {  2,  1,  1 },
                new double[] {  1,  1,  2 },
                new double[] {  1,  2,  2 },
                new double[] {  3,  1,  2 },
                new double[] { 11,  5,  4 },
                new double[] { 15,  5,  6 },
                new double[] { 10,  5,  6 },
            };

            // Create a new K-Means algorithm
            MiniBatchKMeans mbkmeans = new MiniBatchKMeans(k: 3, batchSize: 2)
            {
                // For example, let's say we would like to consider the importance of 
                // the first column as 0.1, the second column as 0.7 and the third 0.9
                Distance = new WeightedSquareEuclidean(new double[] { 0.1, 0.7, 1.1 })
            };

            // Compute and retrieve the data centroids
            var clusters = mbkmeans.Learn(observations);

            // Use the centroids to parition all the data
            int[] labels = clusters.Decide(observations);
            #endregion

            Assert.AreEqual(labels[0], labels[2]);

            Assert.AreEqual(labels[0], labels[2]);
            Assert.AreEqual(labels[0], labels[3]);
            Assert.AreEqual(labels[0], labels[4]);
            Assert.AreEqual(labels[0], labels[4]);

            Assert.AreEqual(labels[6], labels[7]);
            Assert.AreEqual(labels[6], labels[8]);

            Assert.AreNotEqual(labels[0], labels[1]);
            Assert.AreNotEqual(labels[2], labels[6]);
            Assert.AreNotEqual(labels[0], labels[6]);

            int[] labels2 = mbkmeans.Clusters.Decide(observations);
            Assert.IsTrue(labels.IsEqual(labels2));

            var c = new KMeansClusterCollection.KMeansCluster[clusters.Count];
            int i = 0;
            foreach (var cluster in clusters)
                c[i++] = cluster;

            for (i = 0; i < c.Length; i++)
                Assert.AreSame(c[i], clusters[i]);
        }
       
        [Test]
        public void MiniBatchKMeansConstructorTest2()
        {
            // Create a new algorithm
            MiniBatchKMeans mbkmeans = new MiniBatchKMeans(3, 2);
            Assert.IsNotNull(mbkmeans.Clusters);
            Assert.IsNotNull(mbkmeans.Distance);
            Assert.IsNotNull(mbkmeans.Clusters.Centroids);
            Assert.IsNotNull(mbkmeans.Clusters.Count);
            Assert.IsNotNull(mbkmeans.Clusters.Covariances);
            Assert.IsNotNull(mbkmeans.Clusters.Proportions);
        }
        #pragma warning disable
        [Test]
        public void MiniBatchKMeansRandomizationTest()
        {
            Generator.Seed = 1;

            // Declare some observations
            double[][] observations =
            {
                new double[] { -5, -2, -1 },
                new double[] { -5, -5, -6 },
                new double[] {  2,  1,  1 },
                new double[] {  1,  1,  2 },
                new double[] {  1,  2,  2 },
                new double[] {  3,  1,  2 },
                new double[] { 11,  5,  4 },
                new double[] { 15,  5,  6 },
                new double[] { 10,  5,  6 },
            };

            double error, e;

            // Create a new algorithm
            MiniBatchKMeans mbkmeans = new MiniBatchKMeans(3, 2);
            mbkmeans.Randomize(observations);

            // Save the first initialization
            double[][] initial = mbkmeans.Clusters.Centroids.MemberwiseClone();

            // Compute the first K-Means
            // The seed for the random number generator has to be set just before any execution of Learn
            // so that the learning methods always uses the same subsequence of the generator's sequence of pseudo-random numbers
            // if we want the algorithm to behave the same.
            Generator.Seed = 1;
            mbkmeans.ComputeError = true;
            mbkmeans.Learn(observations);
            error = mbkmeans.Error;

            // Create more K-Means algorithms 
            //  with the same initializations
            for (int i = 0; i < 1000; i++)
            {
                mbkmeans = new MiniBatchKMeans(3, 2);
                mbkmeans.Clusters.Centroids = initial;
                mbkmeans.ComputeError = true;
                Generator.Seed = 1;
                mbkmeans.Learn(observations);
                e = mbkmeans.Error;

                Assert.AreEqual(error, e);
            }

            // Create more K-Means algorithms 
            //  without the same initialization
            // Now, because we want different randomization
            // we need to use different seed. It is sufficient to
            // set it only once because different calls to Learn
            // will be using different subsequences of pseudo-random numbers in the generator's sequence.
            Generator.Seed = 2;
            bool differ = false;
            for (int i = 0; i < 1000; i++)
            {
                mbkmeans = new MiniBatchKMeans(3, 2);
                mbkmeans.ComputeError = true;
                mbkmeans.Learn(observations);
                e = mbkmeans.Error;

                if (error != e)
                    differ = true;
            }

            Assert.IsTrue(differ);
        }
        #pragma warning restore
        [Test]
        public void MiniBatchKMeansConstructorTest_Distance()
        {
            // Create a new algorithm
            
            MiniBatchKMeans mbkmeans = new MiniBatchKMeans(3, 2, new Accord.Math.Distances.Manhattan());
            Assert.IsNotNull(mbkmeans.Distance);
            Assert.IsTrue(mbkmeans.Distance is Accord.Math.Distances.Manhattan);
        }

        [Test]
        public void MiniBatchKMeansMoreClustersThanSamples()
        {
            Accord.Math.Tools.SetupGenerator(0);


            // Declare some observations
            double[][] observations =
            {
                new double[] { -5, -2, -1 },
                new double[] { -5, -5, -6 },
                new double[] {  2,  1,  1 },
                new double[] {  1,  1,  2 },
                new double[] {  1,  2,  2 },
                new double[] {  3,  1,  2 },
                new double[] { 11,  5,  4 },
                new double[] { 15,  5,  6 },
                new double[] { 10,  5,  6 },
            };

            MiniBatchKMeans mbkmeans = new MiniBatchKMeans(15, 2);

            Assert.Throws<ArgumentException>(() => mbkmeans.Learn(observations), "");
        }

        [Test]
        public void MiniBatchKMeansMorePointsInBatchThanSamples()
        {
            Accord.Math.Tools.SetupGenerator(0);


            // Declare some observations
            double[][] observations =
            {
                new double[] { -5, -2, -1 },
                new double[] { -5, -5, -6 },
                new double[] {  2,  1,  1 },
                new double[] {  1,  1,  2 },
                new double[] {  1,  2,  2 },
                new double[] {  3,  1,  2 },
                new double[] { 11,  5,  4 },
                new double[] { 15,  5,  6 },
                new double[] { 10,  5,  6 },
            };

            MiniBatchKMeans mbkmeans = new MiniBatchKMeans(3, 15);

            Assert.Throws<ArgumentException>(() => mbkmeans.Learn(observations), "");
        }

        [Test]
        public void MiniBatchKMeansMoreWeightsThanSamples()
        {
            Accord.Math.Tools.SetupGenerator(0);


            // Declare some observations
            double[][] observations =
            {
                new double[] { -5, -2, -1 },
                new double[] { -5, -5, -6 },
                new double[] {  2,  1,  1 },
                new double[] {  1,  1,  2 },
                new double[] {  1,  2,  2 },
                new double[] {  3,  1,  2 },
                new double[] { 11,  5,  4 },
                new double[] { 15,  5,  6 },
                new double[] { 10,  5,  6 },
            };

            double[] weights = {0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1.0};

            MiniBatchKMeans mbkmeans = new MiniBatchKMeans(3, 2);

            Assert.Throws<DimensionMismatchException>(() => mbkmeans.Learn(observations, weights), "");
        }

        [Test]
        public void MiniBatchKMeansNoSamples()
        {
            Accord.Math.Tools.SetupGenerator(0);


            // Declare some observations
            double[][] observations = null;

            MiniBatchKMeans mbkmeans = new MiniBatchKMeans(3, 2);

            Assert.Throws<ArgumentNullException>(() => mbkmeans.Learn(observations), "");
        }

        [Test]
        public void MiniBatchKMeansNotRectangularData()
        {
            Accord.Math.Tools.SetupGenerator(0);


            // Declare some observations
            double[][] observations =
            {
                new double[] { -5, -2, -1 },
                new double[] { -5, -5, -6 },
                new double[] {  2,  1,  1 },
                new double[] {  1,  1,  2,  0 },
                new double[] {  1,  2,  2 },
                new double[] {  3,  1,  2 },
                new double[] { 11,  5,  4 },
                new double[] { 15,  5,  6 },
                new double[] { 10,  5,  6 },
            };

            MiniBatchKMeans mbkmeans = new MiniBatchKMeans(3, 2);

            Assert.Throws<DimensionMismatchException>(() => mbkmeans.Learn(observations), "");
        }

        [Test]
        public void MiniBatchKMeansNegativeSumOfWeights()
        {
            Accord.Math.Tools.SetupGenerator(0);

            // Declare some observations
            double[][] observations =
            {
                new double[] { -5, -2, -1 },
                new double[] { -5, -5, -6 },
                new double[] {  2,  1,  1 },
                new double[] {  1,  1,  2 },
                new double[] {  1,  2,  2 },
                new double[] {  3,  1,  2 },
                new double[] { 11,  5,  4 },
                new double[] { 15,  5,  6 },
                new double[] { 10,  5,  6 },
            };

            double[] weights = new double[] {-1, -1, -1, -1, -1, -1, -1, -1, -1};

            MiniBatchKMeans mbkmeans = new MiniBatchKMeans(3, 2);

            Assert.Throws<ArgumentException>(() => mbkmeans.Learn(observations, weights), "");
        }

        [Test]
        public void MiniBatchKMeansNonpositiveNumberOfInitializations()
        {
            Accord.Math.Tools.SetupGenerator(0);

            // Declare some observations
            double[][] observations =
            {
                new double[] { -5, -2, -1 },
                new double[] { -5, -5, -6 },
                new double[] {  2,  1,  1 },
                new double[] {  1,  1,  2 },
                new double[] {  1,  2,  2 },
                new double[] {  3,  1,  2 },
                new double[] { 11,  5,  4 },
                new double[] { 15,  5,  6 },
                new double[] { 10,  5,  6 },
            };


            MiniBatchKMeans mbkmeans = new MiniBatchKMeans(3, 2);

            Assert.Throws<ArgumentException>(() => mbkmeans.NumberOfInitializations = 0, "");
            Assert.Throws<ArgumentException>(() => mbkmeans.NumberOfInitializations = -1, "");
        }
        [Test]
        public void MiniBatchKMeansNonpositiveBatchSize()
        {
            Accord.Math.Tools.SetupGenerator(0);

            // Declare some observations
            double[][] observations =
            {
                new double[] { -5, -2, -1 },
                new double[] { -5, -5, -6 },
                new double[] {  2,  1,  1 },
                new double[] {  1,  1,  2 },
                new double[] {  1,  2,  2 },
                new double[] {  3,  1,  2 },
                new double[] { 11,  5,  4 },
                new double[] { 15,  5,  6 },
                new double[] { 10,  5,  6 },
            };

            MiniBatchKMeans mbkmeans = new MiniBatchKMeans(3, 2);
            Assert.Throws<ArgumentException>(() => mbkmeans.NumberOfInitializations = 0, "");
            Assert.Throws<ArgumentException>(() => mbkmeans.NumberOfInitializations = -1, "");
            Assert.Throws<ArgumentException>(() => { new MiniBatchKMeans(3, 0);}, "");
            Assert.Throws<ArgumentException>(() => { new MiniBatchKMeans(3, -5);}, "");
        }
        
        [Test]
        public void MiniBatchKMeansNonpositiveInitializationBatchSize()
        {
            Accord.Math.Tools.SetupGenerator(0);

            // Declare some observations
            double[][] observations =
            {
                new double[] { -5, -2, -1 },
                new double[] { -5, -5, -6 },
                new double[] {  2,  1,  1 },
                new double[] {  1,  1,  2 },
                new double[] {  1,  2,  2 },
                new double[] {  3,  1,  2 },
                new double[] { 11,  5,  4 },
                new double[] { 15,  5,  6 },
                new double[] { 10,  5,  6 },
            };

            MiniBatchKMeans mbkmeans = new MiniBatchKMeans(3, 2);
            Assert.Throws<ArgumentException>(() => mbkmeans.InitializationBatchSize = 0, "");
            Assert.Throws<ArgumentException>(() => mbkmeans.InitializationBatchSize = -1, "");
        }
       
/*
#if !NO_BINARY_SERIALIZATION
        [Test]
#if NETCORE
        [Ignore("Models created in .NET desktop cannot be de-serialized in .NET Core/Standard (yet)")]
#endif
        public void DeserializationTest1()
        {
            string fileName = Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources", "kmeans.bin");

            KMeans kmeans = Serializer.Load<MiniBatchKMeans>(fileName);

            KMeans kbase = new MiniBatchKMeans(3, 2);

            Assert.AreEqual(kbase.Iterations, kmeans.Iterations);
            Assert.AreEqual(kbase.MaxIterations, kmeans.MaxIterations);
            Assert.AreEqual(kbase.Tolerance, kmeans.Tolerance);

            Assert.AreEqual(kbase.UseSeeding, kmeans.UseSeeding);
            Assert.AreEqual(kbase.ComputeCovariances, kmeans.ComputeCovariances);

            Assert.AreEqual(kbase.ComputeError, kmeans.ComputeError);
            Assert.AreEqual(kbase.ComputeCovariances, kmeans.ComputeCovariances);
            Assert.AreEqual(kbase.Error, kmeans.Error);

            Assert.IsTrue(kbase.ComputeError);
            Assert.IsTrue(kbase.ComputeCovariances);
            Assert.AreEqual(kbase.Distance.GetType(), kmeans.Distance.GetType());
        }
#endif */
    }
}