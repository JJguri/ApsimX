﻿namespace Models.Factorial
{
    using APSIM.Shared.Utilities;
    using Models.Core;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// This class permutates all child models by each other.
    /// </summary>
    [ValidParent(ParentType = typeof(Factors))]
    [ValidParent(ParentType = typeof(Factor))]
    [ValidParent(ParentType = typeof(Permutation))]
    public class Permutation : Model
    {
        /// <summary>
        /// Get a list of all permutations of child factors and compositefactors.
        /// </summary>
        internal List<List<CompositeFactor>> GetPermutations()
        {
            var factors = new List<List<CompositeFactor>>();
            foreach (Factor factor in Apsim.Children(this, typeof(Factor)))
            {
                if (factor.Enabled)
                    factors.Add(factor.GetCompositeFactors());
            }

            var compositeFactors = Apsim.Children(this, typeof(CompositeFactor)).Where(cf => cf.Enabled);

            var permutations = new List<List<CompositeFactor>>();
            if (compositeFactors.Count() > 0)
            {
                // Loop through each composite factor and permute with the factor children.
                foreach (CompositeFactor compositeFactor in compositeFactors)
                {
                    var valuesToPermutate = new List<List<CompositeFactor>>(factors);
                    valuesToPermutate.Add(new List<CompositeFactor>() { compositeFactor });
                    permutations.AddRange(MathUtilities.AllCombinationsOf<CompositeFactor>(valuesToPermutate.ToArray()));
                }
            }
            else
                permutations = MathUtilities.AllCombinationsOf<CompositeFactor>(factors.ToArray());

            return permutations;
        }
    }
}
