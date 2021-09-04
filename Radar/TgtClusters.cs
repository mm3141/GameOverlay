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
    }
}
