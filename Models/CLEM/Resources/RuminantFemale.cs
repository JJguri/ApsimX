﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Models.CLEM.Resources
{
    /// <summary>
    /// Object for an individual female Ruminant.
    /// </summary>

    public class RuminantFemale : Ruminant
    {
        // Female Ruminant properties

        /// <summary>
        /// The age of female at last birth
        /// </summary>
        public double AgeAtLastBirth { get; set; }

        /// <summary>
        /// Number of births for the female (twins = 1 birth)
        /// </summary>
        public int NumberOfBirths { get; set; }

        /// <summary>
        /// Births this timestep
        /// </summary>
        public int NumberOfBirthsThisTimestep { get; set; }

        /// <summary>
        /// The age at last conception
        /// </summary>
        public double AgeAtLastConception { get; set; }

        /// <summary>
        /// Weight at time of conception
        /// </summary>
        public double WeightAtConception { get; set; }

        /// <summary>
        /// Previous conception rate
        /// </summary>
        public double PreviousConceptionRate { get; set; }

        /// <summary>
        /// Weight lost at birth due to calf
        /// </summary>
        public double WeightLossDueToCalf { get; set; }

        /// <summary>
        /// Indicates if this female is a heifer
        /// Heifer equals less than min breed age and no offspring
        /// </summary>
        public bool IsHeifer
        {
            get
            {
                // wiki - weaned, no calf, <3 years. We use the ageAtFirstMating
                return (this.Weaned && this.NumberOfBirths == 0 && this.Age < this.BreedParams.MinimumAge1stMating);
            }
        }

        /// <summary>
        /// Calculate the number of offspring this preganacy given multiple offspring rates
        /// </summary>
        /// <returns></returns>
        public int CalulateNumberOfOffspringThisPregnancy()
        {
            int birthCount = 1;
            if (this.BreedParams.MultipleBirthRate != null)
            {
                double rnd = ZoneCLEM.RandomGenerator.NextDouble();
                double birthProb = 0;
                foreach (double i in this.BreedParams.MultipleBirthRate)
                {
                    birthCount++;
                    birthProb += i;
                    if (rnd <= birthProb)
                    {
                        return birthCount;
                    }
                }
                birthCount = 1;
            }
            return birthCount;
        }


        /// <summary>
        /// Indicates if birth is due this month
        /// Knows whether the feotus(es) have survived
        /// </summary>
        public bool BirthDue
        {
            get
            {
                if(SuccessfulPregnancy)
                {
                    return this.Age >= this.AgeAtLastConception + this.BreedParams.GestationLength && this.AgeAtLastConception > this.AgeAtLastBirth;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Method to handle birth changes
        /// </summary>
        public void UpdateBirthDetails()
        {
            if (SuccessfulPregnancy)
            {
                NumberOfBirths++;
                NumberOfBirthsThisTimestep = CarryingCount;
            }
            AgeAtLastBirth = this.Age;
            MilkingPerformed = false;
        }

        /// <summary>
        /// Indicates if the individual is pregnant
        /// </summary>
        public bool IsPregnant
        {
            get
            {
                return (this.Age < this.AgeAtLastConception + this.BreedParams.GestationLength && this.SuccessfulPregnancy);
            }
        }

        /// <summary>
        /// Indicates if individual is carrying multiple feotus
        /// </summary>
        public int CarryingCount { get; set; }

        /// <summary>
        /// Method to remove one offspring that dies between conception and death
        /// </summary>
        public void OneOffspringDies()
        {
            CarryingCount--;
            if(CarryingCount <= 0)
            {
                SuccessfulPregnancy = false;
                AgeAtLastBirth = this.Age;
            }
        }

        /// <summary>
        /// Method to handle conception changes
        /// </summary>
        public void UpdateConceptionDetails(int number, double rate, int ageOffsett)
        {
            // if she was dry breeder remove flag as she has become pregnant.
            if (SaleFlag == HerdChangeReason.DryBreederSale)
            {
                SaleFlag = HerdChangeReason.None;
            }
            PreviousConceptionRate = rate;
            CarryingCount = number;
            WeightAtConception = this.Weight;
            AgeAtLastConception = this.Age + ageOffsett;
            SuccessfulPregnancy = true;
        }

        /// <summary>
        /// Indicates if the individual is a dry breeder
        /// </summary>
        public bool DryBreeder { get; set; }

        /// <summary>
        /// Indicates if the individual is lactating
        /// </summary>
        public bool IsLactating
        {
            get
            {
                // Had birth after last conception
                // Time since birth < milking days
                // Last pregnancy was successful
                // Mother has suckling offspring OR
                // Cow has been milked since weaning.
                return (this.AgeAtLastBirth > this.AgeAtLastConception & (this.Age - this.AgeAtLastBirth)*30.4 <= this.BreedParams.MilkingDays & SuccessfulPregnancy & (this.SucklingOffspringList.Count() > 0 | this.MilkingPerformed));
            }            
        }

        /// <summary>
        /// Calculate the MilkinIndicates if the individual is lactating
        /// </summary>
        public double DaysLactating
        {
            get
            {
                if(IsLactating)
                {
                    double dl = (((this.Age - this.AgeAtLastBirth) * 30.4 <= this.BreedParams.MilkingDays) ? (this.Age - this.AgeAtLastBirth) * 30.4 : 0);
                    // add half a timestep
                    return dl+15;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Determines if milking has been performed on individual to increase milk production
        /// </summary>
        public bool MilkingPerformed { get; set; }

        /// <summary>
        /// Amount of milk available in the month (L)
        /// </summary>
        public double MilkCurrentlyAvailable { get; set; }

        /// <summary>
        /// Potential amount of milk produced (L/day)
        /// </summary>
        public double MilkProductionPotential { get; set; }

        /// <summary>
        /// Amount of milk produced (L/day)
        /// </summary>
        public double MilkProduction { get; set; }

        /// <summary>
        /// Amount of milk produced this time step
        /// </summary>
        public double MilkProducedThisTimeStep { get; set; }

        /// <summary>
        /// Amount of milk suckled this time step
        /// </summary>
        public double MilkSuckledThisTimeStep { get; set; }

        /// <summary>
        /// Amount of milk milked this time step
        /// </summary>
        public double MilkMilkedThisTimeStep { get; set; }

        /// <summary>
        /// Method to remove milk from female
        /// </summary>
        /// <param name="amount">Amount to take</param>
        /// <param name="reason">Reason for taking milk</param>
        public void TakeMilk(double amount, MilkUseReason reason)
        {
            amount = Math.Min(amount, MilkCurrentlyAvailable);
            MilkCurrentlyAvailable -= amount;
            switch (reason)
            {
                case MilkUseReason.Suckling:
                    MilkSuckledThisTimeStep += amount;
                    break;
                case MilkUseReason.Milked:
                    MilkMilkedThisTimeStep += amount;
                    break;
                default:
                    throw new ApplicationException("Unknown MilkUseReason [" + reason + "] in TakeMilk method of [r=RuminantFemale]");
            }
        }

        /// <summary>
        /// A list of individuals currently suckling this female
        /// </summary>
        public List<Ruminant> SucklingOffspringList { get; set; }

        /// <summary>
        /// Used to track successful preganacy
        /// </summary>
        public bool SuccessfulPregnancy { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public RuminantFemale()
        {
            SuccessfulPregnancy = false;
            SucklingOffspringList = new List<Ruminant>();
        }
    }

    /// <summary>
    /// Reasons for milk to be taken from female
    /// </summary>
    public enum MilkUseReason
    {
        /// <summary>
        /// Consumed by sucklings
        /// </summary>
        Suckling,
        /// <summary>
        /// Milked
        /// </summary>
        Milked
    }

}
