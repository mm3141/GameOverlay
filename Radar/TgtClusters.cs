// <copyright file="TgtClusters.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Radar
{
    using System.Numerics;

    /// <summary>
    /// For storing clusters created from important tgt files.
    /// </summary>
    public struct TgtClusters
    {
        /// <summary>
        /// Text to display for these clusters.
        /// </summary>
        public string Display;

        /// <summary>
        /// Expected number of clusters.
        /// </summary>
        public int ClustersCount;

        /// <summary>
        /// Clusters locations.
        /// </summary>
        public Vector2[] Clusters;

        /// <summary>
        /// Makes the cluster invalid.
        /// </summary>
        public void MakeInvalid()
        {
            if (this.ClustersCount > 0)
            {
                this.Clusters[0].X = float.NaN;
            }
        }

        /// <summary>
        /// Makes the cluster valid.
        /// </summary>
        public void MakeValid()
        {
            if (this.ClustersCount > 0)
            {
                this.Clusters[0].X = 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the cluster is valid or not.
        /// </summary>
        /// <returns>true in case the cluster is valid to use otherwise false.</returns>
        public bool IsValid()
        {
            return this.ClustersCount > 0 && !float.IsNaN(this.Clusters[0].X);
        }
    }
}
